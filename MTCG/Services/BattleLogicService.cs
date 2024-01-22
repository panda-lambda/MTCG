using MTCG.Models;
using MTCG.Repositories;
using MTCG.Services.RuleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            Deck deckOne = battle.PlayerOne.Deck;
            Deck deckTwo = battle.PlayerTwo.Deck;


            while (deckOne?.CardList?.Count > 0 && deckTwo?.CardList?.Count > 0 && battle.Rounds < 101)
            {

                Card cardOne = GetRandomCard(deckOne.CardList);
                Card cardTwo = GetRandomCard(deckTwo.CardList);
                ResultType? result = HandleSingleCardFight(cardOne, cardTwo);

                if (result == ResultType.FirstPlayerWon)
                {
                    deckTwo.CardList.Remove(cardTwo);
                    deckOne.CardList.Add(cardTwo);
                    battle.LogPlayerOne.Add($"Round {battle.Rounds}: You won against {battle.PlayerTwo.Name} and gained the card {cardTwo.Name} with {cardTwo.Damage}");
                    battle.LogPlayerTwo.Add($"Round {battle.Rounds}: You lost against {battle.PlayerOne.Name} and lost the card {cardTwo.Name} with {cardTwo.Damage}");
                }
                else if (result == ResultType.SecondPlayerWon)
                {
                    deckOne.CardList.Remove(cardOne);
                    deckTwo.CardList.Add(cardOne);
                    battle.LogPlayerOne.Add($"Round {battle.Rounds}: You lost against {battle.PlayerTwo.Name}  and lost the card {cardOne.Name} with {cardOne.Damage}");
                    battle.LogPlayerTwo.Add($"Round {battle.Rounds}: You won against {battle.PlayerOne.Name}  and gained the card {cardOne.Name} with {cardOne.Damage}");
                }
                else
                {
                    battle.LogPlayerOne.Add($"Round {battle.Rounds}: Draw against {battle.PlayerTwo.Name}.");
                    battle.LogPlayerTwo.Add($"Round {battle.Rounds}: Draw against {battle.PlayerOne.Name} ");

                }
                battle.Rounds++;

            }

            if (battle.Rounds == 101)
            {
                battle.Result = ResultType.Draw;
                battle.LogPlayerOne.Add($"The battle ended in a draw after 100 rounds");
                battle.LogPlayerTwo.Add($"The battle ended in a draw after 100 rounds");
            }
            else if (deckOne?.CardList?.Count == 0)
            {
                battle.Result = ResultType.SecondPlayerWon;
                battle.LogPlayerOne.Add($"You lost against {battle.PlayerTwo.Name} after {battle.Rounds} rounds");
                battle.LogPlayerTwo.Add($"You won against {battle.PlayerOne.Name} after {battle.Rounds} rounds");
            }
            else if (deckTwo?.CardList?.Count == 0)
            {
                battle.Result = ResultType.FirstPlayerWon;
                battle.LogPlayerOne.Add($"You won against {battle.PlayerTwo.Name} after {battle.Rounds} rounds");
                battle.LogPlayerTwo.Add($"You lost against {battle.PlayerOne.Name} after {battle.Rounds} rounds");
            }
            return battle;
        }

        private static Card GetRandomCard(List<Card> deck)
        {
            Random rnd = new Random();
            int index = rnd.Next(0, deck.Count);
            return deck[index];
        }

        private static ResultType? HandleSingleCardFight(Card cardOne, Card cardTwo)
        {
            ResultType? result = null; 
            if (cardOne == null || cardTwo == null)
                throw new Exception("No cards in handle Single fight!");

            Engine engine = new();
            result = engine.Evaluate(cardOne, cardTwo); 



            return result;
        }

        


    }
}