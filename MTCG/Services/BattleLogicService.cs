using MTCG.Models;
using MTCG.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Services
{
    public class BattleLogicService : IBattleLogicService

    {
        private readonly SessionService? _sessionService;
        private readonly PackageAndCardService? _packageService;
        private readonly UserRepository? _userRepository;
        public BattleLogicService(ISessionService sessionService, IPackageAndCardService packageService, IUserRepository userRepository)
        {

            _sessionService = (SessionService?)sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _packageService = (PackageAndCardService?)packageService ?? throw new ArgumentNullException(nameof(packageService));
            _userRepository = (UserRepository?)userRepository ?? throw new ArgumentNullException(nameof(userRepository));

        }
        public async Task<Battle> ExecuteBattle(Battle battle)
        {
            
            battle.LogPlayerOne.Add(" some log;");
            battle.LogPlayerTwo.Add("some other log");
            battle.Result = ResultType.FirstPlayerWon;
            battle.Rounds = 4; 

            await Task.Delay(100); 
            return battle; 
        }

    }
}