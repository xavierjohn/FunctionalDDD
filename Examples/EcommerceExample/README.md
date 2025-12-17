# E-Commerce Order Processing Example

This example demonstrates a complete e-commerce order processing system using **Railway Oriented Programming** (ROP) and **Domain-Driven Design** (DDD) principles.

## Overview

The example showcases how to build a robust order processing system with proper error handling, validation, and compensation logic using the FunctionalDDD library.

## Key Components

### Value Objects
- **OrderId**: Unique identifier for orders
- **ProductId**: Unique identifier for products
- **CustomerId**: Unique identifier for customers
- **Money**: Represents monetary amounts with currency validation

### Entities
- **OrderLine**: Represents a line item in an order with product, quantity, and price

### Aggregates
- **Order**: The aggregate root managing the complete order lifecycle with status transitions

### Domain Services
- **PaymentService**: Handles payment processing with card validation
- **InventoryService**: Manages product inventory and stock reservations
- **NotificationService**: Sends email notifications to customers

### Workflows
- **OrderWorkflow**: Orchestrates the complete order processing flow with error handling and compensations

## Features Demonstrated

### 1. **Railway Oriented Programming Patterns**
```csharp
return await Order.TryCreate(customerId)
    .Bind(order => order.AddLine(productId, productName, price, quantity))
    .Bind(order => order.Submit())
    .BindAsync(order => ProcessPaymentAsync(order, paymentInfo))
    .TapAsync(order => SendConfirmationEmailAsync(order))
    .FinallyAsync(
        ok => "Order processed successfully",
        err => $"Order failed: {err.Detail}"
    );
```

### 2. **Error Handling with Compensation**
The workflow demonstrates compensation patterns for:
- **Payment failures**: Automatic retry on gateway timeouts
- **Inventory shortages**: Suggesting alternative products
- **Transaction rollback**: Releasing reserved inventory on failures

```csharp
.CompensateAsync(
    predicate: error => error is UnexpectedError,
    func: async () => await RetryPaymentAsync(order, paymentInfo)
)
```

### 3. **Domain Validation**
- Money amount validation (non-negative, currency format)
- Order line validation (quantity limits, product existence)
- Order status transitions (e.g., can only ship confirmed orders)
- Payment card validation (card number format, CVV)

### 4. **Async Operations**
All service operations are asynchronous with proper cancellation token support:
```csharp
await _paymentService.ProcessPaymentAsync(order, cardNumber, cvv, cancellationToken)
```

### 5. **Parallel Validation**
Multiple order lines are validated in parallel for performance:
```csharp
var validationTasks = items.Select(item =>
    _inventoryService.CheckAvailability(item.ProductId, item.Quantity)
);
var results = await Task.WhenAll(validationTasks);
```

## Running the Examples

```csharp
await EcommerceExamples.RunExamplesAsync();
```

### Example 1: Simple Order Creation
Creates an order with basic validation.

### Example 2: Complete Order Workflow
Demonstrates the full order processing flow:
1. Create order
2. Add items with inventory validation
3. Reserve inventory
4. Process payment
5. Confirm and send notifications

### Example 3: Payment Failure Handling
Shows how payment failures are handled with:
- Inventory rollback
- Order cancellation
- Customer notifications

### Example 4: Insufficient Inventory
Demonstrates compensation when items are out of stock.

## Business Rules Implemented

### Order Lifecycle
- **Draft** ? **Pending** ? **PaymentProcessing** ? **Confirmed** ? **Shipped** ? **Delivered**
- Orders can be cancelled only in Draft, Pending, or PaymentFailed status
- Items can only be added/removed in Draft status
- Payment can only be processed for Pending orders

### Inventory Management
- Stock is reserved when order is submitted
- Stock is released if payment fails or order is cancelled
- Availability is checked before order submission

### Payment Processing
- Card validation (format, CVV)
- Minimum payment amount validation
- Automatic retry on transient failures (timeouts)
- Transaction ID tracking

## Key Learnings

1. **Composability**: Complex workflows are built by composing simple operations
2. **Explicit Error Handling**: All error cases are explicit and typed
3. **Compensation Patterns**: Automatic rollback and retry logic
4. **Type Safety**: Value objects prevent primitive obsession
5. **Testability**: Each component can be tested independently

## Related Examples
- [Banking Transaction Example](../BankingExample/README.md)
- [Shipping Example](../ShippingExample/README.md)
- [Healthcare Appointment Example](../HealthcareExample/README.md)

## How to Use

### Run E-Commerce Examples
```csharp
await EcommerceExample.EcommerceExamples.RunExamplesAsync();
```

**Output**: 4 examples showing order creation, complete workflow, payment failure, inventory shortage

### Run from Command Line

#### Navigate to the project directory:
```bash
cd Examples/EcommerceExample
```

#### Run the examples:
```bash
dotnet run
```

This will execute all 4 examples:
1. **Example 1**: Simple order creation with validation
2. **Example 2**: Complete order workflow with payment and inventory
3. **Example 3**: Payment failure with compensation
4. **Example 4**: Insufficient inventory handling

### Run from Visual Studio

1. Set `EcommerceExample` as the startup project
2. Press F5 or click "Start Debugging"
3. Watch the console output showing each example

### Run Specific Examples

Modify `Program.cs` to run individual examples:
```csharp
using EcommerceExample;

// Run just one example
await EcommerceExamples.Example1_SimpleOrderCreation();
```

### Follow Learning Path
1. Start with [QUICKSTART.md](../QUICKSTART.md) - Choose your path
2. Read [README.md](../README.md) - Get overview
3. Pick complexity level (? to ?????)
4. Study code and run examples
5. Read pattern documentation
