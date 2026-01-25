namespace BankingExample.ValueObjects;

using FunctionalDdd;
using FunctionalDdd.PrimitiveValueObjects;

public partial class AccountId : RequiredGuid<AccountId>
{
}
