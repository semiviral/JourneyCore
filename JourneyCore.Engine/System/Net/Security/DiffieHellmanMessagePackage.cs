namespace JourneyCore.Lib.System.Net.Security
{
    public class DiffieHellmanMessagePackage
    {
        public DiffieHellmanMessagePackage(byte[] remotePublicKey, byte[] secretMessage)
        {
            RemotePublicKey = remotePublicKey;
            SecretMessage = secretMessage;
        }

        public byte[] RemotePublicKey { get; }
        public byte[] SecretMessage { get; }
    }
}