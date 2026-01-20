namespace SampleWebApplication.Controllers;

using FunctionalDdd;
using Microsoft.AspNetCore.Mvc;
using SampleUserLibrary;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
#pragma warning disable ASP0023 // Route conflict detected between controller actions
    [HttpPost("[action]")]
    public ActionResult<User> Register([FromBody] RegisterUserRequest request) =>
        FirstName.TryCreate(request.firstName)
        .Combine(LastName.TryCreate(request.lastName))
        .Combine(EmailAddress.TryCreate(request.email))
        .Bind((firstName, lastName, email) => SampleUserLibrary.User.TryCreate(firstName, lastName, email, request.password))
        .ToActionResult(this);

    /// <summary>
    /// Register a user using automatic value object validation.
    /// </summary>
    /// <remarks>
    /// This action demonstrates the simplified pattern with automatic validation.
    /// The request DTO contains value objects directly (FirstName, LastName, EmailAddress),
    /// which are validated automatically during model binding.
    /// 
    /// With [ApiController], invalid ModelState automatically returns 400.
    /// </remarks>
    [HttpPost("[action]")]
    public ActionResult<User> RegisterWithValidation([FromBody] CreateUserWithValidationRequest request) =>
        // Value objects are already validated - just use them directly!
        SampleUserLibrary.User.TryCreate(request.fname, request.lname, request.mail, request.Password)
            .ToActionResult(this);

    [HttpPost("[action]")]
    public ActionResult<User> RegisterCreated([FromBody] RegisterUserRequest request) =>
        FirstName.TryCreate(request.firstName)
        .Combine(LastName.TryCreate(request.lastName))
        .Combine(EmailAddress.TryCreate(request.email))
        .Bind((firstName, lastName, email) => SampleUserLibrary.User.TryCreate(firstName, lastName, email, request.password))
        .Match(
            onSuccess: ok => CreatedAtAction("Get", new { name = ok.FirstName }, ok),
            onFailure: err => err.ToActionResult<User>(this));

    [HttpPost("[action]")]
    public ActionResult<User> RegisterAccepted([FromBody] RegisterUserRequest request) =>
        FirstName.TryCreate(request.firstName)
            .Combine(LastName.TryCreate(request.lastName))
            .Combine(EmailAddress.TryCreate(request.email))
            .Bind((firstName, lastName, email) => SampleUserLibrary.User.TryCreate(firstName, lastName, email, request.password))
            .Match(
                onSuccess: ok => AcceptedAtAction("Get", new { name = ok.FirstName }, ok),
                onFailure: err => err.ToActionResult<User>(this));

    [HttpGet("{name}")]
    public ActionResult<string> Get(string name) => Ok($"Hello {name}!");

    [HttpDelete("{id}")]
    public ActionResult<Unit> Delete(string id) =>
        UserId.TryCreate(id).Match(
            onSuccess: ok => NoContent(),
            onFailure: err => err.ToActionResult<Unit>(this));
}
