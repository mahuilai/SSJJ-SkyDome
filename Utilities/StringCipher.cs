using System;
using System.Text;

namespace SkyDome.Utilities
{
    public static class TextEncoder
    {
        private static readonly byte[] _key = {
            0x5A, 0x3F, 0x8C, 0x11, 0x47, 0xD2, 0xE9, 0x7B,
            0x33, 0xAA, 0x6E, 0x19, 0xF4, 0x0D, 0xB8, 0x5C
        };

        public static string Decrypt(byte[] encrypted)
        {
            if (encrypted == null || encrypted.Length == 0) return string.Empty;
            byte[] decrypted = new byte[encrypted.Length];
            for (int i = 0; i < encrypted.Length; i++)
            {
                decrypted[i] = (byte)(encrypted[i] ^ _key[i % _key.Length] ^ (byte)(i * 7));
            }
            return Encoding.UTF8.GetString(decrypted);
        }

        public static byte[] Encrypt(string plain)
        {
            if (string.IsNullOrEmpty(plain)) return new byte[0];
            byte[] data = Encoding.UTF8.GetBytes(plain);
            byte[] encrypted = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                encrypted[i] = (byte)(data[i] ^ _key[i % _key.Length] ^ (byte)(i * 7));
            }
            return encrypted;
        }
    }
}
