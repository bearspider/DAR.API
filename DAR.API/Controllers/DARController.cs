using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DAR.API.Models;
using DAR.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DAR.API.Controllers
{
    [Route("api/dar")]
    [ApiController]
    public class DARController : ControllerBase
    {
        private MongoService _mongo;
        public DARController(MongoService mongoService)
        {
            _mongo = mongoService;
        }
        // GET: api/dar
        [HttpGet]
        public string Get()
        {
            return _mongo.GetPackages();
        }
        [Route("cleanup")]
        [HttpGet]
        public void Clean()
        {
            _mongo.Clean();
        }
        // GET: api/dar/id
        [HttpGet("{guid}")]
        public string Get(string guid)
        {
            return _mongo.GetFileName(guid);
        }
        // POST: api/dar
        [Route("payload")]
        [HttpPost]
        public void Post(IFormFile file)
        {
            _mongo.AddFile(file.OpenReadStream(), file.FileName);
        }
        [Route("package")]
        [HttpPost]
        public void InsertPackage(Package package)
        {
            _mongo.AddPackage(package);
        }
        // DELETE: api/dar/id
        [HttpDelete("{guid}")]
        public void Delete(String guid)
        {
            _mongo.DeletePackage(guid);
        }
    }
}
