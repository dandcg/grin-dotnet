using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Grin.Tests.Integration
{
    public class WalletTests
    {
        private readonly TestServer server;
        private readonly HttpClient client;

        public WalletTests()
        {
            // Given
            server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            client = server.CreateClient();


        }


        [Fact]
        public async void ReceiveCoinbase()
        {

            // When
            var content = new StringContent("{\"Coinbase\":{\"fees\":0,\"height\":1,\"key_id\":null}}", Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var result = await client.PostAsync("/v1/receive/coinbase",content );

            // Then
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);


        }


    }
}
