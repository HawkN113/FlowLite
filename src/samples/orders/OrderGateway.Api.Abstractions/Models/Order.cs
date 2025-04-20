namespace OrderGateway.Api.Abstractions.Models;

public class Order
{
    public required int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public required string City { get; set; } = string.Empty;
    public required string Postalcode { get; set; }
    public required string Country { get; set; }
    public required decimal TotalAmount { get; set; }
    public required string Status { get; set; }
    public string TransitionHistory { get; set; } = string.Empty;
}