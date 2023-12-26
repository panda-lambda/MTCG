using Microsoft.IdentityModel.Tokens;
using MTCG.Models;
using MTCG.Repositories;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;


namespace MTCG.Services
{
    public class SessionService : ISessionService
    {
        private readonly UserRepository? _userRepository; // = new UserRepository();
        private readonly SessionRepository? _sessionRepository; // = new UserRepository();
        private readonly ConcurrentDictionary<Guid, UserSession> _sessions;
        private readonly UserService? _userService;
        private readonly string key = Guid.NewGuid().ToString();


        public SessionService(ISessionRepository sessionRepository, IUserRepository userRepository, IUserService userService)
        {
            _sessionRepository = (SessionRepository?)sessionRepository;
            _userRepository = (UserRepository?)userRepository;
            _sessions = new ConcurrentDictionary<Guid, UserSession>();
            _userService = (UserService)userService;
        }


        public string AuthenticateAndCreateSession(UserCredentials userCredentials)
        {
            Console.WriteLine("im sessionservice");
            UserCredentials? user = _userRepository?.GetUserByUsername(userCredentials.Username);

            if (user == null || string.IsNullOrEmpty(user.Password) || !(user.Id.HasValue) || (_userService != null && !_userService.VerifyPassword(userCredentials.Password, user.Password)))
            {
                return string.Empty;
            }
            Console.WriteLine($"User {user.Username} with hased pw: {user.Password}");

            string token = CreateSession((Guid)user.Id, userCredentials.Username);


            return token;
        }


        internal string CreateSession(Guid userId, string username)
        {
            Guid sessionId = Guid.NewGuid();
            long createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long expiresAt = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();
            string token = GenerateJwtToken(userId);
            Console.WriteLine($"Sessioncreated at {createdAt} and expires at {expiresAt}");

            var session = new UserSession { Id = sessionId, Username = username, Token = token, CreatedAt = createdAt, ExpiresAt = expiresAt };

            _sessions.TryAdd(userId, session);
            return session.Token;

        }

        public bool ValidateToken(string token)

        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tempKey = Encoding.ASCII.GetBytes(key);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    //add issuer and audience?
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(tempKey),
                    ValidateLifetime = true, //check expiration
                    ClockSkew = TimeSpan.Zero //5 min tolerance
                },out _);

                Console.WriteLine("validated token!");
                return true;
            }
            catch
            {
                Console.WriteLine(" coud not validate token!");

                return false;
            }
        }

        private string GenerateJwtToken(Guid id)
        {

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var header = new JwtHeader(credentials);
            var payload = new JwtPayload{
         { "sub", id.ToString() }, //subject
         { "exp", DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds() }, //epxiration date
         { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }, // issued at
            };

            var token = new JwtSecurityToken(header, payload);
            string tokenString = new JwtSecurityTokenHandler().WriteToken(token); //"header.payload.signature"

            return tokenString;



        }


        public bool AddSession(Guid userId, UserSession session)
        {
            return _sessions.TryAdd(userId, session);
        }

        public bool RemoveSession(Guid userId)
        {
            return _sessions.TryRemove(userId, out _);
        }

        public UserSession? GetSession(Guid userId)
        {
            _ = _sessions.TryGetValue(userId, value: out UserSession? session);

            return session;
        }

    }
}
