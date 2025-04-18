namespace OrderGateway.Api.DTOs;

public class OrderDto
{
    public required int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Postalcode { get; set; }
    public string Country { get; set; }
    public required decimal TotalAmount { get; set; }
    public required string Status { get; set; }
    public string TransitionHistory { get; set; } = string.Empty;
}