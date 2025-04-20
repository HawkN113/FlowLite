using Microsoft.EntityFrameworkCore;
using Nelibur.ObjectMapper;
using OrderGateway.Api.Data;
using OrderGateway.Api.Data.Models;
using OrderGateway.Api.DTOs;
using OrderGateway.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<OrderDbContext>(options => options.UseInMemoryDatabase("OrdersDb"));
builder.Services.AddMappingConfig();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    dbContext.Database.EnsureDeletedAsync();
});
app.UseHttpsRedirection();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await dbContext.SeedDatabaseAsync();
}

var orderTagName = "Orders";

app.MapGet("/orders", async (OrderDbContext db) =>
    {
        Console.WriteLine("Get orders...");
        var list = await db.Orders.ToListAsync();
        var result = list.Select(order => TinyMapper.Map<OrderDto>(order)).ToList();
        return Results.Ok(result);
    })
    .WithTags(orderTagName)
    .WithDescription("Get orders.")
    .WithOpenApi();

app.MapGet("/orders/{id}", async (int id, OrderDbContext db) =>
    {
        Console.WriteLine($"Get order by id({id})...");
        return await db.Orders.FindAsync(id) is { } order
            ? Results.Ok(TinyMapper.Map<OrderDto>(order))
            : Results.NotFound();
    })
    .WithTags(orderTagName)
    .WithDescription("Get order by id.")
    .WithOpenApi();

app.MapPost("/orders", async (OrderDto order, OrderDbContext db) =>
    {
        var convertedOrder = TinyMapper.Map<Order>(order);

        var existingOrder = await db.Orders.FindAsync(convertedOrder.Id);
        if (existingOrder is not null) return Results.Conflict();
        
        db.Orders.Add(convertedOrder);
        await db.SaveChangesAsync();
        Console.WriteLine($"Create an order with id({order.Id})...");
        return Results.Created($"/orders/{order.Id}", TinyMapper.Map<OrderDto>(convertedOrder));
    })
    .WithTags(orderTagName)
    .WithDescription("Create new order.")
    .WithOpenApi();

app.MapPut("/orders/{id}", async (int id, OrderDto updatedOrder, OrderDbContext db) =>
    {
        var convertedOrder = TinyMapper.Map<Order>(updatedOrder);
        var order = await db.Orders.FindAsync(id);
        if (order is null) return Results.NotFound();

        order.LastName = convertedOrder.LastName;
        order.FirstName = convertedOrder.FirstName; 
        order.AddressLine1 = convertedOrder.AddressLine1;
        order.City = convertedOrder.City;
        order.Postalcode = convertedOrder.Postalcode;
        order.Country = convertedOrder.Country;
        order.Status = convertedOrder.Status;
        order.TotalAmount = convertedOrder.TotalAmount;
        order.TransitionHistory = convertedOrder.TransitionHistory;

        await db.SaveChangesAsync();
        
        Console.WriteLine($"Update the order with id({order.Id})...");
        
        return Results.Ok(TinyMapper.Map<OrderDto>(order));
    })
    .WithTags(orderTagName)
    .WithDescription("Update order by id.")
    .WithOpenApi();

app.MapDelete("/orders/{id}", async (int id, OrderDbContext db) =>
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null) return Results.NotFound();

        db.Orders.Remove(order);
        await db.SaveChangesAsync();
        Console.WriteLine($"Delete an order by id({id})...");
        return Results.NoContent();
    })
    .WithTags(orderTagName)
    .WithDescription("Delete order by id.")
    .WithOpenApi();

await app.RunAsync();