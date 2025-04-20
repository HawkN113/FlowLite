using FlowLite.Core.Abstractions.Configuration;
using FlowLite.Core.Configuration;
using OrderGateway.Api.Abstractions;
using OrderGateway.Api.Abstractions.Models;
using Orders.Processor.Flows.Models;
namespace Orders.Processor.Flows;

public static class FlowTransitions
{
    public static IFlowTransitionBuilder<OrderState, OrderTrigger, Order> GetOrderConfig(IOrderApiClient orderApiClient)
    {
        return new FlowTransitionBuilder<OrderState, OrderTrigger, Order>()
            .AddTransition(OrderState.Pending, OrderTrigger.Create, OrderState.Created, async (_, ctx) =>
            {
                ctx.Entity!.Status = OrderState.Created.ToString();
                ctx.Entity!.TransitionHistory += " -> Created";
                await Task.CompletedTask;
            })
            .AddTransition(OrderState.Created, OrderTrigger.Ship, OrderState.Shipped, async (_, ctx) =>
            {
                ctx.Entity!.Status = OrderState.Shipped.ToString();
                ctx.Entity!.TransitionHistory += " -> Shipped";
                await Task.CompletedTask;
            })
            .AddTransition(OrderState.Shipped, OrderTrigger.Complete, OrderState.Completed, async (moveTo, ctx) =>
            {
                if (ctx.Entity!.Id % 2 != 0)
                {
                    ctx.Entity!.Status = OrderState.Completed.ToString();
                    ctx.Entity!.TransitionHistory += " -> Completed";
                }
                else
                {
                    // Move 'Shipped' to `Canceled`
                    await moveTo(OrderState.Shipped, OrderTrigger.Cancel);
                }
            })
            .AsFinal()
            .AddTransition(OrderState.Shipped, OrderTrigger.Cancel, OrderState.Canceled, async (_, ctx) =>
            {
                ctx.Entity!.Status = OrderState.Canceled.ToString();
                ctx.Entity!.TransitionHistory += " -> Canceled";
                await Task.CompletedTask;
            })
            .AddTransition(OrderState.Canceled, OrderTrigger.Cancel, OrderState.Failed, async (_, ctx) =>
            {
                ctx.Entity!.Status = OrderState.Failed.ToString();
                ctx.Entity!.TransitionHistory += " -> Failed (Cancel trigger)";
                await Task.CompletedTask;
            })
            .AddTransition(OrderState.Canceled, OrderTrigger.Fail, OrderState.Failed, async (_, ctx) =>
            {
                ctx.Entity!.Status = OrderState.Failed.ToString();
                ctx.Entity!.TransitionHistory += " -> Failed (Fail trigger)";
                await Task.CompletedTask;
            })
            .AddTransition(OrderState.Failed, OrderTrigger.Delete, OrderState.Deleted, async (_, ctx) =>
            {
                await orderApiClient.DeleteOrderAsync(ctx.Entity!.Id);
                ctx.MarkForDeletion();
            })
            .AsFinal()
            .Build();
    }
}