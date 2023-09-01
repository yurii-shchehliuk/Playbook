using API.Scrapping.Core;
using API.Scrapping.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PuppeteerSharp;

namespace API.Scrapping
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(c =>
            {
                c.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<AppConfiguration>();
                services.AddSingleton<DatabaseConfiguration>();
                services.AddSingleton(typeof(MongoService<>));

                services.AddHostedService<Worker>();

            })
            .Build();
            await host.RunAsync();
        }
    }
}
