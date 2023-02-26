using Microsoft.Extensions.Configuration;

namespace API.Scrapping.Core
{
    public class Consts
    {
        public static IConfiguration _conf;

        public string URL = _conf["URL"];
        public string BrowserPath = _conf["BrowserPath"];
        //public string SaveDir = _conf["SaveDir"];
        public int OpenPageDelay = Convert.ToInt32(_conf["OpenPageDelay"]);
        public int WaitForLoad = Convert.ToInt32(_conf["WaitForLoad"]);
        public string CollectionName = _conf["PlaybookDatabase:PlaybookCollectionName"];
        public string TeamsCollection = "Teams";
        public string LeaguesCollection = _conf["PlaybookDatabase:LeaguesCollection"];

        public string GetFileName
        {
            get
            {
                var url = URL.Remove(0, 36).Replace('/', '_').Replace('-', '_');
                if (url.EndsWith('_'))
                {
                    url = url.TrimEnd('_');
                }
                return url;
            }
        }
        public string GetDirectory { get { return string.Format("..{0}", URL.Remove(0, 36).Replace('/', '\\')); } }
    }
}
