namespace SampleWebApplication.Controllers;

using FunctionalDDD;
using Microsoft.AspNetCore.Mvc;
using SampleWebApplication.Model;

public class AuthenticationController : ApiControllerBase
{
    [HttpPost("[action]")]
    public ActionResult<User> Register(RegisterRequest request) =>
        FirstName.Create(request.FirstName)
        .Combine(LastName.Create(request.LastName))
        .Combine(EmailAddress.Create(request.Email))
        .Bind((firstName, lastName, email) => SampleWebApplication.User.Create(firstName, lastName, email, request.Password))
        .Finally(result => MapToActionResult(result));
}
