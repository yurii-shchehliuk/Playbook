namespace Scrapping.Core
{
    public class Consts
    {
        public static IConfiguration _conf;

        public string URL = _conf["URL"];
        public string BrowserPath = _conf["BrowserPath"];
        public string SaveDir = _conf["SaveDir"];
        public int Delay = Convert.ToInt32(_conf["Delay"]);

        public string GetFileName { get { return URL.Remove(0, 36).Replace('/', '_'); } }
        public string GetDirectory { get { return string.Format("..{0}", URL.Remove(0, 36).Replace('/','\\')); } }
    }
}
