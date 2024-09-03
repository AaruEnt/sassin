using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class Encryption
{
    public static string EncryptString(byte[] key, string text)
    {
        byte[] iv = new byte[16];
        byte[] array;

        using (Aes aes = Aes.Create()) // AES = advanced encryption system
        {
            aes.Key = key; // secret key for algorithm
            aes.IV = iv; // initialization vector

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV); // creates a symmetric encryptor object with the specified Key property and initialization vector

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                    {
                        streamWriter.Write(text);
                    }

                    array = memoryStream.ToArray();
                }
            }
        }
        return Convert.ToBase64String(array);
    }

    public static string DecryptString(byte[] key, string cipherText)
    {
        byte[] iv = new byte[16];
        byte[] buffer = Convert.FromBase64String(cipherText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }

    public static byte[] sha256_hash(String value)
    {
        byte[] result;
        using (SHA256 hash = SHA256Managed.Create())
        {
            Encoding enc = Encoding.UTF8;
            result = hash.ComputeHash(enc.GetBytes(value));
        }
        return result;
    }
}
