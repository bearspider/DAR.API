using HEAP.API.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HEAP.API.Services
{
    public interface IUserService
    {
        Task<User> Authenticate(string username, string password);
        Task<IEnumerable<User>> GetAll();
    }
    public class UserService : IUserService
    {
        private List<User> _users = new List<User>();
        public UserService(IConfiguration config)
        {
            User credential = new User();
            List<IConfigurationSection> userconfig = config.GetSection("RestCredentials").GetChildren().ToList();
            credential.Username = userconfig.Find(x => x.Key == "Username").Value;
            credential.Password = userconfig.Find(x => x.Key == "Password").Value;
            _users.Add(credential);
        }

        public async Task<User> Authenticate(string username, string password)
        {
            var user = await Task.Run(() => _users.SingleOrDefault(x => x.Username == username && x.Password == password));
            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so return user details without password and base64 for token.
            user.Username = username;
            user.Token = Convert.ToBase64String(
            System.Text.ASCIIEncoding.ASCII.GetBytes(
                string.Format("{0}:{1}", username, password)));
            
            user.Password = null;
            return user;
        }
        public async Task<IEnumerable<User>> GetAll()
        {
            // return users without passwords
            return await Task.Run(() => _users.Select(x => {
                x.Password = null;
                return x;
            }));
        }
    }
}
