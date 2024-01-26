using MTCG.Models;
using System.Net.Sockets;
using NUnit.Framework;
using Newtonsoft.Json.Linq;

namespace UnitTests
{
    public class TestElo
    {

        private int _kFactor = 20;
        private Player playerOne;
        private Player playerTwo;
        [SetUp]
        public void Setup()
        {
            Deck deck = new();
            playerOne = new Player { Client = new TcpClient(), Deck = deck, Name = "a", Stats = new UserStats { Elo = 1200 } };
            playerTwo = new Player { Client = new TcpClient(), Deck = deck, Name = "a", Stats = new UserStats { Elo = 1200 } };
        }

        private (int, int) CalculateEloTest(Player playerOne, Player playerTwo, ResultType result)
        {
            double erwOne = 1 / (1 + Math.Pow(10, (playerTwo.Stats.Elo - playerOne.Stats.Elo) / 400.0));
            double erwTwo = 1 / (1 + Math.Pow(10, (playerOne.Stats.Elo - playerTwo.Stats.Elo) / 400.0));

            double scoreOne = result == ResultType.FirstPlayerWon ? 1 :
                  result == ResultType.SecondPlayerWon ? 0 : 0.5;
            Console.WriteLine($"{scoreOne} sc1");
            double scoreTwo = result == ResultType.SecondPlayerWon ? 1 :
                  result == ResultType.FirstPlayerWon ? 0 : 0.5;
            Console.WriteLine($"{scoreTwo} sc2");

            int newEloPlayerOne = (int)Math.Round(playerOne.Stats.Elo + _kFactor * (scoreOne - erwOne));
            int newEloPlayerTwo = (int)Math.Round(playerTwo.Stats.Elo + _kFactor * (scoreTwo - erwTwo));

            Console.WriteLine($"player one got {newEloPlayerOne} and player two got {newEloPlayerTwo} elo");

            return (newEloPlayerOne, newEloPlayerTwo);
        }

        [Test]
        public void PlayerOneWins()
        {
            int eloOne, eloTwo;
            (eloOne, eloTwo) = CalculateEloTest(playerOne, playerTwo, ResultType.FirstPlayerWon);
            Assert.That(eloOne, Is.EqualTo(1210), "PlayerOne Wins not greater than start");
            Assert.That(eloTwo, Is.EqualTo(1190), "PlayerOne Wins not greater than start");
        }

        [Test]
        public void PlayerTwoWins()
        {
            int eloOne, eloTwo;
            (eloOne, eloTwo) = CalculateEloTest(playerOne, playerTwo, ResultType.SecondPlayerWon);
            Assert.That(eloTwo, Is.EqualTo(1210), "PlayerTwo Wins not greater than start");
            Assert.That(eloOne, Is.EqualTo(1190), "PlayerTwo Wins not greater than start");
        }

        [Test]
        public void Draw()
        {
            int eloOne, eloTwo;
            (eloOne, eloTwo) = CalculateEloTest(playerOne, playerTwo, ResultType.Draw);
            Assert.That(eloOne, Is.EqualTo(1200), "PlayerTwo draw not same as start");
            Assert.That(eloTwo, Is.EqualTo(1200), "PlayerOne draw not same as start");
        }


    }
}