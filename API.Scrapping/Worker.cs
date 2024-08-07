﻿using API.Scrapping.Core;
using API.Scrapping.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PuppeteerSharp;
using System.Globalization;
using System.Reflection;
using Web.Domain.Entities;
using Web.Domain.Extentions;

namespace API.Scrapping
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private AppConfiguration appConfig;
        private readonly MongoService<Match> _matchService;
        private readonly MongoService<TeamBase> _teamService;
        private readonly MongoService<League> _leagueService;
        private int attempts = 0;

        public Worker(
            ILogger<Worker> logger,
            MongoService<Match> matchService,
            MongoService<TeamBase> teamService,
            MongoService<League> leagueService,
            AppConfiguration appConfiguration,
            DatabaseConfiguration playbookDatabase
            )
        {
            _logger = logger;
            _matchService = matchService;
            _teamService = teamService;
            _leagueService = leagueService;
            appConfig = appConfiguration;
            _logger.LogInformation(string.Format("[{0}] Siemanko", DateTime.Now));

            _teamService.SetCollection(playbookDatabase.TeamsCollection);
            _leagueService.SetCollection(playbookDatabase.LeaguesCollection);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var settings = await BrowserSettings.Init(appConfig);

            var matchesResults = new List<Match>();
            var leaguesToParse = await ShowLeagues();

            foreach (var league in leaguesToParse)
            {
                var leagueName = await GetCollectionName(Convert.ToInt32(league));
                var matchesPerLeague = await LoadMatches(leagueName);

                for (int matchIndex = 0; matchIndex < matchesPerLeague.Length; matchIndex++)
                {
                    IElementHandle currentMatch = matchesPerLeague[matchIndex];
                    var matchId = (await currentMatch.GetPropertyAsync("id")).RemoteObject.Value.ToString().Replace("g_1_", "");

                    try
                    {
                        if (await _matchService.GetAsync(matchId) != null)
                        {
                            _logger.LogWarning(string.Format("Match with id: {0} already exists in database {1} [{2}/{3}]", matchId, leagueName, matchIndex + 1, matchesPerLeague.Length));
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
                        var match = await ParseMatchPage(currentMatch);
                        if (match == null)
                        {
                            continue;
                        }
                        matchesResults.Add(match);
                        await _matchService.CreateAsync(match);
                        Console.WriteLine(string.Format("[{0}] Match added: {1}, {2} [{3}/{4}]", DateTime.Now, match.Title, leagueName, matchIndex + 1, matchesPerLeague.Length));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(string.Format("Error parsing match with the id: {0} \n {1}, \n {2}", matchId, ex.Message, ex.InnerException));
                        attempts++;
                        await Task.Delay(appConfig.WaitForLoad * 3);
                    }
                    finally
                    {
                        if (attempts >= 5)
                        {
                            settings.Dispose();
                            _logger.LogError(string.Format("Exiting, too many attempts"));
                            Environment.Exit(1);
                        }
                    }
                }
                _logger.LogInformation(string.Format("\n[{0}] League Parsing finished {1}", DateTime.Now, leagueName));
            }

            _logger.LogInformation(string.Format("\n[{0}] Parsing finished", DateTime.Now));
            settings.Dispose();
        }


        /// <summary>
        /// simulate click to load all
        /// </summary>
        /// <returns></returns>
        /// <todo>check xPath to get all the child of the parent table</todo>
        private async Task<IElementHandle[]> LoadMatches(string leagueCollection)
        {
            Console.WriteLine(string.Format("URL: {0}", appConfig.URL));
            _logger.LogInformation(string.Format("Collection name: {0}", leagueCollection));
            //_logger.LogInformation(string.Format("Teams collection name: {0}", consts.TeamsCollection));

            _matchService.SetCollection(leagueCollection);

            var page = BrowserSettings.page;
            await page.GoToAsync(appConfig.URL);

            //load all
            while (await page.QuerySelectorAsync("a.event__more") != null)
            {
                await page.EvaluateExpressionAsync("document.querySelector('a.event__more')?.click()");
                await Task.Delay(appConfig.WaitForLoad);
            }
            //parse all by class
            var results = await page.QuerySelectorAllAsync("div.event__match");
            _logger.LogInformation(string.Format("Found matches: {0}", results.Length));

            return results;
        }

        private async Task<string[]> ShowLeagues()
        {
            var leguesList = await _leagueService.GetAsync();
            Console.WriteLine(string.Format("Found {0} legues", leguesList.Count));
            if (leguesList.Count < 1)
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var leaguesJson = System.IO.File.ReadAllText(path + "/Data/leagues.json");

                var leaguesArr = JsonConvert.DeserializeObject<List<League>>(leaguesJson);
                foreach (var item in leaguesArr)
                {
                    await _leagueService.CreateAsync(item);
                }
                return await ShowLeagues();
            }

            for (int i = 0; i < leguesList.Count; i++)
            {
                League legue = leguesList[i];
                Console.WriteLine(string.Format("[{0}] {1}", i, legue.Name));
            }

            Console.WriteLine("Select legue to parse (press enter to parse all or provide by ,): ");
            try
            {
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input.Trim()))
                {
                    var arr = new string[leguesList.Count];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = i.ToString();
                    }
                    return arr;
                }
                return input.Split(',');

            }
            catch (Exception)
            {
                _logger.LogError("Wrong input parameter on legue selecting");
                return await ShowLeagues();
            }
        }

        private async Task<string> GetCollectionName(int urlNumber)
        {
            var leguesList = await _leagueService.GetAsync();

            var league = leguesList[urlNumber];
            var URL = league.FlashscoreLink;

            Console.WriteLine("Provide season year xxxx-xxxx(press enter to parse the latest): ");
            var year = appConfig.YearToParse;
            year = Console.ReadLine();
            if (URL.Contains("results"))
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
                appConfig.URL = URL;
            }
            if (string.IsNullOrEmpty(year) && league.Country.Name != "MLS")
            {
                year = (DateTime.Now.Year - 1).ToString() + "-" + DateTime.Now.Year.ToString();
            }
            else if (league.Country.Name == "MLS")
            {
                year = DateTime.Now.Year.ToString();

            }
            return league.Country.Code + league.GetFileName + "_" + year;

        }

        private async Task<Match> ParseMatchPage(IElementHandle elem)
        {
            var match = new Match();
            match.Id = (await elem.GetPropertyAsync("id")).RemoteObject.Value.ToString().Replace("g_1_", "");

            using var page2 = await BrowserSettings.browser.NewPageAsync();

            var matchUrl = $@"https://www.flashscore.com/match/{match.Id}/#/match-summary/";
            await Task.Delay(appConfig.OpenPageDelay);
            await page2.GoToAsync(matchUrl + "match-summary/");

            #region team
            var matchHeader = await page2.QuerySelectorAsync("div.duelParticipant");
            var matchHeaderData = (await matchHeader.GetPropertyAsync("outerText")).Convert().Split(',');
            if (matchHeaderData[5].ToUpper() != "FINISHED")
            {
                return null;
            }
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
            await Task.Delay(appConfig.OpenPageDelay);
            var goalsPerFirst = (await match.PopulateData("div.smv__incidentsHeader", page2)).FirstOrDefault().Split(',').LastOrDefault().Split('-');
            match.THome.GoalsPerFirst = Convert.ToInt32(goalsPerFirst[0]);
            match.TGuest.GoalsPerFirst = Convert.ToInt32(goalsPerFirst[1]);
            var goalsPerSecond = (await match.PopulateData("div.smv__incidentsHeader", page2)).LastOrDefault().Split(',').LastOrDefault().Split('-');
            match.THome.GoalsPerSecond = Convert.ToInt32(goalsPerSecond[0]);
            match.TGuest.GoalsPerSecond = Convert.ToInt32(goalsPerSecond[1]);
            #endregion

            #region summary
            await Task.Delay(appConfig.WaitForLoad);
            var matchRound = (await page2.EvaluateExpressionAsync("document.querySelector('span.tournamentHeader__country').lastChild.innerHTML")).ToString();
            match.RoundNr = Convert.ToInt32(matchRound.Substring(matchRound.IndexOf("Round") + 6));

            match.Date = DateTime.ParseExact(matchHeaderData[0].Replace('.', '-'), "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);

            match.Summary.Add(matchHeaderData[2] + matchHeaderData[3] + matchHeaderData[4]);
            match.Summary = await match.PopulateData("div.smv__participantRow", page2);

            #endregion

            #region stats per half

            var statsArr = await match.PopulateData<Stats>(matchUrl + "match-statistics/0", "div.stat__row", page2, appConfig);
            match.THome.Stats0 = statsArr[0];
            match.TGuest.Stats0 = statsArr[1];

            statsArr = await match.PopulateData<Stats>(matchUrl + "match-statistics/1", "div.stat__row", page2, appConfig);
            match.THome.Stats1 = statsArr[0];
            match.TGuest.Stats1 = statsArr[1];
            statsArr = await match.PopulateData<Stats>(matchUrl + "match-statistics/2", "div.stat__row", page2, appConfig);
            match.THome.Stats2 = statsArr[0];
            match.TGuest.Stats2 = statsArr[1];
            #endregion

            #region lineups
            await Task.Delay(appConfig.OpenPageDelay);
            await page2.GoToAsync(matchUrl + "match-summary/lineups");
            await Task.Delay(appConfig.WaitForLoad);
            var lf = await match.PopulateData<Team>(matchUrl + "lineups", "div.lf__header", page2, appConfig);
            match.THome.Formation = lf[0].Formation;
            match.TGuest.Formation = lf[1].Formation;

            #endregion

            await page2.CloseAsync();
            await page2.DisposeAsync();
            return match;
        }
    }
}
