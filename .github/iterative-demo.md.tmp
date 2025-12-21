# ?? Iterative Development Demo - The "Wow" Factor

**Show how to build a complete application piece-by-piece using GitHub Copilot!**

This demo showcases the power of **iterative AI-assisted development** - start with a minimal scaffold, then add features one-by-one through GitHub issues.

---

## ?? **The Demo Narrative**

> "I'm going to show you something incredible. I'll create a production-ready e-commerce system, but instead of generating everything at once, I'll build it feature-by-feature - just like you would in real development. Except each feature takes 2 minutes instead of 2 days."

---

## ?? **Demo Script: 10-Minute E-Commerce Build**

### **Phase 1: Initial Scaffold** (2 minutes)

**Narrative**: "First, let's create the basic project structure."

```powershell
# Create repository
gh repo create ecommerce-demo --public
cd ecommerce-demo

# Create minimal specification
$spec = @"
project:
  name: ECommerce
  namespace: Demo.ECommerce
  description: E-commerce platform built iteratively

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

# Commit and push
git add .
git commit -m "Initial project specification"
git push

# Trigger scaffold
gh issue create `
  --label "copilot-scaffold" `
  --title "Scaffold E-Commerce Project" `
  --body "Create initial clean architecture structure"
```

**Result**: 
- ? 4-layer clean architecture
- ? Health check endpoint working
- ? All tests passing
- ? Swagger documentation
- ? Ready to extend!

**Show**: 
- Open PR that was auto-created
- Merge it
- Run `dotnet test` - all green!
- Start API, show Swagger with `/health` endpoint

---

### **Phase 2: Add Order Aggregate** (2 minutes)

**Narrative**: "Now let's add our core business entity - the Order."

```powershell
# Create feature request
$body = @"
## Feature Request

**Type**: aggregate
**Layer**: domain

## Specification

``````yaml
feature:
  type: aggregate
  layer: domain

aggregate:
  name: Order
  id: OrderId
  description: Customer order with line items
  
  properties:
    - name: CustomerId
      type: CustomerId
      required: true
      
    - name: Status
      type: OrderStatus
      enum: [Draft, Submitted, Processing, Shipped, Delivered]
      
    - name: Total
      type: Money
      
    - name: CreatedAt
      type: DateTime
  
  behaviors:
    - name: Submit
      description: Submit order for processing
      validates:
        - Status == Draft
        - Total.Amount > 0
      returns: Result<Order>
      
    - name: AddLine
      parameters:
        - productId: ProductId
        - quantity: int
        - price: Money
      validates:
        - Status == Draft
        - quantity > 0
      returns: Result<Order>
  
  dependencies:
    - OrderId: RequiredGuid
    - CustomerId: RequiredGuid
    - ProductId: RequiredGuid
    - Money: ScalarValueObject<decimal>
``````

## Acceptance Criteria

- [x] Order aggregate with validation
- [x] Railway-oriented behaviors
- [x] Comprehensive tests
"@

gh issue create `
  --label "copilot-feature" `
  --label "domain" `
  --title "Add Order aggregate with Submit behavior" `
  --body $body
```

**Result** (after 2 minutes):
- ? Order aggregate created
- ? Submit() and AddLine() behaviors
- ? Value objects (OrderId, Money)
- ? 10+ tests covering all scenarios
- ? PR ready for review

**Show**:
- PR comment appears on issue
- Browse generated code in PR
- Show tests: `git checkout feature/2-add-order; dotnet test`
- Merge PR

---

### **Phase 3: Add Create Order Command** (2 minutes)

**Narrative**: "Now let's add the application logic to create orders."

```powershell
$body = @"
## Feature Request

**Type**: command
**Layer**: application

## Specification

``````yaml
feature:
  type: command
  layer: application

command:
  name: CreateOrder
  description: Create a new customer order
  
  parameters:
    - name: customerId
      type: CustomerId
      required: true
  
  returns: Order
  
  handler:
    steps:
      - validate: CustomerId is valid
      - create: New Order aggregate
      - persist: Save to repository
      - dispatch: OrderCreatedEvent
``````

## Acceptance Criteria

- [x] CreateOrder command with validation
- [x] Command handler implementation
- [x] Repository abstraction
- [x] Tests for success and failure
"@

gh issue create `
  --label "copilot-feature" `
  --label "application" `
  --title "Add CreateOrder command" `
  --body $body
```

**Result**:
- ? CreateOrderCommand class
- ? CreateOrderCommandHandler
- ? IOrderRepository interface (in Application layer)
- ? Tests for command validation
- ? PR created

**Show**:
- Generated command and handler
- Repository abstraction
- Tests showing validation

---

### **Phase 4: Add Repository Implementation** (1 minute)

**Narrative**: "Now we need to persist orders - let's add the repository."

```powershell
$body = @"
``````yaml
feature:
  type: repository
  layer: acl

repository:
  name: OrderRepository
  interface: IOrderRepository
  aggregate: Order
  implementation: InMemory
  
  methods:
    - name: GetByIdAsync
      returns: Task<Order?>
    - name: AddAsync
      parameters:
        - order: Order
      returns: Task
    - name: UpdateAsync
      parameters:
        - order: Order
      returns: Task
``````
"@

gh issue create `
  --label "copilot-feature" `
  --label "acl" `
  --title "Add OrderRepository with in-memory storage" `
  --body $body
```

**Result**:
- ? OrderRepository class in ACL layer
- ? Implements IOrderRepository
- ? In-memory storage (Dictionary-based)
- ? Dependency injection configured
- ? Tests included

---

### **Phase 5: Add API Endpoint** (2 minutes)

**Narrative**: "Finally, let's expose this as an API."

```powershell
$body = @"
``````yaml
feature:
  type: endpoint
  layer: api

endpoint:
  controller: Orders
  version: "2024-01-15"
  
  operation:
    method: POST
    route: ""
    description: Create a new order
    
    requestBody:
      customerId: string
    
    responseType: OrderDto
    
    command: CreateOrderCommand
    
    statusCodes:
      - 201: Created
      - 400: Validation error
      - 404: Customer not found
``````
"@

gh issue create `
  --label "copilot-feature" `
  --label "api" `
  --title "Add POST /orders endpoint" `
  --body $body
```

**Result**:
- ? OrdersController created
- ? POST endpoint with Railway-Oriented Programming
- ? OrderDto for response
- ? Mapster configuration
- ? Swagger documentation updated
- ? Integration tests

**Show**:
- Open Swagger UI
- Show new `/api/orders` endpoint
- Test it live: Create an order!
- Response shows OrderDto with all properties

---

### **Phase 6: Add More Features** (1 minute quick demo)

**Narrative**: "And we can keep going - each feature is just an issue away."

```powershell
# Queue up multiple features
gh issue create --label "copilot-feature" --title "Add Customer aggregate"
gh issue create --label "copilot-feature" --title "Add Product aggregate"
gh issue create --label "copilot-feature" --title "Add GetOrderById query"
gh issue create --label "copilot-feature" --title "Add JWT authentication"
```

**Show**: Multiple PRs being created in parallel!

---

## ?? **Presentation Tips**

### **Setup Before Demo** (Important!)

1. **Pre-create the repository** with minimal scaffold already merged
2. **Have feature specifications ready** in PowerShell scripts or text files
3. **Test the workflow once** to ensure it works
4. **Have a backup video** in case of network issues
5. **Use PowerShell or Windows Terminal** with a clear, readable theme

### **During Demo**

1. **Talk while waiting** - explain what's happening:
   - "While Copilot is generating the code..."
   - "Notice how it follows Railway-Oriented Programming..."
   - "All validation uses FluentValidation..."

2. **Show the generated code**:
   - Open PRs immediately
   - Highlight key patterns
   - Point out tests

3. **Run the tests live**:
   - `dotnet test` shows all green
   - Builds confidence

4. **Use Swagger as visual proof**:
   - API actually works
   - Documentation auto-generated
   - Can test endpoints live

### **Key Messages to Emphasize**

1. **"Each feature is isolated"** - One PR per feature
2. **"Review before merge"** - Not black-box generation
3. **"Learn as you go"** - Each PR teaches patterns
4. **"Team collaboration"** - Different people can request different features
5. **"Production ready"** - Tests, documentation, best practices included

---

## ?? **Before vs After Comparison**

### **Traditional Approach**
```
Week 1: Setup project structure
Week 2: Implement Order aggregate
Week 3: Add CQRS commands/queries
Week 4: Build API layer
Week 5: Write tests
Week 6: Add documentation

Total: 6 weeks, 40 hours
```

### **With Iterative Copilot Approach**
```
Minute 0-2:   Setup project structure ?
Minute 2-4:   Implement Order aggregate ?
Minute 4-6:   Add CQRS commands/queries ?
Minute 6-8:   Build API layer ?
Minute 8-10:  Tests auto-generated ?
Minute 10:    Documentation complete ?

Total: 10 minutes
```

**Time saved**: 40 hours ? 10 minutes = **99.6% reduction!** ??

---

## ?? **Recording the Demo**

### **Suggested Flow**

1. **Intro** (30 sec)
   - "Watch me build an e-commerce system in 10 minutes"
   - Show empty repository

2. **Scaffold** (1 min)
   - Create minimal spec
   - Trigger scaffold
   - Show generated structure

3. **Add Features** (6 min)
   - Order aggregate (show aggregate pattern)
   - CreateOrder command (show CQRS)
   - Repository (show ACL)
   - API endpoint (show Railway-Oriented Programming)

4. **Test & Run** (2 min)
   - Run all tests
   - Start API
   - Demo Swagger
   - Create order via API

5. **Closing** (30 sec)
   - Show all merged PRs
   - Emphasize: "Production-ready in 10 minutes"

---

## ?? **Advanced Demo Scenarios**

### **Scenario 1: Bug Fix via Issue**

Show how to fix bugs using the same workflow:

```powershell
gh issue create `
  --label "copilot-fix" `
  --title "Fix: Order.Submit() should check for empty OrderLines" `
  --body "The Submit behavior doesn't validate that the order has items"
```

Copilot generates a PR with:
- ? Fix in Order.Submit()
- ? Test covering the bug
- ? Documentation update

### **Scenario 2: Refactoring**

```powershell
gh issue create `
  --label "copilot-refactor" `
  --title "Refactor: Extract email validation to EmailAddress value object"
```

### **Scenario 3: Performance Optimization**

```powershell
gh issue create `
  --label "copilot-optimize" `
  --title "Optimize: Add caching to GetOrderById query"
```

---

## ?? **Q&A Preparation**

**Q**: "Does this work for existing projects?"
**A**: "Yes! You can add features to any project with the agent configured."

**Q**: "What if it generates wrong code?"
**A**: "Every change is a PR - review before merging. Plus, tests catch issues."

**Q**: "Can I customize the patterns?"
**A**: "Absolutely - edit `.github/copilot-instructions.md` to change conventions."

**Q**: "How do you handle dependencies between features?"
**A**: "Reference issue numbers in 'Depends on: #123' - Copilot reads the context."

**Q**: "Is this just for demos or production-ready?"
**A**: "Production-ready! Follows all best practices, has tests, documentation."

---

## ?? **Demo Checklist**

Before presenting:

- [ ] Test repository created and scaffold merged
- [ ] Feature specifications prepared in PowerShell scripts
- [ ] GitHub CLI authenticated (`gh auth login`)
- [ ] .NET 10 SDK installed and verified
- [ ] PowerShell or Windows Terminal configured (large font, clear theme)
- [ ] Backup plan prepared (slides, video)
- [ ] Swagger UI tested
- [ ] All commands tested at least once

During demo:

- [ ] Show empty repository initially
- [ ] Create issues one at a time
- [ ] Open PRs immediately when created
- [ ] Run `dotnet test` to show passing tests
- [ ] Open Swagger UI to show working API
- [ ] Emphasize Railway-Oriented Programming
- [ ] Point out FluentValidation

After demo:

- [ ] Share repository link
- [ ] Share template repository
- [ ] Provide documentation links
- [ ] Answer questions
- [ ] Encourage trying it themselves

---

## ?? **Windows-Specific Tips**

### **PowerShell Setup**
```powershell
# Install Windows Terminal (recommended)
winget install Microsoft.WindowsTerminal

# Configure PowerShell profile for better experience
notepad $PROFILE
# Add: Set-PSReadLineOption -PredictionSource History
```

### **GitHub CLI Setup**
```powershell
# Install GitHub CLI
winget install GitHub.cli

# Authenticate
gh auth login
```

### **Useful Aliases**
```powershell
# Add to PowerShell profile
function gci { gh issue create @args }
function gpc { gh pr create @args }
function gpv { gh pr view --web }
```

---

**Ready to deliver an unforgettable demo!** ???

Remember: The key is showing **iterative development** - not a magic black box, but a collaborative AI pair programmer that helps you build features one at a time.

Each feature:
- ? Takes 2 minutes (vs 2 days)
- ? Follows best practices automatically
- ? Includes tests
- ? Gets reviewed before merge
- ? Teaches patterns as you go

**That's the real "wow" factor!** ??
