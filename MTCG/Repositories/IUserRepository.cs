using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Repositories
{
    public interface IUserRepository
    {
        public bool registerUser(UserCredentials userCredentials);
        public UserCredentials? GetUserByUsername (string username);  

    }
}
