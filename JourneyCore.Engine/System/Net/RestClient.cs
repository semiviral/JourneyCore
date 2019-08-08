using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace JourneyCore.Lib.System.Net
{
    public static class RestClient
    {
        static RestClient()
        {
            HttpClient = new HttpClient();
        }

        private static HttpClient HttpClient { get; }

        public static async Task<string> GetAsync(string requestString, bool encode = false)
        {
            HttpResponseMessage _response =
                await HttpClient.GetAsync(encode ? HttpUtility.UrlEncode(requestString) : requestString);
            _response.EnsureSuccessStatusCode();

            return await _response.Content.ReadAsStringAsync();
        }

        public static async Task<string> PostAsync(string url, HttpContent httpContent)
        {
            HttpResponseMessage _response = await HttpClient.PostAsync(url, httpContent);
            _response.EnsureSuccessStatusCode();

            return await _response.Content.ReadAsStringAsync();
        }
    }
}