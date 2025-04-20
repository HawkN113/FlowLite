namespace FlowLite.Console.Models.User;

public class User
{
    public int UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset modifiedAt { get; set; }
    public bool IsActivated { get; set; }
}