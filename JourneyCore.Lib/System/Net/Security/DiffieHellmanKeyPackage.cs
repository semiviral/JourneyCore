namespace JourneyCore.Lib.System.Net.Security
{
    public class DiffieHellmanKeyPackage
    {
        public byte[] RemotePublicKey { get; }
        public byte[] IV { get; }

        public DiffieHellmanKeyPackage(byte[] remotePublicKey, byte[] iv)
        {
            RemotePublicKey = remotePublicKey;
            IV = iv;
        }
    }
}
