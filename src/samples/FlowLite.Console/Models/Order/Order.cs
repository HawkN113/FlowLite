namespace FlowLite.Console.Models.Order;

public class Order
{
    public required string OrderId { get; set; }
    public OrderState State { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
}