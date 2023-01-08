using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;
using Scrapping.Core;
using Scrapping.Services;
using System.Data;
using System.Text;

namespace Scrapping.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private IBrowser browser;
        private IPage page;
        private IPage page2;
        private Consts consts;
        private readonly IMatchService _matchService;
        public HomeController(ILogger<HomeController> logger, IMatchService matchService)
        {
            _logger = logger;
            _matchService = matchService;
            consts = new Consts();
        }

        [HttpGet("ParseMathc")]
        public async Task<IActionResult> ParseMathc()
        {
            Console.WriteLine("Siemanko");
            var options = new LaunchOptions()
            {
                Headless = true,
                ExecutablePath = consts.BrowserPath,
                Product = Product.Chrome
            };

            browser = await Puppeteer.LaunchAsync(options);
            page = await browser.NewPageAsync();
            await page.GoToAsync(consts.URL);

            /// all matches
            var results = await page.QuerySelectorAllAsync("div.event__match");
            List<Match> matchesResults = new List<Match>();

            foreach (var elem in results)
            {
                var match = new Match();

                match.Id = (await elem.GetPropertyAsync("id")).RemoteObject.Value.ToString().Replace("g_1_", "");

                //if (await _matchService.GetMatchById(match.Id) == null)
                //{
                //    continue;
                //}

                page2 = await browser.NewPageAsync();

                var matchUrl = $@"https://www.flashscore.com/match/{match.Id}/#/match-summary/match-summary";
                await Task.Delay(consts.Delay);
                await page2.GoToAsync(matchUrl);

                #region summary
                await Task.Delay(25);
                var matchHeader = await page2.QuerySelectorAsync("div.duelParticipant");
                var matchHeaderData = (await matchHeader.GetPropertyAsync("outerText")).RemoteObject.Value.ToString().Replace("\n", ",").Replace("FINISHED", "").Split(',');
                match.Title = matchHeaderData[1] + " - " + matchHeaderData.LastOrDefault();
                match.Date = Convert.ToDateTime(matchHeaderData[0]);
                match.Summary.Add(matchHeaderData[2] + matchHeaderData[3] + matchHeaderData[4]);
                var matchSummary = await page2.QuerySelectorAllAsync("div.smv__participantRow");
                foreach (var item in matchSummary)
                {
                    var matchContent = (await item.GetPropertyAsync("outerText")).RemoteObject.Value.ToString().Replace("\n", ",");
                    match.Summary.Add(matchContent);
                }

                var matchIncidents = await page2.QuerySelectorAllAsync("div.smv__incidentsHeader");
                foreach (var item in matchIncidents)
                {
                    var matchContent = (await item.GetPropertyAsync("outerText")).RemoteObject.Value.ToString().Replace("\n", ",");
                    match.Incidents.Add(matchContent);
                }
                #endregion

                #region stats per half

                matchUrl = $@"https://www.flashscore.com/match/{match.Id}/#/match-summary/match-statistics";
                match.Stats0 = await PopulateData(matchUrl, "div.stat__row");

                matchUrl = $@"https://www.flashscore.com/match/{match.Id}/#/match-summary/match-statistics/1";
                match.Stats1 = await PopulateData(matchUrl, "div.stat__row");

                matchUrl = $@"https://www.flashscore.com/match/{match.Id}/#/match-summary/match-statistics/2";
                match.Stats2 = await PopulateData(matchUrl, "div.stat__row");

                #endregion
                matchesResults.Add(match);
                await _matchService.SaveItemToDb(match);
            }

            return View();
        }

        /// <summary>
        /// assign data to model
        /// </summary>
        /// <param name="url"></param>
        /// <param name="querySelector"></param>
        /// <returns></returns>
        private async Task<Stats> PopulateData(string url, string querySelector)
        {
            page2 = await browser.NewPageAsync();
            await Task.Delay(250);
            await page2.GoToAsync(url);
            await Task.Delay(25);

            Stats stats = new Stats();
            var matchStats2 = await page2.QuerySelectorAllAsync(querySelector);
            List<string> rowData = new List<string>();
            foreach (var item in matchStats2)
            {
                var matchContent = (await item.GetPropertyAsync("outerText")).RemoteObject.Value.ToString().Replace("\n", ",");
                rowData.Add(matchContent);
            }
            stats.Data = rowData.ToArray();
            return stats;
        }


        private async Task<IPage> MontecarloSimulation()
        {
            return null;
        }
    }
}
