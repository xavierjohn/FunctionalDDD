using FunctionalDDD.CommonValueObjects;
using FunctionalDDD.Core;

namespace Example;

public class ValidationExample
{
    [Fact]
    public void Test1()
    {
        var x = EmailAddress.Create("xavier@somewhere.com")
            .Combine(() => FirstName.Create("Xavier"))
            .Combine(() => LastName.Create("John"));
//            .Validate(LastName.Create(string.empty));
        
//.AndValidate(FirstName.Create(string.empty))
//.Bind((email, lastName, firstName) => CreateUserCommand.Create(firstName, lastName, email))
//.Tap(command => m.Send(command))
//.Finally(r => MapToActionResult(r));

    }
}
