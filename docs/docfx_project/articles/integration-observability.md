# Observability & Monitoring

**Level:** Advanced 🚀 | **Time:** 20-30 min | **Prerequisites:** [Basics](basics.md)

Enable distributed tracing and monitoring for Railway-Oriented Programming operations with OpenTelemetry and standard Problem Details error responses.

## Table of Contents

- [OpenTelemetry Tracing](#opentelemetry-tracing)

## OpenTelemetry Tracing

Enable distributed tracing for Railway Oriented Programming operations and Value Objects.

> **Important:** Auto-instrumentation (`AddFunctionalDddRopInstrumentation()`) traces **every** `Result<T>` operation and can create significant noise in production. It's recommended to **manually instrument** critical paths and use auto-instrumentation only for development/debugging.

### Installation

```bash
dotnet add package OpenTelemetry
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add package OpenTelemetry.Extensions.Hosting
```

### Recommended: Manual Instrumentation

For production, manually instrument critical business operations:

```csharp
using System.Diagnostics;

public class OrderService
{
    private static readonly ActivitySource ActivitySource = new("MyApp.OrderService");

    public async Task<Result<Order>> ProcessOrderAsync(
        CreateOrderCommand command,
        CancellationToken ct)
    {
        using var activity = ActivitySource.StartActivity("ProcessOrder");
        activity?.SetTag("order.customerId", command.CustomerId);
        activity?.SetTag("order.itemCount", command.Items.Count);

        var result = await _validator.ValidateToResultAsync(command, ct)
            .BindAsync((cmd, cancellationToken) => 
                CreateOrderAsync(cmd, cancellationToken), ct)
            .TapAsync(async (order, cancellationToken) => 
                await _repository.SaveAsync(order, cancellationToken), ct);

        // Record result in trace
        activity?.SetTag("result.isSuccess", result.IsSuccess);
        if (result.IsFailure)
        {
            activity?.SetTag("result.error.type", result.Error.GetType().Name);
            activity?.SetTag("result.error.detail", result.Error.Detail);
            activity?.SetStatus(ActivityStatusCode.Error, result.Error.Detail);
        }

        return result;
    }
}

// Register your ActivitySource
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerBuilder =>
    {
        tracerBuilder
            .AddSource("MyApp.OrderService")  // Only trace what you register
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    });
```

**Benefits of manual instrumentation:**
- ✅ **Control** - Trace only critical paths
- ✅ **Performance** - Minimal overhead
- ✅ **Signal-to-noise** - Clear, actionable traces
- ✅ **Business context** - Add domain-specific tags

### Auto-Instrumentation (Development/Debugging Only)

Auto-instrumentation is useful for **development and debugging** to see all ROP operations:

```csharp
// ⚠️ Development/debugging only - creates significant trace noise
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddFunctionalDddRopInstrumentation()      // ⚠️ Traces EVERY Result<T> operation
            .AddFunctionalDddCvoInstrumentation()      // ⚠️ Traces EVERY value object creation
            .AddConsoleExporter();  // Console output for debugging
    });
```

**Use auto-instrumentation when:**
- 🔍 Debugging complex ROP chains
- 🧪 Development/testing environments
- 📊 Analyzing performance bottlenecks
- 🐛 Troubleshooting specific issues

**Avoid in production when:**
- ❌ High-traffic applications (performance overhead)
- ❌ Cost-sensitive environments (trace volume = $$$)
- ❌ Noise-sensitive monitoring (hard to find signal)

### Production Configuration

For production, use **selective instrumentation** with sampling:

```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: builder.Configuration["OpenTelemetry:ServiceName"] ?? "MyApp",
            serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString()))
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            // ✅ Only register your own ActivitySources
            .AddSource("MyApp.OrderService")
            .AddSource("MyApp.UserService")
            .AddSource("MyApp.PaymentService")
            
            // ✅ Standard infrastructure instrumentation
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            
            // ✅ Sample to reduce overhead (10% of traces)
            .SetSampler(new TraceIdRatioBasedSampler(0.1))
            
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(
                    builder.Configuration["OpenTelemetry:Endpoint"] 
                    ?? "http://localhost:4317");
            });
    });
```

**Configuration (appsettings.json):**
```json
{
  "OpenTelemetry": {
    "ServiceName": "FunctionalDddApi",
    "Endpoint": "https://otel-collector.example.com:4317"
  }
}
```

### Manual Instrumentation Patterns

#### Pattern 1: Command Handlers

```csharp
public class CreateUserCommandHandler
{
    private static readonly ActivitySource ActivitySource = new("MyApp.UserCommands");

    public async ValueTask<Result<User>> Handle(
        CreateUserCommand command,
        CancellationToken ct)
    {
        using var activity = ActivitySource.StartActivity("CreateUser");
        activity?.SetTag("user.email", command.Email);
        
        var result = await _validator.ValidateToResultAsync(command, ct)
            .BindAsync((cmd, cancellationToken) => 
                User.CreateAsync(cmd, cancellationToken), ct)
            .TapAsync(async (user, cancellationToken) => 
                await _repository.SaveAsync(user, cancellationToken), ct);
        
        RecordResult(activity, result);
        return result;
    }

    private static void RecordResult<T>(Activity? activity, Result<T> result)
    {
        if (activity == null) return;
        
        activity.SetTag("result.isSuccess", result.IsSuccess);
        if (result.IsFailure)
        {
            activity.SetTag("result.error.type", result.Error.GetType().Name);
            activity.SetTag("result.error.code", result.Error.Code);
            activity.SetStatus(ActivityStatusCode.Error, result.Error.Detail);
        }
    }
}
```

#### Pattern 2: Domain Services

```csharp
public class PaymentService
{
    private static readonly ActivitySource ActivitySource = new("MyApp.Payments");

    public async Task<Result<Payment>> ProcessPaymentAsync(
        Order order,
        PaymentMethod method,
        CancellationToken ct)
    {
        using var activity = ActivitySource.StartActivity("ProcessPayment");
        activity?.SetTag("payment.orderId", order.Id);
        activity?.SetTag("payment.amount", order.TotalAmount);
        activity?.SetTag("payment.method", method.ToString());

        var result = await ValidatePaymentMethod(method)
            .BindAsync(async (m, cancellationToken) => 
                await _gateway.ChargeAsync(order.TotalAmount, m, cancellationToken), ct)
            .TapAsync(async (payment, cancellationToken) => 
                await _repository.SaveAsync(payment, cancellationToken), ct);

        RecordResult(activity, result);
        return result;
    }
}
```

#### Pattern 3: Complex Workflows

```csharp
public class CheckoutWorkflow
{
    private static readonly ActivitySource ActivitySource = new("MyApp.Checkout");

    public async Task<Result<Order>> ExecuteAsync(
        CheckoutCommand command,
        CancellationToken ct)
    {
        using var activity = ActivitySource.StartActivity("Checkout");
        activity?.SetTag("checkout.customerId", command.CustomerId);

        // Trace each step
        using var validateActivity = ActivitySource.StartActivity("ValidateCheckout");
        var validationResult = await ValidateCheckoutAsync(command, ct);
        RecordResult(validateActivity, validationResult);
        
        if (validationResult.IsFailure)
            return validationResult.Error;

        using var inventoryActivity = ActivitySource.StartActivity("CheckInventory");
        var inventoryResult = await CheckInventoryAsync(command.Items, ct);
        RecordResult(inventoryActivity, inventoryResult);
        
        if (inventoryResult.IsFailure)
            return inventoryResult.Error;

        using var paymentActivity = ActivitySource.StartActivity("ProcessPayment");
        var paymentResult = await ProcessPaymentAsync(command.Payment, ct);
        RecordResult(paymentActivity, paymentResult);

        // Continue workflow...
        return await CreateOrderAsync(command, ct);
    }
}
