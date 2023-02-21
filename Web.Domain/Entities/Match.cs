using API.Scrapping.Core;
using MongoDB.Bson.Serialization.Attributes;
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
        public string Result { get; set; }
        public DateTime Date { get; set; }
        public Team THome { get; set; }
        public Team TGuest { get; set; }
        public int RoundNr { get; set; }
        public List<string> Incidents { get; set; } = new List<string>();
        public List<string> Summary { get; set; } = new List<string>();
        public List<string> Stats0 { get; set; } = new List<string>();
        public List<string> Stats1 { get; set; } = new List<string>();
        public List<string> Stats2 { get; set; } = new List<string>();

        /// <summary>
        /// assign data to model
        /// </summary>
        public async Task<List<string>> PopulateData(string url, string querySelector, IPage page2, Consts consts)
        {
            await Task.Delay(consts.OpenPageDelay);
            await page2.GoToAsync(url);
            await Task.Delay(consts.WaitForLoad);

            var matchData = await page2.QuerySelectorAllAsync(querySelector);
            List<string> rowData = new List<string>();
            foreach (var item in matchData)
            {
                var matchContent = (await item.GetPropertyAsync("outerText")).Convert();
                rowData.Add(matchContent);
            }

            return rowData;
        }

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
    }
}
