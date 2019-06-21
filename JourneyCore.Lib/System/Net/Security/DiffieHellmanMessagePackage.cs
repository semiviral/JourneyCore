namespace JourneyCore.Lib.System.Net.Security
{
    public class DiffieHellmanMessagePackage
    {
        public byte[] RemotePublicKey { get; }
        public byte[] SecretMessage { get; }

        public DiffieHellmanMessagePackage(byte[] remotePublicKey, byte[] secretMessage)
        {
            RemotePublicKey = remotePublicKey;
            SecretMessage = secretMessage;
        }
    }
}