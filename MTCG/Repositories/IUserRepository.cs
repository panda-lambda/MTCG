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
        bool registerUser(UserCredentials userCredentials);

        UserCredentials? GetHashByUsername(string username);

        Guid? GetGuidByUserName(string userName);

        int? GetCoinsByUser(string username);

        bool SetCoinsByUser(string username, int amount);

        UserData? GetUserData(Guid userId);

        bool UpdateUserData(Guid userId, UserData userData);

        UserStats GetUserStats(Guid userId);
        List<UserStats>? GetScoreboard();
        void UpdateUserStats(Guid id, UserStats stats);
    }
}
