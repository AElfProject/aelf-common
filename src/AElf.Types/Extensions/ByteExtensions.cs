using System;
using System.Linq;
using System.Security.Cryptography;
using Google.Protobuf;

namespace AElf
{
    public static class ByteExtensions
    {
        public static string ToPlainBase58(this byte[] value)
        {
            return Base58CheckEncoding.EncodePlain(value);
        }

        public static string ToPlainBase58(this ByteString value)
        {
            return Base58CheckEncoding.EncodePlain(value);
        }

        public static string ToHex(this byte[] bytes, bool withPrefix = false)
        {
            var offset = withPrefix ? 2 : 0;
            var length = bytes.Length * 2 + offset;
            var c = new char[length];

            byte b;

            if (withPrefix)
            {
                c[0] = '0';
                c[1] = 'x';
            }

            for (int bx = 0, cx = offset; bx < bytes.Length; ++bx, ++cx)
            {
                b = (byte)(bytes[bx] >> 4);
                c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

                b = (byte)(bytes[bx] & 0x0F);
                c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
            }

            return new string(c);
        }

        public static int ToInt32(this byte[] bytes, bool bigEndian)
        {
            var needReverse = !bigEndian ^ BitConverter.IsLittleEndian;
            return BitConverter.ToInt32(needReverse ? bytes.Reverse().ToArray() : bytes, 0);
        }

        public static long ToInt64(this byte[] bytes, bool bigEndian)
        {
            var needReverse = !bigEndian ^ BitConverter.IsLittleEndian;
            return BitConverter.ToInt64(needReverse ? bytes.Reverse().ToArray() : bytes, 0);
        }

        /// <summary>
        ///     Calculates the hash for a byte array.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] ComputeHash(this byte[] bytes)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(bytes);
            }
        }

        public static byte[] LeftPad(this byte[] bytes, int length)
        {
            if (length <= bytes.Length)
                return bytes;

            var paddedBytes = new byte[length];
            Buffer.BlockCopy(bytes, 0, paddedBytes, length - bytes.Length, bytes.Length);
            return paddedBytes;
        }

        /// <summary>
        ///     Find subarray in the source array.
        /// </summary>
        /// <param name="array">Source array to search for needle.</param>
        /// <param name="needle">Needle we are searching for.</param>
        /// <param name="startIndex">Start index in source array.</param>
        /// <returns>Returns starting position of the needle if it was found or <b>-1</b> otherwise.</returns>
        public static int Find(this byte[] array, byte[] needle, int startIndex = 0)
        {
            var needleLen = needle.Length;
            var sourceLength = array.Length;
            int index;

            while (sourceLength >= needleLen)
            {
                // find needle's starting element
                index = Array.IndexOf(array, needle[0], startIndex, sourceLength - needleLen + 1);

                // if we did not find even the first element of the needls, then the search is failed
                if (index == -1)
                    return -1;

                int i, p;
                // check for needle
                for (i = 0, p = index; i < needleLen; i++, p++)
                    if (array[p] != needle[i])
                        break;

                if (i == needleLen)
                    // needle was found
                    return index;

                // continue to search for needle
                sourceLength -= index - startIndex + 1;
                startIndex = index + 1;
            }

            return -1;
        }
    }
}