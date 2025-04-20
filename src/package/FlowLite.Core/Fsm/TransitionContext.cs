using FlowLite.Core.Abstractions.Fsm;
namespace FlowLite.Core.Fsm;

internal sealed class TransitionContext<TEntity>(TEntity? entity, Action markForDeletion) : ITransitionContext<TEntity>
{
    public TEntity? Entity { get; } = entity;
    public void MarkForDeletion() => markForDeletion();
}