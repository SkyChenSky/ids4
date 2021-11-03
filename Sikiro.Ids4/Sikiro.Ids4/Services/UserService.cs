using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sikiro.Ids4.Services
{
    public class UserService
    {
        private List<Users> Users = new List<Users>
        {
            new Users {UserId = "0001", UserName = "chengong", Password = "123456", Phone = "18988551111"},
            new Users {UserId = "0002", UserName = "chengong2", Password = "123456", Phone = "18988551110"}
        };

        public UserService()
        {

        }
        public Users GetUser(string userName, string password)
        {
            return Users.FirstOrDefault(a => a.UserName == userName && a.Password == password);
        }
    }

    public class Users
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Phone { get; set; }
    }
}
