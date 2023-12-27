using Microsoft.IdentityModel.Tokens;
using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Repositories;
using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;


namespace MTCG.Services
{
    public class SessionService : ISessionService
    {
        private readonly UserRepository? _userRepository; 
        private readonly SessionRepository? _sessionRepository; 
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
        public UserSession? AuthenticateUserAndSession(HttpSvrEventArgs e)
        {
            string? token = e.Headers?.FirstOrDefault(header => header.Name == "Authorization")?.Value;
            if (token == null)
            {
                e.Reply((int)HttpCodes.UNAUTORIZED, "Invalid username/password provided.");
                return null;
            }
            Guid? userId = ValidateToken(token);
            Console.WriteLine("userid"+ userId);
            if (userId == null || !userId.HasValue)
            {
                Console.WriteLine("userId is null in authenticateuserandsesion in session servioce");
                e.Reply((int)HttpCodes.UNAUTORIZED, "Invalid username/password provided.");
                return null;
            }
            UserSession? session = GetSession((Guid)userId);
            if (session != null && userId!= null) {
                Console.WriteLine("session guid with name : "+ _sessions[(Guid)userId].Username);
            }
            Console.WriteLine();
            return GetSession((Guid)userId);
           
        }

        public string AuthenticateAndCreateSession(UserCredentials userCredentials)
        {
            Console.WriteLine("im sessionservice");
            UserCredentials? user = _userRepository?.GetHashByUsername(userCredentials.Username);

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
            string token = GenerateJwtToken(userId, username);
            Console.WriteLine($"Sessioncreated at {createdAt} and expires at {expiresAt}");

            var session = new UserSession { Id = sessionId, Username = username, Token = token, CreatedAt = createdAt, ExpiresAt = expiresAt };

            _sessions.TryAdd(userId, session);
            return session.Token;

        }

        private Guid? ValidateToken(string token)

        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tempKey = Encoding.ASCII.GetBytes(key);
            try
            {
                Console.WriteLine("token string last 10:" + token.Substring(token.Length - 10) + "\n");
                if ("-mtcgToken" == token.Substring(token.Length - 10))
                {
                    string userName = token.Replace("-mtcgToken", "");
                    userName = userName.Replace("Bearer ", "");
                    Console.WriteLine("username in validatetoken: "+userName+".\n\n");

                    return _userRepository?.GetGuidByUserName(userName);
                }

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    //add issuer and audience?
                    ValidateIssuerSigningKey = true,

                    IssuerSigningKey = new SymmetricSecurityKey(tempKey),
                    ValidateLifetime = true, //check expiration
                    ClockSkew = TimeSpan.Zero //5 min tolerance
                }, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;
                var userNameClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "uid");

                if (userNameClaim != null)
                {
                    if (Guid.TryParse(userNameClaim?.Value, out Guid userId))
                    {
                        return userId;
                    }
                }
                return null;

            }
            catch (SecurityTokenExpiredException)
            {
                Console.WriteLine("token expired");
                Guid? userId = GetGuidFromExpiredToken(token);

                return userId;
            }
            catch
            {
                Console.WriteLine(" could not validdate token");

                return null;
            }
        }

        public Guid? GetGuidFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tempKey = Encoding.ASCII.GetBytes(key);

            try
            {
                // Validieren des Tokens, ohne das Ablaufdatum zu berücksichtigen
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(tempKey),
                    ValidateLifetime = false,
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userNameClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "uid");

                if (Guid.TryParse(userNameClaim?.Value, out Guid userId))
                {
                    return userId;
                }
                return null;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing token: {ex.Message}");
                return null;
            }
        }


        private string GenerateJwtToken(Guid id, string userName)
        {

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var header = new JwtHeader(credentials);
            var payload = new JwtPayload{
                 { "uid", id.ToString() }, //subject
                 { "sub",userName },//username
                 { "exp", DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds() }, //epxiration date
                 { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }, // issued at
            };

            var token = new JwtSecurityToken(header, payload);
            string tokenString = new JwtSecurityTokenHandler().WriteToken(token); //"header.payload.signature"

            return tokenString;



        }


        private bool AddSession(Guid userId, UserSession session)
        {
            return _sessions.TryAdd(userId, session);
        }

        private bool RemoveSession(Guid userId)
        {
            return _sessions.TryRemove(userId, out _);
        }

       private UserSession? GetSession(Guid userId)
        {
            _ = _sessions.TryGetValue(userId, value: out UserSession? session);

            return session;
        }

    }
}
