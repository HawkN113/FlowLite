using FlowLite.Core.Abstractions.Fsm;
using FlowLite.Diagnostics.Common;
using FlowLite.Diagnostics.Listeners;
using FlowLite.Diagnostics.Observability;
using FlowLite.Diagnostics.Options;
using FlowLite.Diagnostics.Telemetry;
using Microsoft.Extensions.DependencyInjection;
namespace FlowLite.Diagnostics.Extensions;

public static class DiagnosticsExtensions
{
    /// <summary>
    /// Adds and configures FlowLite diagnostics support globally using dependency injection.
    /// </summary>
    /// <typeparam name="TState">The state enum type.</typeparam>
    /// <typeparam name="TTrigger">The trigger enum type.</typeparam>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The entity class type.</typeparam>
    /// <param name="services">The IServiceCollection instance.</param>
    /// <param name="configure">Optional configuration delegate for diagnostics options.</param>
    /// <returns>The IServiceCollection for chaining.</returns>
    public static IServiceCollection AddFlowLiteDiagnostics<TState, TTrigger, TKey, TEntity>(
        this IServiceCollection services,
        Action<DiagnosticsOptions>? configure = null)
        where TState : struct
        where TTrigger : struct
        where TEntity : class
        where TKey : notnull
    {
        var options = new DiagnosticsOptions();
        configure?.Invoke(options);

        if (options.Logging.Enabled)
        {
            if (options.Logging.UseConsole)
                DiagnosticsFlowLiteRegistry.Register(new ConsoleTelemetryFlowLiteListener());
            if (options.Logging is { UseLogger: true, LoggerFactory: not null })
                DiagnosticsFlowLiteRegistry.Register(
                    new LoggerDiagnosticsFlowLiteListener(options.Logging.LoggerFactory));
        }

        if (options.Telemetry.Enabled)
        {
            DiagnosticsFlowLiteRegistry.Register(
                new TelemetryDiagnosticsFlowLiteListener(options.Telemetry.Source));
        }

        if (options.DiagnosticObserver.Enabled)
            DiagnosticsFlowLiteRegistry.Register(
                new DiagnosticObserverFlowLiteListener(options.DiagnosticObserver.Source));

        if (options.EnableGlobalDiagnostics)
        {
            FlowLiteGlobal<TState, TTrigger, TKey, TEntity>.OnMachineCreated = static fsm =>
            {
                var type = fsm.GetType();
                if (!type.IsGenericType) return;

                var args = type.GetGenericArguments();
                if (args.Length != 4) return;

                var method = typeof(DiagnosticsFlowLiteRegistry)
                    .GetMethod(nameof(DiagnosticsFlowLiteRegistry.ApplyAll))?
                    .MakeGenericMethod(args);

                method?.Invoke(null, [fsm]);
            };
        }

        return services;
    }

    /// <summary>
    /// Enables FSM logging to console and/or Microsoft.Extensions.Logging.
    /// </summary>
    /// <typeparam name="TState">FSM state enum.</typeparam>
    /// <typeparam name="TTrigger">FSM trigger enum.</typeparam>
    /// <typeparam name="TKey">FSM entity key type.</typeparam>
    /// <typeparam name="TEntity">FSM entity type.</typeparam>
    /// <param name="fsm">The FSM instance.</param>
    /// <param name="options">Logging configuration options.</param>
    public static void UseLogging<TState, TTrigger, TKey, TEntity>(
        this IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm,
        LoggingOptions options)
        where TState : struct
        where TTrigger : struct
        where TEntity : class
        where TKey : notnull
    {
        if (!options.Enabled) return;
        if (options.UseConsole)
            StateHandler.RegisterEvents(fsm);

        if (options is not { UseLogger: true, LoggerFactory: not null }) return;

        var logger =
            options.LoggerFactory!.CreateLogger(
                $"FlowLite.FSM.{typeof(TState).Name}.{typeof(TTrigger).Name}");
        StateHandler.RegisterLoggingEvents(fsm, logger);
    }

    /// <summary>
    /// Enables simple console-based FSM logging with no configuration.
    /// </summary>
    /// <typeparam name="TState">FSM state enum.</typeparam>
    /// <typeparam name="TTrigger">FSM trigger enum.</typeparam>
    /// <typeparam name="TKey">FSM entity key type.</typeparam>
    /// <typeparam name="TEntity">FSM entity type.</typeparam>
    /// <param name="fsm">The FSM instance.</param>
    public static void UseConsoleLogging<TState, TTrigger, TKey, TEntity>(
        this IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm)
        where TState : struct
        where TTrigger : struct
        where TEntity : class
        where TKey : notnull
    {
        var options = new LoggingOptions()
        {
            Enabled = true,
            UseConsole = true
        };
        fsm.UseLogging(options);
    }

    /// <summary>
    /// Enables OpenTelemetry-compatible metrics emission using System.Diagnostics.Metrics.
    /// </summary>
    /// <typeparam name="TState">FSM state enum.</typeparam>
    /// <typeparam name="TTrigger">FSM trigger enum.</typeparam>
    /// <typeparam name="TKey">FSM entity key type.</typeparam>
    /// <typeparam name="TEntity">FSM entity type.</typeparam>
    /// <param name="fsm">The FSM instance.</param>
    /// <param name="options">Telemetry configuration options.</param>
    public static void UseTelemetry<TState, TTrigger, TKey, TEntity>(
        this IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm,
        TelemetryOptions options)
        where TState : struct
        where TTrigger : struct
        where TEntity : class
        where TKey : notnull
    {
        if (!options.Enabled) return;
        var emitter = new StateTelemetryEmitter(options.Source);
        fsm.OnStateChanged += (state, trigger) =>
            emitter.RecordTransition(state.ToString() ?? "null", trigger.ToString() ?? "null");
        fsm.OnEntityDeleted += key => emitter.RecordEntityDeleted();
    }

    /// <summary>
    /// Enables FSM telemetry using default options.
    /// </summary>
    /// <typeparam name="TState">FSM state enum.</typeparam>
    /// <typeparam name="TTrigger">FSM trigger enum.</typeparam>
    /// <typeparam name="TKey">FSM entity key type.</typeparam>
    /// <typeparam name="TEntity">FSM entity type.</typeparam>
    /// <param name="fsm">The FSM instance.</param>
    public static void UseTelemetry<TState, TTrigger, TKey, TEntity>(
        this IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm)
        where TState : struct
        where TTrigger : struct
        where TEntity : class
        where TKey : notnull
    {
        var options = new TelemetryOptions()
        {
            Enabled = true
        };
        fsm.UseTelemetry(options);
    }

    /// <summary>
    /// Attaches a DiagnosticObserver to publish FSM events via DiagnosticListener.
    /// </summary>
    /// <typeparam name="TState">FSM state enum.</typeparam>
    /// <typeparam name="TTrigger">FSM trigger enum.</typeparam>
    /// <typeparam name="TKey">FSM entity key type.</typeparam>
    /// <typeparam name="TEntity">FSM entity type.</typeparam>
    /// <param name="fsm">The FSM instance.</param>
    /// <param name="options">Diagnostic observer options.</param>
    public static void UseDiagnosticObserver<TState, TTrigger, TKey, TEntity>(
        this IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm, DiagnosticObserverOptions options)
        where TState : struct
        where TTrigger : struct
        where TEntity : class
        where TKey : notnull
    {
        if (!options.Enabled) return;
        var observer = new DiagnosticsFlowLiteObserver<TState, TTrigger, TKey, TEntity>(options.Source);
        observer.Attach(fsm);
    }

    /// <summary>
    /// Attaches a DiagnosticObserver using default source for FSM DiagnosticListener events.
    /// </summary>
    /// <typeparam name="TState">FSM state enum.</typeparam>
    /// <typeparam name="TTrigger">FSM trigger enum.</typeparam>
    /// <typeparam name="TKey">FSM entity key type.</typeparam>
    /// <typeparam name="TEntity">FSM entity type.</typeparam>
    /// <param name="fsm">The FSM instance.</param>
    public static void UseDiagnosticObserver<TState, TTrigger, TKey, TEntity>(
        this IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm)
        where TState : struct
        where TTrigger : struct
        where TEntity : class
        where TKey : notnull
    {
        var options = new DiagnosticObserverOptions()
        {
            Enabled = true
        };
        fsm.UseDiagnosticObserver(options);
    }
}