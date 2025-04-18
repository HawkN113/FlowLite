using FlowLite.Core.Abstractions.Fsm;
using FlowLite.Diagnostics.Abstractions;
using FlowLite.Diagnostics.Common;
using Microsoft.Extensions.Logging;
namespace FlowLite.Diagnostics.Listeners;

internal sealed class LoggerDiagnosticsFlowLiteListener(ILoggerFactory loggerFactory) : IDiagnosticsFlowLiteListener
{
    public void OnAttached<TState, TTrigger, TKey, TEntity>(
        IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm)
        where TState : struct
        where TTrigger : struct
        where TEntity : class
        where TKey : notnull
    {
        var logger =
            loggerFactory.CreateLogger(
                $"FlowLite.FSM.{typeof(TState).Name}.{typeof(TTrigger).Name}");
        StateHandler.RegisterLoggingEvents(fsm, logger);
    }
}