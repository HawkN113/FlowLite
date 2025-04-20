using FlowLite.Core.Fsm;
using FlowLite.Core.Validators;
namespace FlowLite.Core.Tests.Validators;

public class TransitionValidatorTests
{
    private enum State
    {
        A,
        B,
        C
    }

    private enum Trigger
    {
        X,
        Y
    }

    private class Entity
    {
        public string Name { get; set; } = "Entity";
    }

    [Fact]
    public void IdentifyCycleStates_ShouldReturnTrue_WhenCycleExists()
    {
        // Arrange
        var transitions = new Dictionary<StateTriggerKey<State, Trigger>, StateTransition<State, Trigger, Entity>>();
        transitions.TryAdd(new StateTriggerKey<State, Trigger>(State.A, Trigger.X),
            new StateTransition<State, Trigger, Entity>(State.B, async (_, _) => { await Task.CompletedTask; }));
        transitions.TryAdd(new StateTriggerKey<State, Trigger>(State.B, Trigger.Y),
            new StateTransition<State, Trigger, Entity>(State.C, async (_, _) => { await Task.CompletedTask; }));
        transitions.TryAdd(new StateTriggerKey<State, Trigger>(State.C, Trigger.Y),
            new StateTransition<State, Trigger, Entity>(State.A, async (_, _) => { await Task.CompletedTask; }));
        
        // Act
        var result = TransitionValidator.IdentifyCycleStates(State.A, State.A, transitions);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IdentifyCycleStates_ShouldReturnFalse_WhenNoCycleExists()
    {
        // Arrange
        var transitions = new Dictionary<StateTriggerKey<State, Trigger>, StateTransition<State, Trigger, Entity>>();
        transitions.TryAdd(new StateTriggerKey<State, Trigger>(State.A, Trigger.X),
            new StateTransition<State, Trigger, Entity>(State.B, async (_, _) => { await Task.CompletedTask; }));
        transitions.TryAdd(new StateTriggerKey<State, Trigger>(State.B, Trigger.Y),
            new StateTransition<State, Trigger, Entity>(State.C, async (_, _) => { await Task.CompletedTask; }));

        // Act
        var result = TransitionValidator.IdentifyCycleStates(State.A, State.C, transitions);
        
        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IdentifyCycleStates_ShouldReturnTrue_WhenSelfTransitionExists()
    {
        // Arrange
        var transitions = new Dictionary<StateTriggerKey<State, Trigger>, StateTransition<State, Trigger, Entity>>();
        transitions.TryAdd(new StateTriggerKey<State, Trigger>(State.A, Trigger.X),
            new StateTransition<State, Trigger, Entity>(State.A, async (_, _) => { await Task.CompletedTask; }));

        // Act
        var result = TransitionValidator.IdentifyCycleStates(State.A, transitions);
        
        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IdentifyCycleStates_ShouldReturnFalse_WhenNoTransitionsExist()
    {
        // Arrange
        var transitions = new Dictionary<StateTriggerKey<State, Trigger>, StateTransition<State, Trigger, Entity>>();
        
        // Act
        var result = TransitionValidator.IdentifyCycleStates(State.A, transitions);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IdentifyCycleStates_ShouldReturnFalse_WhenNoPathToTarget()
    {
        // Arrange
        var transitions = new Dictionary<StateTriggerKey<State, Trigger>, StateTransition<State, Trigger, Entity>>();

        transitions.TryAdd(new StateTriggerKey<State, Trigger>(State.A, Trigger.X),
            new StateTransition<State, Trigger, Entity>(State.B, async (_, _) => { await Task.CompletedTask; }));

        // Act
        var result = TransitionValidator.IdentifyCycleStates(State.C, State.A, transitions);
        
        // Assert
        Assert.False(result);
    }
}