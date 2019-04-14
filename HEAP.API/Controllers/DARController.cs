using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HEAP.API.Models;
using HEAP.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Newtonsoft.Json;

namespace HEAP.API.Controllers
{
    [Authorize]
    [Route("api/heap")]
    [ApiController]
    public class DARController : ControllerBase
    {
        private MongoService _mongo;
        public DARController(MongoService mongoService)
        {
            _mongo = mongoService;
        }

        //GET all packages in the database
        [AllowAnonymous]
        [HttpGet]
        public string Get()
        {
            return _mongo.GetPackages();
        }

        //CLEANUP database, anything over 30 minutes.  Job scheduled externally.
        [AllowAnonymous]
        [Route("cleanup")]
        [HttpGet]
        public void Clean()
        {
            _mongo.Clean();
        }

        //GET pacakge by GUID, this will come programmatically
        [AllowAnonymous]
        [HttpGet("{guid}")]
        public async Task<IActionResult> Get(string guid)
        {
            String filename = _mongo.GetFileName(guid);
            if(String.IsNullOrEmpty(filename))
            {
                return NotFound();
            }
            Stream memstream = await Task.Run(() => _mongo.GetFile(_mongo.GetFileObject(filename)));
            memstream.Seek(0, SeekOrigin.Begin);
            return File(memstream, "application/zip");
        }

        //POST a new zip file
        [Route("payload")]
        [HttpPost]
        public void Post(IFormFile file)
        {
            _mongo.AddFile(file.OpenReadStream(), file.FileName);
        }

        //POST a new package
        [Route("package")]
        [HttpPost]
        public void InsertPackage(Package package)
        {
            _mongo.AddPackage(package);
        }

        //DELETE a package which also will delete the payload chunk
        [HttpDelete("{guid}")]
        public void Delete(String guid)
        {
            _mongo.DeletePackage(guid);
        }
    }
}
