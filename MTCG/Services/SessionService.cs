﻿using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Repositories;
using MTCG.Utilities;
using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;


namespace MTCG.Services
{
    public class SessionService : ISessionService,ISessionServiceWithSessions
    {
        private readonly UserRepository? _userRepository;
        private readonly SessionRepository? _sessionRepository;
        private readonly ConcurrentDictionary<Guid, UserSession> _sessions;

        private readonly string? _key;
        private readonly bool _inDebugMode;
        private const int KeySize = 32;


        public SessionService(ISessionRepository sessionRepository, IUserRepository userRepository, IConfiguration configuration)
        {
            _sessionRepository = (SessionRepository?)sessionRepository;
            _userRepository = (UserRepository?)userRepository;
            _sessions = new ConcurrentDictionary<Guid, UserSession>();

            _key = configuration["AppSettings:SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key is not configured properly.");
            string? debugConfig = configuration["AppSettings:Debug"];
            if (!string.IsNullOrEmpty(debugConfig))
            {
                bool.TryParse(debugConfig, out _inDebugMode);
            }
        }
        public Guid AuthenticateUserAndSession(HttpSvrEventArgs e, string? username)
        {
            try
            {
                string? token = e.Headers?.FirstOrDefault(header => header.Name == "Authorization")?.Value;
                if (token == null)
                {
                    if (_inDebugMode) Console.WriteLine("EXEption in in authenticateUserAndSession - SessionService");
                    throw new UnauthorizedException();

                }
                Guid? userId = ValidateToken(token);
                if (userId == null || !userId.HasValue)
                {
                    if (_inDebugMode) Console.WriteLine("userId is null in authenticateuserandsesion in session servioce");
                    throw new UnauthorizedException();
                }

                UserSession? session = GetSession((Guid)userId);


                if (session == null || username != null && session?.Username != username)
                {
                    if (_inDebugMode) Console.WriteLine("username not matching in authenticateUserAndSession - SessionService");
                    throw new UnauthorizedException();
                }
                if (session.IsFighting)
                {
                    throw new UserCurrentlyFightingException();
                }

                return (Guid)userId;
            }
            catch (UnauthorizedException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string AuthenticateAndCreateSession(UserCredentials userCredentials, TcpClient client)
        {
            try
            {

                if(client == null)
                {
                    Console.WriteLine(" client is NULL in auth and create session");
                }
                else
                {
                    Console.WriteLine("client is NOT NUll in auth and create session");
                }
                UserCredentials? user = _userRepository?.GetHashByUsername(userCredentials.Username);
                if (user == null || user.Username == null)
                {
                    throw new UserNotFoundException("");
                }

                if (string.IsNullOrEmpty(user.Password) || !(user.Id.HasValue) || !VerifyPassword(userCredentials.Password, user.Password))
                {
                    throw new UnauthorizedException();
                }
                Console.WriteLine($"User {user.Username} with hased pw: {user.Password}");
                return CreateSession((Guid)user.Id, userCredentials.Username, client);
            }
          catch(Exception e)
            {
                Console.WriteLine("something wnent wrong in authenticae and create session");
                Console.WriteLine(e.Message        );
                return ""; 
            }



        }


        internal string CreateSession(Guid userId, string username, TcpClient client)
        {
            Guid sessionId = Guid.NewGuid();
            long createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long expiresAt = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();
            string token = GenerateJwtToken(userId, username);
            Console.WriteLine($"Sessioncreated at {createdAt} and expires at {expiresAt}");

            var session = new UserSession { Id = sessionId, Username = username, Token = token, CreatedAt = createdAt, ExpiresAt = expiresAt, TCPClient = client };
            if (session.TCPClient != null)
            {
                Console.WriteLine("client in createSession NOT NULL!\n\n\n");
            }
            else
            {
                Console.WriteLine("client in createSession IS NULL!\n\n\n");
            }
            _sessions.TryAdd(userId, session);
            return session.Token;

        }
        private static bool VerifyPassword(string inputPassword, string hash)
        {
            //try
            string[] parts = hash.Split('.');
            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var originalKey = parts[2];

            // same stuff again
            using (var algorithm = new Rfc2898DeriveBytes(
                inputPassword,
                salt,
                iterations,
                HashAlgorithmName.SHA256))
            {
                var inputKey = Convert.ToBase64String(algorithm.GetBytes(KeySize));
                Console.WriteLine($"comparing {inputKey} input vs {originalKey} the orginal\n");
                if (inputKey == originalKey)
                    Console.WriteLine("user succesfully authenticated!\n");
                return inputKey == originalKey;
            }
        }

        private Guid? ValidateToken(string token)

        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tempKey = Encoding.ASCII.GetBytes(_key);
            try
            {
                Console.WriteLine("token string last 10:" + token.Substring(token.Length - 10) + "\n");
                if ("-mtcgToken" == token.Substring(token.Length - 10))
                {
                    string userName = token.Replace("-mtcgToken", "");
                    userName = userName.Replace("Bearer ", "");
                    Console.WriteLine("username in validatetoken: " + userName + ".\n\n");


                    Guid? userId = _userRepository?.GetGuidByUserName(userName);
                    Console.WriteLine("userId in validate token " + userId?.ToString());
                    return userId;
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
            var tempKey = Encoding.ASCII.GetBytes(_key);

            try
            {
                // nur validieren, ohne timecheck und hole userid claim
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

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
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

       public UserSession GetSession(Guid userId)
        {
            _ = _sessions.TryGetValue(userId, value: out UserSession? session);

            return session;
        }

        public TcpClient? GetClientFromSession (Guid userId)
        {
            _ = _sessions.TryGetValue(userId, value: out UserSession? session);

            return session?.TCPClient;
        }


        public string? GetUsernameFromSession(Guid userId)
        {
            _ = _sessions.TryGetValue(userId, value: out UserSession? session);
            return session?.Username;

        }
        public void SetFightingState(Guid userId, bool isFighting)
        {
            _ = _sessions.TryGetValue(userId, value: out UserSession? session);
            if (session != null)
                session.IsFighting = isFighting;
        }
    }
}
