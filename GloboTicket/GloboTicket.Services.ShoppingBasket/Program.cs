using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using org.SpocWeb.Micro.Logging;
using Serilog;

namespace GloboTicket.Services.ShoppingBasket
{
    public class Program
    {
        public static void Main(string[] args)
        {
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
