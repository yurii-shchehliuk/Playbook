﻿using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class BaseEntity
    {
        [BsonId]
        //[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
    }
}
