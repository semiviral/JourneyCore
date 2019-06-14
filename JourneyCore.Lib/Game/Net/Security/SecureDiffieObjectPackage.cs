namespace JourneyCore.Lib.Game.Net.Security
{
    public struct SecureDiffieObjectPackage
    {
        public byte[] IV { get; }
        public byte[] SecretMessage { get; }

        public SecureDiffieObjectPackage(byte[] iv, byte[] secretMessage)
        {
            IV = iv;
            SecretMessage = secretMessage;
        }
    }
}
