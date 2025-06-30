using Web.Domain.Entities;

namespace Web.Domain.IServices
{
    public interface IScrapingService
    {
        Task<List<Match>> ScrapeMatchesForLeagueAsync(string leagueId, CancellationToken cancellationToken = default);
        Task<Match> ScrapeMatchAsync(string matchId, CancellationToken cancellationToken = default);
    }
} 