using FlowLite.Core.Abstractions.Storage;
using FlowLite.Core.Configuration;
using FlowLite.Core.Fsm;
using Moq;
namespace FlowLite.Core.Tests.Core;

public enum OrderState { Pending, Processing, Shipped, Completed, Canceled }
public enum OrderTrigger { Approve, Ship, Deliver, Cancel }

public class Order
{
    public int Id { get; set; }
    public string Status { get; set; } = "New";
}

public class StateFlowMachineTests
{
    private readonly Mock<IEntityStateStorage<OrderState, int, Order>> _mockStorage = new();

    [Fact]
    public void Constructor_WithStorage_InitializesCorrectly()
    {
        // Arrange & Act
        var order = new Order { Id = 1, Status = "New" };
        var machine = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            OrderState.Pending, null, order.Id, order
        );

        // Assert
        Assert.Equal(OrderState.Pending, machine.CurrentState);
        Assert.Equal(order, machine.CurrentEntity);
    }

    [Fact]
    public void Constructor_WithoutStorage_InitializesCorrectly()
    {
        // Arrange & Act
        var order = new Order { Id = 1, Status = "New" };
        var machine = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            OrderState.Pending, order.Id, order
        );

        // Assert
        Assert.Equal(OrderState.Pending, machine.CurrentState);
        Assert.Equal(order, machine.CurrentEntity);
    }

    [Fact]
    public async Task FireAsync_ValidTransition_ChangesState()
    {
        var order = new Order { Id = 2, Status = "New" };
        var machine = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            OrderState.Pending, null, order.Id, order
        );

        machine.AddTransition(OrderState.Pending, OrderTrigger.Approve, OrderState.Processing,
            async (_, ctx) =>
            {
                ctx.Entity!.Status = "Approved";
                await Task.CompletedTask;
            });

        var result = await machine.FireAsync(OrderTrigger.Approve);

        Assert.True(result);
        Assert.Equal(OrderState.Processing, machine.CurrentState);
        Assert.Equal("Approved", machine.CurrentEntity!.Status);
    }

    [Fact]
    public async Task FireAsync_InvalidTransition_DoesNotChangeState()
    {
        var order = new Order { Id = 3, Status = "New" };
        var machine = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            OrderState.Pending, null, order.Id, order
        );

        var result = await machine.FireAsync(OrderTrigger.Ship);

        Assert.False(result);
        Assert.Equal(OrderState.Pending, machine.CurrentState);
    }

    [Fact]
    public async Task FireAsync_FinalState_PreventsFurtherTransitions()
    {
        var order = new Order { Id = 4, Status = "New" };
        var machine = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            OrderState.Completed, null, order.Id, order
        );

        var result = await machine.FireAsync(OrderTrigger.Cancel);

        Assert.False(result);
        Assert.Equal(OrderState.Completed, machine.CurrentState);
    }

    [Fact]
    public void AddTransition_Duplicate_ThrowsException()
    {
        var order = new Order { Id = 5, Status = "New" };
        var machine = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            OrderState.Pending, null, order.Id, order
        );

        machine.AddTransition(OrderState.Pending, OrderTrigger.Approve, OrderState.Processing,
            async (_, ctx) =>
            {
                ctx.Entity!.Status = "Approved";
                await Task.CompletedTask;
            });

        Assert.Throws<InvalidOperationException>(() =>
        {
            machine.AddTransition(OrderState.Pending, OrderTrigger.Approve, OrderState.Processing,
                async (_, ctx) =>
                {
                    ctx.Entity!.Status = "Approved";
                    await Task.CompletedTask;
                });
        });
    }

    [Fact]
    public async Task FireAsync_UsesStateStorage()
    {
        var order = new Order { Id = 6, Status = "New" };

        _mockStorage.Setup(s => s.LoadStateAsync(order.Id))
            .ReturnsAsync(OrderState.Pending);
        _mockStorage.Setup(s => s.LoadEntityAsync(order.Id))
            .ReturnsAsync(order);
        _mockStorage.Setup(s => s.SaveStateAsync(order.Id, OrderState.Processing, order))
            .Returns(Task.CompletedTask);

        var machine = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            OrderState.Pending, _mockStorage.Object, order.Id, order
        );

        machine.AddTransition(OrderState.Pending, OrderTrigger.Approve, OrderState.Processing,
            async (_, ctx) =>
            {
                ctx.Entity!.Status = "Approved";
                await Task.CompletedTask;
            });

        var result = await machine.FireAsync(OrderTrigger.Approve);

        Assert.True(result);
        _mockStorage.Verify(s => s.SaveStateAsync(order.Id, OrderState.Processing, order), Times.Once);
    }

    [Fact]
    public async Task FireAsync_StoresHistory()
    {
        var order = new Order { Id = 7, Status = "New" };
        var machine = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            OrderState.Pending, null, order.Id, order
        );

        machine.AddTransition(OrderState.Pending, OrderTrigger.Approve, OrderState.Processing,
            async (_, _) => { await Task.CompletedTask; });
        machine.AddTransition(OrderState.Processing, OrderTrigger.Ship, OrderState.Shipped,
            async (_, _) => { await Task.CompletedTask; });
        machine.AddTransition(OrderState.Shipped, OrderTrigger.Deliver, OrderState.Completed,
            async (_, _) => { await Task.CompletedTask; });

        await machine.FireAsync(OrderTrigger.Approve);
        await machine.FireAsync(OrderTrigger.Ship);
        await machine.FireAsync(OrderTrigger.Deliver);

        Assert.Equal(4, machine.GetTransitionHistory().Count);
        Assert.Contains(machine.GetTransitionHistory(), h => h.State == OrderState.Completed);
    }

    [Fact]
    public void OnStateChanged_IsTriggeredOnStateChange()
    {
        var order = new Order { Id = 9, Status = "New" };
        var machine = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            OrderState.Pending, null, order.Id, order
        );

        var eventTriggered = false;
        machine.OnStateChanged += (_,_) => { eventTriggered = true; };

        machine.AddTransition(OrderState.Pending, OrderTrigger.Approve, OrderState.Processing, async (_, ctx) =>
        {
            ctx.Entity!.Status = "Approved";
            await Task.CompletedTask;
        });
        machine.FireAsync(OrderTrigger.Approve).GetAwaiter();

        Assert.True(eventTriggered);
    }

    [Fact]
    public void OnEntityChanged_IsTriggeredOnEntityChange()
    {
        var order = new Order { Id = 10, Status = "New" };
        var machine = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            OrderState.Pending, null, order.Id, order
        );

        var eventTriggered = false;
        machine.OnEntityChanged += _ => { eventTriggered = true; };

        machine.AddTransition(OrderState.Pending, OrderTrigger.Approve, OrderState.Processing,
            async (_, ctx) =>
            {
                ctx.Entity!.Status = "Approved";
                await Task.CompletedTask;
            });
        machine.FireAsync(OrderTrigger.Approve).GetAwaiter();

        Assert.True(eventTriggered);
    }

    [Fact]
    public void GetLogs_ReturnsTransitionHistory()
    {
        var order = new Order { Id = 1, Status = "New" };
        var machine = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            OrderState.Pending, null, order.Id, order
        );

        machine.AddTransition(OrderState.Pending, OrderTrigger.Approve, OrderState.Processing,
            async (_, _) => await Task.CompletedTask);
        machine.AddTransition(OrderState.Processing, OrderTrigger.Ship, OrderState.Shipped,
            async (_, _) => await Task.CompletedTask);
        machine.AddTransition(OrderState.Shipped, OrderTrigger.Deliver, OrderState.Completed,
            async (_, _) => await Task.CompletedTask);

        machine.FireAsync(OrderTrigger.Approve).GetAwaiter();
        machine.FireAsync(OrderTrigger.Ship).GetAwaiter();
        machine.FireAsync(OrderTrigger.Deliver).GetAwaiter();

        var logs = machine.GetLogs().ToList();

        Assert.Equal(4, logs.Count);
    }


    [Fact]
    public async Task ConfigureTransitions_UsingTransitionBuilder_AddsMultipleTransitions()
    {
        var order = new Order { Id = 3, Status = "New" };
        var machine = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            OrderState.Pending, null, order.Id, order
        );

        var builder = new FlowTransitionBuilder<OrderState, OrderTrigger, Order>()
            .AddTransition(OrderState.Pending, OrderTrigger.Approve, OrderState.Processing,
                async (_, _) => await Task.CompletedTask)
            .AddTransition(OrderState.Processing, OrderTrigger.Ship, OrderState.Completed,
                async (_, _) => await Task.CompletedTask)
            .Build();

        machine.ConfigureTransitions(builder);

        Assert.True(await machine.FireAsync(OrderTrigger.Approve));
        Assert.Equal(OrderState.Processing, machine.CurrentState);

        Assert.True(await machine.FireAsync(OrderTrigger.Ship));
        Assert.Equal(OrderState.Completed, machine.CurrentState);
    }

    [Fact]
    public async Task TryFireAsync_ValidTransition_ReturnsTrueAndChangesState()
    {
        var order = new Order { Id = 4, Status = "New" };
        var machine = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            OrderState.Pending, null, order.Id, order
        );

        machine.AddTransition(OrderState.Pending, OrderTrigger.Approve, OrderState.Processing,
            async (_, _) => await Task.CompletedTask);

        var result = await machine.TryFireAsync(OrderTrigger.Approve);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderState.Processing, machine.CurrentState);
    }

    [Fact]
    public async Task TryFireAsync_InvalidTransition_ReturnsFalseAndDoesNotChangeState()
    {
        var order = new Order { Id = 5, Status = "New" };
        var machine = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            OrderState.Pending, null, order.Id, order
        );

        var result = await machine.TryFireAsync(OrderTrigger.Ship);

        Assert.False(result.IsSuccess);
        Assert.Equal(OrderState.Pending, machine.CurrentState);
    }

    [Fact]
    public void Dispose_ShouldSetDisposedFlagTrue()
    {
        // Arrange
        var stateMachine = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            initialState: 0,
            entityKey: 1,
            entity: new Order()
        );

        // Act
        stateMachine.Dispose();

        // Assert
        var field = typeof(StateFlowMachine<OrderState, OrderTrigger, int, Order>)
            .GetField("_disposed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.True((bool)field!.GetValue(stateMachine)!);
    }

    [Fact]
    public void Dispose_ShouldSetEventsToNull()
    {
        // Arrange
        var stateMachine = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            initialState: 0,
            entityKey: 1,
            entity: new Order()
        );

        // Act
        stateMachine.Dispose();

        // Assert
        var onStateChangedField = typeof(StateFlowMachine<OrderState, OrderTrigger, int, Order>)
            .GetField("OnStateChanged",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var onEntityChangedField = typeof(StateFlowMachine<OrderState, OrderTrigger, int, Order>)
            .GetField("OnEntityChanged",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.Null(onStateChangedField!.GetValue(stateMachine));
        Assert.Null(onEntityChangedField!.GetValue(stateMachine));
    }

}