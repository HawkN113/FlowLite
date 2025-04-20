# FlowLite.Diagnostics

**FlowLite.Diagnostics** is an extension for [FlowLite](https://www.nuget.org/packages/FlowLite) that adds logging, telemetry, observability, and global FSM diagnostics. It allows you to monitor the full lifecycle of your finite state machine, including transitions, errors, entity changes, and deletions.

## Features

- **Console logging** of all FSM events.
- **Telemetry metrics via System.Diagnostics.Metrics** (OpenTelemetry-compatible).
- **DiagnosticListener events** (StateChanged, EntityChanged, EntityDeleted).
- **Integration with ILogger** (Microsoft.Extensions.Logging).
- **Global diagnostics**: auto-attach listeners to all FSMs via FlowLite global hook.
- Extensibility via custom **IDiagnosticsFlowLiteListener**.

---

## Installation

To install the latest version of the `FlowLite.Diagnostics` [NuGet package](https://www.nuget.org/packages/FlowLite.Diagnostics/):

### NuGet Package Manager
```bash
Install-Package FlowLite.Diagnostics -Version 8.0.0
```
### .NET CLI
```bash
dotnet add package FlowLite.Abstractions --version 8.0.0
dotnet add package FlowLite.Diagnostics --version 8.0.0
```
---

## Usage Guide

### Global setup in ASP.NET Core
```csharp
services.AddFlowLiteDiagnostics<[State], [Trigger], [Key], [Entity]>(opt =>
{
    opt.EnableGlobalDiagnostics = true;
    opt.Telemetry.Enabled = true;
    opt.Logging.Enabled = true;
    opt.Logging.UseConsole = true;
    opt.Logging.UseLogger = true;
    opt.Logging.LoggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
    opt.DiagnosticObserver.Enabled = true;
});
```
After registration, all state machines created will automatically get hooked into diagnostics.
**Sample**:
```csharp
services.AddFlowLiteDiagnostics<OrderState, OrderTrigger, int, Order>(opt =>
{
    opt.EnableGlobalDiagnostics = true;
    opt.Telemetry.Enabled = true;
    opt.Logging.Enabled = true;
    opt.Logging.UseConsole = true;
    opt.Logging.UseLogger = true;
    opt.Logging.LoggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
    opt.DiagnosticObserver.Enabled = true;
});
```
### Emitted Metrics (OpenTelemetry-compatible)
- `fsm_transitions_total` - number of FSM transitions.
- `fsm_failures_total` - number of failed FSM transitions.
- `fsm_state_changed_total` - number of state changes.
- `fsm_entity_changed_total` - number of entity updates.
- `fsm_entity_deleted_total`- number of entity deletions.
- `fsm_transition_failed_total` - number of failed transitions.

### Telemetry (only for state machine)
```csharp
fsm.UseTelemetry(new TelemetryOptions()
{
    Enabled = true, 
    Source = "FlowLite.FSM"
});
```
or 
```csharp
fsm.UseTelemetry();
```

### Console Logging (only for state machine)
```csharp
fsm.UseLogging(new LoggingOptions()
{
    Enabled = true,
    UseConsole = true,
    UseLogger = false
});
```
or 
```csharp
fsm.UseConsoleLogging();
```

### Using ILogger (only for state machine)
```csharp
fsm.UseLogging(new LoggingOptions()
{
    Enabled = true,
    UseConsole = true,
    UseLogger = true,
    LoggerFactory = services.GetRequiredService<ILoggerFactory>()
});
```

### Diagnostic observer (only for state machine)
```csharp
fsm.UseDiagnosticObserver(new DiagnosticObserverOptions()
{
    Enabled = true,
    Source = "FlowLite.Diagnostics"
});
```
or 
```csharp
fsm.UseDiagnosticObserver();
```

### Register a Custom Listener
```csharp
services.AddFlowLiteDiagnostics<OrderState, OrderTrigger, int, Order>(opt =>
{
    opt.EnableGlobalDiagnostics = true;
    opt.Telemetry.Enabled = true;
    opt.Logging.Enabled = true;
    opt.Logging.UseConsole = true;
    opt.Logging.UseLogger = true;
    opt.Logging.LoggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
    opt.DiagnosticObserver.Enabled = true;
    opt.CustomListeners.Add(new MyCustomFlowDiagnosticsListener());
});
```
Use **CustomListeners** to add a custom listener (based on **IDiagnosticsFlowLiteListener**)

---

## License
This project is licensed under the MIT License.