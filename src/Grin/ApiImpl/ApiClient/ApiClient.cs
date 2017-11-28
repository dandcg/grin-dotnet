using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Grin.ApiImpl.ApiClient
{
    public static class ApiClient
    {
        public static async Task<HttpResponseMessage> PostAsync(string uri, object obj)
        {
            using (var c = new HttpClient())
            {
                c.Timeout = TimeSpan.FromSeconds(15);
                var res = await c.PostAsync(uri, new JsonContent(obj));
                return res;
            }
        }

        public static async Task<HttpResponseMessage> GetAsync(string uri)
        {
            using (var c = new HttpClient())
            {
                c.Timeout = TimeSpan.FromSeconds(15);
                var res = await c.GetAsync(uri);
                return res;
            }
        }
    }
}