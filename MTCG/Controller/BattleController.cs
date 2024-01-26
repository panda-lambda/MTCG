using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Services;
using MTCG.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MTCG.Controller
{
    /// <summary>
    /// Controls the battle logic and handles related request
    /// </summary>
    public class BattleController : BaseController
    {
        private IPackageAndCardService _packageService;
        private ISessionService _sessionService;
        private IBattleService _battleService;

        public BattleController(IPackageAndCardService packageAndCardService, ISessionService sessionService, IBattleService battleService)
        {
            _packageService = packageAndCardService ?? throw new ArgumentNullException(nameof(packageAndCardService));
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _battleService = battleService ?? throw new ArgumentNullException(nameof(battleService));
        }

        /// <summary>
        /// Handles the request and calls the appropriate method.
        /// </summary>
        /// <param name="e">HttpSvrEvenArgs</param>
        public override void HandleRequest(HttpSvrEventArgs e)
        {
            if (e.Path.StartsWith("/stats") && e.Method == "GET")
            {
                ExecuteWithExceptionHandling(e, GetStatsByUser);
            }

            if (e.Path.StartsWith("/scoreboard") && e.Method == "GET")
            {
                ExecuteWithExceptionHandling(e, GetScoreboard);
            }
            if (e.Path.StartsWith("/battles") && e.Method == "POST")
            {
                ExecuteWithExceptionHandling(e, StartBattle);
               
            }
        }


        /// <summary>
        /// Gets the sorted scoreboard for all users.
        /// </summary>
        /// <param name="e">HTTPServerEventArgs</param>
        /// <exception cref="InternalServerErrorException"></exception>
        private void GetScoreboard(HttpSvrEventArgs e)
        {
            List<UserStats> scoreboard = _battleService.GetScoreboard(e);
            if (scoreboard == null)
            {
                throw new InternalServerErrorException("scoreboard has no entries");

            }
            //obviously admin should be excluded from the scoreboard,should use user roles, but test script & yaml lack definition
            scoreboard.RemoveAll(stat => stat.Name.StartsWith("admin"));
            e.Reply((int)HttpCodes.OK, System.Text.Json.JsonSerializer.Serialize(scoreboard));

        }


        /// <summary>
        /// Starts the battle
        /// </summary>
        /// <param name="e">HTTPSvrEventArgs</param>
        private void StartBattle(HttpSvrEventArgs e)
        {
            _battleService.StartBattle(e);

        }


        /// <summary>
        /// Get the statistics of a single user.
        /// </summary>
        /// <param name="e"></param>
        private void GetStatsByUser(HttpSvrEventArgs e)
        {
            UserStats userStats = _battleService.GetUserStats(e);
            e.Reply((int)HttpCodes.OK, System.Text.Json.JsonSerializer.Serialize(userStats));

        }

    }
}
