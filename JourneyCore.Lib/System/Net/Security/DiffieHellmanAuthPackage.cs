namespace JourneyCore.Lib.System.Net.Security
{
    public class DiffieHellmanAuthPackage
    {
        public byte[] RemotePublicKey { get; }
        public byte[] IV { get; }

        public DiffieHellmanAuthPackage(byte[] remotePublicKey, byte[] iv)
        {
            RemotePublicKey = remotePublicKey;
            IV = iv;
        }
    }
}