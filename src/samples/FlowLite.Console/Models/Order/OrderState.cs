namespace FlowLite.Console.Models.Order;

public enum OrderState
{
    Created = 0,
    Paid = 1,
    Shipped = 2,
    Canceled = 3,
    Failed = 4,
    Completed = 5,
    Final = 6
}