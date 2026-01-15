namespace SampleUserLibrary;

using FunctionalDdd;

/// <summary>
/// A generic name value object. Can be used for any name field.
/// </summary>
public partial class Name : RequiredString { }

/// <summary>
/// Request DTO with two Name properties to test property-name-aware validation.
/// When both fname and lname fail validation, errors should show "fname" and "lname",
/// not "name" twice.
/// </summary>
public record NameTestRequest(Name fname, Name lname);

/// <summary>
/// Response DTO for the NameTestRequest.
/// </summary>
public record NameTestResponse(string FirstName, string LastName);
