using OrderGateway.Api.Abstractions.Models;
using RestEase;
namespace OrderGateway.Api.Abstractions;

public interface IOrderApiClient
{
    [Get("orders")]
    Task<List<Order>> GetOrdersAsync();

    [Get("orders/{id}")]
    [AllowAnyStatusCode]
    Task<Order?> GetOrderByIdAsync([Path] int id);

    [Post("orders")]
    Task<Order> CreateOrderAsync([Body] Order order);

    [Put("orders/{id}")]
    Task UpdateOrderAsync([Path] int id, [Body] Order order);

    [Delete("orders/{id}")]
    [AllowAnyStatusCode]
    Task DeleteOrderAsync([Path] int id);
}