using Domain;
using Domain.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Persistance;

namespace Scrapping.Services
{
    public class MongoService<T> : IMongoService<T> where T: BaseEntity
    {
        private readonly IMongoCollection<T> _mongoCollection;

        public MongoService(
            IOptions<MongoSettings> itemStoreDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                itemStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                itemStoreDatabaseSettings.Value.DatabaseName);

            _mongoCollection = mongoDatabase.GetCollection<T>(
                itemStoreDatabaseSettings.Value.PlaybookCollectionName);
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
