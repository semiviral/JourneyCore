namespace JourneyCore.Lib.System.Net.Security
{
    public class EncryptionTicket
    {
        public byte[] RemotePublicKey { get; }
        public byte[] IV { get; }

        public EncryptionTicket(byte[] remotePublicKey, byte[] iv)
        {
            RemotePublicKey = remotePublicKey;
            IV = iv;
        }
    }
}