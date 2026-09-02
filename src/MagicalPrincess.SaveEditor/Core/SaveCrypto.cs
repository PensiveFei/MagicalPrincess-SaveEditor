using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MagicalPrincess.SaveEditor.Core
{
    /// <summary>
    /// Faithful port of the game's Crypt.cs:
    /// Rijndael (AES-128) CBC with Zeros padding and a hardcoded key/IV.
    /// The game writes JSON -> Encrypt -> Base64 into the .dat/.cfg files.
    /// </summary>
    public static class SaveCrypto
    {
        private const string AesIV = "jC34fOybW3zEh0Kl";
        private const string AesKey = "giNArbHRlWBDIggF";

        private static ICryptoTransform MakeTransform(bool encrypt)
        {
#pragma warning disable SYSLIB0022
            var rj = new RijndaelManaged
#pragma warning restore SYSLIB0022
            {
                BlockSize = 128,
                KeySize = 128,
                Padding = PaddingMode.Zeros,
                Mode = CipherMode.CBC,
                Key = Encoding.UTF8.GetBytes(AesKey),
                IV = Encoding.UTF8.GetBytes(AesIV)
            };
            return encrypt ? rj.CreateEncryptor() : rj.CreateDecryptor();
        }

        public static string Encrypt(string text)
        {
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, MakeTransform(true), CryptoStreamMode.Write))
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                cs.Write(bytes, 0, bytes.Length);
                cs.FlushFinalBlock();
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cryptText)
        {
            var array = Convert.FromBase64String(cryptText.Trim());
            using var ms = new MemoryStream(array);
            using var cs = new CryptoStream(ms, MakeTransform(false), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}