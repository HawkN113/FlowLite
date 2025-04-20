namespace FlowLite.Core.Abstractions.Fsm;

/// <summary>
/// Global registry to hook into FSM creation and apply optional behaviors (e.g., diagnostics).
/// </summary>
public static class FlowLiteGlobal<TState, TTrigger, TKey, TEntity> 
    where TState : struct
    where TTrigger : struct
    where TEntity : class
    where TKey : notnull
{
    /// <summary>
    /// Called every time a new state machine is created. Can be used to hook global behaviors (e.g., diagnostics).
    /// </summary>
    public static Action<IStateFlowMachine<TState, TTrigger, TKey, TEntity>>? OnMachineCreated { get; set; }
}