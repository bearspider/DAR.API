using DAR.API.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DAR.API.Services
{
    public class MongoService
    {
        private readonly IMongoCollection<Package> _packages;
        private readonly IMongoDatabase _db;
        private readonly IGridFSBucket _bucket;

        public MongoService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("DarDb"));
            _db = client.GetDatabase("dardb");
            _packages = _db.GetCollection<Package>("Packages");
            _bucket = new GridFSBucket(_db);
        }
        public void Clean()
        {
            List<Package> packages = _packages.Find(_ => true).ToList();
            foreach (Package package in packages)
            {
                if ((DateTime.Now - package.UploadTime).TotalMinutes > 30)
                {
                    DeletePackage(package.Guid);
                    DeleteChunk(package.Payload);
                }
            }
        }
        public void Initialize()
        {
            Package package = new Package();
            _packages.InsertOne(package);
        }
        public void DeletePackage(String guid)
        {
            _packages.FindOneAndDelete(x => x.Guid == guid);
        }
        public void DeleteChunk(String filename)
        {
            FilterDefinition<GridFSFileInfo> filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, filename);
            ObjectId todelete = _bucket.Find(filter).First().Id;
            _bucket.Delete(todelete);
        }
        public String GetPackages()
        {
            List<Package> packages = _packages.Find(_ => true).ToList();
            JArray jsonarray = new JArray();
            foreach (Package package in packages)
            {
                JObject jobject = new JObject(
                    new JProperty("Guid", package.Guid),
                    new JProperty("Date", package.UploadTime),
                    new JProperty("Payload", package.Payload)
                    );
                jsonarray.Add(jobject);
            }
            return JsonConvert.SerializeObject(jsonarray);
        }
        public Package GetPackage(String guid)
        {
            Package rval = new Package();
            var results = _packages.Find(x => x.Guid == guid).ToList();
            rval = results.First();
            return rval;
        }
        public ObjectId AddFile(Stream fileStream, string fileName)
        {
            ObjectId id = _bucket.UploadFromStream(fileName, fileStream);
            return id;
        }

        public Stream GetFile(ObjectId id)
        {
            Stream destination = new MemoryStream();
            _bucket.DownloadToStream(id, destination);
            return destination;
        }
        public void AddPackage(Package newpackage)
        {
            newpackage.UploadTime = DateTime.Now;
            _packages.InsertOne(newpackage);
        }
        public void CloseStream(Stream stream)
        {
            stream.Close();
        }
        public String GetFileName(String guid)
        {
            return _packages.Find(x => x.Guid == guid).First().Payload;
        }
    }
}
