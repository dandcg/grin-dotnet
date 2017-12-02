using System;
using Serilog;
using Serilog.Events;

namespace Grin.Tests.Unit
{
    public class LoggingFixture : IDisposable
    {
        public LoggingFixture()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
        }

        public void Dispose()
        {
           Log.CloseAndFlush();
        }

   
    }
}
