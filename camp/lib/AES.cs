using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace camp.lib
{
    class AES
    {

        private readonly byte[] encryptionKey;
        private readonly byte[] initializationVector;
        private readonly string WorkDirectory;

        public AES(string WorkDirectory)
        {
            this.WorkDirectory = WorkDirectory;
            LoadOrGenerateKeyAndIV(out encryptionKey, out initializationVector);
        }


        public byte[] Encrypt(string text)
        {
            return EncryptStringToBytes_Aes(text, encryptionKey, initializationVector);
        }


        public string Decrypt(byte[] cipherText)
        {
            return DecryptStringFromBytes_Aes(cipherText, encryptionKey, initializationVector);
        }


        private void LoadOrGenerateKeyAndIV(out byte[] key, out byte[] iv)
        {
            // Load key and IV from a secure storage
            // If not available, generate new ones
            key = LoadKey();
            iv = LoadIV();

            if (key == null || iv == null)
            {
                key = GenerateRandomBytes(32); // 256 bits for AES-256
                iv = GenerateRandomBytes(16); // 128 bits for AES

                // Save the generated key and IV securely
                SaveKey(key);
                SaveIV(iv);
            }
        }


        private byte[] LoadKey()
        {
            string keyFilePath = Path.Combine(WorkDirectory, "key.dat");

            // Load key from a secure storage
            if (File.Exists(keyFilePath))
            {
                return File.ReadAllBytes(keyFilePath);
            }

            return null;
        }


        private byte[] LoadIV()
        {
            string ivFilePath = Path.Combine(WorkDirectory, "iv.dat");

            // Load IV from a secure storage
            if (File.Exists(ivFilePath))
            {
                return File.ReadAllBytes(ivFilePath);
            }

            return null;
        }


        private void SaveKey(byte[] key)
        {
            string keyFilePath = Path.Combine(WorkDirectory, "key.dat");

            // Save key to a secure storage
            File.WriteAllBytes(keyFilePath, key);

            // Set the Hidden attribute for the file
            File.SetAttributes(keyFilePath, File.GetAttributes(keyFilePath) | FileAttributes.Hidden);
        }


        private void SaveIV(byte[] iv)
        {
            string ivFilePath = Path.Combine(WorkDirectory, "iv.dat");

            // Save IV to a secure storage
            File.WriteAllBytes(ivFilePath, iv);

            // Set the Hidden attribute for the file
            File.SetAttributes(ivFilePath, File.GetAttributes(ivFilePath) | FileAttributes.Hidden);
        }


        private byte[] GenerateRandomBytes(int length)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);
                return randomBytes;
            }
        }


        private byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);
                        csEncrypt.Write(plaintextBytes, 0, plaintextBytes.Length);
                    }

                    return msEncrypt.ToArray();
                }
            }
        }


        private string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
        {
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (CryptographicException ex)
            {
                // Log or handle the exception
                Console.WriteLine($"Decryption error: {ex.Message}");
                Console.WriteLine($"Data length: {cipherText.Length}");
                throw;
            }
        }
    }
}
