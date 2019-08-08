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
        public byte[] Iv { get; set; }
        public string IvString => Convert.ToBase64String(Iv);

        public void CalculateSharedKey(byte[] remotePublicKey)
        {
            _SharedKey =
                _DiffieHellmanCng.DeriveKeyMaterial(CngKey.Import(remotePublicKey, CngKeyBlobFormat.EccPublicBlob));
        }

        public void CalculateSharedKey(byte[] remotePublicKey, byte[] iv)
        {
            Iv = iv;

            CalculateSharedKey(remotePublicKey);
        }

        public async Task<byte[]> EncryptAsync(string secretMessage)
        {
            using (Aes _aes = new AesCryptoServiceProvider
            {
                Padding = PaddingMode.PKCS7,
                Key = _SharedKey,
                IV = Iv
            })
            {
                using (MemoryStream _cipherText = new MemoryStream())
                {
                    using (CryptoStream _cryptoStream =
                        new CryptoStream(_cipherText, _aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] _plainTextMessage = Encoding.UTF8.GetBytes(secretMessage);

                        await _cryptoStream.WriteAsync(_plainTextMessage, 0, _plainTextMessage.Length);
                        await _cryptoStream.FlushAsync();
                        _cryptoStream.Close();

                        return _cipherText.ToArray();
                    }
                }
            }
        }

        public async Task<byte[]> EncryptAsync(byte[] secretMessageBytes)
        {
            using (Aes _aes = new AesCryptoServiceProvider
            {
                Padding = PaddingMode.PKCS7,
                Key = _SharedKey,
                IV = Iv
            })
            {
                using (MemoryStream _cipherText = new MemoryStream())
                {
                    using (CryptoStream _cryptoStream =
                        new CryptoStream(_cipherText, _aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        await _cryptoStream.WriteAsync(secretMessageBytes, 0, secretMessageBytes.Length);
                        await _cryptoStream.FlushAsync();
                        _cryptoStream.Close();

                        return _cipherText.ToArray();
                    }
                }
            }
        }

        public async Task<string> DecryptAsync(byte[] remotePublicKey, byte[] secretMessage)
        {
            try
            {
                using (Aes _aes = new AesCryptoServiceProvider
                {
                    Padding = PaddingMode.PKCS7,
                    Key = _DiffieHellmanCng.DeriveKeyMaterial(CngKey.Import(remotePublicKey,
                        CngKeyBlobFormat.EccPublicBlob)),
                    IV = Iv
                })
                {
                    using (MemoryStream _cipherText = new MemoryStream())
                    {
                        using (CryptoStream _cryptoStream =
                            new CryptoStream(_cipherText, _aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            await _cryptoStream.WriteAsync(secretMessage, 0, secretMessage.Length);
                            await _cryptoStream.FlushAsync();
                            _cryptoStream.Close();

                            return Encoding.UTF8.GetString(_cipherText.ToArray());
                        }
                    }
                }
            }
            catch (Exception _ex)
            {
                return _ex.Message;
            }
        }
    }
}