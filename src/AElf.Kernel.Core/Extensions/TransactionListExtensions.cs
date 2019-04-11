using System.Linq;
using AElf.Types;

namespace AElf.Kernel
{
    public static class TransactionListExtensions
    {
        public static string ToDebugString(this TransactionList transactionList)
        {
            return $"[{string.Join(", ", transactionList.Transactions.Select(t => t.GetHashBytes().ToHex()))}]";
        }
    }
}