using FlowLite.Core.Abstractions.Storage;
using FlowLite.Core.Configuration;
using FlowLite.Core.Extensions;
using FlowLite.Core.Fsm;
using FlowLite.Diagnostics.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderGateway.Api.Abstractions;
using OrderGateway.Api.Abstractions.Models;
using Orders.Processor.Flows;
using Orders.Processor.Flows.Models;
using RestEase;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(context =>
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        context.AddJsonFile($"appsettings.{env}.json", false);
    })
    .ConfigureServices((context, services) =>
    {
        // Register logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Trace);
        });
        // Global diagnostics
        services.AddFlowLiteDiagnostics<OrderState, OrderTrigger, int, Order>(opt =>
        {
            opt.EnableGlobalDiagnostics = true;
            opt.Telemetry.Enabled = true;
            opt.Logging.Enabled = true;
            opt.Logging.UseConsole = true;
            opt.Logging.UseLogger = true;
            opt.Logging.LoggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
            opt.DiagnosticObserver.Enabled = true;
        });
        // Register order client based `RestEase` (can be used `Refit`)
        services.AddSingleton<IOrderApiClient>(_ =>
            RestClient.For<IOrderApiClient>(baseUrl: context.Configuration["OrdersGatewayApi:BaseUrl"]!.ToString()));
        // Register FlowLite storage
        services.AddFlowLiteStorage<OrderState, int, Order>(
            StorageType.Json,
            "C:\\Experiments\\FlowLite\\src\\samples\\orders\\Orders.Processor\\"
        );
    })
    .Build();

try
{
    Console.WriteLine("Start services...");

    using var scope = host.Services.CreateScope();
    var services = scope.ServiceProvider;
    var orderApiClient = services.GetRequiredService<IOrderApiClient>();
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    var orderStorage = services.GetFlowLiteStorage<OrderState, int, Order>();
    
    // Get orders from service
    var orders = await orderApiClient.GetOrdersAsync();
    
    var stateFlowConfig2 = new FlowTransitionBuilder<OrderState, OrderTrigger, Order>()
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
        .AddTransition(OrderState.Shipped, OrderTrigger.Cancel, OrderState.Canceled, async (_, ctx) =>
        {
            ctx.Entity!.Status = OrderState.Canceled.ToString();
            ctx.Entity!.TransitionHistory += " -> Canceled";
            await Task.CompletedTask;
        });

    stateFlowConfig2.AddTransition(OrderState.Failed, OrderTrigger.Delete, OrderState.Deleted2, async (_, ctx) =>
    {
        await orderApiClient.DeleteOrderAsync(ctx.Entity!.Id);
        ctx.MarkForDeletion();
    }).AsFinal()
    .Build();

    // Create transition configuration
    var stateFlowConfig = FlowTransitions.GetOrderConfig(orderApiClient);

    Console.WriteLine("-------------------Process orders...-------------------");
    foreach (var item in orders)
    {
        // create a disposable instance of state machine flow
        using var fsm = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
                Enum.Parse<OrderState>(item.Status),
                orderStorage,
                item.Id,
                item)
            .ConfigureTransitions(stateFlowConfig);
        
        // Local basic diagnostics
        /*
        fsm.UseConsoleLogging();
        fsm.UseTelemetry();
        fsm.UseDiagnosticObserver();
        */
        
        // Local advanced diagnostics
        /*
        fsm.UseLogging(new LoggingOptions()
        {
            Enabled = true,
            UseConsole = true,
            UseLogger = true,
            LoggerFactory = services.GetRequiredService<ILoggerFactory>()
        });
        fsm.UseTelemetry(new TelemetryOptions()
        {
            Enabled = true, 
            Source = "FlowLite.FSM"
        });
        fsm.UseDiagnosticObserver(new DiagnosticObserverOptions()
        {
            Enabled = true,
            Source = "FlowLite.Diagnostics"
        });
        */
        
        // Fire using trigger
        await fsm.FireAsync(OrderTrigger.Create);
        await fsm.FireAsync(OrderTrigger.Ship);
        await fsm.FireAsync(OrderTrigger.Complete);
        
        // Save entity changes
        await orderApiClient.UpdateOrderAsync(fsm.CurrentEntity!.Id, fsm.CurrentEntity);
    }
    
    Console.WriteLine("-------------------Process (cancel) orders...-------------------");
    orders = await orderApiClient.GetOrdersAsync();
    foreach (var item in orders)
    {
        // create a disposable instance of state machine flow
        //using var fsm = GetStateMachineFlow(orderStorage, item)
        using var fsm = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
                Enum.Parse<OrderState>(item.Status), 
                orderStorage, 
                item.Id, 
                item)
            .ConfigureTransitions(stateFlowConfig);
        
        // Fire using trigger
        await fsm.FireAsync(OrderTrigger.Cancel);
        
        // Save entity changes
        await orderApiClient.UpdateOrderAsync(fsm.CurrentEntity!.Id, fsm.CurrentEntity);
    }
    
    Console.WriteLine("-------------------Process (delete) orders...-------------------");
    orders = await orderApiClient.GetOrdersAsync();
    foreach (var item in orders)
    {
        // create a disposable instance of state machine flow
        //using var fsm = GetStateMachineFlow(orderStorage, item)
        using var fsm = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
                Enum.Parse<OrderState>(item.Status),
                orderStorage,
                item.Id,
                item)
            .ConfigureTransitions(stateFlowConfig);
        
        // Fire using trigger
        await fsm.FireAsync(OrderTrigger.Delete);
    }

    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal error: {ex.Message}, {ex.StackTrace}");
}

return;