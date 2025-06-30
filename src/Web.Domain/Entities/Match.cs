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
            try
            {
                var matchIncidents0 = await page2.QuerySelectorAllAsync(querySelector);
                var matchIncidents = new List<string>();
                
                if (matchIncidents0 == null || matchIncidents0.Length == 0)
                {
                    return matchIncidents;
                }
                
                foreach (var item in matchIncidents0)
                {
                    if (item != null)
                    {
                        var matchContent = (await item.GetPropertyAsync("outerText")).Convert();
                        if (!string.IsNullOrWhiteSpace(matchContent))
                        {
                            matchIncidents.Add(matchContent);
                        }
                    }
                }
                return matchIncidents;
            }
            catch (Exception ex)
            {
                // Return empty list on error to prevent crashes
                return new List<string>();
            }
        }

        /// <summary>
        /// assign data to model
        /// </summary>
        public async Task<T[]> PopulateData<T>(string url, string querySelector, IPage page2, AppConfiguration consts) where T : class
        {
            try
            {
                await Task.Delay(consts.OpenPageDelay);
                await page2.GoToAsync(url);
                await Task.Delay(consts.WaitForLoad);

                var matchData = await page2.QuerySelectorAllAsync(querySelector);
                
                if (matchData == null || matchData.Length == 0)
                {
                    return new T[0];
                }
                
                Dictionary<string, string> homeData = new Dictionary<string, string>();
                Dictionary<string, string> guestData = new Dictionary<string, string>();

                foreach (var item in matchData)
                {
                    if (item == null) continue;
                    
                    var matchContent = (await item.GetPropertyAsync("outerText")).Convert();
                    if (string.IsNullOrWhiteSpace(matchContent) || !matchContent.Contains(','))
                    {
                        continue;
                    }
                    
                    var rowSplit = matchContent.Replace("%", "").Split(',');
                    if (rowSplit.Length < 3)
                    {
                        continue;
                    }
                    
                    var colName = rowSplit[1].Replace(" ", "").Trim();
                    if (string.IsNullOrWhiteSpace(colName))
                    {
                        continue;
                    }

                    // Ensure we don't add duplicate keys
                    if (!homeData.ContainsKey(colName))
                    {
                        homeData.Add(colName, rowSplit[0].Trim());
                    }
                    if (!guestData.ContainsKey(colName))
                    {
                        guestData.Add(colName, rowSplit[2].Trim());
                    }
                }
                
                if (homeData.Count == 0 || guestData.Count == 0)
                {
                    return new T[0];
                }
                
                string homeDict = JsonConvert.SerializeObject(homeData);
                var homeStats = JsonConvert.DeserializeObject<T>(homeDict);

                string guestDict = JsonConvert.SerializeObject(guestData);
                var guestStats = JsonConvert.DeserializeObject<T>(guestDict);

                return new T[] { homeStats, guestStats };
            }
            catch (Exception ex)
            {
                // Return empty array on error to prevent crashes
                return new T[0];
            }
        }
    }
}
