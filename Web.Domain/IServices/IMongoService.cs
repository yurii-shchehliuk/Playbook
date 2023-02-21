
namespace API.Scrapping.Services
{
    public interface IMongoService<T>
    {
        Task<List<T>> GetAsync();

        Task<T?> GetAsync(string id);

        Task CreateAsync(T newItem);

        Task UpdateAsync(string id, T updatedItem);

        Task RemoveAsync(string id);
    }
}
