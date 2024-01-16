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
    public class BattleService

    {
        private readonly SessionService? _sessionService;
        private readonly PackageAndCardService? _packageService;
        public BattleService(ISessionService sessionService,IPackageAndCardService packageService )
        {
           
            _sessionService = (SessionService? )sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _packageService = (PackageAndCardService?)packageService ?? throw new ArgumentNullException(nameof(packageService));
           
            
        }
        public string? StartBattle(HttpSvrEventArgs e)
        {
            Guid? userId = _sessionService?.AuthenticateUserAndSession(e, null);
            if (userId == null || userId == Guid.Empty)
            {
                throw new UnauthorizedException("User is not logged in");
            }

            if (!CheckForValidDeck(e))
            {
                throw new UserHasNoValidDeckException("");
            }


            return null; 
        }

        private bool CheckForValidDeck(HttpSvrEventArgs e)
        {

            return (bool)_packageService.CheckForValidDeck(e);
        }

    }
}
