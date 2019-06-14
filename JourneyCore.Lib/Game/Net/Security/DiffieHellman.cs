using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JourneyCore.Lib.Game.Net.Security
{
    public class DiffieHellman
    {
        private readonly ECDiffieHellmanCng _DiffieHellmanCng;
        private byte[] _SharedKey;

        public byte[] PublicKey { get; }
        public byte[] IV { get; set; }

        public DiffieHellman()
        {
            _DiffieHellmanCng = new ECDiffieHellmanCng
            {
                KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
                HashAlgorithm = CngAlgorithm.Sha256
            };

            PublicKey = _DiffieHellmanCng.PublicKey.ToByteArray();
        }

        public DiffieHellman(byte[] remotePublicKey) : this()
        {
            CalculateSharedKey(remotePublicKey);
        }

        public DiffieHellman(byte[] publicKey, byte[] remotePublicKey)
        {
            _DiffieHellmanCng = new ECDiffieHellmanCng(CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob))
            {
                KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
                HashAlgorithm = CngAlgorithm.Sha256
            };

            PublicKey = _DiffieHellmanCng.PublicKey.ToByteArray();

            CalculateSharedKey(remotePublicKey);
        }

        public void CalculateSharedKey(byte[] remotePublicKey)
        {
            _SharedKey =
                _DiffieHellmanCng.DeriveKeyMaterial(CngKey.Import(remotePublicKey, CngKeyBlobFormat.EccPublicBlob));
        }

        public async Task<byte[]> Encrypt(string secretMessage)
        {
            using (Aes aes = new AesCryptoServiceProvider
            {
                Key = _SharedKey,
                IV = IV
            })
            {
                using (MemoryStream cipherText = new MemoryStream())
                {
                    using (CryptoStream cryptoStream =
                        new CryptoStream(cipherText, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] plainTextMessage = Encoding.UTF8.GetBytes(secretMessage);

                        await cryptoStream.WriteAsync(plainTextMessage, 0, plainTextMessage.Length);
                        cryptoStream.Close();

                        return plainTextMessage;
                    }
                }
            }
        }

        public async Task<string> Decrypt(DiffieHellmanKeyPackage keyPackage, byte[] secretMessage)
        {
            using (Aes aes = new AesCryptoServiceProvider
            {
                Key = keyPackage.RemotePublicKey,
                IV = keyPackage.IV
            })
            {
                using (MemoryStream cipherText = new MemoryStream())
                {
                    using (CryptoStream cryptoStream =
                        new CryptoStream(cipherText, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        await cryptoStream.WriteAsync(secretMessage, 0, secretMessage.Length);
                        cryptoStream.Close();

                        return Encoding.UTF8.GetString(secretMessage);
                    }
                }
            }
        }
    }
}