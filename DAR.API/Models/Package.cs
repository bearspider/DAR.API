using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAR.API.Models
{
    public class Package
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        [BsonElement("Guid")]
        public string Guid { get; set; }
        [BsonElement("Date")]
        public DateTime UploadTime { get; set; }
        [BsonElement("Payload")]
        public String Payload { get; set; }

        public Package()
        {

        }
    }
}
