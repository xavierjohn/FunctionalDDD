# Banking Transaction Example

This example demonstrates a complete banking system with account management, fraud detection, and transaction processing using **Railway Oriented Programming** and **Domain-Driven Design**.

## Overview

A comprehensive banking system showcasing secure transaction processing, fraud detection, daily limits, overdraft protection, and interest calculations.

## Key Components

### Value Objects
- **AccountId**: Unique identifier for bank accounts
- **TransactionId**: Unique identifier for transactions
- **CustomerId**: Unique identifier for customers
- **Money**: Represents monetary amounts with precision

### Entities
- **Transaction**: Represents individual account transactions (deposit, withdrawal, transfer, fee, interest)

### Aggregates
- **BankAccount**: Aggregate root managing account state, balance, and transaction history

### Domain Services
- **FraudDetectionService**: Analyzes transactions for suspicious patterns and validates customer identity

### Workflows
- **BankingWorkflow**: Orchestrates complex banking operations with fraud detection and validation

## Features Demonstrated

### 1. **Account Operations with Validation**
```csharp
return await account.Deposit(amount, "Salary")
    .Ensure(acc => acc.Status == AccountStatus.Active, Error.Validation("Account not active"))
    .Bind(acc => acc.Withdraw(withdrawAmount, "Rent"))
    .Match(
        ok => $"Balance: {ok.Balance}",
        err => $"Failed: {err.Detail}"
    );
```

### 2. **Fraud Detection Integration**
```csharp
return await account.ToResult()
    .EnsureAsync(
        async acc => await _fraudDetection.AnalyzeTransactionAsync(acc, amount, "withdrawal"),
        Error.Validation("Fraud check failed")
    )
    .Bind(acc => acc.Withdraw(amount))
    .CompensateAsync(
        predicate: error => error.Code == "fraud",
        func: async error => await account.Freeze("Suspicious activity")
    );
```

### 3. **Transfer with Parallel Validation**
```csharp
var fromValidation = _fraudDetection.AnalyzeTransactionAsync(fromAccount, amount, "transfer-out");
var toValidation = _fraudDetection.AnalyzeTransactionAsync(toAccount, amount, "transfer-in");

var results = await Task.WhenAll(fromValidation, toValidation);
// Process transfer only if both validations pass
```

### 4. **Daily Withdrawal Limits**
- Tracks all withdrawals for the current day
- Prevents exceeding daily limit
- Configurable per account type

### 5. **Overdraft Protection**
- Allows negative balance up to overdraft limit
- Validates before each withdrawal
- Different limits per account type

### 6. **Security Features**
- Multi-factor authentication for large transactions
- Suspicious amount detection ($5,000+ threshold)
- High-frequency trading detection (10+ transactions/hour)
- Unusual pattern detection (multiple round-number transactions)
- Account freeze on suspicious activity

## Business Rules

### Account Types
- **Checking**: Standard account with daily withdrawal limits
- **Savings**: Interest-bearing account with restricted withdrawals
- **MoneyMarket**: Higher interest rates with balance requirements

### Account Status Transitions
- **Active** -> **Frozen** (on suspicious activity or manual freeze)
- **Frozen** -> **Active** (manual unfreeze after review)
- **Active/Frozen** -> **Closed** (only if balance is zero)

### Transaction Limits
- Maximum single deposit: $10,000
- Daily withdrawal limit: Configurable per account
- Overdraft limit: Configurable per account
- Fraud threshold: $5,000 per transaction

### Fraud Detection Rules
1. **Amount Threshold**: Transactions > $5,000 require manual review
2. **High Frequency**: More than 10 transactions in 1 hour triggers alert
3. **Pattern Detection**: 3+ round-number (multiples of $1,000) transactions in 24 hours
4. **MFA Required**: Withdrawals > $1,000 require verification code

## How to Use

### Run Banking Examples
```csharp
await BankingExample.BankingExamples.RunExamplesAsync();
```

**Output**: 6 examples showing basic operations, transfers, fraud detection, daily limits, interest, and domain events

### Run from Command Line

Navigate to the project directory:
```bash
cd Examples/BankingExample
```

Run the examples:
```bash
dotnet run
```

This will execute all 6 examples:
1. **Example 1**: Basic account operations with domain events
2. **Example 2**: Transfer between accounts with event publishing
3. **Example 3**: Fraud detection preventing suspicious transactions
4. **Example 4**: Daily withdrawal limit enforcement
5. **Example 5**: Interest payment calculation
6. **Example 6**: Domain events and change tracking

### Run from Visual Studio

1. Set `BankingExample` as the startup project
2. Press F5 or click "Start Debugging"
3. Watch the console output showing each example

### Run Specific Examples

Modify `Program.cs` to run individual examples:
```csharp
using BankingExample;

// Run just one example
await BankingExamples.Example3_FraudDetection();
```

### Expected Output

You'll see detailed console output for each example showing success indicators, expected failures (fraud detection, limits), transaction logging, notification messages, and security alerts.

## Running the Examples

```csharp
await BankingExamples.RunExamplesAsync();
```

### Example 1: Basic Account Operations
Creates account, deposits salary, and makes a withdrawal. Demonstrates domain events and change tracking.

### Example 2: Transfer Between Accounts
Demonstrates secure transfer with fraud detection on both accounts and domain event publishing.

### Example 3: Fraud Detection
Shows how suspicious large transactions are blocked and require review.

### Example 4: Daily Withdrawal Limit
Attempts multiple withdrawals and enforces daily limit.

### Example 5: Interest Payment
Calculates and applies daily interest to savings account with domain events.

### Example 6: Domain Events and Change Tracking
Deep dive into domain events, `UncommittedEvents()`, `AcceptChanges()`, and `IsChanged` property.

## Error Handling Patterns

### Validation Errors
- Insufficient funds
- Daily limit exceeded
- Account not active
- Invalid amount

### Security Errors
- Fraud detection triggered
- Identity verification failed
- Account frozen

### Compensation Patterns
- Freeze account on fraud detection
- Notify security team
- Rollback batch transactions on failure
- Release resources on transfer failure

## Key Learnings

1. **Business Rule Enforcement**: All rules are explicit in the domain model
2. **Fraud Detection**: Security checks integrated into transaction flow
3. **Audit Trail**: All transactions are recorded with timestamps
4. **Type Safety**: Money value object prevents decimal arithmetic errors
5. **Parallel Operations**: Fraud detection runs in parallel for transfers
6. **Compensation**: Automatic freeze on suspicious activity

## Extensions

Consider implementing:
- **Account Statements**: Generate monthly statements
- **Standing Orders**: Recurring automatic payments
- **Multiple Currencies**: Foreign exchange support
- **Joint Accounts**: Multiple account holders
- **Transaction Categories**: Categorize spending for budgeting

## Related Examples
- [E-Commerce Example](../EcommerceExample/README.md) - Payment processing
- [Healthcare Example](../HealthcareExample/README.md) - Appointment management
- [API Integration Example](../ApiIntegrationExample/README.md) - External service calls
