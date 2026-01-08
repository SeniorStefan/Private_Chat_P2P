using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Chat_P2P
{
    internal class RSAHelper
    {
        public static string Encrypt(string text, string publicKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKey);
                return Convert.ToBase64String(
                    rsa.Encrypt(Encoding.UTF8.GetBytes(text), false));
            }
        }

        public static string Decrypt(string cipher, string privateKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                return Encoding.UTF8.GetString(
                    rsa.Decrypt(Convert.FromBase64String(cipher), false));
            }
        }

        public static string DecryptPrivateKey(string enc, string password)
        {
            byte[] data = Convert.FromBase64String(enc);
            byte[] key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password));

            using (var aes = new AesCryptoServiceProvider())
            {
                byte[] iv = new byte[16];
                Buffer.BlockCopy(data, 0, iv, 0, 16);
                aes.Key = key;
                aes.IV = iv;

                byte[] encrypted = new byte[data.Length - 16];
                Buffer.BlockCopy(data, 16, encrypted, 0, encrypted.Length);

                return Encoding.UTF8.GetString(
                    aes.CreateDecryptor().TransformFinalBlock(encrypted, 0, encrypted.Length));
            }
        }
    }
}
