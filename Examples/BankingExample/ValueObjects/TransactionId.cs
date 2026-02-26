namespace BankingExample.ValueObjects;

using Trellis;
using Trellis.PrimitiveValueObjects;

public partial class TransactionId : RequiredGuid<TransactionId>
{
}