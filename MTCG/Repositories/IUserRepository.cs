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
        public UserCredentials? GetHashByUsername (string username);
        public Guid? GetGuidByUserName(string userName);

        public int? GetCoinsByUser(string username);

        public bool SetCoinsByUser(string username, int amount);

        public UserData? GetUserData(Guid userId);



    }
}
