using System.Text.Json;
using FlowLite.Console.Models;
using FlowLite.Console.Models.Order;
using FlowLite.Console.Models.User;
using FlowLite.Core.Abstractions.Storage;
using FlowLite.Core.Extensions;
using FlowLite.Core.Fsm;
using FlowLite.Diagnostics.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddFlowLiteStorage<OrderState, string, Order>(StorageType.Memory,
            "C:\\Experiments\\FlowLite\\src\\samples\\FlowLite.Console\\");
        services.AddFlowLiteStorage<UserState, int, User>(StorageType.Memory,
            "C:\\Experiments\\FlowLite\\src\\samples\\FlowLite.Console\\");
        // Register logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Trace);
        });
        // Global diagnostics
        services.AddFlowLiteDiagnostics<UserState, UserTrigger, int, User>(opt =>
        {
            opt.EnableGlobalDiagnostics = true;
            opt.Telemetry.Enabled = true;
            opt.Logging.Enabled = true;
            opt.Logging.UseConsole = true;
            opt.Logging.UseLogger = true;
            opt.Logging.LoggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
            opt.DiagnosticObserver.Enabled = true;
        });

    })
    .Build();
try
{
    var order = new Order()
    {
        OrderId = "order-123",
        State = OrderState.Created,
        CreatedAt = DateTimeOffset.UtcNow,
        ModifiedAt = DateTimeOffset.UtcNow
    };

    var storage = host.Services.GetFlowLiteStorage<OrderState, string, Order>();
    var ustorage = host.Services.GetFlowLiteStorage<UserState, int, User>();

    using var fsm = new StateFlowMachine<OrderState, OrderTrigger, string, Order>(
        OrderState.Created,
        storage,
        order.OrderId,
        order);

    fsm.OnStateChanged += (state,_) => Console.WriteLine($"The state was changed: {state.ToString()}");
    fsm.OnEntityChanged +=
        entity => Console.WriteLine($"The entity was changed: {entity.State.ToString()}");

    var createdToPaidIssue = false;
    var paidToFailedIssue = false;
    var completeToFailedIssue = false;

    // Scenario: Created -> Shipped -> Completed 
    fsm.AddTransition(OrderState.Created, OrderTrigger.Pay, OrderState.Paid, async (moveTo, ctx) =>
        {
            Console.WriteLine("Processing payment...");
            await Task.Delay(1000);
            if (createdToPaidIssue)
            {
                ctx.Entity!.State = OrderState.Failed;
                await moveTo(OrderState.Failed, OrderTrigger.Pay);
                return;
            }

            ctx.Entity!.State = OrderState.Paid;
            ctx.Entity!.ModifiedAt = DateTimeOffset.UtcNow;
            Console.WriteLine("Payment successful!");
        }).AddTransition(OrderState.Paid, OrderTrigger.Ship, OrderState.Shipped, async (moveTo, ctx) =>
        {
            Console.WriteLine("Shipping order...");
            await Task.Delay(500);
            if (paidToFailedIssue)
            {
                ctx.Entity!.State = OrderState.Failed;
                await moveTo(OrderState.Failed, OrderTrigger.Ship);
                return;
            }

            ctx.Entity!.State = OrderState.Shipped;
            ctx.Entity!.ModifiedAt = DateTimeOffset.UtcNow;
            Console.WriteLine("The order was shipped!");
        }).AddTransition(OrderState.Shipped, OrderTrigger.Complete, OrderState.Completed, async (moveTo, ctx) =>
        {
            Console.WriteLine("Completing order...");
            await Task.Delay(500);
            if (completeToFailedIssue)
            {
                ctx.Entity!.State = OrderState.Failed;
                await moveTo(OrderState.Failed, OrderTrigger.Complete);
                return;
            }

            ctx.Entity!.State = OrderState.Completed;
            ctx.Entity!.ModifiedAt = DateTimeOffset.UtcNow;
            Console.WriteLine("The order was completed!");
        })
        .AddTransition(OrderState.Paid, OrderTrigger.Cancel, OrderState.Canceled, async (_, ctx) =>
        {
            Console.WriteLine("Canceling order...");
            await Task.Delay(500);
            ctx.Entity!.State = OrderState.Canceled;
            ctx.Entity!.ModifiedAt = DateTimeOffset.UtcNow;
            Console.WriteLine("The order was canceled!");
        }).AsFinal();

    await fsm.FireAsync(OrderTrigger.Pay);
    await fsm.FireAsync(OrderTrigger.Ship);
    await fsm.FireAsync(OrderTrigger.Complete);

    var currentState = fsm.CurrentState;
    var currentEntity = fsm.CurrentEntity;
    var history = fsm.GetTransitionHistory();

    Console.WriteLine($"Current state: {currentState}");
    Console.WriteLine($"Current entity: {JsonSerializer.Serialize(currentEntity)}");
    foreach (var (trigger, state) in history)
        Console.WriteLine(trigger is not null ? $"{trigger} -> {state}" : $"Start: {state}");

    var logs = fsm.GetLogs();
    foreach (var log in logs)
        Console.WriteLine(log);

    var userList = new List<User>()
    {
        new User()
        {
            UserId = 1,
            modifiedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        },
        new User()
        {
            UserId = 2,
            modifiedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        }
    };

    foreach (var user in userList)
    {
        using var userFsm = new StateFlowMachine<UserState, UserTrigger, int, User>(
            UserState.Created,
            ustorage,
            user.UserId,
            user);

        userFsm.AddTransition(UserState.Created, UserTrigger.Create, UserState.WaitActivation, async (_, ctx) =>
            {
                ctx.Entity!.modifiedAt = DateTimeOffset.UtcNow;
                await Task.CompletedTask;
            })
            .AddTransition(UserState.WaitActivation, UserTrigger.Activate, UserState.Activated,
                async (_, ctx) =>
                {
                    ctx.Entity!.IsActivated = true;
                    ctx.Entity!.modifiedAt = DateTimeOffset.UtcNow;
                    await Task.CompletedTask;
                });
        await userFsm.FireAsync(UserTrigger.Create);
        await userFsm.FireAsync(UserTrigger.Activate);
    }

    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}