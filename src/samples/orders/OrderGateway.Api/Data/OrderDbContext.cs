using Bogus;
using Microsoft.EntityFrameworkCore;
using OrderGateway.Api.Data.Models;
namespace OrderGateway.Api.Data;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public virtual DbSet<Order> Orders => Set<Order>();

    public async Task SeedDatabaseAsync()
    {
        if (!await Orders.AnyAsync())
        {
            var orderFaker = new Faker<Order>()
                .RuleFor(o => o.Id, f => f.IndexFaker + 1)
                .RuleFor(o => o.FirstName, f => f.Name.FirstName())
                .RuleFor(o => o.LastName, f => f.Name.LastName())
                .RuleFor(o => o.AddressLine1, f => f.Address.StreetAddress())
                .RuleFor(o => o.City, f => f.Address.City())
                .RuleFor(o => o.Postalcode, f => f.Address.ZipCode())
                .RuleFor(o => o.Country, f => f.Address.Country())
                .RuleFor(o => o.TotalAmount, f => f.Finance.Amount(10, 500))
                .RuleFor(o => o.Status, f => f.PickRandom("Pending", "Created", "Shipped", "Canceled"))
                .RuleFor(o => o.TransitionHistory, (f, o) => $"[*] -> {o.Status}");
            var orders = orderFaker.Generate(10);
            await Orders.AddRangeAsync(orders);
            await SaveChangesAsync();
        }
    }
}