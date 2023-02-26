﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Domain.Entities
{
    public abstract class BaseEntity
    {
        [BsonId]
        //[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public virtual string Id { get; set; }
    }

    //public partial class BaseEntity<ObjectId>
    //{
    //    [BsonId]
    //    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    //    public ObjectId Id { get; set; }
    //}
}
