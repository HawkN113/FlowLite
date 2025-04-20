using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OrderGateway.Api.Data.Models;

[Table("Orders")]
public class Order
{
    [Key] [Column("Id")] public required int Id { get; set; }
    [Column("FirstName")] public required string FirstName { get; set; }
    [Column("LastName")] public required string LastName { get; set; }
    [Column("AddressLine1")] public string AddressLine1 { get; set; } = string.Empty;
    [Column("City")] public string City { get; set; } = string.Empty;
    [Column("Postalcode")] public string Postalcode { get; set; }
    [Column("Country")] public string Country { get; set; }
    [Column("TotalAmount")] public required decimal TotalAmount { get; set; }
    [Column("Status")] public required string Status { get; set; }
    [Column("TransitionHistory")] public string TransitionHistory { get; set; } = string.Empty;
}