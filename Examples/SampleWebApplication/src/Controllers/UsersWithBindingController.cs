namespace SampleWebApplication.Controllers;

using FunctionalDdd;
using Microsoft.AspNetCore.Mvc;
using SampleUserLibrary;

/// <summary>
/// Example controller demonstrating AUTOMATIC value object binding.
/// Compare with UsersController.cs to see the difference between manual and automatic validation.
/// </summary>
[ApiController]
[Route("[controller]")]
public class UsersWithBindingController : ControllerBase
{
    /// <summary>
    /// Request DTO using VALUE OBJECTS instead of strings.
    /// Model binding automatically validates each property before the controller action executes.
    /// </summary>
    public record CreateUserRequest(
        FirstName FirstName,      // ✅ Automatically validated via ITryCreatable
        LastName LastName,        // ✅ Automatically validated via ITryCreatable
        EmailAddress Email,       // ✅ Automatically validated via ITryCreatable
        string Password           // Regular string (validated by User.TryCreate)
    );

    /// <summary>
    /// Register user with automatic value object validation.
    /// 
    /// Compare with UsersController.Register():
    /// - BEFORE: FirstName.TryCreate(...).Combine(LastName.TryCreate(...)).Combine(...)
    /// - AFTER:  ModelState.ToResult().Bind(_ => User.TryCreate(...))
    /// 
    /// Railway pattern: Only calls User.TryCreate if model binding validation passed!
    /// </summary>
    /// <remarks>
    /// Invalid value objects return 400 Bad Request automatically with Problem Details:
    /// {
    ///   "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "FirstName": ["First Name cannot be empty."],
    ///     "Email": ["Email address is not valid."]
    ///   }
    /// }
    /// </remarks>
    [HttpPost("[action]")]
    public ActionResult<SampleUserLibrary.User> Register([FromBody] CreateUserRequest request) =>
        ModelState.ToResult()  // ✅ Check model binding validation first
            .Bind(_ => SampleUserLibrary.User.TryCreate(request.FirstName, request.LastName, request.Email, request.Password))
            .ToActionResult(this);

    /// <summary>
    /// Register user with 201 Created response.
    /// Demonstrates custom status codes with automatic binding.
    /// </summary>
    [HttpPost("[action]")]
    public ActionResult<SampleUserLibrary.User> RegisterCreated([FromBody] CreateUserRequest request) =>
        ModelState.ToResult()
            .Bind(_ => SampleUserLibrary.User.TryCreate(request.FirstName, request.LastName, request.Email, request.Password))
            .Match(
                onSuccess: ok => CreatedAtAction("Get", new { name = ok.FirstName }, ok),
                onFailure: err => err.ToActionResult<SampleUserLibrary.User>(this));

    /// <summary>
    /// Register user with 202 Accepted response.
    /// Demonstrates async processing patterns with automatic binding.
    /// </summary>
    [HttpPost("[action]")]
    public ActionResult<SampleUserLibrary.User> RegisterAccepted([FromBody] CreateUserRequest request) =>
        ModelState.ToResult()
            .Bind(_ => SampleUserLibrary.User.TryCreate(request.FirstName, request.LastName, request.Email, request.Password))
            .Match(
                onSuccess: ok => AcceptedAtAction("Get", new { name = ok.FirstName }, ok),
                onFailure: err => err.ToActionResult<SampleUserLibrary.User>(this));

    /// <summary>
    /// Get user by name.
    /// Simple endpoint for testing CreatedAtAction/AcceptedAtAction.
    /// </summary>
    [HttpGet("{name}")]
    public ActionResult<string> Get(string name) => Ok($"Hello {name}!");

    /// <summary>
    /// Delete user with automatic UserId validation.
    /// 
    /// Route parameter validation:
    /// - GET /usersWithBinding/delete/550e8400-e29b-41d4-a716-446655440000 → Success
    /// - GET /usersWithBinding/delete/invalid-guid → 400 Bad Request
    /// 
    /// Error response:
    /// {
    ///   "errors": {
    ///     "id": ["Guid should contain 32 digits with 4 dashes..."]
    ///   }
    /// }
    /// </summary>
    [HttpDelete("{id}")]
    public ActionResult<Unit> Delete(UserId id) =>
        // If we get here, UserId is already validated!
        // Manual validation: UserId.TryCreate(id).Match(...)
        // Automatic binding: Just use it!
        Result.Success()  // Simulate deletion
            .ToActionResult(this);
}
