# API Documentation for FunctionalDDD Library

Bringing the functional programming paradigm to C#.

## Functional Programming

### [Result](xref:FunctionalDDD.Results.Result`1)

The Result type used in functional programming languages to represent a success value or an error.

### [Maybe](xref:FunctionalDDD.Results.Maybe`1)

The Maybe type used in functional programming languages to represent an optional value.



## Domain Driven Design

### [Aggregate](xref:FunctionalDDD.Domain.Aggregate`1)

A DDD aggregate is a cluster of domain objects that can be treated as a single unit. An aggregate will have one of its component objects be the aggregate root.Any references from outside the aggregate should only go to the aggregate root. The root can thus ensure the integrity of the aggregate as a whole.

[For more details](https://martinfowler.com/bliki/DDD_Aggregate.html)

### [ValueObject](xref:FunctionalDDD.Domain.ValueObject)

A value object is an object that represents a descriptive aspect of the domain with no conceptual identity.
It is a small, simple object that encapsulates a concept from your problem domain.
Unlike an aggregate, a value object does not have a unique identity and is immutable.
Value objects support and enrich the ubiquitous language of your domain.
