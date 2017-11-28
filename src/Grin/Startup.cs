using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grin.KeychainImpl;
using Grin.WalletImpl.WalletHandlers;
using Grin.WalletImpl.WalletReceiver;
using Grin.WalletImpl.WalletTypes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Grin
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {



            services.AddMvc();

            var walletConfig = WalletConfig.Default();
            walletConfig.check_node_api_http_addr = "http://localhost:13413";

            var keychain =Keychain.From_random_seed();

            services.AddSingleton(pr => walletConfig);
            services.AddSingleton(pr => keychain);

            services.AddSingleton<CoinbaseHandler>(pr=>new CoinbaseHandler(walletConfig, keychain));
            services.AddSingleton<WalletReceiver>(pr => new WalletReceiver(walletConfig, keychain));
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

        }


    }
}
