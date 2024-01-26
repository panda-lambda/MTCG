using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Repositories;
using System.Security.Cryptography;
using MTCG.Utilities;
using Newtonsoft.Json;


namespace MTCG.Services
{
    internal class UserService : IUserService
    {
        private readonly UserRepository? _userRepository;
        private readonly SessionService? _sessionService;
        private const int SaltSize = 16; // 128 bit 
        private const int KeySize = 32;  // 256 bit
        private const int Iterations = 10000;  // iterations


        public UserService(IUserRepository userRepository, ISessionService sessionService)
        {
            _userRepository = (UserRepository?)userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _sessionService = (SessionService?)sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        }

        internal static string HashPassword(string password)
        {
            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                SaltSize,
                Iterations,
                HashAlgorithmName.SHA256))
            {
                var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
                var salt = Convert.ToBase64String(algorithm.Salt);

                return $"{Iterations}.{salt}.{key}";
            }
        }


        public bool CreateUser(string username, string password)
        {
            string hashedPassword = HashPassword(password);
            Console.WriteLine(hashedPassword);

            UserCredentials userCredentials = new() { Username = username, Password = hashedPassword };

            return _userRepository != null && _userRepository.registerUser(userCredentials);
           


        }

        public bool UpdateUserData(HttpSvrEventArgs e)
        {
            string? usernameClaim = e.Path.Replace("/users/", "");
            Guid? userId = _sessionService?.AuthenticateUserAndSession(e, usernameClaim);
            if (usernameClaim == null || userId == null && usernameClaim != "admin")
            {
                throw new UnauthorizedException();
            }
            if (usernameClaim == "admin")
            {
                userId = _sessionService?.AuthenticateUserAndSession(e, null);
            }
            if (userId == Guid.Empty || userId == null)
            {
                throw new UnauthorizedException();
            }


            UserData? userData = JsonConvert.DeserializeObject<UserData>(e.Payload);
            if (userData == null)
            {
                throw new UnauthorizedException();
            }
            return _userRepository != null && _userRepository.UpdateUserData((Guid)userId, userData);


        }


        public UserData? GetUserData(HttpSvrEventArgs e)
        {
            string? usernameClaim = e.Path.Replace("/users/", "");
            if (usernameClaim == null)
            {
                throw new BadRequestException("No username provided.");
            }
            Guid userId = _sessionService.AuthenticateUserAndSession(e, usernameClaim);

            return _userRepository?.GetUserData((Guid)userId);
        }


    }
}

