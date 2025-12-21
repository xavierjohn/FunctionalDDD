# Feature Addition Specification
# Use this template to add new features to your scaffolded project

## How to Use

1. Copy this template
2. Fill in the feature details
3. Create GitHub issue with label `copilot-feature`
4. Copilot will generate the code and create a PR

---

## Feature Templates

### ?? **Add Aggregate**

```yaml
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
      description: Submit the order for processing
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
  
  # Dependencies (auto-creates if needed)
  dependencies:
    - OrderId: RequiredGuid
    - CustomerId: RequiredGuid
    - ProductId: RequiredGuid
    - Money: ScalarValueObject<decimal>
```

**Issue Title**: `Add Order aggregate with Submit and AddLine behaviors`

---

### ?? **Add Value Object**

```yaml
feature:
  type: valueObject
  layer: domain

valueObject:
  name: EmailAddress
  type: ScalarValueObject<string>
  description: Validated email address
  
  validation:
    - rule: "!string.IsNullOrWhiteSpace(value)"
      message: "Email cannot be empty"
    - rule: "value.Contains('@')"
      message: "Email must contain @"
    - rule: "value.Length <= 100"
      message: "Email cannot exceed 100 characters"
```

**Issue Title**: `Add EmailAddress value object with validation`

---

### ?? **Add Query**

```yaml
feature:
  type: query
  layer: application

query:
  name: GetOrderById
  description: Retrieve order by ID
  
  parameters:
    - name: orderId
      type: OrderId
      required: true
  
  returns: Order
  
  # Handler implementation
  handler:
    repository: IOrderRepository
    method: GetByIdAsync
    errorIfNotFound: true
    errorMessage: "Order {orderId} not found"
```

**Issue Title**: `Add GetOrderById query with handler`

---

### ?? **Add Command**

```yaml
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
    - name: shippingAddress
      type: Address
      required: true
  
  returns: Order
  
  # Handler implementation
  handler:
    steps:
      - validate: Customer exists and is active
      - create: New Order aggregate
      - persist: Save to repository
      - dispatch: OrderCreatedEvent
```

**Issue Title**: `Add CreateOrder command with validation`

---

### ?? **Add Controller Endpoint**

```yaml
feature:
  type: endpoint
  layer: api

endpoint:
  controller: Orders
  version: "2024-01-15"
  
  operation:
    method: POST
    route: "{orderId}/submit"
    description: Submit an order for processing
    
    parameters:
      - name: orderId
        type: string
        source: route
    
    requestBody: null
    
    responseType: OrderDto
    
    # Uses existing command
    command: SubmitOrderCommand
    
    statusCodes:
      - 200: Success
      - 400: Validation error
      - 404: Order not found
```

**Issue Title**: `Add POST /orders/{id}/submit endpoint`

---

### ?? **Add Repository**

```yaml
feature:
  type: repository
  layer: acl

repository:
  name: OrderRepository
  interface: IOrderRepository
  
  aggregate: Order
  
  methods:
    - name: GetByIdAsync
      returns: Task<Order?>
      implementation: InMemory  # or CosmosDb, SqlServer, etc.
      
    - name: GetByCustomerIdAsync
      parameters:
        - customerId: CustomerId
      returns: Task<List<Order>>
      
    - name: AddAsync
      parameters:
        - order: Order
      returns: Task
      
    - name: UpdateAsync
      parameters:
        - order: Order
      returns: Task
```

**Issue Title**: `Add OrderRepository with CRUD operations`

---

### ?? **Add Tests**

```yaml
feature:
  type: tests
  layer: domain  # or application, api

tests:
  target: Order aggregate
  
  scenarios:
    - name: CreateOrder_WithValidData_Succeeds
      arrange:
        - CustomerId valid
      act:
        - Order.TryCreate(customerId)
      assert:
        - result.IsSuccess
        - result.Value.Status == Draft
    
    - name: SubmitOrder_WhenDraft_Succeeds
      arrange:
        - Valid order in Draft status
        - Order has line items
      act:
        - order.Submit()
      assert:
        - result.IsSuccess
        - result.Value.Status == Submitted
    
    - name: SubmitEmptyOrder_Fails
      arrange:
        - Order in Draft status
        - No line items
      act:
        - order.Submit()
      assert:
        - result.IsFailure
        - error contains "Cannot submit empty order"
```

**Issue Title**: `Add tests for Order aggregate`

---

### ?? **Add Middleware**

```yaml
feature:
  type: middleware
  layer: api

middleware:
  name: AuthenticationMiddleware
  description: JWT token authentication
  
  dependencies:
    - Microsoft.AspNetCore.Authentication.JwtBearer
  
  configuration:
    - appsettings key: "Authentication:Jwt:Secret"
    - appsettings key: "Authentication:Jwt:Issuer"
  
  implementation:
    - Validate JWT token
    - Extract claims
    - Set User identity
```

**Issue Title**: `Add JWT authentication middleware`

---

## ?? **Feature Request Template**

When creating a GitHub issue, use this format:

**Title**: `Add [Feature Name]` (e.g., "Add Order aggregate")

**Labels**: 
- `copilot-feature` (required)
- `domain` / `application` / `acl` / `api` (layer)
- `enhancement`

**Body**:
```markdown
## Feature Request

**Type**: [aggregate / value-object / query / command / endpoint / repository / tests / middleware]

**Layer**: [domain / application / acl / api]

**Description**: 
Brief description of what this feature does and why it's needed.

## Specification

```yaml
[Paste feature specification from template above]
```

## Acceptance Criteria

- [ ] Code generated following clean architecture
- [ ] Railway-oriented programming used
- [ ] FluentValidation for validation
- [ ] Tests included
- [ ] Documentation updated

## Related Issues

- Depends on: #123 (if applicable)
- Blocks: #456 (if applicable)
```

---

## ?? **Example: Complete Feature Addition Flow**

### **Scenario**: Add Order Management

**Issue 1**: `Add Order aggregate` (Label: `copilot-feature`, `domain`)
```yaml
feature:
  type: aggregate
aggregate:
  name: Order
  # ... full specification
```

**Issue 2**: `Add CreateOrder command` (Label: `copilot-feature`, `application`)
```yaml
feature:
  type: command
command:
  name: CreateOrder
  # ... depends on Issue 1
```

**Issue 3**: `Add POST /orders endpoint` (Label: `copilot-feature`, `api`)
```yaml
feature:
  type: endpoint
endpoint:
  controller: Orders
  # ... uses CreateOrder command
```

**Issue 4**: `Add Order tests` (Label: `copilot-feature`, `domain`)
```yaml
feature:
  type: tests
tests:
  target: Order aggregate
```

Each issue gets:
- ? Automatic code generation
- ? Pull request with changes
- ? Tests included
- ? Documentation updated

---

## ?? **Benefits of Iterative Approach**

1. **Start Small**: Minimal scaffold, add what you need
2. **Review Each Feature**: Every feature is a separate PR
3. **Learn Gradually**: Understand each layer as you build
4. **Team Collaboration**: Different team members can request different features
5. **Version Control**: Each feature has its own commit history
6. **Rollback Friendly**: Easy to revert a specific feature

---

## ?? **Recommended Feature Addition Order**

1. **Domain Layer First**:
   - Add aggregates
   - Add value objects
   - Add domain events

2. **Application Layer**:
   - Add queries for aggregates
   - Add commands for behaviors
   - Add abstractions (repositories)

3. **ACL Layer**:
   - Add repository implementations
   - Add external service integrations

4. **API Layer**:
   - Add controllers
   - Add endpoints
   - Add middleware

5. **Cross-Cutting**:
   - Add tests for each layer
   - Add documentation

---

## ?? **Resources**

- [Minimal Scaffold Spec](.github/project-spec-minimal.yml)
- [Demo Guide](.github/DEMO_GUIDE.md)
- [Agent Instructions](.github/copilot-instructions.md)

---

**Start with minimal scaffold, grow with features!** ??????
