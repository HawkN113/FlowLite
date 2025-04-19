# FlowLite

FlowLite is a high-performance, thread-safe, and developer-friendly finite state machine (FSM) library for .NET. It helps you manage entity state transitions, workflow automation, and domain-driven state logic in a structured and testable way. Features: fluent API for state and trigger configuration, async support for transition logic, export as Mermaid.js or DOT graph, built-in JSON and in-memory storage.

## Features
- **Lightweight & Fast** – Optimized for high-performance applications.
- **Asynchronous & Thread-Safe** – Ensures smooth execution in concurrent environments.
- **Declarative API** – Define state transitions with a clean and fluent syntax.
- **State Persistence** – Supports **JSON-based storage**, **in-memory storage**.
- **Event Hooks** – Capture and log every state change for debugging and auditing (check state, check entry, delete entry).
- **Cycle Prevention** – Detects and prevents cyclic transitions.
- **Error Handling** – Graceful fallback mechanisms for invalid transitions.
- **Final States** – Define terminal states where transitions are restricted.
- **Flexible Configuration** – Supports **builder pattern** for easy transition setup.
- **Parallel & Sequential State Execution** – Suitable for multi-threaded workflows.
- **Custom Transition Conditions** – Add business logic to control transitions.
- **Dependency Injection Support** – Seamless integration into DI containers.
- **Storage-Agnostic** – Plug in your own storage strategy
- **Diagram Export** – Export transitions as **Mermaid.js** or **Graphviz DOT**.
---

## Getting Started

### When to Use FlowLite?
- **Order Processing Systems** – Manage payment, shipping, and delivery statuses.
- **Workflow Automation** – Control document approval, user onboarding, or task execution.
- **Game Development** – Manage player states, AI behavior, and event triggers.
- **IoT Device State Tracking** – Handle device power states, connectivity, and error handling.
- **Business Processes** – Automate transitions in **CRM, ERP, BPM** applications.

### Prerequisites

- .NET 8 or higher.
---

## Installation

To install the latest version of the `FlowLite` [NuGet package](https://www.nuget.org/packages/FlowLite/):

### NuGet Package Manager
```bash
Install-Package FlowLite -Version 8.0.0
```
### .NET CLI
```bash
dotnet add package FlowLite.Abstractions --version 8.0.0
dotnet add package FlowLite --version 8.0.0
```
---

## Prerequisites
- **.NET 8** or higher.
---

## Usage Guide

### Required Namespaces
```csharp
using FlowLite.Configuration;
using FlowLite.Core;
using FlowLite.Extensions;
using FlowLite.Storage;
using FlowLite.Storage.Abstractions;
```
---

### 1. Define States & Triggers
```csharp
public enum OrderState { Created, Paid, Shipped, Delivered, Canceled }
public enum OrderTrigger { Pay, Ship, Deliver, Cancel }
```
---

### 2. Define Your Entity
```csharp
public class Order
{
    public int Id { get; set; }
    public string? Status { get; set; }
    public string? TransitionHistory { get; set; }
    public bool PaymentConfirmed { get; set; }
}
```
---

### 3. Configure Storage

#### JSON Storage (Persistent)
```csharp
services.AddFlowLiteStorage<OrderState, int, Order>(
    StorageType.Json,
    "C:\FlowLite_Storage\"
);
```
`JSON` storage is used for learning or review

#### In-Memory Storage (Volatile)
```csharp
services.AddFlowLiteStorage<OrderState, int, Order>(
    StorageType.Memory,
    "C:\FlowLite_Storage\"
);
```
In-Memory storage is used for production environment.
---

### 4. Initialize State Machine
```csharp
var stateMachine = new StateFlowMachine<OrderState, OrderTrigger, int, Order>(
    initialState: OrderState.Created,
    stateStorage: storage, 
    entityKey: order.Id,  
    entity: order
);
```
---

### 5. Configure Transitions

#### Fluent API
```csharp
stateMachine
    .AddTransition(OrderState.Created, OrderTrigger.Pay, OrderState.Paid, async (_, ctx) => {
        ctx.Order.Status = "Paid";
        await Task.CompletedTask;
    })
    .AddTransition(OrderState.Paid, OrderTrigger.Ship, OrderState.Shipped, async (_, ctx) => {
        ctx.Order.Status = "Shipped";
        await Task.CompletedTask;
    }).AsFinal();
```
#### Transitions with Async actions
Each transition can include custom actions executed when the state changes:
```csharp
var flowBuilder = new FlowTransitionBuilder<OrderState, OrderTrigger, Order>()
    .AddTransition(OrderState.Created, OrderTrigger.Pay, OrderState.Paid, async (_, ctx) => {
        ctx.Entity!.Status = "Paid";
        await Task.CompletedTask;
    })
    .AddTransition(OrderState.Paid, OrderTrigger.Ship, OrderState.Shipped, async (_, ctx) => {
        ctx.Entity!.Status = "Shipped";
        await Task.CompletedTask;
    })
    .AsFinal()
    .Build();
```
Apply the configuration with builder
```csharp
stateMachine.ConfigureTransitions(flowBuilder);
```

#### Custom Transition Conditions
You can enforce custom conditions before a transition is allowed:
```csharp
stateMachine.AddTransition(OrderState.Created, OrderTrigger.Pay, OrderState.Paid,
    async (moveTo, ctx) => {
        if (!ctx.Entity.PaymentConfirmed)
        {
            // Move to transition with state `Canceled` and trigger `Cancel` (entity should be saved)
            await moveTo(OrderState.Canceled, OrderTrigger.Cancel, true);
        }
    }
);
```
**Sample**
```csharp
stateMachine.AddTransition(OrderState.Failed, OrderTrigger.Delete, OrderState.Deleted, async (_, ctx) =>
   {
       await orderApiClient.DeleteOrderAsync(ctx.Entity!.Id);
       ctx.MarkForDeletion();
   })
```
You can delete entry for storage

**Sample**
```csharp
stateMachine.AddTransition(OrderState.Created, OrderTrigger.Pay, OrderState.Paid,
    async (moveTo, ctx) => {
        if (!ctx.Entity.PaymentConfirmed)
            await moveTo(OrderState.Canceled, true);
    }
);
```
Move to transition with state `Canceled` and entity will be saved in the storage

**Sample**
```csharp
stateMachine.AddTransition(OrderState.Created, OrderTrigger.Pay, OrderState.Paid,
    async (moveTo, ctx) => {
        if (!ctx.Entity.PaymentConfirmed) {
            ctx.Entity.Status = OrderState.Canceled.ToString();
            await moveTo(OrderState.Canceled, true);
        }
    }
);
```
The entity (order) will be changed the status to `Canceled`. After that the flow  is moving to transition with state `Canceled`, entity will be saved.

**Sample**
```csharp
stateMachine.AddTransition(OrderState.Created, OrderTrigger.Pay, OrderState.Paid,
    async (_, _) => {
        await Task.CompletedTask;
    }
);
```
There is a default implementation of empty transition.
---

### 6. Execute Transitions
```csharp
await stateMachine.FireAsync(OrderTrigger.Pay);
await stateMachine.FireAsync(OrderTrigger.Ship);
```
or use the alternative with configuration
```csharp
stateMachine.ConfigureTransitions(stateFlowConfig);

await stateMachine.FireAsync(OrderTrigger.Pay);
await stateMachine.FireAsync(OrderTrigger.Ship);
```
---

### 7. Track State History & Logs
```csharp
var history = stateMachine.GetTransitionHistory();
var logs = stateMachine.GetLogs();
```
---

### 8. Event Handling
- `OnStateChanged` event is triggered whenever the state of an entity changes. It allows you to execute custom logic when a state transition occurs.
```csharp
stateMachine.OnStateChanged += async (state, trigger) => {
    Console.WriteLine($"State changed to {state} ({trigger})");
    await Task.CompletedTask;
};
```
Use Cases:
- Logging state transitions.
- Sending notifications when a state changes.
- Updating UI or external services based on state changes.

- `OnEntityChanged` event is fired when an entity is modified during a transition. It helps track updates to entity data as the workflow progresses.
```csharp
stateMachine.OnEntityChanged += async (entity) => {
    Console.WriteLine($"Entity {entity.Id} updated: {entity}");
    await Task.CompletedTask;
};
```
Use Cases:
- Persisting entity changes after a transition.
- Auditing modifications to entity properties.
- Triggering additional actions based on entity updates.

- `OnEntityDeleted` event is fired when an entity is deleted during a transition. It helps track deletes entity data during workflow progress.
```csharp
stateMachine.OnEntityDeleted += async (id) => {
    Console.WriteLine($"Entity {id.ToString()} was deleted");
    await Task.CompletedTask;
};
```

- `OnTransitionFailed` event is fired when an unhandled exception occurs during transition execution in FireAsync.
```csharp
fsm.OnTransitionFailed += async (state, trigger, ex) =>
{
    Console.WriteLine($"Transition failed: [{state}] + [{trigger}] => {ex.Message}");
    await Task.CompletedTask;
};
```
---

### 9. Export as Diagram
```csharp
var diagramMermaid = stateMachine.Export(ExportType.Mermaid);
var diagramDot = stateMachine.Export(ExportType.Dot);
```
---