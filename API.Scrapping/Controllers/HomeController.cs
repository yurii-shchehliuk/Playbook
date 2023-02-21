using API.Scrapping.Core;
using API.Scrapping.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using System.Data;
using System.Globalization;
using System.Text;
using Web.Domain.Entities;
using Web.Domain.Extentions;

namespace API.Scrapping.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase, IDisposable
    {
        private readonly ILogger<HomeController> _logger;
        private Consts consts;
        private readonly MongoService<Match> _mongoService;

        public HomeController(ILogger<HomeController> logger, MongoService<Match> mongoService)
        {
            _logger = logger;
            _mongoService = mongoService;
            consts = new Consts();
            _logger.LogInformation(string.Format("[{0}] Siemanko", DateTime.Now));
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
                        _logger.LogWarning(string.Format("Match with id: {0} already exists in database", matchId));
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
            return Ok();
        }

        [HttpGet("GetMatches")]
        public async Task<ActionResult<IEnumerable<Match>>> GetMatches()
        {
            return await _mongoService.GetAsync();
        }

        /// <summary>
        /// simulate click to load all
        /// </summary>
        /// <returns></returns>
        /// <todo>check xPath to get all the child of the parent table</todo>
        private async Task<IElementHandle[]> LoadMatches()
        {
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

            #region team
            var matchHeader = await page2.QuerySelectorAsync("div.duelParticipant");
            var matchHeaderData = (await matchHeader.GetPropertyAsync("outerText")).Convert().Replace("FINISHED", "").Split(',');
            match.Title = matchHeaderData[1] + " - " + matchHeaderData.LastOrDefault();
            var matchIncidents2 = (await match.PopulateData("div.smv__incidentsHeader", page2));

            var participants = await page2.QuerySelectorAllAsync("a.participant__participantLink");
            match.THome = await new Team().ConfigTeam(participants.FirstOrDefault(), matchHeaderData[1]);
            match.TGuest = await new Team().ConfigTeam(participants.LastOrDefault(), matchHeaderData.LastOrDefault());

            var matchIncidentsFirst = (await match.PopulateData("div.smv__incidentsHeader", page2)).FirstOrDefault().Split(',').LastOrDefault().Split('-');
            match.THome.GoalsPerFirst = Convert.ToInt32(matchIncidentsFirst[0]);
            match.TGuest.GoalsPerFirst = Convert.ToInt32(matchIncidentsFirst[1]);
            var matchIncidentsSecond = (await match.PopulateData("div.smv__incidentsHeader", page2)).LastOrDefault().Split(',').LastOrDefault().Split('-');
            match.THome.GoalsPerSecond = Convert.ToInt32(matchIncidentsSecond[0]);
            match.TGuest.GoalsPerSecond = Convert.ToInt32(matchIncidentsSecond[1]);
            #endregion


            #region summary
            await Task.Delay(consts.WaitForLoad);
            var matchRound = (await page2.EvaluateExpressionAsync("document.querySelector('span.tournamentHeader__country').lastChild.innerHTML")).ToString();
            match.RoundNr = Convert.ToInt32(matchRound.Substring(matchRound.IndexOf("Round") + 6));

            _logger.LogInformation(string.Format("Parsing {0}", match.Title));
            try
            {
                match.Date = DateTime.ParseExact(matchHeaderData[0].Replace('.', '-'), "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);
            }
            catch
            {
                //match.Date = matchHeaderData[0];
            }

            //match.Result = matchHeaderData[2] + matchHeaderData[3] + matchHeaderData[4];

            match.Summary = await match.PopulateData("div.smv__participantRow", page2);

            #endregion

            #region stats per half

            match.Stats0 = await match.PopulateData(matchUrl + "match-statistics/0", "div.stat__row", page2, consts);

            match.Stats1 = await match.PopulateData(matchUrl + "match-statistics/1", "div.stat__row", page2, consts);

            match.Stats2 = await match.PopulateData(matchUrl + "match-statistics/2", "div.stat__row", page2, consts);
            #endregion
            await page2.CloseAsync();
            await page2.DisposeAsync();
            return match;
        }



        private async Task<IPage> MontecarloSimulation()
        {
            return null;
        }

        public void Dispose()
        {
        }
    }
    internal sealed class BrowserSettings : IDisposable
    {
        public static IBrowser browser;
        public static IPage page;

        private static BrowserSettings _instance;

        public static async Task<BrowserSettings> Init(Consts consts)
        {
            if (_instance != null)
                return _instance;

            _instance = new BrowserSettings();
            var options = new LaunchOptions()
            {
                Headless = false,
                ExecutablePath = consts.BrowserPath,
                Product = Product.Chrome
            };

            browser = await Puppeteer.LaunchAsync(options);
            page = await browser.NewPageAsync();

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
