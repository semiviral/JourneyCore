using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace JourneyCore.Lib.Game.Net.Security
{
    public class SecureKeyExchange
    {
        private Aes Aes { get; }
        private ECDiffieHellmanCng DiffieHellman { get; }

        public byte[] PublicKey { get; }
        public byte[] IV => Aes.IV;

        public SecureKeyExchange()
        {
            Aes = new AesCryptoServiceProvider();
            DiffieHellman = new ECDiffieHellmanCng()
            {
                KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
                HashAlgorithm = CngAlgorithm.Sha256
            };

            PublicKey = DiffieHellman.PublicKey.ToByteArray();
        }

        public byte[] Encrypt(byte[] publicKey, string secretMessage)
        {
            byte[] encryptedMessage;
            CngKey key = CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob);
            byte[] derivedKey = DiffieHellman.DeriveKeyMaterial(key);

            Aes.Key = derivedKey;

            using (var cipherText = new MemoryStream())
            {

            }
        }
    }
}
