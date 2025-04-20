using FlowLite.Diag.Analysis;
namespace FlowLite.Diag.Tests.Analysis;

public class FlowLiteInspectorTests
{
    [Fact]
    public async Task ScanAsync_ShouldFindSingleBuilderAndTransitions()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        var code = """
            var builder = new FlowTransitionBuilder<OrderState, OrderTrigger, Order>()
                .AddTransition(OrderState.Pending, OrderTrigger.Create, OrderState.Created, async (_, ctx) =>
                {
                    ctx.Entity.Status = "Created";
                })
                .AddTransition(OrderState.Created, OrderTrigger.Cancel, OrderState.Canceled, async (_, ctx) =>
                {
                    ctx.MarkForDeletion();
                }).AsFinal();
            """;

        var filePath = Path.Combine(tempDir, "Sample.cs");
        await File.WriteAllTextAsync(filePath, WrapInMethod(code));

        var inspector = new FlowLiteInspector();

        // Act
        await inspector.ScanAsync(tempDir);
        var results = inspector.Results;

        // Assert
        Assert.Single(results);
        var (path, entries) = results.First();
        Assert.Equal(filePath, path);
        var entry = entries.First();
        Assert.Equal("builder", entry.ClassName);
        Assert.Equal(2, entry.Transitions.Count);
        Assert.False(entry.Transitions[0].IsFinal);
        Assert.True(entry.Transitions[1].IsFinal);
        Assert.Equal("Pending", entry.Transitions[0].FromState);
        Assert.Equal("Create", entry.Transitions[0].Trigger);
        Assert.Equal("Created", entry.Transitions[0].ToState);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task ScanAsync_ShouldHandleMultipleBuilders()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        var code = """
            var builder1 = new FlowTransitionBuilder<OrderState, OrderTrigger, Order>()
                .AddTransition(OrderState.Pending, OrderTrigger.Create, OrderState.Created, async (_, ctx) => { });
            
            var builder2 = new FlowTransitionBuilder<OrderState, OrderTrigger, Order>()
                .AddTransition(OrderState.Created, OrderTrigger.Ship, OrderState.Shipped, async (_, ctx) => { }).AsFinal();
            """;

        var filePath = Path.Combine(tempDir, "Sample2.cs");
        await File.WriteAllTextAsync(filePath, WrapInMethod(code));

        var inspector = new FlowLiteInspector();

        // Act
        await inspector.ScanAsync(tempDir);
        var results = inspector.Results;

        // Assert
        Assert.Single(results);
        var (_, entries) = results.FirstOrDefault();
        Assert.Equal(2, entries.Count());
        Assert.Contains(entries, e => e.ClassName == "builder1");
        Assert.Contains(entries, e => e.ClassName == "builder2");

        var builder2 = entries.First(e => e.ClassName == "builder2");
        Assert.Single(builder2.Transitions);
        Assert.True(builder2.Transitions[0].IsFinal);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    private static string WrapInMethod(string code)
    {
        return $$"""
        using System.Threading.Tasks;

        public class Order { public string Status { get; set; } }
        public enum OrderState { Pending, Created, Canceled, Shipped }
        public enum OrderTrigger { Create, Cancel, Ship }

        public class Demo {
            public async Task ConfigureAsync()
            {
                {{code}}
            }
        }
        """;
    }
}
