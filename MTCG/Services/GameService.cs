﻿using System;
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
        private ISessionServiceWithSessions _sessionService;

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
                    if (playerQueue.TryDequeue(out Player playerOne) && playerQueue.TryDequeue(out Player playerTwo))
                    {
                        //Console.WriteLine("got player one: " + playerOne.Name);
                        //Console.WriteLine("and player two: " + playerTwo.Name);
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
            


            //string logString = string.Join(", ", log);
            //await Console.Out.WriteLineAsync("userId " + userId + " mit log: " + logString);


            //UserSession session = _sessionService.GetSession(userId);
            //if (session != null)
            //{

            //    await Console.Out.WriteLineAsync($"session:  {session.Id} mit token {session.Token} and {session.Username}");
            //}
            //else
            //{
            //    await Console.Out.WriteLineAsync("session already null");
            //}

            //if (session.TCPClient != null)
            //{
            //    await Console.Out.WriteLineAsync("got from session : " + session.Username);
            //   await Response(session?.TCPClient, logString);


            //}
            //else
            //{
            //    await Console.Out.WriteLineAsync("client is null!");

            //}
            return;

        }

        private async Task Response(TcpClient Client, string? payload = null )
        {
            if (Client == null)
            {
                Console.WriteLine("Client is null!\n\n");
                return;
            }
            else
            {
                Console.WriteLine("client is not nuLL\n\n");
            }
            string statusDescription;
            statusDescription = "HTTP/1.1 200 OK\r\n";
            string headers = "Content-Type: application/json\r\n";
            if (!string.IsNullOrEmpty(payload))
            {
                int contentLength = Encoding.UTF8.GetByteCount(payload);
                headers += $"Content-Length: {contentLength}\r\n";
            }

            string fullResponse = statusDescription + headers + "\r\n" + payload + "\r\n\n";

            Console.WriteLine("Full response: " + fullResponse + "\n\n\n------");

            try
            {
                byte[] responseBytes = Encoding.UTF8.GetBytes(fullResponse);
                NetworkStream stream = Client.GetStream();
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                await stream.FlushAsync();
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("Client already disposed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending response: {ex.Message}");
            }
            finally
            {
                Client.Close();
                Client.Dispose();
            }
        }


    }
}
