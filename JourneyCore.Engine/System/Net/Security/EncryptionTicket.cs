using System;
using System.Text;
using JourneyCore.Lib.System.Static;
using Newtonsoft.Json;

namespace JourneyCore.Lib.System.Net.Security
{
    public class EncryptionTicket
    {
        public EncryptionTicket(byte[] publicKey, byte[] iv)
        {
            PublicKey = publicKey;
            Iv = iv;
        }

        public byte[] PublicKey { get; }
        public byte[] Iv { get; }

        public string ConvertToHtmlSafeBase64()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this))).HtmlEncodeBase64();
        }

        public static EncryptionTicket ConvertFromHtmlSafeBase64(string htmlSafeBase64Ticket)
        {
            return JsonConvert.DeserializeObject<EncryptionTicket>(
                Encoding.UTF8.GetString(Convert.FromBase64String(htmlSafeBase64Ticket.HtmlDecodeBase64())));
        }
    }
}