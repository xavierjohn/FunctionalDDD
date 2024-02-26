namespace FunctionalDdd;
using System.Collections.Generic;
using System.ComponentModel;

public interface IAggregate : IChangeTracking
{
    IReadOnlyList<IDomainEvent> UncommittedEvents();
}
