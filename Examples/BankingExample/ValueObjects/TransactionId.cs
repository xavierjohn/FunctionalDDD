namespace BankingExample.ValueObjects;
using FunctionalDdd;
using FunctionalDdd.PrimitiveValueObjects;

public partial class TransactionId : RequiredGuid<TransactionId>
{
}
