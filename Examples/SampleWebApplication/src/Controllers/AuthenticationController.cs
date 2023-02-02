namespace SampleWebApplication.Controllers;

using FunctionalDDD;
using Microsoft.AspNetCore.Mvc;
using SampleWebApplication.Model;

[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    [HttpPost("[action]")]
    public ActionResult<User> Register([FromBody] RegisterRequest request) =>
        FirstName.Create(request.firstName)
        .Combine(LastName.Create(request.lastName))
        .Combine(EmailAddress.Create(request.email))
        .Bind((firstName, lastName, email) => SampleWebApplication.User.Create(firstName, lastName, email, request.password))
        .ToOkActionResult(this);

    [HttpPost("[action]")]
    public ActionResult<User> RegisterCreated([FromBody] RegisterRequest request) =>
        FirstName.Create(request.firstName)
        .Combine(LastName.Create(request.lastName))
        .Combine(EmailAddress.Create(request.email))
        .Bind((firstName, lastName, email) => SampleWebApplication.User.Create(firstName, lastName, email, request.password))
        .Finally(result => result.IsSuccess
            ? CreatedAtAction("Get", new { name = result.Ok.FirstName }, result.Ok)
            : result.ToErrorActionResult(this));

    [HttpPost("[action]")]
    public ActionResult<User> RegisterAccepted([FromBody] RegisterRequest request) =>
        FirstName.Create(request.firstName)
        .Combine(LastName.Create(request.lastName))
        .Combine(EmailAddress.Create(request.email))
        .Bind((firstName, lastName, email) => SampleWebApplication.User.Create(firstName, lastName, email, request.password))
        .Finally(result => result.IsSuccess
            ? AcceptedAtAction("Get", new { name = result.Ok.FirstName }, result.Ok)
            : result.ToErrorActionResult(this));

    [HttpGet("[action]")]
    public ActionResult<string> Get(string name) => Ok($"Hello {name}!");
}
