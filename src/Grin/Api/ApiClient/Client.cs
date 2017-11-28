using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Grin.Api
{
    public static class Client
    {
        public static async Task<HttpResponseMessage> PostAsync(string uri, object obj)
        {
            using (var c = new HttpClient())
            {
                var res = await c.PostAsync(uri, new JsonContent(obj));
                return res;
            }
        }

        public static async Task<HttpResponseMessage> GetAsync(string uri)
        {
            using (var c = new HttpClient())
            {
                var res = await c.GetAsync(uri);
                return res;
            }
        }
    }


    public class JsonContent : StringContent
    {
        public JsonContent(object obj) :
            base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
        {
        }
    }
}