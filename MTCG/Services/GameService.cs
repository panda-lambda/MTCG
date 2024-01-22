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

namespace MTCG.Services
{
    public class GameService : IGameService
    {
        private ConcurrentQueue<Player> playerQueue = new ConcurrentQueue<Player>();

        private IBattleLogicService _battleLogicService;
        private readonly object queueLock = new object();
        private ISessionService _sessionService;

        public GameService(IBattleLogicService battleLogicService, ISessionService sessionService)
        {
            _battleLogicService = (BattleLogicService)battleLogicService ?? throw new ArgumentNullException(nameof(battleLogicService));
            _sessionService = (SessionService)sessionService ?? throw new ArgumentNullException(nameof(sessionService));
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

                foreach(var p in playerQueue)
                {
                    Console.WriteLine("player in queue: "+ p.Name);

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
            battle.LogPlayerOne.Add(completedBattle.PlayerOne.Name);
            battle.LogPlayerTwo.Add(completedBattle.PlayerTwo.Name);

            await ReportLogToPlayer(completedBattle.PlayerOne.Client, completedBattle.LogPlayerOne);
            await ReportLogToPlayer(completedBattle.PlayerTwo.Client, completedBattle.LogPlayerTwo);

        }

        private async Task UpdateDatabaseWithBattleResults(Battle battle)
        {
            await Task.Delay(500);
            return;
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
              
                await Console.Out.WriteLineAsync("client not null in reportlog!!\n\n");
                await Console.Out.WriteLineAsync($"Connected: {client.Connected}");
                await Console.Out.WriteLineAsync($"Client stream: {client.GetStream}");  

                string logString = string.Join(", ", log)+"\n\r\n\r";
                
                HttpSvrEventArgs e = new HttpSvrEventArgs(client);


                e.Reply((int)HttpCodes.OK, JsonConvert.SerializeObject(logString));
            }       


            return;

        }


    }
}
