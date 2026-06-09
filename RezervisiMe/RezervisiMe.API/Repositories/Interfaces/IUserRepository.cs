using RezervisiMe.RezervisiMe.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RezervisiMe.RezervisiMe.API.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        User GetByUsername(string username);    
        bool UsernameExists(string username); 
    }
}