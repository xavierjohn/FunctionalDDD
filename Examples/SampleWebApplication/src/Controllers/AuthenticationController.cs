namespace SampleWebApplication.Controllers;

using FunctionalDDD;
using Microsoft.AspNetCore.Mvc;
using SampleWebApplication.Model;

[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    [HttpPost("[action]")]
    public ActionResult<User> Register([FromBody] RegisterRequest request) =>
        FirstName.New(request.firstName)
        .Combine(LastName.New(request.lastName))
        .Combine(EmailAddress.New(request.email))
        .OnOk((firstName, lastName, email) => SampleWebApplication.User.New(firstName, lastName, email, request.password))
        .ToOkActionResult(this);

    [HttpPost("[action]")]
    public ActionResult<User> RegisterCreated([FromBody] RegisterRequest request) =>
        FirstName.New(request.firstName)
        .Combine(LastName.New(request.lastName))
        .Combine(EmailAddress.New(request.email))
        .OnOk((firstName, lastName, email) => SampleWebApplication.User.New(firstName, lastName, email, request.password))
        .Finally(result => result.IsOk
            ? CreatedAtAction("Get", new { name = result.Ok.FirstName }, result.Ok)
            : result.ToErrorActionResult(this));

    [HttpPost("[action]")]
    public ActionResult<User> RegisterCreated2([FromBody] RegisterRequest request) =>
        FirstName.New(request.firstName)
        .Combine(LastName.New(request.lastName))
        .Combine(EmailAddress.New(request.email))
        .OnOk((firstName, lastName, email) => SampleWebApplication.User.New(firstName, lastName, email, request.password))
        .Finally(
            ok => CreatedAtAction("Get", new { name = ok.FirstName }, ok),
            err => err.ToErrorActionResult<User>(this));

    [HttpPost("[action]")]
    public ActionResult<User> RegisterAccepted([FromBody] RegisterRequest request) =>
        FirstName.New(request.firstName)
        .Combine(LastName.New(request.lastName))
        .Combine(EmailAddress.New(request.email))
        .OnOk((firstName, lastName, email) => SampleWebApplication.User.New(firstName, lastName, email, request.password))
        .Finally(result => result.IsOk
            ? AcceptedAtAction("Get", new { name = result.Ok.FirstName }, result.Ok)
            : result.ToErrorActionResult(this));

    [HttpGet("[action]")]
    public ActionResult<string> Get(string name) => Ok($"Hello {name}!");
}
