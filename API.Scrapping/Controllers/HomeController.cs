using Domain.Entities;
using Domain.Extentions;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using Scrapping.Core;
using Scrapping.Services;
using System.Data;
using System.Globalization;
using System.Text;

namespace Scrapping.Controllers
{
    public class HomeController : BaseController, IDisposable
    {
        private readonly ILogger<HomeController> _logger;
        private Consts consts;
        private readonly MongoService<Domain.Entities.Match> _mongoService;

        public HomeController(ILogger<HomeController> logger, MongoService<Domain.Entities.Match> mongoService)
        {
            _logger = logger;
            _mongoService = mongoService;
            consts = new Consts();
        }


        [HttpGet("ParseMatches")]
        public async Task<IActionResult> ParseMatches()
        {
            using var settings = await BrowserSettings.Init(consts);

            var matchesResults = new List<Match>();

            foreach (var elem in await LoadMatches())
            {
                var matchId = (await elem.GetPropertyAsync("id")).RemoteObject.Value.ToString().Replace("g_1_", "");

                try
                {
                    if (await _mongoService.GetAsync(matchId) != null)
                    {
                        _logger.LogInformation(string.Format("Match with id: {0} already exists in database", matchId));
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(string.Format("Mongo service couldn't start, use 'net start MongoDB' to run \n {0} \n {1}", ex.Message, ex.InnerException));
                    throw ex;
                }

                try
                {
                    var match = await ParseMatchPage(elem);

                    matchesResults.Add(match);
                    await _mongoService.CreateAsync(match);
                    _logger.LogInformation(string.Format("[{0}] Match added: {1}", DateTime.Now, match.Title));
                }
                catch (Exception ex)
                {
                    _logger.LogError(string.Format("Error parsing match with the id: {0} \n {1}, \n {2}", matchId, ex.Message, ex.InnerException));
                    throw ex;
                }
            }

            _logger.LogInformation(string.Format("\n[{0}] Parsing finished", DateTime.Now));
            settings.Dispose();
            return View();
        }

        [HttpGet("GetMatches")]
        public async Task<ActionResult<IEnumerable<Domain.Entities.Match>>> GetMatches()
        {
            return await _mongoService.GetAsync(); ;
        }

        /// <summary>
        /// simulate click to load all
        /// </summary>
        /// <returns></returns>
        /// <todo>check xPath to get all the child of the parent table</todo>
        private async Task<IElementHandle[]> LoadMatches()
        {
            _logger.LogInformation(string.Format("[{0}] Siemanko", DateTime.Now));
            _logger.LogInformation(string.Format("URL: {0}", consts.GetFileName));
            _logger.LogInformation(string.Format("Collection name: {0}", consts.CollectionName));

            var page = BrowserSettings.page;
            await page.GoToAsync(consts.URL);

            //load all
            while (await page.QuerySelectorAsync("a.event__more") != null)
            {
                await page.EvaluateExpressionAsync("document.querySelector('a.event__more')?.click()");
                await Task.Delay(consts.OpenPageDelay);
            }
            //parse all by calss
            var results = await page.QuerySelectorAllAsync("div.event__match");
            _logger.LogInformation(string.Format("Found matches: {0}", results.Length));

            return results;
        }

        private async Task<Match> ParseMatchPage(IElementHandle elem)
        {
            var match = new Match();
            match.Id = (await elem.GetPropertyAsync("id")).RemoteObject.Value.ToString().Replace("g_1_", "");

            using var page2 = await BrowserSettings.browser.NewPageAsync();

            var matchUrl = $@"https://www.flashscore.com/match/{match.Id}/#/match-summary/";
            await Task.Delay(consts.OpenPageDelay);
            await page2.GoToAsync(matchUrl + "match-summary/");

            #region summary
            await Task.Delay(consts.WaitForLoad);
            var matchRound0 = (await page2.EvaluateExpressionAsync("document.querySelector('span.tournamentHeader__country').lastChild.innerHTML")).ToString();
            match.RoundNr = matchRound0.Substring(matchRound0.IndexOf("Round") + 6);

            var matchHeader0 = await page2.QuerySelectorAsync("div.duelParticipant");
            var matchHeaderData = (await matchHeader0.GetPropertyAsync("outerText")).Convert().Replace("FINISHED", "").Split(',');
            match.Title = matchHeaderData[1] + " - " + matchHeaderData.LastOrDefault();
            _logger.LogInformation(string.Format("Parsing {0}", match.Title));
            try
            {
                match.Date = DateTime.ParseExact(matchHeaderData[0].Replace('.', '/'), "d/MM/yyyy hh:mm", CultureInfo.InvariantCulture).ToString();
            }
            catch
            {
                match.Date = matchHeaderData[0];
            }

            match.Result = matchHeaderData[2] + matchHeaderData[3] + matchHeaderData[4];

            var matchSummary0 = await page2.QuerySelectorAllAsync("div.smv__participantRow");
            foreach (var item in matchSummary0)
            {
                var matchContent = (await item.GetPropertyAsync("outerText")).Convert();
                match.Summary.Add(matchContent);
            }

            var matchIncidents0 = await page2.QuerySelectorAllAsync("div.smv__incidentsHeader");
            foreach (var item in matchIncidents0)
            {
                var matchContent = (await item.GetPropertyAsync("outerText")).Convert();
                match.Incidents.Add(matchContent);
            }
            #endregion

            #region stats per half

            match.Stats0 = await PopulateData(matchUrl + "matchUrl/match-statistics/0", "div.stat__row", page2);

            match.Stats1 = await PopulateData(matchUrl + "matchUrl/match-statistics/1", "div.stat__row", page2);

            match.Stats2 = await PopulateData(matchUrl + "matchUrl/match-statistics/2", "div.stat__row", page2);
            #endregion
            await page2.CloseAsync();
            await page2.DisposeAsync();
            return match;
        }

        /// <summary>
        /// assign data to model
        /// </summary>
        private async Task<List<string>> PopulateData(string url, string querySelector, IPage page2)
        {
            await Task.Delay(consts.OpenPageDelay);
            await page2.GoToAsync(url);
            await Task.Delay(consts.WaitForLoad);

            var matchStats0 = await page2.QuerySelectorAllAsync(querySelector);
            List<string> rowData = new List<string>();
            foreach (var item in matchStats0)
            {
                var matchContent = (await item.GetPropertyAsync("outerText")).Convert();
                rowData.Add(matchContent);
            }

            return rowData;
        }

        private async Task<IPage> MontecarloSimulation()
        {
            return null;
        }
    }
    internal sealed class BrowserSettings : IDisposable
    {
        public static IBrowser browser;
        public static IPage page;
        private BrowserSettings()
        {
        }
        private static BrowserSettings _instance;

        public static async Task<BrowserSettings> Init(Consts consts)
        {
            if (_instance == null)
            {
                _instance = new BrowserSettings();
                var options = new LaunchOptions()
                {
                    Headless = true,
                    ExecutablePath = consts.BrowserPath,
                    Product = Product.Chrome
                };

                browser = await Puppeteer.LaunchAsync(options);
                page = await browser.NewPageAsync();
            }
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
