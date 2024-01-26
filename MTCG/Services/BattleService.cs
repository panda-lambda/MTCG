using Microsoft.Extensions.Configuration;
using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Repositories;
using MTCG.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Services
{
    public class BattleService : IBattleService

    {
        private readonly SessionService? _sessionService;
        private readonly PackageAndCardService? _packageService;
        private readonly UserRepository? _userRepository;
        private readonly GameService? _gameService;
        public BattleService(ISessionService sessionService, IPackageAndCardService packageService, IUserRepository userRepository, IGameService gameservice)
        {

            _sessionService = (SessionService?)sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _packageService = (PackageAndCardService?)packageService ?? throw new ArgumentNullException(nameof(packageService));
            _userRepository = (UserRepository?)userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _gameService = (GameService?)gameservice ?? throw new ArgumentNullException(nameof(gameservice));

        }

        /// <summary>
        /// authenticates user, checks for valid deck and adds the player to the game
        /// </summary>
        /// <param name="e"></param>
        /// <exception cref="UnauthorizedException"> no valid token</exception>
        /// <exception cref="ForbiddenException"> no valid deck</exception>
        public void StartBattle(HttpSvrEventArgs e)
        {
            Guid? userId = _sessionService?.AuthenticateUserAndSession(e, null);
            if (userId == null || userId == Guid.Empty)
            {
                throw new UnauthorizedException();
            }

            if (!CheckForValidDeck((Guid)userId))
            {
                throw new ForbiddenException("Your deck is not valid to start a battle!");
            }
            Player player = new Player
            {
                Id = (Guid)userId,
                Deck = _packageService!.GetDeckByUser(e),
                Stats = _userRepository!.GetUserStats((Guid)userId),
                Name = _userRepository!.GetNameByGuid(userId),
                Client = e.Client
            };

            _gameService?.AddPlayer(player);

        }

        /// <summary>
        /// checks if the deck is valid
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>true if valid</returns>

        private bool CheckForValidDeck(Guid userId)
        {
            return (_packageService != null && (bool)_packageService.CheckForValidDeck(userId));
        }

        /// <summary>
        /// gets the user stats for the battle object
        /// </summary>
        /// <param name="e"></param>
        /// <returns>user stats</returns>
        /// <exception cref="UnauthorizedException">invalid token</exception>
        /// <exception cref="InternalServerErrorException"></exception>
        public UserStats GetUserStats(HttpSvrEventArgs e)
        {
            Guid? userId = _sessionService?.AuthenticateUserAndSession(e, null);
            if (userId == null)
            {
                throw new UnauthorizedException();
            }
            UserStats? userStats = _userRepository?.GetUserStats((Guid)userId);
            if (userStats == null)
            {
                throw new InternalServerErrorException("something went wrong -getting the user stats");
            }
            return (UserStats)userStats;
        }


        /// <summary>
        /// gets the scoreboard
        /// </summary>
        /// <param name="e"></param>
        /// <returns>list of user stats</returns>
        /// <exception cref="UnauthorizedException">invalid token</exception>
        /// <exception cref="InternalServerErrorException"></exception>
        public List<UserStats> GetScoreboard(HttpSvrEventArgs e)
        {
            Guid? userId = _sessionService?.AuthenticateUserAndSession(e, null);
            if (userId == null)
            {
                throw new UnauthorizedException();
            }
            List<UserStats>? scoreboard = _userRepository?.GetScoreboard();
            if (scoreboard == null)
            {
                throw new InternalServerErrorException("something went wrong -getting the scoreboard");
            }
            return scoreboard;
            ;
        }

    }
}
