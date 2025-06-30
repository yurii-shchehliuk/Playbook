using Web.Domain.Entities;

namespace Web.Domain.IServices
{
    public interface IUIService
    {
        Task<string[]> GetUserLeagueSelectionAsync(List<League> leagues);
        Task<string> GetUserYearInputAsync(string defaultYear = null);
        void DisplayLeagues(List<League> leagues);
        void DisplayProgress(string message);
    }
} 