namespace SampleWebApplication.Controllers;

using FunctionalDDD.Asp;
using FunctionalDDD.Domain;
using FunctionalDDD.Results;
using Microsoft.AspNetCore.Mvc;
using SampleWebApplication.Model;

[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    [HttpPost("[action]")]
    public ActionResult<User> Register([FromBody] RegisterUserRequest request) =>
        FirstName.TryCreate(request.firstName)
        .Combine(LastName.TryCreate(request.lastName))
        .Combine(EmailAddress.TryCreate(request.email))
        .Bind((firstName, lastName, email) => SampleWebApplication.User.TryCreate(firstName, lastName, email, request.password))
        .ToOkActionResult(this);

    [HttpPost("[action]")]
    public ActionResult<User> RegisterCreated([FromBody] RegisterUserRequest request) =>
        FirstName.TryCreate(request.firstName)
        .Combine(LastName.TryCreate(request.lastName))
        .Combine(EmailAddress.TryCreate(request.email))
        .Bind((firstName, lastName, email) => SampleWebApplication.User.TryCreate(firstName, lastName, email, request.password))
        .Finally(result => result.IsSuccess
            ? CreatedAtAction("Get", new { name = result.Value.FirstName }, result.Value)
            : result.ToErrorActionResult(this));

    [HttpPost("[action]")]
    public ActionResult<User> RegisterCreated2([FromBody] RegisterUserRequest request) =>
        FirstName.TryCreate(request.firstName)
        .Combine(LastName.TryCreate(request.lastName))
        .Combine(EmailAddress.TryCreate(request.email))
        .Bind((firstName, lastName, email) => SampleWebApplication.User.TryCreate(firstName, lastName, email, request.password))
        .Finally(
            ok => CreatedAtAction("Get", new { name = ok.FirstName }, ok),
            err => err.ToErrorActionResult<User>(this));

    [HttpPost("[action]")]
    public ActionResult<User> RegisterAccepted([FromBody] RegisterUserRequest request) =>
        FirstName.TryCreate(request.firstName)
        .Combine(LastName.TryCreate(request.lastName))
        .Combine(EmailAddress.TryCreate(request.email))
        .Bind((firstName, lastName, email) => SampleWebApplication.User.TryCreate(firstName, lastName, email, request.password))
        .Finally(result => result.IsSuccess
            ? AcceptedAtAction("Get", new { name = result.Value.FirstName }, result.Value)
            : result.ToErrorActionResult(this));

    [HttpGet("[action]")]
    public ActionResult<string> Get(string name) => Ok($"Hello {name}!");

    [HttpDelete("[action]/{id}")]
    public ActionResult<Unit> Delete(string id) =>
        UserId.TryCreate(id).Finally(
            ok => NoContent(),
            err => err.ToErrorActionResult<Unit>(this));
}
