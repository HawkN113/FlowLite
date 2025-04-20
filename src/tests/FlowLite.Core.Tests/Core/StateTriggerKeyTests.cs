using FlowLite.Core.Fsm;
namespace FlowLite.Core.Tests.Core;

public class StateTriggerKeyTests
{
    public enum OrderState { Pending, Paid, Shipped, Delivered, Canceled }
    public enum OrderTrigger { Create, Pay, Ship, Deliver, Cancel }

    [Theory]
    [InlineData(OrderState.Pending, OrderTrigger.Create, "pending-create")]
    [InlineData(OrderState.Canceled, OrderTrigger.Cancel, "canceled-cancel")]
    [InlineData(OrderState.Canceled, OrderTrigger.Create, "canceled-create")]
    public void StateTriggerKey_ShouldCreateValidKey(OrderState state, OrderTrigger trigger, string expectedKey)
    {
        // Arrange & Act
        var key = new StateTriggerKey<OrderState, OrderTrigger>(state, trigger);

        // Assert
        Assert.Equal(expectedKey, key.ToString());
    }

    [Fact]
    public void StateTriggerKey_ShouldBeEqual_WhenSameStateAndTrigger()
    {
        // Arrange
        var key1 = new StateTriggerKey<OrderState, OrderTrigger>(OrderState.Pending, OrderTrigger.Create);
        var key2 = new StateTriggerKey<OrderState, OrderTrigger>(OrderState.Pending, OrderTrigger.Create);

        // Assert
        Assert.Equal(key1, key2);
        Assert.True(key1.Equals(key2));
        Assert.Equal(key1.GetHashCode(), key2.GetHashCode());
    }

    [Fact]
    public void StateTriggerKey_ShouldNotBeEqual_WhenDifferentStateOrTrigger()
    {
        // Arrange
        var key1 = new StateTriggerKey<OrderState, OrderTrigger>(OrderState.Pending, OrderTrigger.Create);
        var key2 = new StateTriggerKey<OrderState, OrderTrigger>(OrderState.Paid, OrderTrigger.Create);
        var key3 = new StateTriggerKey<OrderState, OrderTrigger>(OrderState.Pending, OrderTrigger.Pay);

        // Assert
        Assert.NotEqual(key1, key2);
        Assert.NotEqual(key1, key3);
        Assert.False(key1.Equals(key2));
        Assert.False(key1.Equals(key3));
        Assert.NotEqual(key1.GetHashCode(), key2.GetHashCode());
        Assert.NotEqual(key1.GetHashCode(), key3.GetHashCode());
    }

    [Fact]
    public void StateTriggerKey_ShouldWorkAsDictionaryKey()
    {
        // Arrange
        var dictionary = new Dictionary<StateTriggerKey<OrderState, OrderTrigger>, string>
        {
            { new StateTriggerKey<OrderState, OrderTrigger>(OrderState.Pending, OrderTrigger.Create), "Order Created" },
            { new StateTriggerKey<OrderState, OrderTrigger>(OrderState.Paid, OrderTrigger.Ship), "Order Shipped" }
        };

        var key1 = new StateTriggerKey<OrderState, OrderTrigger>(OrderState.Pending, OrderTrigger.Create);
        var key2 = new StateTriggerKey<OrderState, OrderTrigger>(OrderState.Paid, OrderTrigger.Ship);

        // Act
        var value1 = dictionary.TryGetValue(key1, out var result1);
        var value2 = dictionary.TryGetValue(key2, out var result2);

        // Assert
        Assert.True(value1);
        Assert.Equal("Order Created", result1);

        Assert.True(value2);
        Assert.Equal("Order Shipped", result2);
    }
}