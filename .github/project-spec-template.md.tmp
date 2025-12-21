# FunctionalDDD Clean Architecture - Project Specification

This file defines the structure of your .NET application. Fill out this template and create a GitHub issue with the label `copilot-scaffold` to have GitHub Copilot generate your entire clean architecture project.

## Project Specification Template

```yaml
project:
  # Project name (PascalCase, no spaces)
  name: OrderManagement
  
  # Root namespace for the project
  namespace: Contoso.OrderManagement
  
  # Brief description of the project
  description: Order management system with inventory tracking and customer management
  
  # Author information
  author:
    name: Your Name
    company: Your Company
    email: you@example.com

# Domain Layer - Core business logic
domain:
  # Aggregates (main business entities with identity and lifecycle)
  aggregates:
    - name: Order
      description: Customer order aggregate
      id: OrderId  # Strongly-typed ID
      properties:
        - name: CustomerId
          type: CustomerId
          required: true
          description: Customer who placed the order
          
        - name: OrderLines
          type: List<OrderLine>
          description: Items in the order
          
        - name: Status
          type: OrderStatus
          enum: [Draft, Submitted, Processing, Shipped, Delivered, Cancelled]
          description: Current order status
          
        - name: Total
          type: Money
          description: Order total amount
          
        - name: CreatedAt
          type: DateTime
          description: When the order was created
          
        - name: SubmittedAt
          type: DateTime?
          description: When the order was submitted
      
      # Business behaviors/methods
      behaviors:
        - name: Submit
          description: Submit the order for processing
          validates:
            - Status == Draft
            - OrderLines.Count > 0
            - Total.Amount > 0
          returns: Result<Order>
          
        - name: AddLine
          description: Add item to the order
          parameters:
            - productId: ProductId
            - quantity: int
            - price: Money
          validates:
            - Status == Draft
            - quantity > 0
          returns: Result<Order>
          
        - name: RemoveLine
          description: Remove item from order
          parameters:
            - productId: ProductId
          validates:
            - Status == Draft
          returns: Result<Order>
          
        - name: Ship
          description: Mark order as shipped
          validates:
            - Status == Submitted
          returns: Result<Order>
          
        - name: Cancel
          description: Cancel the order
          parameters:
            - reason: string
          validates:
            - Status in [Draft, Submitted]
          returns: Result<Order>
    
    - name: Customer
      description: Customer aggregate
      id: CustomerId
      properties:
        - name: Name
          type: string
          required: true
        - name: Email
          type: EmailAddress
          required: true
        - name: Phone
          type: PhoneNumber
        - name: ShippingAddress
          type: Address
        - name: BillingAddress
          type: Address
        - name: CreatedAt
          type: DateTime
        - name: IsActive
          type: bool
      behaviors:
        - name: UpdateEmail
          parameters:
            - email: EmailAddress
          returns: Result<Customer>
        - name: Deactivate
          returns: Result<Customer>
    
    - name: Product
      description: Product catalog item
      id: ProductId
      properties:
        - name: Name
          type: string
          required: true
        - name: Description
          type: string
        - name: Price
          type: Money
          required: true
        - name: Stock
          type: int
        - name: IsAvailable
          type: bool
      behaviors:
        - name: UpdatePrice
          parameters:
            - newPrice: Money
          validates:
            - newPrice.Amount > 0
          returns: Result<Product>
        - name: AdjustStock
          parameters:
            - quantity: int
          returns: Result<Product>
  
  # Value Objects (immutable objects identified by their value)
  valueObjects:
    # Simple value objects (use source generator)
    - name: OrderId
      type: RequiredGuid
      
    - name: CustomerId
      type: RequiredGuid
      
    - name: ProductId
      type: RequiredGuid
      
    - name: CustomerName
      type: RequiredString
    
    # Complex value objects with custom validation
    - name: EmailAddress
      type: ScalarValueObject<string>
      validation:
        - rule: "!string.IsNullOrWhiteSpace(value)"
          message: "Email cannot be empty"
        - rule: "value.Contains('@')"
          message: "Email must contain @"
        - rule: "value.Length <= 100"
          message: "Email cannot exceed 100 characters"
    
    - name: PhoneNumber
      type: ScalarValueObject<string>
      validation:
        - rule: "!string.IsNullOrWhiteSpace(value)"
          message: "Phone number cannot be empty"
        - rule: "Regex.IsMatch(value, @'^\+?[1-9]\d{1,14}$')"
          message: "Invalid phone number format"
    
    - name: Money
      type: ValueObject
      properties:
        - name: Amount
          type: decimal
        - name: Currency
          type: string
      validation:
        - rule: "Amount >= 0"
          message: "Amount cannot be negative"
        - rule: "Currency.Length == 3"
          message: "Currency must be 3-letter ISO code"
      methods:
        - name: Add
          parameters:
            - other: Money
          returns: Result<Money>
        - name: Subtract
          parameters:
            - other: Money
          returns: Result<Money>
    
    - name: Address
      type: ValueObject
      properties:
        - name: Street
          type: string
        - name: City
          type: string
        - name: State
          type: string
        - name: PostalCode
          type: string
        - name: Country
          type: string
      validation:
        - rule: "!string.IsNullOrWhiteSpace(Street)"
          message: "Street is required"
        - rule: "!string.IsNullOrWhiteSpace(City)"
          message: "City is required"

  # Domain Events
  events:
    - name: OrderCreatedEvent
      properties:
        - orderId: OrderId
        - customerId: CustomerId
        - createdAt: DateTime
    
    - name: OrderSubmittedEvent
      properties:
        - orderId: OrderId
        - total: Money
        - submittedAt: DateTime
    
    - name: OrderShippedEvent
      properties:
        - orderId: OrderId
        - shippedAt: DateTime

# Application Layer - Use cases and business workflows
application:
  # Queries (read operations)
  queries:
    - name: GetOrderById
      description: Retrieve order by ID
      parameters:
        - orderId: OrderId
      returns: Order
      
    - name: ListOrders
      description: List orders with filtering
      parameters:
        - customerId: CustomerId?
        - status: OrderStatus?
        - pageSize: int
        - pageNumber: int
      returns: List<Order>
      
    - name: GetCustomerById
      description: Retrieve customer by ID
      parameters:
        - customerId: CustomerId
      returns: Customer
      
    - name: SearchProducts
      description: Search products by name
      parameters:
        - searchTerm: string
        - pageSize: int
        - pageNumber: int
      returns: List<Product>
  
  # Commands (write operations)
  commands:
    - name: CreateOrder
      description: Create a new order
      parameters:
        - customerId: CustomerId
      returns: Order
      
    - name: AddOrderLine
      description: Add item to order
      parameters:
        - orderId: OrderId
        - productId: ProductId
        - quantity: int
      returns: Order
      
    - name: SubmitOrder
      description: Submit order for processing
      parameters:
        - orderId: OrderId
      returns: Order
      
    - name: RegisterCustomer
      description: Register a new customer
      parameters:
        - name: string
        - email: string
        - phone: string?
      returns: Customer
      
    - name: UpdateProductPrice
      description: Update product price
      parameters:
        - productId: ProductId
        - newPrice: decimal
        - currency: string
      returns: Product
  
  # Service Abstractions (interfaces for external dependencies)
  abstractions:
    - name: IOrderRepository
      methods:
        - name: GetByIdAsync
          parameters:
            - orderId: OrderId
            - ct: CancellationToken
          returns: Task<Order?>
        - name: AddAsync
          parameters:
            - order: Order
            - ct: CancellationToken
          returns: Task
        - name: UpdateAsync
          parameters:
            - order: Order
            - ct: CancellationToken
          returns: Task
    
    - name: IEmailService
      methods:
        - name: SendOrderConfirmationAsync
          parameters:
            - order: Order
            - ct: CancellationToken
          returns: Task<Result<Unit>>
    
    - name: IInventoryService
      methods:
        - name: ReserveStockAsync
          parameters:
            - productId: ProductId
            - quantity: int
            - ct: CancellationToken
          returns: Task<Result<Unit>>
        - name: ReleaseStockAsync
          parameters:
            - productId: ProductId
            - quantity: int
            - ct: CancellationToken
          returns: Task<Result<Unit>>

# API Layer - HTTP endpoints and versioning
api:
  # API version (use date format YYYY-MM-DD)
  version: "2024-01-15"
  
  # Observability settings
  observability:
    openTelemetry: true
    serviceName: OrderManagementApi
    includeTracing: true
    includeMetrics: true
  
  # REST endpoints
  endpoints:
    - resource: orders
      description: Order management endpoints
      operations:
        - method: GET
          route: ""
          description: List all orders
          queryParameters:
            - customerId: string?
            - status: string?
            - pageSize: int
            - pageNumber: int
          
        - method: GET
          route: "{orderId}"
          description: Get order by ID
          
        - method: POST
          route: ""
          description: Create new order
          requestBody: CreateOrderRequest
          
        - method: POST
          route: "{orderId}/lines"
          description: Add item to order
          requestBody: AddOrderLineRequest
          
        - method: POST
          route: "{orderId}/submit"
          description: Submit order
          
        - method: DELETE
          route: "{orderId}"
          description: Cancel order
          requestBody: CancelOrderRequest
    
    - resource: customers
      description: Customer management
      operations:
        - method: GET
          route: "{customerId}"
          description: Get customer by ID
          
        - method: POST
          route: ""
          description: Register new customer
          requestBody: RegisterCustomerRequest
          
        - method: PUT
          route: "{customerId}/email"
          description: Update customer email
          requestBody: UpdateEmailRequest
    
    - resource: products
      description: Product catalog
      operations:
        - method: GET
          route: ""
          description: Search products
          queryParameters:
            - searchTerm: string
            - pageSize: int
            - pageNumber: int
          
        - method: GET
          route: "{productId}"
          description: Get product by ID
          
        - method: PUT
          route: "{productId}/price"
          description: Update product price
          requestBody: UpdatePriceRequest
  
  # DTOs for API responses
  dtos:
    - name: OrderDto
      properties:
        - orderId: string
        - customerId: string
        - status: string
        - total: decimal
        - currency: string
        - lines: List<OrderLineDto>
        - createdAt: DateTime
    
    - name: OrderLineDto
      properties:
        - productId: string
        - productName: string
        - quantity: int
        - price: decimal
        - lineTotal: decimal

# Infrastructure/ACL Layer - External dependencies
infrastructure:
  # Database
  database:
    type: CosmosDB  # or SqlServer, PostgreSQL, etc.
    connectionStringKey: "CosmosDb:ConnectionString"
  
  # External services
  externalServices:
    - name: EmailService
      type: SendGrid
      configSection: "SendGrid"
    
    - name: InventoryService
      type: HttpClient
      baseUrl: "https://inventory-api.example.com"

# CI/CD Configuration
cicd:
  platform: GitHub Actions
  buildSteps:
    - restore
    - build
    - test
    - publish
  deployments:
    - environment: dev
      autoTrigger: true
    - environment: staging
      requiresApproval: false
    - environment: production
      requiresApproval: true
```

## How to Use

1. **Copy this template** to `.github/project-spec.yml` in your repository
2. **Fill out the sections** with your project details
3. **Create a GitHub Issue** with:
   - Title: "Scaffold [ProjectName] Clean Architecture Project"
   - Label: `copilot-scaffold`
   - Body: Reference the specification file
4. **Assign to @copilot** and wait for the magic! ?

GitHub Copilot will:
- ? Generate complete clean architecture solution
- ? Create all layers (Domain, Application, ACL, API)
- ? Generate aggregates, value objects, and domain events
- ? Create CQRS queries and commands with handlers
- ? Generate railway-oriented controllers
- ? Set up dependency injection
- ? Add comprehensive tests
- ? Configure OpenTelemetry and observability
- ? Create Swagger/OpenAPI documentation
- ? Set up CI/CD pipeline
- ? Generate README with architecture documentation

## Simplified Examples

### Minimal E-Commerce Project

```yaml
project:
  name: SimpleStore
  namespace: MyCompany.SimpleStore
  description: Basic e-commerce store

domain:
  aggregates:
    - name: Order
      id: OrderId
      properties:
        - name: Total
          type: Money
      behaviors:
        - name: Submit
          validates: Total.Amount > 0
  
  valueObjects:
    - name: OrderId
      type: RequiredGuid
    - name: Money
      type: ScalarValueObject<decimal>

application:
  queries:
    - name: GetOrderById
      parameters:
        - orderId: OrderId
      returns: Order
  
  commands:
    - name: CreateOrder
      returns: Order

api:
  version: "2024-01-15"
  endpoints:
    - resource: orders
      operations: [GET, POST]
```

### Todo List Project

```yaml
project:
  name: TodoList
  namespace: MyCompany.TodoList
  description: Simple todo list application

domain:
  aggregates:
    - name: TodoItem
      id: TodoId
      properties:
        - name: Title
          type: string
          required: true
        - name: IsCompleted
          type: bool
      behaviors:
        - name: Complete
          validates: !IsCompleted
  
  valueObjects:
    - name: TodoId
      type: RequiredGuid

application:
  queries:
    - name: ListTodos
      returns: List<TodoItem>
  
  commands:
    - name: CreateTodo
      parameters:
        - title: string
      returns: TodoItem
    - name: CompleteTodo
      parameters:
        - todoId: TodoId
      returns: TodoItem

api:
  version: "2024-01-15"
  endpoints:
    - resource: todos
      operations: [GET, POST, PUT, DELETE]
```

## Next Steps

After scaffolding:
1. Review generated code
2. Run tests: `dotnet test`
3. Start API: `cd Api/src && dotnet run`
4. Open Swagger: `https://localhost:5001`
5. Customize business logic as needed

## Resources

- [FunctionalDDD Documentation](https://github.com/xavierjohn/FunctionalDDD)
- [Clean Architecture Guide](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Railway-Oriented Programming](https://fsharpforfunandprofit.com/rop/)
