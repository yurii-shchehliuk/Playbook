using Microsoft.Extensions.Configuration;

namespace API.Scrapping.Core
{
    public class Consts
    {
        public static IConfiguration _conf;

        public string URL = string.Empty;
        public string BrowserPath = _conf["BrowserPath"];
        public int OpenPageDelay = Convert.ToInt32(_conf["OpenPageDelay"]);
        public int WaitForLoad = Convert.ToInt32(_conf["WaitForLoad"]);
        public string LeaguesCollection = _conf["PlaybookDatabase:LeaguesCollection"];
        public string YearToParse = _conf["YearToParse"];
        public bool HeadlessBrowser = Convert.ToBoolean(_conf["HeadlessBrowser"]);
        public string TeamsCollection = "Teams";

        public string GetFileName
        {
            get
            {
                var url = URL.Remove(0, 36).Replace('/', '_').Replace('-', '_').Replace("results", "");
                if (url.EndsWith('_'))
                {
                    url = url.TrimEnd('_');
                }
                //remove country name
                url = url.Substring(url.IndexOf("_"));
                return url;
            }
        }
    }
}
