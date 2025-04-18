using System.Collections.Concurrent;
using FlowLite.Core.Abstractions.Configuration;
using FlowLite.Core.Abstractions.Exporter;
using FlowLite.Core.Abstractions.Fsm;
using FlowLite.Core.Abstractions.Helper;
using FlowLite.Core.Abstractions.Logging;
using FlowLite.Core.Abstractions.Models;
using FlowLite.Core.Abstractions.Storage;
using FlowLite.Core.Export;
using FlowLite.Core.Logging;
using FlowLite.Core.Templates;
using FlowLite.Core.Validators;
namespace FlowLite.Core.Fsm;

/// <summary>
/// Defines a Finite State Machine (FSM) for managing entity states.
/// </summary>
/// <typeparam name="TState">The type representing states.</typeparam>
/// <typeparam name="TTrigger">The type representing triggers (events that cause state transitions).</typeparam>
/// <typeparam name="TKey">The type of the entity key (e.g., Guid or int).</typeparam>
/// <typeparam name="TEntity">The type of the entity associated with the FSM.</typeparam>
public sealed class StateFlowMachine<TState, TTrigger, TKey, TEntity>
    : IStateFlowMachine<TState, TTrigger, TKey, TEntity>
    where TState : struct
    where TTrigger : struct
    where TEntity : class
    where TKey : notnull
{
    private readonly Dictionary<StateTriggerKey<TState, TTrigger>, StateTransition<TState, TTrigger, TEntity>> _transitions =
        new();
    private readonly IEntityStateStorage<TState, TKey, TEntity>? _stateStorage;
    private readonly ConcurrentQueue<(TTrigger?, TState)> _stateHistory = new();
    private readonly HashSet<TState> _finalStates = [];
    private readonly HashSet<TKey> _markedForDeletion = [];
    private readonly StateFlowLogger _logger;
    private readonly AsyncLock _lock = new();
    private readonly TKey _entityKey;
    private readonly TState _initialState;
    private TState _currentState;
    private TEntity? _entity;
    private bool _usedConfiguration;
    private bool _buildConfiguration;
    private bool _disposed;
    private bool _isFinal;

    private const int MaxHistorySize = 100;

    public event Action<TState, TTrigger>? OnStateChanged;
    public event Action<TState, TTrigger, Exception>? OnTransitionFailed;
    public event Action<TEntity>? OnEntityChanged;
    public event Action<TKey>? OnEntityDeleted;

    public TState? CurrentState => _currentState;
    public TEntity? CurrentEntity => _entity;

    private bool IsFinalState => _isFinal;
    public IReadOnlyList<(TTrigger? Trigger, TState State)> GetTransitionHistory() => _stateHistory
        .ToList()
        .AsReadOnly();

    public IReadOnlyList<(DateTime Timestamp, LogLevel Level, string Message)> GetLogs(LogLevel? level = null) =>
        _logger
            .GetLogs(level)
            .ToList()
            .AsReadOnly();

    /// <summary>
    /// The state flow machine with storage
    /// </summary>
    /// <param name="initialState"></param>
    /// <param name="stateStorage"></param>
    /// <param name="entityKey"></param>
    /// <param name="entity"></param>
    public StateFlowMachine(
        TState initialState,
        IEntityStateStorage<TState, TKey, TEntity>? stateStorage,
        TKey entityKey,
        TEntity entity)
    {
        _stateStorage = stateStorage;
        _entityKey = entityKey;
        _entity = entity;
        _logger = new StateFlowLogger();
        _initialState = initialState;
        _ = InitializeStateAsync(_initialState);
        FlowLiteGlobal<TState, TTrigger, TKey, TEntity>.OnMachineCreated?.Invoke(this);
    }
    
    /// <summary>
    /// Default state flow machine (without storage)
    /// </summary>
    /// <param name="initialState"></param>
    /// <param name="entityKey"></param>
    /// <param name="entity"></param>
    public StateFlowMachine(
        TState initialState,
        TKey entityKey,
        TEntity entity)
    {
        _stateStorage = null;
        _entityKey = entityKey;
        _entity = entity;
        _logger = new StateFlowLogger();
        _initialState = initialState;
        _ = InitializeStateAsync(_initialState);
        FlowLiteGlobal<TState, TTrigger, TKey, TEntity>.OnMachineCreated?.Invoke(this);
    }

    public IFluentTransitionBuilder<TState, TTrigger, TKey, TEntity> AddTransition(
        TState fromState,
        TTrigger trigger,
        TState toState,
        Func<Func<TState, TTrigger?, ValueTask>, ITransitionContext<TEntity?>, ValueTask> onTransition)
    {
        ThrowIfUsedConfiguration();
        var key = new StateTriggerKey<TState, TTrigger>(fromState, trigger);
        if (!_buildConfiguration)
        {
            if (TransitionValidator.IdentifyCycleStates(fromState, toState, _transitions))
                throw new InvalidOperationException(string.Format(ErrorTemplates.CycleDetectedTemplate, fromState,
                    toState));

            var transition = new StateTransition<TState, TTrigger, TEntity>(toState, onTransition);
            if (!_transitions.TryAdd(key, transition))
                throw new InvalidOperationException(string.Format(
                    ErrorTemplates.DuplicateTransitionTemplate,
                    fromState,
                    trigger,
                    toState));
            if (transition.IsFinal)
                _finalStates.Add(transition.ToState);
        }
        else
            _transitions.TryAdd(key,
                new StateTransition<TState, TTrigger, TEntity>(toState, onTransition));

        return new FluentTransitionBuilder<TState, TTrigger, TKey, TEntity>(this, toState);
    }

    public IStateFlowMachine<TState, TTrigger, TKey, TEntity> ConfigureTransitions(
        IFlowTransitionBuilder<TState, TTrigger, TEntity> config)
    {
        _buildConfiguration = true;
        if (_transitions.Keys.Count > 0)
        {
            _usedConfiguration = true;
            ThrowIfUsedConfiguration();
        }

        if (config.Transitions is null || config.Transitions.Count == 0)
            throw new InvalidOperationException(ErrorTemplates.NoTransitionsDefinedTemplate);

        foreach (var transitionConfig in config.Transitions)
        {
            AddTransition(transitionConfig.FromState, transitionConfig.Trigger, transitionConfig.ToState,
                transitionConfig.OnTransition!);
            if (transitionConfig.IsFinal)
                _finalStates.Add(transitionConfig.ToState);
        }

        _usedConfiguration = true;        
        return this;
    }

    public async ValueTask<bool> FireAsync(TTrigger trigger)
    {
        using (await _lock.LockAsync())
        {
            try
            {
                return await FireInternalAsync(trigger);
            }
            catch (Exception ex)
            {
                OnTransitionFailed?.Invoke(_currentState, trigger, ex);
                _logger.Write(LogLevel.Error, ErrorTemplates.ExceptionTransitionTemplate, ex.Message);
                return false;
            }
        }
    }

    private async ValueTask<bool> FireInternalAsync(TTrigger trigger)
    {
        ThrowIfDisposed();
        if (_stateStorage is not null)
        {
            _currentState = await _stateStorage.LoadStateAsync(_entityKey);
            _entity = await _stateStorage.LoadEntityAsync(_entityKey) ?? _entity;
        }
        if (IsFinalStateOf(_currentState))
        {
            _logger.Write(LogLevel.Info, LogTemplates.FinalStateReachedTemplate, _currentState);
            return false;
        }
        
        var key = new StateTriggerKey<TState, TTrigger>(_currentState, trigger);
        if (_transitions.TryGetValue(key, out var transition))
            return await HandleRegularTransitionAsync(transition, trigger);
        
        _logger.Write(LogLevel.Warning, LogTemplates.InvalidTransitionTemplate, _currentState, trigger);
        return false;
    }

    public async ValueTask<Result<bool>> TryFireAsync(TTrigger trigger)
    {
        try
        {
            var success = await FireAsync(trigger);
            return success ? Result<bool>.Success(true) : Result<bool>.Failure("Invalid transition.");
        }
        catch (Exception ex)
        {
            OnTransitionFailed?.Invoke(_currentState, trigger, ex);
            _logger.Write(LogLevel.Error, LogTemplates.InvalidTriggerTemplate, trigger, ex.Message);
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }

    public string Export(ExportType type)
    {
        var exporter = new StateFlowExporter<TState, TTrigger>(GetRawTransitions());
        return type switch
        {
            ExportType.Mermaid => exporter.ExportAsMermaid(),
            ExportType.Dot => exporter.ExportAsDot(),
            _ => throw new NotSupportedException(string.Format(ErrorTemplates.UnknownExportTypeTemplate, type))
        };
    }

    private Dictionary<StateTriggerKey<TState, TTrigger>, StateTransition<TState, TTrigger, object>> GetRawTransitions()
    {
        if (_transitions.Count > 0)
            return _transitions.ToDictionary(
                kvp => kvp.Key,
                kvp => new StateTransition<TState, TTrigger, object>(
                    kvp.Value.ToState,
                    async (_, _) => await Task.CompletedTask
                )
            );
        return new Dictionary<StateTriggerKey<TState, TTrigger>, StateTransition<TState, TTrigger, object>>();
    }

    private async ValueTask<bool> HandleRegularTransitionAsync(StateTransition<TState, TTrigger, TEntity> transition,
        TTrigger trigger)
    {
        var stateChanged = false;
        await transition.OnTransition(async (newState, newTrigger) =>
        {
            if (IsFinalStateOf(_currentState))
            {
                _logger.Write(LogLevel.Info, LogTemplates.FinalStateReachedTemplate, _currentState);
                return;
            }
            if (newTrigger is not null)
            {
                await UpdateStateAsync(newState, newTrigger);
                stateChanged = await FireInternalAsync(newTrigger.Value);
            }
            else
                stateChanged = await MoveToStateAsync(newState, trigger);
        }, new TransitionContext<TEntity>(
            _entity!,
            () => _markedForDeletion.Add(_entityKey)
        ));
        return stateChanged || await MoveToStateAsync(transition.ToState, trigger);
    }

    private async ValueTask<bool> MoveToStateAsync(TState newState, TTrigger? trigger)
    {
        if (EqualityComparer<TState>.Default.Equals(_currentState, newState))
            return false;

        await UpdateStateAsync(newState, trigger);

        _isFinal = IsFinalStateOf(newState);
        if (IsFinalState)
            _logger.Write(LogLevel.Info, LogTemplates.FinalStateReachedTemplate, newState);

        return true;
    }
    
    private bool IsFinalStateOf(TState state) => _finalStates.Contains(state);

    private async Task InitializeStateAsync(TState defaultState)
    {
        if (_stateStorage is not null)
        {
            var isExist = await _stateStorage.ExistEntryByIdAsync(_entityKey);
            if (!isExist)
                await _stateStorage.SaveStateAsync(_entityKey, _initialState, _entity);
            _currentState = await _stateStorage.LoadStateAsync(_entityKey);
            _entity = await _stateStorage.LoadEntityAsync(_entityKey) ?? _entity;
        }
        else
        {
            _currentState = defaultState;
        }
        _stateHistory.Enqueue((null, _currentState));
        _logger.Write(LogLevel.Info, LogTemplates.InitializedStateTemplate, _currentState);
    }

    private async Task UpdateStateAsync(TState state, TTrigger? trigger)
    {
        _currentState = state;
        var isDeleted = false;

        if (_stateStorage is not null)
        {
            if (_markedForDeletion.Contains(_entityKey))
            {
                await _stateStorage.DeleteEntryAsync(_entityKey);
                _entity = null;
                isDeleted = true;
            }
            else
                await _stateStorage.SaveStateAsync(_entityKey, _currentState, _entity);

            _currentState = await _stateStorage.LoadStateAsync(_entityKey);
            _entity = await _stateStorage.LoadEntityAsync(_entityKey);
        } else if (_markedForDeletion.Contains(_entityKey))
        {
            _entity = null;
            isDeleted = true;
        }

        if (_stateHistory.Count >= MaxHistorySize)
            _stateHistory.TryDequeue(out _);
        _stateHistory.Enqueue((trigger, _currentState));

        _logger.Write(LogLevel.Info, LogTemplates.ChangedStateTemplate, state);

        if (!isDeleted)
        {
            OnStateChanged?.Invoke(_currentState, trigger!.Value);
            OnEntityChanged?.Invoke(_entity!);
        }
        else
            OnEntityDeleted?.Invoke(_entityKey);

        if (_finalStates.Contains(_currentState))
            _logger.Write(LogLevel.Info, LogTemplates.FinalStateReachedTemplate, _currentState);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(StateFlowMachine<TState, TTrigger, TKey, TEntity>));
    }

    private void ThrowIfUsedConfiguration()
    {
        if (_usedConfiguration)
            throw new InvalidOperationException(ErrorTemplates.InvalidTransitionConfigurationTemplate);
    }
    
    internal void MarkFinalState(TState state)
    {
        _finalStates.Add(state);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposing) return;
        _disposed = true;
        _transitions.Clear();
        _stateHistory.Clear();
        _finalStates.Clear();
        _markedForDeletion.Clear();
        OnStateChanged = null;
        OnEntityChanged = null;
    }
}