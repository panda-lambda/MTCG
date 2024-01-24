using System;
using MTCG.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MTCG.HttpServer;
using Newtonsoft.Json;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Npgsql;
using System.Net.Http;
using MTCG.Repositories;
using Microsoft.Extensions.Configuration;
using MTCG.Utilities;

namespace MTCG.Services
{
    public class GameService : IGameService
    {
        private ConcurrentQueue<Player> playerQueue = new ConcurrentQueue<Player>();

        private IBattleLogicService _battleLogicService;
        private readonly object queueLock = new object();
        private readonly ISessionService _sessionService;
        private readonly IUserRepository _userRepository;
        private readonly IPackageAndCardRepository _cardRepository;
        private readonly int _kFactor;

        public GameService(IBattleLogicService battleLogicService, ISessionService sessionService, IUserRepository userRepository, IPackageAndCardRepository cardRepository, IConfiguration configuration)
        {
            _battleLogicService = (BattleLogicService)battleLogicService ?? throw new ArgumentNullException(nameof(battleLogicService));
            _sessionService = (SessionService)sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _userRepository = (UserRepository)userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _cardRepository = (PackageAndCardRepository)cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
            string k = configuration["AppSettings:kFactor"] ?? throw new InvalidOperationException("JWT Secret Key is not configured properly.");
            _kFactor = Int32.Parse(k);
        }


        public void AddPlayer(Player player)
        {
            playerQueue.Enqueue(player);
            Console.WriteLine("added player: " + player.Name);

            StartBattle();
        }
        private void StartBattle()
        {
            lock (queueLock)
            {

                foreach (var p in playerQueue)
                {
                    Console.WriteLine("player in queue: " + p.Name);
                }
                if (playerQueue.Count >= 2)
                {
                    if (playerQueue.TryDequeue(out Player? playerOne) && playerQueue.TryDequeue(out Player? playerTwo))
                    {
                        var battle = new Battle
                        {
                            BattleID = Guid.NewGuid(),
                            PlayerOne = playerOne,
                            PlayerTwo = playerTwo,

                        };

                        ProcessBattle(battle);
                    }
                }
            }
        }


        private async void ProcessBattle(Battle battle)
        {
            Battle completedBattle = await _battleLogicService.ExecuteBattle(battle);
            await UpdateDatabaseWithBattleResults(completedBattle);
            await ReportLogToPlayer(completedBattle.PlayerOne.Client, completedBattle.LogPlayerOne);
            await ReportLogToPlayer(completedBattle.PlayerTwo.Client, completedBattle.LogPlayerTwo);

        }
        private async Task UpdateDatabaseWithBattleResults(Battle battle)
        {
            await Task.Run(() =>
            {
                if (battle.Result == null)
                {
                    throw new BattleResultIsNullException();
                }
                (battle.PlayerOne.Stats.Elo, battle.PlayerTwo.Stats.Elo) = CalculateElo(battle.PlayerOne, battle.PlayerTwo, (ResultType)battle.Result);
                if (battle.Result == ResultType.FirstPlayerWon)
                {
                    _userRepository.SetCoinsByUser(battle.PlayerTwo.Name, (int)_userRepository?.GetCoinsByUser(battle.PlayerTwo.Name) + 1);
                    battle.PlayerOne.Stats.Wins += 1;
                    battle.PlayerTwo.Stats.Losses += 1;

                }
                else if (battle.Result == ResultType.SecondPlayerWon)
                {
                    _userRepository.SetCoinsByUser(battle.PlayerOne.Name, (int)_userRepository?.GetCoinsByUser(battle.PlayerOne.Name) + 1);
                    battle.PlayerTwo.Stats.Wins += 1;
                    battle.PlayerOne.Stats.Losses += 1;
                }

                _cardRepository.UpdateCardsById(battle.PlayerOne.Id, battle.PlayerOne.Deck);
                _cardRepository.UpdateCardsById(battle.PlayerTwo.Id, battle.PlayerTwo.Deck);
                _userRepository.UpdateUserStats(battle.PlayerOne.Id, battle.PlayerOne.Stats);
                _userRepository.UpdateUserStats(battle.PlayerOne.Id, battle.PlayerOne.Stats);


            });

        }


        private (int, int) CalculateElo(Player playerOne, Player playerTwo, ResultType result)
        {
            double erwOne = 1 / (1 + Math.Pow(10, (playerTwo.Stats.Elo - playerOne.Stats.Elo) / 400.0));
            double erwTwo = 1 / (1 + Math.Pow(10, (playerOne.Stats.Elo - playerTwo.Stats.Elo) / 400.0));

            double scoreOne = result == ResultType.FirstPlayerWon ? 1 :
                  result == ResultType.SecondPlayerWon ? 0 : 0.5;
            double scoreTwo = result == ResultType.SecondPlayerWon ? 1 :
                  result == ResultType.FirstPlayerWon ? 0 : 0.5;

            int newEloPlayerOne = (int)Math.Round(playerOne.Stats.Elo + _kFactor * (scoreOne - erwOne));
            int newEloPlayerTwo = (int)Math.Round(playerTwo.Stats.Elo + _kFactor * (scoreTwo - erwTwo));

            return (newEloPlayerOne, newEloPlayerTwo);
        }


        private async Task ReportLogToPlayer(TcpClient client, List<string> log)
        {
            if (client == null)
            {
                await Console.Out.WriteLineAsync(" client IS NULL in ReportLOg\n\n");
                return;
            }
            else
            {
                string logString = string.Join("\n", log);
                string result = "{\"description\":\"" + logString + "\"}";
                HttpSvrEventArgs e = new HttpSvrEventArgs(client);
                e.Reply((int)HttpCodes.OK, result);
            }
            return;
        }


    }
}
