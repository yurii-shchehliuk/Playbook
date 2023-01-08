//using Domain.Entities;
//using Domain.Interfaces;
//using Microsoft.AspNetCore.Mvc;
//using PuppeteerSharp;
//using Scrapping.Core;
//using Scrapping.Services;
//using System.Data;
//using System.Text;

//namespace Scrapping.Controllers
//{
//    public class HomeControllerV1 : BaseController
//    {
//        private readonly ILogger<HomeController> _logger;
//        private IBrowser browser;
//        private IPage page;
//        private IPage page2;
//        private Consts consts;
//        private readonly IMatchService _matchService;
//        public HomeControllerV1(ILogger<HomeController> logger, IMatchService matchService)
//        {
//            _logger = logger;
//            _matchService = matchService;
//            consts = new Consts();
//        }

//        [HttpGet("ParseMathc")]
//        public async Task<IActionResult> ParseMathc()
//        {
//            Console.WriteLine("Siemanko");
//            var options = new LaunchOptions()
//            {
//                Headless = true,
//                ExecutablePath = consts.BrowserPath,
//                Product = Product.Chrome
//            };

//            browser = await Puppeteer.LaunchAsync(options);
//            page = await browser.NewPageAsync();
//            await page.GoToAsync(consts.URL);

//            /// all matches
//            var results = await page.QuerySelectorAllAsync("div.event__match");
//            List<string> matchesResults = new List<string>();

//            foreach (var elem in results)
//            {
//                var match = new Match();

//                var content = (await elem.GetPropertyAsync("outerText")).RemoteObject.Value.ToString();
//                matchesResults.Add(content);
//                match.Id = (await elem.GetPropertyAsync("id")).RemoteObject.Value.ToString().Replace("g_1_", "");
//                if (await _matchService.GetMatchById(match.Id) == null)
//                {
//                    continue;
//                }

//                page2 = await browser.NewPageAsync();

//                var matchUrl = $@"https://www.flashscore.com/match/{match.Id}/#/match-summary/match-summary";
//                await Task.Delay(consts.Delay);
//                await page2.GoToAsync(matchUrl);

//                #region summary
//                await Task.Delay(25);
//                var matchHeader= await page2.QuerySelectorAsync("div.duelParticipant");
//                var matchHeaderData = (await matchHeader.GetPropertyAsync("outerText")).RemoteObject.Value.ToString().Replace("\n", ",").Replace("FINISHED", "").Split(',');
//                match.Title = matchHeaderData[1] +" - "+ matchHeaderData.LastOrDefault();
//                match.Date = Convert.ToDateTime(matchHeaderData[0]);
//                match.Summary.Add(matchHeaderData[2]+ matchHeaderData[3] + matchHeaderData[4]);
//                var matchSummary = await page2.QuerySelectorAllAsync("div.smv__participantRow");
//                foreach (var item in matchSummary)
//                {
//                    var matchContent = (await item.GetPropertyAsync("outerText")).RemoteObject.Value.ToString().Replace("\n", ",");
//                    match.Summary.Add(matchContent);
//                }

//                var matchIncidents = await page2.QuerySelectorAllAsync("div.smv__incidentsHeader");
//                foreach (var item in matchIncidents)
//                {
//                    var matchContent = (await item.GetPropertyAsync("outerText")).RemoteObject.Value.ToString().Replace("\n", ",");
//                    match.Incidents.Add(matchContent);
//                }
//                #endregion

//                #region stats per half

//                matchUrl = $@"https://www.flashscore.com/match/{match.Id}/#/match-summary/match-statistics";
//                match.Stats0 = await PopulateData(matchUrl, "div.stat__row");

//                matchUrl = $@"https://www.flashscore.com/match/{match.Id}/#/match-summary/match-statistics/1";
//                match.Stats1 = await PopulateData(matchUrl, "div.stat__row");

//                matchUrl = $@"https://www.flashscore.com/match/{match.Id}/#/match-summary/match-statistics/2";
//                match.Stats2 = await PopulateData(matchUrl, "div.stat__row");

//                #endregion

//                GenerateReport(match);
//            }
//            WriteToCsv(matchesResults, consts.GetFileName);

//            return View();
//        }

//        /// <summary>
//        /// assign data to model
//        /// </summary>
//        /// <param name="url"></param>
//        /// <param name="querySelector"></param>
//        /// <returns></returns>
//        private async Task<List<string>> PopulateData(string url, string querySelector)
//        {
//            page2 = await browser.NewPageAsync();
//            await Task.Delay(250);
//            await page2.GoToAsync(url);
//            await Task.Delay(25);

//            List<string> rowData = new List<string>();
//            var matchStats2 = await page2.QuerySelectorAllAsync(querySelector);
//            foreach (var item in matchStats2)
//            {
//                var matchContent = (await item.GetPropertyAsync("outerText")).RemoteObject.Value.ToString().Replace("\n", ",");
//                rowData.Add(matchContent);
//            }
//            return rowData;
//        }

//        /// <summary>
//        /// prepare data
//        /// </summary>
//        /// <param name="match"></param>
//        private void GenerateReport(Match match)
//        {
//            var names = typeof(Match).GetProperties()
//                       .Select(property => property.Name)
//                       .ToArray();

//            DataTable dataTable = new DataTable(typeof(Match).Name);
//            foreach (var item in names)
//            {
//                dataTable.Columns.Add(item);
//            }
//            int maxLength = match.GetMatchLength();

//            for (int i = 0; i < maxLength; i++)
//            {
//                string matchIncidents = Match.GetValue(match.Incidents, i);
//                string matchSummary = ""; //Match.GetValue(match.Summary, i);
//                string matchStats0 = Match.GetValue(match.Stats0, i);
//                string matchStats1 = Match.GetValue(match.Stats1, i);
//                string matchStats2 = Match.GetValue(match.Stats2, i);
//                string title = Match.GetTitle(match.Title, i);
//                dataTable.Rows.Add(title, matchIncidents, matchSummary, matchStats0, matchStats1, matchStats2);
//            }
//            ToCSV(dataTable, match.Title.Remove(0, 16).Replace(",", ""));
//        }

//        /// save data to csv
//        /// TODO: save to db; MSSQL, MySQL
//        /// TODO: host db, github
//        private void ToCSV(DataTable dtDataTable, string strFilePath)
//        {
//            try
//            {

//                bool exists = System.IO.Directory.Exists(consts.SaveDir);

//                if (!exists)
//                    System.IO.Directory.CreateDirectory(consts.SaveDir);

//                StreamWriter sw = new StreamWriter($"{consts.SaveDir}\\{strFilePath}.csv", false);
//                //headers    
//                for (int i = 0; i < dtDataTable.Columns.Count; i++)
//                {
//                    sw.Write(dtDataTable.Columns[i]);
//                    if (i < dtDataTable.Columns.Count - 1)
//                    {
//                        sw.Write(",");
//                    }
//                }
//                sw.Write(sw.NewLine);
//                foreach (DataRow dr in dtDataTable.Rows)
//                {
//                    for (int i = 0; i < dtDataTable.Columns.Count; i++)
//                    {
//                        if (!Convert.IsDBNull(dr[i]))
//                        {
//                            string value = dr[i].ToString();
//                            ///
//                            if (value.Contains(','))
//                            {
//                                //value = String.Format("\"{0}\"", value);
//                                sw.Write(value);
//                            }
//                            else
//                            {
//                                sw.Write(dr[i].ToString());
//                            }
//                        }
//                        if (i < dtDataTable.Columns.Count - 1)
//                        {
//                            sw.Write(",");
//                        }
//                    }
//                    sw.Write(sw.NewLine);
//                }

//                sw.Close();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(string.Format("\"ERROR: \n{0}\"", ex.Message));
//                throw;
//            }
//            Console.WriteLine($"Finished with {strFilePath}");
//        }

//        private void WriteToCsv(List<string> links, string fileName)
//        {
//            StringBuilder sb = new StringBuilder();
//            foreach (var link in links)
//            {
//                sb.AppendLine(link);
//            }
//            System.IO.File.WriteAllText($"{consts.SaveDir}\\{fileName}.csv", sb.ToString());
//        }

//        private async Task<IPage> MontecarloSimulation()
//        {
//            return null;
//        }
//public static string GetValue(List<string> data, int currentRow)
//{
//    if (data.Count > currentRow)
//    {
//        return data[currentRow];
//    }
//    return "";
//}

//public static string GetTitle(string title, int currentRow)
//{
//    if (currentRow < 1)
//    {
//        return title.Replace("-", "");
//    }
//    return "";
//}

//public int GetMatchLength()
//{
//    return Max(Incidents.Count, Summary.Count, Stats0.Count, Stats1.Count, Stats2.Count);
//}
//private static int Max(params int[] values)
//{
//    return values.Max();
//}
//    }
//}
