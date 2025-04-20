namespace Orders.Processor.Flows.Models;

public enum OrderTrigger
{
    Create = 0,
    Ship = 1,
    Cancel = 2,
    Complete = 3,
    Fail = 4,
    Delete = 5
}