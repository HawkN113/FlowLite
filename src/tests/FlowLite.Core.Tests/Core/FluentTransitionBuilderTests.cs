using FlowLite.Core.Fsm;
namespace FlowLite.Core.Tests.Core;

public class FluentTransitionBuilderTests
{
    private enum OrderState { Pending, Created, Completed }
    private enum OrderTrigger { Create, Complete }

    private class Order
    {
        public int Id { get; set; }
    }

    [Fact]
    public async Task AsFinal_ShouldMarkStateAsFinal()
    {
        // Arrange
        var order = new Order { Id = 1 };
        var fsm = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            OrderState.Pending, 1, order);

        // Act
        fsm.AddTransition(OrderState.Pending, OrderTrigger.Create, OrderState.Created, async (_, _) => await Task.CompletedTask)
            .AsFinal();
        
        await fsm.FireAsync(OrderTrigger.Create);
        var result = await fsm.FireAsync(OrderTrigger.Complete);

        // Assert
        Assert.False(result); // Should not proceed from final state
        Assert.Equal(OrderState.Created, fsm.CurrentState);
    }

    [Fact]
    public async Task AddTransition_ShouldAddNextTransition()
    {
        // Arrange
        var order = new Order { Id = 42 };
        var fsm = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            OrderState.Pending, 42, order);

        // Act
        fsm.AddTransition(OrderState.Pending, OrderTrigger.Create, OrderState.Created, async (_, _) => await Task.CompletedTask)
            .AddTransition(OrderState.Created, OrderTrigger.Complete, OrderState.Completed, async (_, _) => await Task.CompletedTask);

        // Assert
        var createResult = await fsm.FireAsync(OrderTrigger.Create);
        Assert.True(createResult);
        Assert.Equal(OrderState.Created, fsm.CurrentState);

        var completeResult = await fsm.FireAsync(OrderTrigger.Complete);
        Assert.True(completeResult);
        Assert.Equal(OrderState.Completed, fsm.CurrentState);
    }
}