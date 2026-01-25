namespace SampleWebApplication.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("")]
public class HomeController : ControllerBase
{
    private static readonly string[] ErrorEndpoints =
    [
        "GET /users/notfound/{id} - Returns 404 Not Found",
        "GET /users/conflict/{id} - Returns 409 Conflict",
        "GET /users/forbidden/{id} - Returns 403 Forbidden",
        "GET /users/unauthorized/{id} - Returns 401 Unauthorized",
        "GET /users/unexpected/{id} - Returns 500 Internal Server Error"
    ];

    [HttpGet]
    public ActionResult<WelcomeResponse> Welcome() => Ok(new WelcomeResponse(
        Name: "FunctionalDDD Sample Web Application",
        Version: "1.0.0",
        Description: "Demonstrates FunctionalDDD Railway Oriented Programming with MVC Controllers",
        Endpoints: new EndpointsInfo(
            Users: new UserEndpoints(
                Register: "POST /users/register - Register user with manual validation (Result.Combine)",
                RegisterCreated: "POST /users/registerCreated - Register user returning 201 Created",
                RegisterAutoValidation: "POST /users/RegisterWithAutoValidation - Register with automatic value object validation",
                Errors: ErrorEndpoints
            )
        ),
        Documentation: "See SampleApi.http for complete API examples"
    ));
}

public record WelcomeResponse(string Name, string Version, string Description, EndpointsInfo Endpoints, string Documentation);
public record EndpointsInfo(UserEndpoints Users);
public record UserEndpoints(string Register, string RegisterCreated, string RegisterAutoValidation, string[] Errors);