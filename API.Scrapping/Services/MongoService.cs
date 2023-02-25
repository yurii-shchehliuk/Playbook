using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Web.Domain.Entities;
using Web.Persistance;

namespace API.Scrapping.Services
{
    public class MongoService<T> : IMongoService<T> where T : BaseEntity
    {
        private readonly IMongoDatabase _db;
        private IMongoCollection<T> _mongoCollection;

        public MongoService(
            IOptions<MongoSettings> itemStoreDatabaseSettings)
        {
            var client = new MongoClient(
                itemStoreDatabaseSettings.Value.ConnectionString);

            _db = client.GetDatabase(
                itemStoreDatabaseSettings.Value.DatabaseName);

            _mongoCollection = _db.GetCollection<T>(
                itemStoreDatabaseSettings.Value.PlaybookCollectionName);
        }

        public IMongoCollection<T> GetCollection<T>(string collection)
        {
            return _db.GetCollection<T>(collection);
        }

        public void SetCollection(string collection)
        {
            _mongoCollection = _db.GetCollection<T>(collection);
        }

        public async Task<List<T>> GetAsync() =>
            await _mongoCollection.Find(_ => true).ToListAsync();

        public async Task<T?> GetAsync(string id) =>
            await _mongoCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(T newItem) =>
            await _mongoCollection.InsertOneAsync(newItem);

        public async Task UpdateAsync(string id, T updatedItem) =>
            await _mongoCollection.ReplaceOneAsync(x => x.Id == id, updatedItem);

        public async Task RemoveAsync(string id) =>
            await _mongoCollection.DeleteOneAsync(x => x.Id == id);
    }
}
