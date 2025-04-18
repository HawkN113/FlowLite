using Bogus;
using FlowLite.Core.Fsm;
using FlowLite.Testing.Extensions;
using Moq;
using OrderGateway.Api.Abstractions;
using OrderGateway.Api.Abstractions.Models;
using Orders.Processor.Flows.Models;
namespace Orders.Processor.Tests;

public class FlowTransitions
{
    private readonly Mock<IOrderApiClient> _orderApiClient = new();

    [Theory]
    [InlineData(OrderState.Pending, OrderTrigger.Create, OrderState.Created)]
    [InlineData(OrderState.Created, OrderTrigger.Ship, OrderState.Shipped)]
    [InlineData(OrderState.Shipped, OrderTrigger.Complete, OrderState.Completed)]
    public void Order_Should_Transition_Through_Valid_States(OrderState initialState, OrderTrigger trigger, OrderState expectedState)
    {
        // Arrange
        var config = Flows.FlowTransitions.GetOrderConfig(_orderApiClient.Object);
        var orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, f => f.IndexFaker + 1)
            .RuleFor(o => o.FirstName, f => f.Name.FirstName())
            .RuleFor(o => o.LastName, f => f.Name.LastName())
            .RuleFor(o => o.AddressLine1, f => f.Address.StreetAddress())
            .RuleFor(o => o.City, f => f.Address.City())
            .RuleFor(o => o.Postalcode, f => f.Address.ZipCode())
            .RuleFor(o => o.Country, f => f.Address.Country())
            .RuleFor(o => o.TotalAmount, f => f.Finance.Amount(10, 500))
            .RuleFor(o => o.Status, f => initialState.ToString())
            .RuleFor(o => o.TransitionHistory, (f, o) => $"[*] -> {o.Status}");
        var order = orderFaker.Generate(1)[0];

        var fsm = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            initialState: Enum.Parse<OrderState>(order.Status),
            entityKey: order.Id,
            entity: order
        ).ConfigureTransitions(config);

        // Act
        fsm.FireAsync(trigger);

        // Assert
        fsm.Should()
            .BeIn(expectedState)
            .HaveEntity(e => Assert.Equal(expectedState.ToString(), e?.Status))
            .NotAllow(trigger)
            .History().HaveInHistory(initialState, expectedState);
        fsm.Assert().NotAllow(trigger);
    }
    
    [Theory]
    [InlineData(OrderState.Pending, OrderTrigger.Create, OrderState.Created, OrderTrigger.Ship)]
    [InlineData(OrderState.Created, OrderTrigger.Ship, OrderState.Shipped, OrderTrigger.Complete)]
    public void Order_Should_Transition_Through_Allowed_Trigger(OrderState initialState, OrderTrigger trigger, OrderState expectedState, OrderTrigger allowedTrigger)
    {
        // Arrange
        var config = Flows.FlowTransitions.GetOrderConfig(_orderApiClient.Object);
        var orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, f => f.IndexFaker + 1)
            .RuleFor(o => o.FirstName, f => f.Name.FirstName())
            .RuleFor(o => o.LastName, f => f.Name.LastName())
            .RuleFor(o => o.AddressLine1, f => f.Address.StreetAddress())
            .RuleFor(o => o.City, f => f.Address.City())
            .RuleFor(o => o.Postalcode, f => f.Address.ZipCode())
            .RuleFor(o => o.Country, f => f.Address.Country())
            .RuleFor(o => o.TotalAmount, f => f.Finance.Amount(10, 500))
            .RuleFor(o => o.Status, f => initialState.ToString())
            .RuleFor(o => o.TransitionHistory, (f, o) => $"[*] -> {o.Status}");
        var order = orderFaker.Generate(1)[0];

        var fsm = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            initialState: Enum.Parse<OrderState>(order.Status),
            entityKey: order.Id,
            entity: order
        ).ConfigureTransitions(config);

        // Act
        fsm.FireAsync(trigger);

        // Assert
        fsm.Should()
            .BeIn(expectedState);
        fsm.Assert().Allow(allowedTrigger);
    }
    
    [Theory]
    [InlineData(OrderState.Shipped, OrderTrigger.Complete, OrderState.Completed)]
    [InlineData(OrderState.Failed, OrderTrigger.Delete, OrderState.Deleted)]
    public void Order_Should_Transition_Through_Final_States(OrderState initialState, OrderTrigger trigger, OrderState expectedState)
    {
        // Arrange
        var config = Flows.FlowTransitions.GetOrderConfig(_orderApiClient.Object);
        var orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, f => f.IndexFaker + 1)
            .RuleFor(o => o.FirstName, f => f.Name.FirstName())
            .RuleFor(o => o.LastName, f => f.Name.LastName())
            .RuleFor(o => o.AddressLine1, f => f.Address.StreetAddress())
            .RuleFor(o => o.City, f => f.Address.City())
            .RuleFor(o => o.Postalcode, f => f.Address.ZipCode())
            .RuleFor(o => o.Country, f => f.Address.Country())
            .RuleFor(o => o.TotalAmount, f => f.Finance.Amount(10, 500))
            .RuleFor(o => o.Status, f => initialState.ToString())
            .RuleFor(o => o.TransitionHistory, (f, o) => $"[*] -> {o.Status}");
        var order = orderFaker.Generate(1)[0];

        var fsm = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            initialState: Enum.Parse<OrderState>(order.Status),
            entityKey: order.Id,
            entity: order
        ).ConfigureTransitions(config);

        // Act
        fsm.FireAsync(trigger);

        // Assert
        fsm.Should()
            .BeIn(expectedState)
            .Logs().BeFinalState();
    }

    [Theory]
    [InlineData(OrderState.Shipped, OrderTrigger.Complete, OrderState.Completed)]
    [InlineData(OrderState.Failed, OrderTrigger.Delete, OrderState.Deleted)]
    public void Order_Should_Transition_Through_Valid_In_History(OrderState initialState, OrderTrigger trigger,
        OrderState expectedState)
    {
        // Arrange
        var config = Flows.FlowTransitions.GetOrderConfig(_orderApiClient.Object);
        var orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, f => f.IndexFaker + 1)
            .RuleFor(o => o.FirstName, f => f.Name.FirstName())
            .RuleFor(o => o.LastName, f => f.Name.LastName())
            .RuleFor(o => o.AddressLine1, f => f.Address.StreetAddress())
            .RuleFor(o => o.City, f => f.Address.City())
            .RuleFor(o => o.Postalcode, f => f.Address.ZipCode())
            .RuleFor(o => o.Country, f => f.Address.Country())
            .RuleFor(o => o.TotalAmount, f => f.Finance.Amount(10, 500))
            .RuleFor(o => o.Status, f => initialState.ToString())
            .RuleFor(o => o.TransitionHistory, (f, o) => $"[*] -> {o.Status}");
        var order = orderFaker.Generate(1)[0];

        var fsm = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            initialState: Enum.Parse<OrderState>(order.Status),
            entityKey: order.Id,
            entity: order
        ).ConfigureTransitions(config);

        // Act
        fsm.FireAsync(trigger);

        // Assert
        fsm.Should().History()
            .Contains(trigger, expectedState)
            .ContainsTrigger(trigger)
            .StartWith(initialState)
            .HaveLength(2);
    }
    
    [Theory]
    [InlineData(OrderState.Failed, OrderTrigger.Delete)]
    public void Order_Should_Transition_Through_Delete_Entity(OrderState initialState, OrderTrigger trigger)
    {
        // Arrange
        var config = Flows.FlowTransitions.GetOrderConfig(_orderApiClient.Object);
        var orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, f => f.IndexFaker + 1)
            .RuleFor(o => o.FirstName, f => f.Name.FirstName())
            .RuleFor(o => o.LastName, f => f.Name.LastName())
            .RuleFor(o => o.AddressLine1, f => f.Address.StreetAddress())
            .RuleFor(o => o.City, f => f.Address.City())
            .RuleFor(o => o.Postalcode, f => f.Address.ZipCode())
            .RuleFor(o => o.Country, f => f.Address.Country())
            .RuleFor(o => o.TotalAmount, f => f.Finance.Amount(10, 500))
            .RuleFor(o => o.Status, f => initialState.ToString())
            .RuleFor(o => o.TransitionHistory, (f, o) => $"[*] -> {o.Status}");
        var order = orderFaker.Generate(1)[0];

        var fsm = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
            initialState: Enum.Parse<OrderState>(order.Status),
            entityKey: order.Id,
            entity: order
        ).ConfigureTransitions(config);

        // Act
        fsm.Should().NotNullEntity();
        fsm.FireAsync(trigger);

        // Assert
        fsm.Should()
            .DeleteEntity();
    }
}