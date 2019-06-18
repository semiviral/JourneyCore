using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace JourneyCore.Lib.System.Net
{
    public static class RestClient
    {
        private static HttpClient HttpClient { get; }

        static RestClient()
        {
            HttpClient = new HttpClient();
        }

        public static async Task<string> GetAsync(string requestString, bool encode = false)
        {
            HttpResponseMessage response = await HttpClient.GetAsync(encode ? HttpUtility.UrlEncode(requestString) : requestString);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> PostAsync(string url, HttpContent httpContent)
        {
            HttpResponseMessage response = await HttpClient.PostAsync(url, httpContent);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}