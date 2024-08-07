﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Domain.Entities
{
    public class League : BaseEntity
    {
        [BsonElement("name")]
        public string Name { get; set; }
        //public string machineName { get; set; }
        [BsonElement("country")]
        public CountryObj Country { get; set; }
        [BsonElement("flashscoreLink")]
        public string FlashscoreLink { get; set; }
        [BsonElement("footystatsLink")]
        public string FootystatsLink { get; set; }
        [BsonElement("tdslLink")]
        public string TdslLink { get; set; }
        [BsonElement("whoScoredLink")]
        public string WhoScoredLink { get; set; }

        public class CountryObj
        {
            [BsonElement("name")]
            public string Name { get; set; }
            [BsonElement("code")]
            public string Code { get; set; }
        }

        public string GetFileName
        {
            get
            {
                var url = FlashscoreLink.Remove(0, 36).Replace('/', '_').Replace('-', '_').Replace("results", "");
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
