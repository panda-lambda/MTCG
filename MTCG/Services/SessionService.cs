using MTCG.Controller;
using MTCG.Models;
using MTCG.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Services
{
    public class SessionService : ISessionService
    {
        private readonly UserRepository? _userRepository; // = new UserRepository();
        private readonly SessionRepository? _sessionRepository; // = new UserRepository();

        public SessionService(ISessionRepository sessionRepository, IUserRepository userRepository)
        {
            _sessionRepository = (SessionRepository?)sessionRepository;
            _userRepository = (UserRepository?)userRepository;
        }


        public string AuthenticateAndCreateSession(UserCredentials userCredentials)
        {
            Console.WriteLine("im sessionservice");
            UserCredentials? user = _userRepository?.GetUserByUsername(userCredentials.Username);
            if (user == null || string.IsNullOrEmpty(user.Password) || !(user.Id.HasValue) || !VerifyPassword(userCredentials.Password, user.Password))
            {
                return string.Empty;
            }

            Console.WriteLine($"User {user.Username} with hased pw: {user.Password}");
            //var token = GenerateJwtToken(user)..
            string token = "testtoken";
            //store token in threadsafe dictionary!!!
            //and maybe some other infors?

            return token;
        }

        internal static bool VerifyPassword(string password, string hash)
        {

            return true;
        }

    }
}
