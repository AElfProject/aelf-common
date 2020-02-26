using System;
using System.Linq;
using AElf.Types;

namespace AElf
{
    public static class HashExtensions
    {
        public static Hash ToHash(this int intValue)
        {
            return Hash.FromRawBytes(BitConverter.GetBytes(intValue));
        }

        public static Hash Xor(this Hash hash, Hash another)
        {
            return HashHelper.Xor(hash, another);
        }
        
        public static Hash ComputeWith(this Hash left, Hash right)
        {
            var res = left.Value.ToByteArray().Concat(right.Value.ToByteArray()).ToArray();
            return Hash.FromRawBytes(res);
        }
    }
}