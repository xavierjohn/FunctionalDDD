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
        .Bind((firstName, lastName, email) => SampleWebApplication.User.New(firstName, lastName, email, request.password))
        .ToOkActionResult(this);

    [HttpPost("[action]")]
    public ActionResult<User> RegisterCreated([FromBody] RegisterRequest request) =>
        FirstName.New(request.firstName)
        .Combine(LastName.New(request.lastName))
        .Combine(EmailAddress.New(request.email))
        .Bind((firstName, lastName, email) => SampleWebApplication.User.New(firstName, lastName, email, request.password))
        .Finally(result => result.IsSuccess
            ? CreatedAtAction("Get", new { name = result.Value.FirstName }, result.Value)
            : result.ToErrorActionResult(this));

    [HttpPost("[action]")]
    public ActionResult<User> RegisterCreated2([FromBody] RegisterRequest request) =>
        FirstName.New(request.firstName)
        .Combine(LastName.New(request.lastName))
        .Combine(EmailAddress.New(request.email))
        .Bind((firstName, lastName, email) => SampleWebApplication.User.New(firstName, lastName, email, request.password))
        .Finally(
            ok => CreatedAtAction("Get", new { name = ok.FirstName }, ok),
            err => err.ToErrorActionResult<User>(this));

    [HttpPost("[action]")]
    public ActionResult<User> RegisterAccepted([FromBody] RegisterRequest request) =>
        FirstName.New(request.firstName)
        .Combine(LastName.New(request.lastName))
        .Combine(EmailAddress.New(request.email))
        .Bind((firstName, lastName, email) => SampleWebApplication.User.New(firstName, lastName, email, request.password))
        .Finally(result => result.IsSuccess
            ? AcceptedAtAction("Get", new { name = result.Value.FirstName }, result.Value)
            : result.ToErrorActionResult(this));

    [HttpGet("[action]")]
    public ActionResult<string> Get(string name) => Ok($"Hello {name}!");
}
