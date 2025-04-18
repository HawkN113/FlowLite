using System.Text.Json;
using FlowLite.Core.Converters;
namespace FlowLite.Core.Tests.Converters;

public class TupleConverterTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new TupleConverter<OrderState, Order>() },
        WriteIndented = false
    };

    [Fact]
    public void Serialize_ShouldConvertTupleToJson()
    {
        // Arrange
        const OrderState state = OrderState.Processing;
        var entity = new Order { Id = 1, Product = "Laptop", Quantity = 2 };
        var tuple = (state, entity);

        // Act
        var json = JsonSerializer.Serialize(tuple, _options);

        // Assert
        var expectedJson = "{\"State\":1,\"Entity\":{\"Id\":1,\"Product\":\"Laptop\",\"Quantity\":2}}";
        Assert.Equal(expectedJson, json);
    }

    [Fact]
    public void Deserialize_ShouldConvertJsonToTuple()
    {
        // Arrange
        const string json = "{\"State\":2,\"Entity\":{\"Id\":5,\"Product\":\"Phone\",\"Quantity\":1}}";

        // Act
        var result = JsonSerializer.Deserialize<(OrderState State, Order Entity)>(json, _options);

        // Assert
        Assert.Equal(OrderState.Shipped, result.State);
        Assert.Equal(5, result.Entity.Id);
        Assert.Equal("Phone", result.Entity.Product);
        Assert.Equal(1, result.Entity.Quantity);
    }
}

public enum OrderState
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}

public class Order
{
    public int Id { get; set; }
    public string Product { get; set; } = string.Empty;
    public int Quantity { get; set; }
}