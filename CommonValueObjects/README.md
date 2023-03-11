# Common Value Object
This library contains common value objects used across the applications. 
At this point there are three classes:

- `EmailAddress` - Email address value object that validates the email address format.
- `RequiredString` - String value object that cannot be null or empty.
- `RequiredGuid` - Guid value object that cannot be null or default.

## Usage
`RequiredString` and `RequiredGuid` uses source code generation to generate the `New` method
and other boilerplate code. Make sure it is declared as partial class so that the source code
generator can do the rest.

Here is an example of how to use `RequiredString`:

```csharp
public partial class TrackingId : RequiredString<TrackingId>
{
}
```

The source code generator will generate the following

```csharp
public partial class TrackingId : RequiredString<TrackingId>
{
    protected static readonly Error CannotBeEmptyError = Error.Validation("Tracking Id cannot be empty", "trackingId");

    private TrackingId(String value) : base(value)
    {
    }

    public static explicit operator TrackingId(String trackingId) => New(trackingId).Value;

    public static Result<TrackingId, Error> New(string? requiredStringOrNothing)
        requiredStringOrNothing
            .EnsureNotNullOrWhiteSpace(CannotBeEmptyError)
            .Map(str => new TrackingId(str));
}
```
