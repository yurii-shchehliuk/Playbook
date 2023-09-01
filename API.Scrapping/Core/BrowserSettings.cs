using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace API.Scrapping.Core
{
    public sealed class BrowserSettings : IDisposable
    {
        public static IBrowser browser;
        public static IPage page;

        private static BrowserSettings _instance;

        public static async Task<BrowserSettings> Init(AppConfiguration appConfig)
        {
            if (_instance != null)
                return _instance;

            _instance = new BrowserSettings();
            var osNameAndVersion = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
            Console.WriteLine(osNameAndVersion);

            var browserPath = appConfig.BrowserPath;
            if (!osNameAndVersion.ToLowerInvariant().Contains("windows"))
            {
                browserPath = appConfig.BrowserPathMac;
            }

            var options = new LaunchOptions()
            {
                Headless = appConfig.HeadlessBrowser,
                ExecutablePath = browserPath,
                Product = Product.Chrome
            };

            browser = await Puppeteer.LaunchAsync(options);
            page = await browser.NewPageAsync();

            return _instance;
        }

        public void Dispose()
        {
            page.CloseAsync();
            browser.CloseAsync();
            browser.Dispose();
        }
    }
}
