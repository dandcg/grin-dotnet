using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Serilog;
using Serilog.Events;
using Xunit;

namespace Grin.Tests.Integration
{
    public class WalletTests
    {
        private readonly TestServer server;
        private readonly HttpClient client;

        public WalletTests()
        {


            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            // Given
            server = new TestServer(new WebHostBuilder().UseSerilog().UseStartup<Startup>());
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
            Console.WriteLine(await result.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);


        }


    }
}
