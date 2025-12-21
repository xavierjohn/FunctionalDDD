# 🤖 FunctionalDDD Clean Architecture Agent

**Build production-ready .NET applications iteratively using GitHub Copilot!**

[![Build](https://github.com/xavierjohn/FunctionalDDD/actions/workflows/build.yml/badge.svg)](https://github.com/xavierjohn/FunctionalDDD/actions/workflows/build.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

## ✨ **What is This?**

The FunctionalDDD Clean Architecture Agent is a specialized GitHub Copilot configuration that helps you build .NET applications **feature-by-feature** through GitHub issues. Start with a minimal scaffold, then add features iteratively - each feature generated in ~2 minutes with tests, documentation, and best practices built-in.

### **Two Approaches** 🎯

#### **🌱 Iterative (Recommended)**
Start minimal, grow feature-by-feature:

```powershell
# Step 1: Create minimal scaffold (2 min)
gh issue create --label "copilot-scaffold" --title "Create project"

# Step 2: Add features via issues (2 min each)
gh issue create --label "copilot-feature" --title "Add Order aggregate"
gh issue create --label "copilot-feature" --title "Add CreateOrder command"
gh issue create --label "copilot-feature" --title "Add POST /orders endpoint"
```

**Result**: Each issue becomes a PR with:
- ✅ Complete implementation
- ✅ Railway-Oriented Programming
- ✅ Comprehensive tests
- ✅ Documentation

**Benefits**:
- 📝 Review each feature separately
- 🎓 Learn patterns as you build
- 👥 Team can request different features
- 🔄 Easy to rollback specific features

#### **🚀 Complete (Alternative)**
Generate entire application at once:

```yaml
# .github/project-spec.yml - Define everything upfront
domain:
  aggregates: [Order, Customer, Product]
  valueObjects: [Money, Address, Email]
application:
  queries: [GetOrderById, ListOrders]
  commands: [CreateOrder, SubmitOrder]
api:
  endpoints: [orders, customers, products]
```

**Result**: Complete solution in 5 minutes

---

## 🚀 **Quick Start: Iterative Approach**

### **1. Create Repository & Minimal Scaffold**

```powershell
gh repo create my-project --public
cd my-project

# Create minimal specification
$spec = @"
project:
  name: MyProject
  namespace: MyCompany.MyProject

domain:
  aggregates:
    - name: HealthCheck
      id: HealthCheckId
  valueObjects:
    - name: HealthCheckId
      type: RequiredGuid

application:
  queries:
    - name: GetHealthCheck
      returns: HealthCheck

api:
  version: "2024-01-15"
  endpoints:
    - resource: health
      operations: [GET]
"@

New-Item -Path ".github" -ItemType Directory -Force
$spec | Out-File -FilePath ".github/project-spec.yml" -Encoding UTF8

git add .
git commit -m "Initial specification"
git push

# Trigger scaffold
gh issue create `
  --label "copilot-scaffold" `
  --title "Create project structure"
```

**Result** (2 minutes):
- ✅ 4-layer clean architecture
- ✅ Health check endpoint
- ✅ Tests passing
- ✅ Swagger documentation
- ✅ Ready to extend!

### **2. Add Your First Feature**

```powershell
$body = @"
``````yaml
feature:
  type: aggregate
  layer: domain

aggregate:
  name: Order
  id: OrderId
  properties:
    - name: Status
      type: OrderStatus
      enum: [Draft, Submitted, Shipped]
    - name: Total
      type: Money
  behaviors:
    - name: Submit
      validates:
        - Status == Draft
      returns: Result<Order>
``````
"@

gh issue create `
  --label "copilot-feature" `
  --title "Add Order aggregate" `
  --body $body
```

**Result** (2 minutes):
- ✅ Order aggregate created
- ✅ Submit() behavior with validation
- ✅ Value objects (OrderId, Money)
- ✅ 10+ tests
- ✅ PR ready for review

### **3. Keep Adding Features!**

```powershell
# Add command
gh issue create --label "copilot-feature" --title "Add CreateOrder command"

# Add query
gh issue create --label "copilot-feature" --title "Add GetOrderById query"

# Add API endpoint
gh issue create --label "copilot-feature" --title "Add POST /orders endpoint"

# Add repository
gh issue create --label "copilot-feature" --title "Add OrderRepository"
```

Each issue → PR in 2 minutes! 🚀

---

## 📚 **Documentation**

### **For Users**

- **[Iterative Demo Guide](.github/ITERATIVE_DEMO.md)** - 10-minute e-commerce build walkthrough ⭐
- **[Feature Template](.github/FEATURE_TEMPLATE.md)** - How to add features via issues
- **[Minimal Scaffold](.github/project-spec-minimal.yml)** - Starting point
- **[Complete Demo](.github/project-spec-demo.yml)** - Full e-commerce example

### **For Developers**

- **[Agent Instructions](.github/copilot-instructions.md)** - How Copilot works
- **[Scaffold Workflow](.github/workflows/copilot-scaffold.yml)** - Initial setup automation
- **[Feature Workflow](.github/workflows/copilot-feature.yml)** - Feature addition automation

---

## 🎯 **What Gets Generated**

The agent creates a complete .NET 10 solution with:

### **Domain Layer**
- ✅ Aggregates with business logic and validation
- ✅ Value objects (simple and complex)
- ✅ Domain events for event sourcing
- ✅ FluentValidation integration
- ✅ Railway-oriented `TryCreate()` methods

### **Application Layer**
- ✅ CQRS queries and commands
- ✅ Mediator handlers
- ✅ Service abstractions
- ✅ Validation at query/command level

### **Anti-Corruption Layer (ACL)**
- ✅ Repository implementations
- ✅ External service integrations
- ✅ Dependency injection setup

### **API Layer**
- ✅ Railway-oriented controllers
- ✅ API versioning (date-based folders)
- ✅ Swagger/OpenAPI documentation
- ✅ Error handling middleware
- ✅ OpenTelemetry integration

### **Tests**
- ✅ Domain tests
- ✅ Application tests
- ✅ API integration tests
- ✅ xUnit v3 + FluentAssertions

### **Configuration**
- ✅ Directory.Build.props
- ✅ Directory.Packages.props (central package management)
- ✅ global.json
- ✅ .editorconfig
- ✅ Solution file
- ✅ CI/CD pipeline

---

## 💡 **Example Projects**

### **Minimal Todo List**

```yaml
project:
  name: TodoList
  
domain:
  aggregates:
    - name: TodoItem
      behaviors:
        - name: Complete
```

**Generated**: ~20 files, 1,000+ LOC
**Time**: 2 minutes

### **E-Commerce System**

```yaml
project:
  name: ECommerce
  
domain:
  aggregates:
    - name: Order
    - name: Customer
    - name: Product
  valueObjects:
    - name: Money
    - name: Address
```

**Generated**: ~50 files, 5,000+ LOC
**Time**: 5 minutes

### **Banking System**

```yaml
domain:
  aggregates:
    - name: BankAccount
      behaviors:
        - name: Withdraw
          validates:
            - amount > 0
            - Balance >= amount
        - name: Transfer
```

**Generated**: ~60 files, 6,000+ LOC
**Time**: 5 minutes

---

## 🎨 **Key Features**

### **Railway-Oriented Programming**

All code uses Result monad pattern:

```csharp
[HttpPost]
public async ValueTask<ActionResult<OrderDto>> Create(
    [FromBody] CreateOrderRequest request,
    CancellationToken ct)
    => await CustomerId.TryCreate(request.CustomerId)
        .Bind(customerId => CreateOrderCommand.TryCreate(customerId))
        .BindAsync(command => _sender.Send(command, ct))
        .MapAsync(order => order.Adapt<OrderDto>())
        .ToActionResultAsync(this);
```

### **Automatic Validation**

FluentValidation integrated everywhere:

```csharp
public class Order : Aggregate<OrderId>
{
    public static Result<Order> TryCreate(CustomerId customerId)
    {
        var order = new Order(customerId);
        return s_validator.ValidateToResult(order);
    }
    
    private static readonly InlineValidator<Order> s_validator = new()
    {
        v => v.RuleFor(x => x.CustomerId).NotNull()
    };
}
```

### **CQRS Pattern**

Commands and queries properly separated:

```csharp
// Query
public class GetOrderByIdQuery : IQuery<Result<Order>>
{
    public static Result<GetOrderByIdQuery> TryCreate(OrderId orderId)
        => s_validator.ValidateToResult(new GetOrderByIdQuery(orderId));
}

// Command
public class CreateOrderCommand : ICommand<Result<Order>>
{
    public static Result<CreateOrderCommand> TryCreate(CustomerId customerId)
        => s_validator.ValidateToResult(new CreateOrderCommand(customerId));
}
```

### **Clean Separation**

Four layers, zero coupling:
```
Domain → Application → ACL → API
    ↓          ↓        ↓      ↓
  Tests     Tests    Tests  Tests
```

---

## 🏆 **Best Practices Built-In**

1. ✅ **Railway-Oriented Programming** - Explicit error handling
2. ✅ **Domain-Driven Design** - Rich domain models
3. ✅ **CQRS** - Commands and queries separated
4. ✅ **Value Objects** - Type safety, no primitive obsession
5. ✅ **FluentValidation** - Comprehensive validation
6. ✅ **Dependency Injection** - Proper IoC container usage
7. ✅ **Testing** - High test coverage from day one
8. ✅ **API Versioning** - Date-based versioning
9. ✅ **OpenTelemetry** - Built-in observability
10. ✅ **Swagger** - Auto-generated API documentation

---

## 🎬 **Demo Scenarios**

Perfect for presentations and workshops!

### **5-Minute Demo**
Create a complete e-commerce system from scratch:
- Show YAML specification
- Create GitHub issue
- Watch automated scaffolding
- Run tests (all green!)
- Start API and show Swagger
- **Audience reaction**: 🤯

See the [Demo Guide](.github/DEMO_GUIDE.md) for full script.

### **2-Minute Lightning Demo**
Create a todo list application:
- Minimal YAML specification
- Create issue
- Show generated code
- Run and test
- **Perfect for**: Conference lightning talks

### **Workshop Demo**
Guide attendees through creating their own projects:
- Each person creates their own specification
- Everyone scaffolds their project
- Compare and discuss generated code
- **Perfect for**: Training sessions

---

## 📊 **Comparison**

### **Traditional Development**

```
Time to scaffold: 2-3 weeks
Lines of code written: 5,000+
Files created: 50+
Consistency: Variable
Test coverage: Variable
Documentation: Often missing
Best practices: Developer dependent
```

### **With FunctionalDDD Agent**

```
Time to scaffold: 5 minutes ⚡
Lines of code written: 100 (YAML)
Files created: 50+ (automated)
Consistency: 100% ✅
Test coverage: High ✅
Documentation: Complete ✅
Best practices: Enforced ✅
```

---

## 🔧 **How It Works**

1. **User creates specification** - Simple YAML file describing the domain
2. **GitHub issue triggers workflow** - Label `copilot-scaffold` activates automation
3. **Copilot reads instructions** - From `.github/copilot-instructions.md`
4. **Code generation** - Complete solution generated following patterns
5. **PR created** - All code reviewed in pull request
6. **User merges** - Production-ready code in main branch

**Technology**:
- GitHub Copilot (AI code generation)
- GitHub Actions (automation)
- YAML (specification format)
- .NET 10 (target framework)

---

## 🚀 **Getting Started**

### **Prerequisites**

- GitHub account with Copilot enabled
- .NET 10 SDK
- Git and GitHub CLI

### **Installation**

1. **Add agent configuration to your repository**:

```powershell
# Copy these files to your repo
.github/
├── copilot-instructions.md
├── workflows/copilot-scaffold.yml
├── PROJECT_SPEC_TEMPLATE.md
└── DEMO_GUIDE.md
```

2. **Or use the template repository**:

```powershell
gh repo create my-project --template xavierjohn/FunctionalDDD
```

3. **Create your specification**:

```powershell
Copy-Item ".github/PROJECT_SPEC_TEMPLATE.md" ".github/project-spec.yml"
# Edit project-spec.yml with your requirements
```

4. **Trigger scaffolding**:

```powershell
gh issue create --label copilot-scaffold --title "Scaffold My Project"
```

---

## 📖 **Specification Format**

The YAML specification has these main sections:

### **Project**
Basic project information:
```yaml
project:
  name: MyProject
  namespace: MyCompany.MyProject
  description: What this project does
```

### **Domain**
Business logic and models:
```yaml
domain:
  aggregates:
    - name: Order
      behaviors:
        - name: Submit
  valueObjects:
    - name: OrderId
      type: RequiredGuid
```

### **Application**
Use cases and workflows:
```yaml
application:
  queries:
    - name: GetOrderById
  commands:
    - name: CreateOrder
```

### **API**
HTTP endpoints:
```yaml
api:
  version: "2024-01-15"
  endpoints:
    - resource: orders
      operations: [GET, POST]
```

See [PROJECT_SPEC_TEMPLATE.md](.github/PROJECT_SPEC_TEMPLATE.md) for complete reference.

---

## 🤝 **Contributing**

We welcome contributions! Here's how:

1. **Report issues** - Found a bug? Create an issue
2. **Suggest features** - Have an idea? Start a discussion
3. **Improve templates** - Better patterns? Submit a PR
4. **Add examples** - More scenarios? Share them

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

---

## 📄 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 **Acknowledgments**

- Built on [FunctionalDDD](https://github.com/xavierjohn/FunctionalDDD)
- Inspired by [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- Powered by GitHub Copilot
- Railway-Oriented Programming from [F# for Fun and Profit](https://fsharpforfunandprofit.com/rop/)

---

## 📞 **Support**

- 📖 [Documentation](https://github.com/xavierjohn/FunctionalDDD)
- 💬 [Discussions](https://github.com/xavierjohn/FunctionalDDD/discussions)
- 🐛 [Issues](https://github.com/xavierjohn/FunctionalDDD/issues)
- ✉️ Email: support@functionalddd.dev

---

## ⭐ **Star This Project**

If you find this helpful, please give it a star! It helps others discover it.

[![GitHub stars](https://img.shields.io/github/stars/xavierjohn/FunctionalDDD?style=social)](https://github.com/xavierjohn/FunctionalDDD/stargazers)

---

**Ready to scaffold your first project?** 🚀

```powershell
gh repo create my-awesome-project --public
cd my-awesome-project
# Copy project-spec-demo.yml to .github/project-spec.yml
gh issue create --label copilot-scaffold --title "Scaffold My Project"
# ✨ Magic happens! ✨
```

---

*Scaffold production-ready .NET applications in minutes, not weeks.*
