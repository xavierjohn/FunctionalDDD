# Common Value Object
This library contains common value objects used across the applications. 
At this point there are three classes:

- `EmailAddress` - Email address value object that validates the email address format.
- `RequiredString` - String value object that cannot be null or empty.
- `RequiredGuid` - Guid value object that cannot be null or default.

## Usage
`RequiredString` and `RequiredGuid` uses source code generation to generate the `TryCreate` method
and other boilerplate code. Make sure it is declared as partial class so that the source code
generator can do the rest.

Here is an example of how to use `RequiredString`:

```csharp
public partial class TrackingId : RequiredString
{
}
```

The source code generator will generate the following

```csharp
public partial class TrackingId : RequiredString, IParsable<TrackingId>
{
    protected static readonly Error CannotBeEmptyError = Error.Validation("Tracking Id cannot be empty.", "trackingId");

    private TrackingId(string value) : base(value)
    {
    }

    public static explicit operator TrackingId(string trackingId) => TryCreate(trackingId).Value;

    public static TrackingId Parse(string s, IFormatProvider? provider)
    {
        var r = TryCreate(s);
        if (r.IsFailure)
            throw new FormatException(r.Error.Message);
        return r.Value;
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out TrackingId result)
    {
        var r = TryCreate(s);
        if (r.IsFailure)
        {
            result = default;
            return false;
        }

        result = r.Value;
        return true;
    }

    public static Result<TrackingId> TryCreate(string? requiredStringOrNothing) =>
        requiredStringOrNothing
            .EnsureNotNullOrWhiteSpace(CannotBeEmptyError)
            .Map(str => new TrackingId(str));
}
```
Here is an example of how to use `RequiredGuid`:

```csharp
public partial class EmployeeId : RequiredGuid
{
}
```

The source code generator will generate the following

```csharp
public partial class EmployeeId : RequiredGuid, IParsable<EmployeeId>
{
    protected static readonly Error CannotBeEmptyError = Error.Validation("Employee Id cannot be empty.", "employeeId");

    private EmployeeId(Guid value) : base(value)
    {
    }

    public static explicit operator EmployeeId(Guid employeeId) => TryCreate(employeeId).Value;

    public static EmployeeId Parse(string s, IFormatProvider? provider)
    {
        var r = TryCreate(s);
        if (r.IsFailure)
            throw new FormatException(r.Error.Message);
        return r.Value;
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out EmployeeId result)
    {
        var r = TryCreate(s);
        if (r.IsFailure)
        {
            result = default;
            return false;
        }

        result = r.Value;
        return true;
    }

    public static EmployeeId NewUnique() => new(Guid.NewGuid());

    public static Result<EmployeeId> TryCreate(Guid? requiredGuidOrNothing) =>
        requiredGuidOrNothing
            .ToResult(CannotBeEmptyError)
            .Ensure(x => x != Guid.Empty, CannotBeEmptyError)
            .Map(guid => new EmployeeId(guid));

    public static Result<EmployeeId> TryCreate(string? stringOrNull)
    {
         Guid parsedGuid = Guid.Empty;
         return stringOrNull
            .ToResult(CannotBeEmptyError)
            .Ensure(x => Guid.TryParse(x, out parsedGuid), Error.Validation("Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)", "employeeId"))
            .Ensure(_ => parsedGuid != Guid.Empty, CannotBeEmptyError)
            .Map(guid => new EmployeeId(parsedGuid));
    }
}
```