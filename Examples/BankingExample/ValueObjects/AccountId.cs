namespace BankingExample.ValueObjects;

using Trellis;
using Trellis.PrimitiveValueObjects;

public partial class AccountId : RequiredGuid<AccountId>
{
}