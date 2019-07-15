using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JourneyCore.Lib.System.Net.Security
{
    public class DiffieHellman
    {
        private readonly ECDiffieHellmanCng _DiffieHellmanCng;
        private byte[] _PublicKey;
        private byte[] _SharedKey;

        public byte[] PublicKey
        {
            get => _PublicKey;
            set
            {
                _PublicKey = value;
                PublicKeyString = Convert.ToBase64String(PublicKey);
            }
        }

        public string PublicKeyString { get; private set; }
        public byte[] IV { get; set; }
        public string IVString => Convert.ToBase64String(IV);


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

        public void CalculateSharedKey(byte[] remotePublicKey, byte[] iv)
        {
            IV = iv;

            CalculateSharedKey(remotePublicKey);
        }

        public async Task<byte[]> EncryptAsync(string secretMessage)
        {
            using (Aes aes = new AesCryptoServiceProvider
            {
                Padding = PaddingMode.PKCS7,
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
                        await cryptoStream.FlushAsync();
                        cryptoStream.Close();

                        return cipherText.ToArray();
                    }
                }
            }
        }

        public async Task<byte[]> EncryptAsync(byte[] secretMessageBytes)
        {
            using (Aes aes = new AesCryptoServiceProvider
            {
                Padding = PaddingMode.PKCS7,
                Key = _SharedKey,
                IV = IV
            })
            {
                using (MemoryStream cipherText = new MemoryStream())
                {
                    using (CryptoStream cryptoStream =
                        new CryptoStream(cipherText, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        await cryptoStream.WriteAsync(secretMessageBytes, 0, secretMessageBytes.Length);
                        await cryptoStream.FlushAsync();
                        cryptoStream.Close();

                        return cipherText.ToArray();
                    }
                }
            }
        }

        public async Task<string> DecryptAsync(byte[] remotePublicKey, byte[] secretMessage)
        {
            try
            {
                using (Aes aes = new AesCryptoServiceProvider
                {
                    Padding = PaddingMode.PKCS7,
                    Key = _DiffieHellmanCng.DeriveKeyMaterial(CngKey.Import(remotePublicKey,
                        CngKeyBlobFormat.EccPublicBlob)),
                    IV = IV
                })
                {
                    using (MemoryStream cipherText = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream =
                            new CryptoStream(cipherText, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            await cryptoStream.WriteAsync(secretMessage, 0, secretMessage.Length);
                            await cryptoStream.FlushAsync();
                            cryptoStream.Close();

                            return Encoding.UTF8.GetString(cipherText.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}