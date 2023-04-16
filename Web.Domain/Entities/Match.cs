using API.Scrapping.Core;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using PuppeteerSharp;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Web.Domain.Extentions;
using Web.Domain.IEntities;

namespace Web.Domain.Entities
{
    public class Match : BaseEntity, IFlashscore
    {
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public Team THome { get; set; }
        public Team TGuest { get; set; }
        public int RoundNr { get; set; }
        public List<string> Summary { get; set; } = new List<string>();


        public async Task<List<string>> PopulateData(string querySelector, IPage page2)
        {
            var matchIncidents0 = await page2.QuerySelectorAllAsync(querySelector);
            var matchIncidents = new List<string>();
            foreach (var item in matchIncidents0)
            {
                var matchContent = (await item.GetPropertyAsync("outerText")).Convert();
                matchIncidents.Add(matchContent);
            }
            return matchIncidents;
        }

        /// <summary>
        /// assign data to model
        /// </summary>
        public async Task<T[]> PopulateData<T>(string url, string querySelector, IPage page2, Consts consts) where T : class
        {
            await Task.Delay(consts.OpenPageDelay);
            await page2.GoToAsync(url);
            await Task.Delay(consts.WaitForLoad);

            var matchData = await page2.QuerySelectorAllAsync(querySelector);
            Dictionary<string, string> homeData = new Dictionary<string, string>();
            Dictionary<string, string> guestData = new Dictionary<string, string>();

            foreach (var item in matchData)
            {
                var matchContent = (await item.GetPropertyAsync("outerText")).Convert();
                if (!matchContent.Contains(','))
                {
                    continue;
                }
                var rowSplit = matchContent.Replace("%", "").Split(',');
                var colName = rowSplit[1].Replace(" ", "");

                homeData.Add(colName, rowSplit[0]);
                guestData.Add(colName, rowSplit[2]);
            }
            string homeDict = JsonConvert.SerializeObject(homeData);
            var homeStats = JsonConvert.DeserializeObject<T>(homeDict);

            string guestDict = JsonConvert.SerializeObject(guestData);
            var guestStats = JsonConvert.DeserializeObject<T>(guestDict);

            return new T[] { homeStats, guestStats };
        }
    }
}
