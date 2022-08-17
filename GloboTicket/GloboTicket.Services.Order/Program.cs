using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using GloboTicket.Common;
using Microsoft.Extensions.Logging;

namespace GloboTicket.Services.Ordering
{
    public class Program
    {
        public static void Main(string[] args)
        {
	        var loggerFactory = LoggerFactory.Create(builder => { /*configure*/ });
	        var startupLogger = loggerFactory.CreateLogger<Startup>();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).UseSerilog(Logging.ConfigureLogger);
    }
}
