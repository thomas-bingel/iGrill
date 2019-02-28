using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IGrillLibrary
{
    internal class Encryption
    {
        public static byte[] Decrypt(byte[] bytes, byte[] key)
        {
            byte[] iv = new byte[16];
            AesManaged algorithm = new AesManaged();
            algorithm.IV = iv;
            algorithm.Key = key;
            algorithm.Padding = PaddingMode.None;

            byte[] ret = null;
            using (var decryptor = algorithm.CreateDecryptor())
            {
                using (MemoryStream msDecrypted = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msDecrypted, decryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(bytes, 0, bytes.Length);
                    }
                    ret = msDecrypted.ToArray();
                }
            }
            return ret;
        }

        public static byte[] Encrypt(byte[] bytes, byte[] key)
        {
            var algorithm = Aes.Create();
            algorithm.Mode = CipherMode.ECB;
            algorithm.Key = key;
            algorithm.Padding = PaddingMode.Zeros;

            byte[] ret = null;
            using (var encryptor = algorithm.CreateEncryptor())
            {
                using (MemoryStream msEncrypted = new MemoryStream())
                {
                    using (CryptoStream csDecrypted = new CryptoStream(msEncrypted, encryptor, CryptoStreamMode.Write))
                    {
                        csDecrypted.Write(bytes, 0, bytes.Length);
                    }
                    ret = msEncrypted.ToArray();
                }
            }
            return ret;

        }

    }
}
