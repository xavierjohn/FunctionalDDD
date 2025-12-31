namespace FunctionalDdd;

/// <summary>
/// Represents a domain event - a record of something significant that happened in the business domain.
/// Domain events capture state changes and business occurrences that domain experts care about.
/// </summary>
/// <remarks>
/// <para>
/// Domain events are a key tactical pattern in Domain-Driven Design that enable:
/// <list type="bullet">
/// <item>Loose coupling between aggregates and bounded contexts</item>
/// <item>Temporal decoupling of side effects from core business logic</item>
/// <item>Event sourcing and audit trails</item>
/// <item>Integration between microservices</item>
/// <item>Communication of state changes to external systems</item>
/// </list>
/// </para>
/// <para>
/// Best practices for domain events:
/// <list type="bullet">
/// <item>Name events in the past tense (e.g., OrderSubmitted, PaymentProcessed)</item>
/// <item>Make events immutable - use readonly properties or init-only setters</item>
/// <item>Include all relevant data needed by handlers to avoid querying</item>
/// <item>Keep events focused on domain concepts, not technical implementation</item>
/// <item>Include metadata like timestamps, user IDs, and correlation IDs</item>
/// </list>
/// </para>
/// <para>
/// Domain events vs. Integration events:
/// <list type="bullet">
/// <item><strong>Domain events</strong>: Internal to the bounded context, raised by aggregates</item>
/// <item><strong>Integration events</strong>: Published across bounded contexts, may be derived from domain events</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Define a domain event as an immutable record
/// public record OrderSubmittedEvent(
///     OrderId OrderId,
///     CustomerId CustomerId,
///     Money Total,
///     DateTime SubmittedAt
/// ) : IDomainEvent;
/// 
/// // Raise the event from an aggregate
/// public class Order : Aggregate&lt;OrderId&gt;
/// {
///     public Result&lt;Order&gt; Submit()
///     {
///         return this.ToResult()
///             .Ensure(_ => Status == OrderStatus.Draft, Error.Validation("Wrong status"))
///             .Tap(_ =>
///             {
///                 Status = OrderStatus.Submitted;
///                 SubmittedAt = DateTime.UtcNow;
///                 DomainEvents.Add(new OrderSubmittedEvent(Id, CustomerId, Total, SubmittedAt.Value));
///             });
///     }
/// }
/// 
/// // Handle the event
/// public class OrderSubmittedHandler
/// {
///     public async Task Handle(OrderSubmittedEvent evt, CancellationToken ct)
///     {
///         await _emailService.SendOrderConfirmationAsync(evt.CustomerId, evt.OrderId, ct);
///         await _inventoryService.ReserveStockAsync(evt.OrderId, ct);
///     }
/// }
/// </code>
/// </example>
public interface IDomainEvent
{
}
