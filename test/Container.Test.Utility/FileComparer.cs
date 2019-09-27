using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Container.Test.Utility
{
    public class FileComparer : IEqualityComparer<FileInfo>
    {
        public bool Equals(FileInfo f1, FileInfo f2)
        {
            if (f1 == null)
            {
                throw new ArgumentNullException(nameof(f1));
            }

            if (f2 == null)
            {
                throw new ArgumentNullException(nameof(f2));
            }

            return f1.Name == f2.Name &&
                   f1.Length == f2.Length &&
                   ComputeMd5(f1).SequenceEqual(ComputeMd5(f2));
        }

        // Return a hash that reflects the comparison criteria. According to the
        // rules for IEqualityComparer<T>, if Equals is true, then the hash codes must
        // also be equal. Because equality as defined here is a simple value equality, not
        // reference identity, it is possible that two or more objects will produce the same
        // hash code.
        public int GetHashCode(FileInfo fi)
        {
            string s = $"{fi.Name}{fi.Length}{ComputeMd5(fi)}";
            return s.GetHashCode();
        }

        private static byte[] ComputeMd5(FileInfo fileInfo)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = fileInfo.OpenRead())
                {
                    return md5.ComputeHash(stream);
                }
            }
        }
    }
}
