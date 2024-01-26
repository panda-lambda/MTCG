using MTCG.Models;

namespace MTCG.Repositories
{
    public interface IUserRepository
    {
        bool registerUser(UserCredentials userCredentials);

        UserCredentials? GetHashByUsername(string username);

        Guid? GetGuidByUserName(string userName);

        int? GetCoinsByUserId(Guid userId);

        bool SetCoinsByUserId(Guid userId, int amount);

        UserData? GetUserData(Guid userId);

        bool UpdateUserData(Guid userId, UserData userData);

        UserStats GetUserStats(Guid userId);
        List<UserStats>? GetScoreboard();
        void UpdateUserStats(Guid id, UserStats stats);
    }
}
