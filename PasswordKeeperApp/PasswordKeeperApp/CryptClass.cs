using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace PasswordKeeperApp
{
    class CryptClass
    {
        public static string Encrypt
        (
            // New Password
            string dbPass,
            // EncKey textbox content
            string encKey
        )
        {
            // Convert encrypt key to lowercase
            encKey = encKey.ToLower();
            // Set salt value
            string saltValue = new string(encKey.Reverse().ToArray());
            // Hash algorithm
            string hashAlgorithm = "SHA256";
            // Create password iterations
            int passwordIterations = 2;
            // Vector for first text block encrypt
            string initVector = "!1A3g2D4s9K556g7";

            int keySize = 256;

            // Convert strings to byte array
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] saltValueBytes = Encoding.UTF8.GetBytes(saltValue);
            byte[] dbPassBytes = Encoding.UTF8.GetBytes(dbPass);

            // Create Hash password
            PasswordDeriveBytes password = new PasswordDeriveBytes
            (
                encKey,
                saltValueBytes,
                hashAlgorithm,
                passwordIterations
            );

            // Get pseudo-random key bytes
            byte[] keyBytes = password.GetBytes(keySize / 8);

            // Create symmetricKey obj
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;

            // Create encryptor
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor
            (
                keyBytes,
                initVectorBytes
            );

            // Crypto stream definition
            MemoryStream memoryStream = new MemoryStream();

            CryptoStream cryptoStream = new CryptoStream
            (
                memoryStream,
                encryptor,
                CryptoStreamMode.Write
            );

            // Start crypto stream
            cryptoStream.Write(dbPassBytes, 0, dbPassBytes.Length);

            // Final block encryption
            cryptoStream.FlushFinalBlock();


            byte[] cipherTextBytes = memoryStream.ToArray();

            // Close crypto and memory stream
            memoryStream.Close();
            cryptoStream.Close();

            // Convert crypto text to string
            string cipherText = Convert.ToBase64String(cipherTextBytes);

            return cipherText;
        }

        public static string Decrypt
        (
            // Password from db
            string dbPass,
            // EncKey textbox content
            string encKey
        )
        {
            // Convert encrypt key to lowercase
            encKey = encKey.ToLower();
            // Set salt value
            string saltValue = new string(encKey.Reverse().ToArray());
            // Hash algorithm
            string hashAlgorithm = "SHA256";
            // Create password iterations
            int passwordIterations = 2;
            // Vector for first text block encrypt
            string initVector = "!1A3g2D4s9K556g7";

            int keySize = 256;

            // Convert strings to byte array
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] saltValueBytes = Encoding.UTF8.GetBytes(saltValue);
            byte[] dbPassBytes = Convert.FromBase64String(dbPass);


            // Create Hash password
            PasswordDeriveBytes password = new PasswordDeriveBytes
            (
                encKey,
                saltValueBytes,
                hashAlgorithm,
                passwordIterations
            );

            // Get pseudo-random key bytes
            byte[] keyBytes = password.GetBytes(keySize / 8);

            // Create symmetricKey obj
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;

            // Create decryptor
            ICryptoTransform encryptor = symmetricKey.CreateDecryptor
            (
                keyBytes,
                initVectorBytes
            );

            // Crypto stream definition
            MemoryStream memoryStream = new MemoryStream(dbPassBytes);

            CryptoStream cryptoStream = new CryptoStream
            (
                memoryStream,
                encryptor,
                CryptoStreamMode.Read
            );
            byte[] plainTextBytes = new byte[dbPassBytes.Length];

            // Start decryption
            int decryptedByteCount = cryptoStream.Read
            (
                plainTextBytes,
                0,
                plainTextBytes.Length
            );

            // Close crypto and memory stream
            memoryStream.Close();
            cryptoStream.Close();

            // Incoding data to string
            dbPass = Encoding.UTF8.GetString
            (
                plainTextBytes,
                0,
                decryptedByteCount
            );

            return dbPass;
        }
    }
}
