# The basics

Let us learn some basics of the library by looking at "Avoiding primitive obsession" as a scenario and building up from there.
To ensure type safety for parameters in C# code, it's important to avoid primitive obsession. Passing strings as parameters can lead to errors, such as accidentally switching the order of the first and last names. For example, the `CreatePerson` function could be called with `lastName` as the first parameter and `firstName` as the second, resulting in a person with the wrong name.

```csharp
Person CreatePerson(string firstName, string lastName)
{
    return new Person(firstName, lastName);
}

var firstName = "John";
var lastName = "Smith";
var person = CreatePerson(lastName, firstName);
```

This would result in a person with the first name of "Smith" and the last name of "John".

To avoid this problem we need type safety for the parameters. We can achieve this by creating a class for different domain types.
In this case we need `FirstName` and `LastName` classes.
In Domain Driven Design, objects have to be in a valid state at all time so we need to validate the parameters before creating an instance of the class.
Often that check is as simple as checking if the string is null or empty. To avoid, writing the same validation code over and over again, we can use the `RequiredString` class.

Let us see how we can use it:

```csharp
public partial class FirstName : RequiredString<FirstName>
{
}

public partial class LastName : RequiredString<LastName>
{
}

Person CreatePerson(FirstName firstName, LastName lastName)
{
    return new Person(firstName, lastName);
}
```

The class has to be partial so that the source code generator can add the `New` method to it.
Now let us use it:

```csharp
Result<FirstName> firstNameResult = FirstName.New("John");
```

The `New` method returns a `Result` type and based on the input it can be either `Success` or `Failure` so we need to handle the failure case.
Here is a possible solution:

```csharp
Result<FirstName> firstNameResult = FirstName.New("John");
if (firstNameResult.IsFailure)
{
    Console.WriteLine(firstNameResult.Error);
    return;
}

Result<LastName> lastNameResult = LastName.New("Smith");
if (lastNameResult.IsFailure)
{
    Console.WriteLine(lastNameResult.Error);
    return;
}

var person = CreatePerson(firstNameResult.Value, lastNameResult.Value);
```

If by mistake the developer passes the parameters in the wrong order, the compiler will catch it.

## Result{TValue} class

The Result is a generic class and can be used to hold any type of value or error.
The need to handle failure after each method call can be tedious so the `Result` class has a few extension methods to help with that.

First, let us look at the definition of the `Result` class:

[!code-csharp[](../../../RailwayOrientedProgramming/src/Result/Result{TValue}.cs#L11-L32)]

This class help chain functions on the success or error path in a concept called [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/).
If the Result is in failed state, accessing the Value property will throw an exception. Similarly, if the Result is in success state, accessing the Error property will throw an exception.

Next let us look at some of the extension methods.

## Combine extension method

We need to combine the result of `FirstName.New` and `LastName.New` to create a person. This can be achieved by using the `Combine` method.

```csharp
var result = FirstName.New("John")
    .Combine(LastName.New("Smith"));
```

The resulting result will either contain validation errors from the `FirstName` and/or `LastName` class, or a success with a tuple containing both values.

## Bind extension method

We need a method to call `CreatePerson` with the values from the `FirstName` and `LastName` classes if the result is in a success state. This can be achieved by using the `Bind` method.

```csharp
var result = FirstName.New("John")
    .Combine(LastName.New("Smith"))
    .Bind((firstName, lastName) => CreatePerson(firstName, lastName));
```

The result will either contain validation errors from the `FirstName` and/or `LastName` class, or a success with a `Person` object. It is possible `CreatePerson` can fail, in which case the `Result` will contain the error.

## Finally extension method

So far we still have a `Result` type and we need to unwrap it to get the underlying value. This can be achieved by using the `Finally` method.

```csharp
string result = FirstName.New("John")
    .Combine(LastName.New("Smith"))
    .Bind((firstName, lastName) => CreatePerson(firstName, lastName))
    .Finally(ok => "Okay: Person created", error => error.Message);
```

## Conclusion

To prevent incorrect parameter assignment, it is recommended to use strongly typed classes that are always in a valid state. Additionally, to improve code readability, consider applying the railway-oriented programming model.
