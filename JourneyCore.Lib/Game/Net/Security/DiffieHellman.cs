using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JourneyCore.Lib.Game.Net.Security
{
    public class DiffieHellman : IDisposable
    {
        private Aes Aes { get; }
        private ECDiffieHellmanCng DiffieHellmanCng { get; }

        public byte[] PublicKey { get; }
        public byte[] IV => Aes.IV;

        public DiffieHellman()
        {
            Aes = new AesCryptoServiceProvider();
            DiffieHellmanCng = new ECDiffieHellmanCng()
            {
                KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
                HashAlgorithm = CngAlgorithm.Sha256
            };

            PublicKey = DiffieHellmanCng.PublicKey.ToByteArray();
        }

        public async Task<byte[]> Encrypt(byte[] publicKey, string secretMessage)
        {
            byte[] encryptedMessage;
            CngKey key = CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob);
            byte[] derivedKey = DiffieHellmanCng.DeriveKeyMaterial(key);

            Aes.Key = derivedKey;

            using (MemoryStream cipherText = new MemoryStream())
            {
                using (ICryptoTransform encryptor = Aes.CreateEncryptor())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(cipherText, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] cipherTextMessage = Encoding.UTF8.GetBytes(secretMessage);
                        await cryptoStream.WriteAsync(cipherTextMessage, 0, cipherTextMessage.Length);
                    }
                }

                encryptedMessage = cipherText.ToArray();
            }

            return encryptedMessage;
        }

        public async Task<string> Decrypt(byte[] publicKey, byte[] encryptedMessage, byte[] iv)
        {
            string decryptedMessage = string.Empty;
            CngKey key = CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob);
            byte[] derivedKey = DiffieHellmanCng.DeriveKeyMaterial(key);

            Aes.Key = derivedKey;
            Aes.IV = iv;

            using (MemoryStream plainText = new MemoryStream())
            {
                using (ICryptoTransform decryptor = Aes.CreateDecryptor())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(plainText, decryptor, CryptoStreamMode.Write))
                    {
                        await cryptoStream.WriteAsync(encryptedMessage, 0, encryptedMessage.Length);
                    }
                }

                decryptedMessage = Encoding.UTF8.GetString(plainText.ToArray());
            }

            return decryptedMessage;
        }

        #region IDISPOSABLE

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Aes?.Dispose();
            DiffieHellmanCng?.Dispose();
        }

        #endregion
    }
}
