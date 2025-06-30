using Web.Domain.Entities;

namespace Web.Domain.IServices
{
    public interface ILeagueService
    {
        Task<List<League>> GetAllLeaguesAsync();
        Task<League> GetLeagueByIdAsync(string id);
        Task<League> GetLeagueByIndexAsync(int index);
        Task<string> GetCollectionNameAsync(int leagueIndex, string year = null);
        Task<string[]> GetLeaguesToParseAsync();
    }
} 