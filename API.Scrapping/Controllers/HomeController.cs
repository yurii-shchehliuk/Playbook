﻿using API.Scrapping.Core;
using API.Scrapping.Services;
using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;
using System.Globalization;
using System.Text;
using Web.Domain.Entities;
using Web.Domain.Extentions;
using System.Reflection;
using Newtonsoft.Json;

namespace API.Scrapping.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase, IDisposable
    {
        private readonly ILogger<HomeController> _logger;
        private Consts consts;
        private readonly MongoService<Match> _matchService;
        private readonly MongoService<TeamBase> _teamService;
        private readonly MongoService<League> _leagueService;

        public HomeController(ILogger<HomeController> logger,
                              MongoService<Match> matchService,
                              MongoService<TeamBase> teamService,
                              MongoService<League> leagueService)
        {
            _logger = logger;
            _matchService = matchService;
            _teamService = teamService;
            _leagueService = leagueService;
            consts = new Consts();
            _logger.LogInformation(string.Format("[{0}] Siemanko", DateTime.Now));

            _teamService.SetCollection(consts.TeamsCollection);
            _leagueService.SetCollection(consts.LeaguesCollection);
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
                    if (await _matchService.GetAsync(matchId) != null)
                    {
                        _logger.LogWarning(string.Format("Match with id: {0} already exists in database", matchId));
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(string.Format("Mongo service couldn't start, use 'net start MongoDB' to run \n {0} \n {1}", ex.Message, ex.InnerException));
                    throw;
                }

                try
                {
                    var match = await ParseMatchPage(elem);

                    matchesResults.Add(match);
                    await _matchService.CreateAsync(match);
                    _logger.LogInformation(string.Format("[{0}] Match added: {1}", DateTime.Now, match.Title));
                }
                catch (Exception ex)
                {
                    _logger.LogError(string.Format("Error parsing match with the id: {0} \n {1}, \n {2}", matchId, ex.Message, ex.InnerException));
                    settings.Dispose();
                    throw;
                }
            }

            _logger.LogInformation(string.Format("\n[{0}] Parsing finished", DateTime.Now));
            settings.Dispose();
            return Ok();
        }

        [HttpGet("GetMatches")]
        public async Task<ActionResult<IEnumerable<Match>>> GetMatches()
        {
            return await _matchService.GetAsync();
        }

        /// <summary>
        /// simulate click to load all
        /// </summary>
        /// <returns></returns>
        /// <todo>check xPath to get all the child of the parent table</todo>
        private async Task<IElementHandle[]> LoadMatches()
        {
            var leguesList = await _leagueService.GetAsync();
            Console.WriteLine(string.Format("Found {0} legues", leguesList.Count));
            for (int i = 0; i < leguesList.Count; i++)
            {
                League? legue = leguesList[i];
                Console.WriteLine(string.Format("[{0}] {1}", i, legue.Name));
            }

            if (leguesList.Count < 1)
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var leaguesJson = System.IO.File.ReadAllText(path +"/Data/leagues2.json");

                var leaguesArr = JsonConvert.DeserializeObject<List<League>>(leaguesJson);
                foreach (var item in leaguesArr)
                {
                    await _leagueService.CreateAsync(item);
                }
            }

            Console.WriteLine("Select legue to parse (press enter to parse the latest): ");
            int urlNumber = 0;
            try
            {
                urlNumber = Convert.ToInt32(Console.ReadLine());
                if (urlNumber >= leguesList.Count)
                {
                    _logger.LogError("Wrong input parameter on legue selecting");
                    return await LoadMatches();
                }
            }
            catch (Exception)
            {
                _logger.LogError("Wrong input parameter on legue selecting");
                return await LoadMatches();
            }
            var URL = leguesList[urlNumber].FlashscoreLink;
            Console.WriteLine("Provide season year: ");
            var year = Console.ReadLine();
            if (URL.Contains("results") && !string.IsNullOrEmpty(year))
            {
                URL = URL.Replace("/results", "");
                URL += "-" + year;
            }
            else
            {
                if (!string.IsNullOrEmpty(year))
                {
                    URL = URL.Substring(0, URL.LastIndexOf('/'));
                    URL += "-" + year;
                }
                //_logger.LogWarning("Url is not pointing to the results \nIt may cause incorrect data parsing \nTrying to add manually");
                if (URL.EndsWith('/'))
                {
                    URL += "results";
                }
                else
                {
                    URL += "/results";
                }
                consts.URL = URL;
            }
            var FileName = leguesList[urlNumber].Country.Code + consts.GetFileName;
            Console.WriteLine(string.Format("URL: {0}", consts.URL));
            _logger.LogInformation(string.Format("Collection name: {0}", FileName));
            _logger.LogInformation(string.Format("Teams collection name: {0}", consts.TeamsCollection));

            _matchService.SetCollection(FileName);

            var page = BrowserSettings.page;
            await page.GoToAsync(consts.URL);

            //load all
            while (await page.QuerySelectorAsync("a.event__more") != null)
            {
                await page.EvaluateExpressionAsync("document.querySelector('a.event__more')?.click()");
                await Task.Delay(consts.WaitForLoad);
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
            _logger.LogInformation(string.Format("Parsing {0}", match.Title));

            var participants = await page2.QuerySelectorAllAsync("a.participant__participantLink");
            match.THome = await new Team().ConfigTeam(participants.FirstOrDefault());
            match.TGuest = await new Team().ConfigTeam(participants.LastOrDefault());

            // add to teams-collection
            if (await _teamService.GetAsync(match.THome.Id) == null)
            {
                await _teamService.CreateAsync(match.THome.GetInstance());
                Console.WriteLine(string.Format("Added team: {0}", match.THome.Name));
            }
            if (await _teamService.GetAsync(match.TGuest.Id) == null)
            {
                await _teamService.CreateAsync(match.TGuest.GetInstance());
                Console.WriteLine(string.Format("Added team: {0}", match.TGuest.Name));
            }

            var goalsPerFirst = (await match.PopulateData("div.smv__incidentsHeader", page2)).FirstOrDefault().Split(',').LastOrDefault().Split('-');
            match.THome.GoalsPerFirst = Convert.ToInt32(goalsPerFirst[0]);
            match.TGuest.GoalsPerFirst = Convert.ToInt32(goalsPerFirst[1]);
            var goalsPerSecond = (await match.PopulateData("div.smv__incidentsHeader", page2)).LastOrDefault().Split(',').LastOrDefault().Split('-');
            match.THome.GoalsPerSecond = Convert.ToInt32(goalsPerSecond[0]);
            match.TGuest.GoalsPerSecond = Convert.ToInt32(goalsPerSecond[1]);
            #endregion


            #region summary
            await Task.Delay(consts.WaitForLoad);
            var matchRound = (await page2.EvaluateExpressionAsync("document.querySelector('span.tournamentHeader__country').lastChild.innerHTML")).ToString();
            match.RoundNr = Convert.ToInt32(matchRound.Substring(matchRound.IndexOf("Round") + 6));

            match.Date = DateTime.ParseExact(matchHeaderData[0].Replace('.', '-'), "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);

            match.Summary.Add(matchHeaderData[2] + matchHeaderData[3] + matchHeaderData[4]);
            match.Summary = await match.PopulateData("div.smv__participantRow", page2);

            #endregion

            #region stats per half

            var statsArr = await match.PopulateData(matchUrl + "match-statistics/0", "div.stat__row", page2, consts);
            match.THome.Stats0 = statsArr[0];
            match.TGuest.Stats0 = statsArr[1];

            statsArr = await match.PopulateData(matchUrl + "match-statistics/1", "div.stat__row", page2, consts);
            match.THome.Stats1 = statsArr[0];
            match.TGuest.Stats1 = statsArr[1];
            statsArr = await match.PopulateData(matchUrl + "match-statistics/2", "div.stat__row", page2, consts);
            match.THome.Stats2 = statsArr[0];
            match.TGuest.Stats2 = statsArr[1];
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
