using PuppeteerSharp;

namespace API.Scrapping.Core
{
    public sealed class BrowserSettings : IDisposable
    {
        public static IBrowser browser;
        public static IPage page;

        private static BrowserSettings _instance;

        public static async Task<BrowserSettings> Init(Consts consts)
        {
            if (_instance != null)
                return _instance;

            _instance = new BrowserSettings();
            var options = new LaunchOptions()
            {
                Headless = consts.HeadlessBrowser,
                ExecutablePath = consts.BrowserPath,
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
