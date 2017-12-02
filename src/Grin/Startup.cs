using Grin.KeychainImpl;
using Grin.WalletImpl.WalletHandlers;
using Grin.WalletImpl.WalletReceiver;
using Grin.WalletImpl.WalletTypes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            var directory = System.IO.Path.GetDirectoryName(location);


            var walletConfig = WalletConfig.Default();
            walletConfig.CheckNodeApiHttpAddr = "http://localhost:13413";
            walletConfig.DataFileDir = directory;

            var walletSeed = WalletSeed.from_file(walletConfig);
            var keychain =Keychain.From_seed(walletSeed.Value);

            services.AddSingleton(pr => walletConfig);
            services.AddSingleton(pr => keychain);

            services.AddSingleton(pr=>new CoinbaseHandler(walletConfig, keychain));
            services.AddSingleton(pr => new WalletReceiver(walletConfig, keychain));
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
