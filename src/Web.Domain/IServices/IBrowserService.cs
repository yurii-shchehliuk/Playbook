using PuppeteerSharp;

namespace Web.Domain.IServices
{
    public interface IBrowserService : IDisposable
    {
        Task<IBrowser> GetBrowserAsync();
        Task<IPage> GetPageAsync();
        Task<IPage> CreateNewPageAsync();
        Task InitializeAsync();
    }
} 