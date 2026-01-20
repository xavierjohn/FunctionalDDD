namespace SampleWebApplication.Controllers;

using FunctionalDdd;
using Microsoft.AspNetCore.Mvc;
using SampleUserLibrary;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost("[action]")]
    public ActionResult<User> Register([FromBody] RegisterUserRequest request) =>
        FirstName.TryCreate(request.firstName)
        .Combine(LastName.TryCreate(request.lastName))
        .Combine(EmailAddress.TryCreate(request.email))
        .Bind((firstName, lastName, email) => SampleUserLibrary.User.TryCreate(firstName, lastName, email, request.password))
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
