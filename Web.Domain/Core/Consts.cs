﻿using Microsoft.Extensions.Configuration;

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
        public string LeaguesCollection = "leagues2";

        public string GetFileName { get { return URL.Remove(0, 36).Replace('/', '_'); } }
        public string GetDirectory { get { return string.Format("..{0}", URL.Remove(0, 36).Replace('/', '\\')); } }
    }
}
