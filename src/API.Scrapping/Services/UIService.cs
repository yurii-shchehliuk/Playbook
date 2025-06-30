using Microsoft.Extensions.Logging;
using Web.Domain.Entities;
using Web.Domain.IServices;

namespace API.Scrapping.Services
{
    public class UIService : IUIService
    {
        private readonly ILogger<UIService> _logger;

        public UIService(ILogger<UIService> logger)
        {
            _logger = logger;
        }

        public void DisplayLeagues(List<League> leagues)
        {
            for (int i = 0; i < leagues.Count; i++)
            {
                var league = leagues[i];
                Console.WriteLine($"[{i}] {league.Name}");
            }
        }

        public void DisplayProgress(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] {message}");
        }

        public async Task<string[]> GetUserLeagueSelectionAsync(List<League> leagues)
        {
            Console.WriteLine("Select league to parse (press enter to parse all or provide by ,): ");
            
            try
            {
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input?.Trim()))
                {
                    var arr = new string[leagues.Count];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = i.ToString();
                    }
                    return arr;
                }
                return input.Split(',');
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Wrong input parameter on league selecting");
                return await GetUserLeagueSelectionAsync(leagues);
            }
        }

        public async Task<string> GetUserYearInputAsync(string defaultYear = null)
        {
            Console.WriteLine("Provide season year xxxx-xxxx(press enter to parse the latest): ");
            var year = defaultYear ?? Console.ReadLine();
            return year;
        }
    }
} 