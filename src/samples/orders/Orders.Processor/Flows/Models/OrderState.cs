namespace Orders.Processor.Flows.Models;

public enum OrderState
{
    Pending = 0,
    Created = 1,
    Shipped = 2,
    Canceled = 3,
    Completed = 4,
    Failed = 5,
    Deleted = 6,
    Deleted2 = 7
}