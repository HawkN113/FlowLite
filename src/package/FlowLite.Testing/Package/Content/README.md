# FlowLite.Testing

**FlowLite.Testing** is a minimalistic and fluent testing library designed for writing expressive and structured unit tests for your **FlowLite FSM** workflows.
It builds on top of [FlowLite](https://www.nuget.org/packages/FlowLite), enabling easy assertions for:
- State transitions
- Entity lifecycle
- Log messages
- Transition history
- Entity deletion

## Features

- **Fluent Assertions** — expressive `.Should().Allow(...)`, `.BeIn(...)`, `.Logs()`, etc.
- **Trigger Validation** — `.Assert().Allow(...)` or `.NotAllow(...)`.
- **Entity Validation** — validate entity changes or deletions via `.HaveEntity(...)` and `.DeleteEntity()`.
- **Log Inspection** — assert warnings, errors, or specific messages.
- **History Validation** — verify transition path, length, or contents.
- **Minimal & Fast** — no dependencies on other testing libraries or frameworks.
- Designed for full compatibility with [FlowLite FSM](https://www.nuget.org/packages/FlowLite).

---

## Installation

To install the latest version of the `FlowLite.Testing` [NuGet package](https://www.nuget.org/packages/FlowLite.Testing/):

### NuGet Package Manager
```bash
Install-Package FlowLite.Testing -Version 8.0.0
```
### .NET CLI
```bash
dotnet add package FlowLite.Abstractions --version 8.0.0
dotnet add package FlowLite.Testing --version 8.0.0
```
---

## Usage Guide

### Required Namespaces
```csharp
using FlowLite.Testing;
using FlowLite.Testing.Assertions;
```
---

## Fluent Testing API
### 1. Assert a Trigger is Allowed or Rejected
```csharp
fsm.Should().Allow(OrderTrigger.Pay);
fsm.Should().NotAllow(OrderTrigger.Cancel);
```
---

### 2. Verify Current State
```csharp
fsm.Should().BeIn(OrderState.Paid);
```
---

### 3. Entity Assertions
```csharp
fsm.Should().NotNullEntity();
fsm.Should().HaveEntity(order =>
{
    Assert.Equal("Shipped", order?.Status);
});
```
---

### 4. Log Assertions
```csharp
fsm.Should().Logs().Log(LogLevel.Warning, "invalid transition");
fsm.Should().Logs().BeFinalState();
fsm.Should().Logs().ContainFinalStateLog();
fsm.Should().Logs().MatchSnapshot(new[] { "FSM initialized", "Transitioned to Paid" });
```
---

### 5. History Assertions
```csharp
fsm.Should().History()
    .StartWith(OrderState.Created)
    .HaveInHistory(OrderState.Created, OrderState.Paid, OrderState.Shipped)
    .Contains(OrderTrigger.Pay, OrderState.Paid)
    .HaveLength(3)
    .ContainsTrigger(OrderTrigger.Ship);
```
---

## Trigger-Only Assertions
You can use .Assert() style for concise checks:
```csharp
fsm.Assert().Allow(OrderTrigger.Pay);
fsm.Assert().NotAllow(OrderTrigger.Cancel);
```
---

**Minimal sample**
```csharp
fsm.AddTransition(OrderState.Created, OrderTrigger.Pay, OrderState.Paid, async (_, ctx) =>
   {
       ctx.Entity!.Status = "Paid";
       await Task.CompletedTask;
   });
await fsm.FireAsync(OrderTrigger.Pay);
fsm.Should()
    .BeIn(OrderState.Paid)
    .NotNullEntity()
    .Logs().Log(LogLevel.Info, "Paid");
```
---

## License
This project is licensed under the MIT License.