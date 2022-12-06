using FunctionalDDD.CommonValueObjects;
using FunctionalDDD.Core;

namespace Example;

public class ValidationExample
{
    [Fact]
    public void Test1()
    {
        var email = EmailAddress.Create("xavier@somewhere.com");
        var firstName = FirstName.Create("Xavier");
//            .Validate(LastName.Create(string.empty));
        
//.AndValidate(FirstName.Create(string.empty))
//.Bind((email, lastName, firstName) => CreateUserCommand.Create(firstName, lastName, email))
//.Tap(command => m.Send(command))
//.Finally(r => MapToActionResult(r));

    }
}
