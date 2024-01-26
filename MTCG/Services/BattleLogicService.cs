using Microsoft.Extensions.Configuration;
using MTCG.Models;
using MTCG.Repositories;
using MTCG.Services.RuleEngine;


namespace MTCG.Services
{
    public class BattleLogicService : IBattleLogicService

    {
        private readonly SessionService? _sessionService;
        private readonly PackageAndCardService? _packageService;
        private readonly UserRepository? _userRepository;
        private readonly int _refund;
        public BattleLogicService(ISessionService sessionService, IPackageAndCardService packageService, IUserRepository userRepository, IConfiguration configuration)
        {

            _sessionService = (SessionService?)sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _packageService = (PackageAndCardService?)packageService ?? throw new ArgumentNullException(nameof(packageService));
            _userRepository = (UserRepository?)userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            string refundstring = configuration["AppSettings:refund"] ?? throw new InvalidOperationException("JWT Secret Key is not configured properly.");
            _refund = Int32.Parse(refundstring);
        }
        public async Task<Battle> ExecuteBattle(Battle battle)
        {
               await Console.Out.WriteLineAsync($"{battle.PlayerOne.Name} fighting vs {battle.PlayerTwo.Name}");
            Deck deckOne = battle.PlayerOne.Deck;
            Deck deckTwo = battle.PlayerTwo.Deck;
            string deckOneOld = getCardsAsString(deckOne);
            string deckTwoOld = getCardsAsString(deckTwo);
            battle.LogPlayerOne.Add($"You with deck: {deckOneOld} are in battle with {battle.PlayerTwo.Name} with deck: {deckTwoOld}");
            battle.LogPlayerTwo.Add($"You with deck: {deckTwoOld} are in battle with {battle.PlayerOne.Name} with deck: {deckOneOld}");


            while (deckOne?.CardList?.Count > 0 && deckTwo?.CardList?.Count > 0 && battle.Rounds < 101)
            {
                //await Console.Out.WriteLineAsync("starting round " + battle.Rounds);

                Card cardOne = GetRandomCard(deckOne.CardList);
                Card cardTwo = GetRandomCard(deckTwo.CardList);
               // await Console.Out.WriteLineAsync($"{cardOne.Name} fighting vs {cardTwo.Name}");
                battle.LogPlayerOne.Add($"Round {battle.Rounds}");
                battle.LogPlayerTwo.Add($"Round {battle.Rounds}"); 
                RoundResult? result = HandleSingleCardFight(cardOne, cardTwo) ?? throw new Exception("Result is null");
               // await Console.Out.WriteLineAsync(result.ToString());
                if (result.LogRoundPlayerOne != null && result.LogRoundPlayerOne != String.Empty )
                    battle.LogPlayerOne.Add(result.LogRoundPlayerOne);
                if (result.LogRoundPlayerTwo != null && result.LogRoundPlayerTwo != String.Empty)
                    battle.LogPlayerTwo.Add(result.LogRoundPlayerTwo);


                if (result?.Result == ResultType.FirstPlayerWon)
                {
                    deckOne.CardList.Add(cardTwo);
                    deckTwo.CardList.Remove(cardTwo);
                    battle.LogPlayerOne.Add($"Your {cardOne.Name} ({cardOne.Damage}) won against {cardTwo.Name} ({cardTwo.Damage}) and you won {cardTwo.Name}.");
                    battle.LogPlayerTwo.Add($"Your {cardTwo.Name} ({cardTwo.Damage}) lost against {cardOne.Name} ({cardOne.Damage}) and you lost {cardTwo.Name}.");
                }
                else if (result?.Result == ResultType.SecondPlayerWon)
                {
                    deckOne.CardList.Remove(cardOne);
                    deckTwo.CardList.Add(cardOne);
                    battle.LogPlayerOne.Add($"Your {cardOne.Name} ({cardOne.Damage}) lost against {cardTwo.Name} ({cardTwo.Damage}) and you lost {cardOne.Name}.");
                    battle.LogPlayerTwo.Add($"Your {cardTwo.Name} ({cardTwo.Damage}) won against {cardOne.Name} ({cardOne.Damage}) and you won {cardOne.Name}.");
                }
                else if (result?.Result == ResultType.Draw)
                {
                    battle.LogPlayerOne.Add($"Your {cardOne.Name} ({cardOne.Damage}) drew against {cardTwo.Name} ({cardTwo.Damage}).");
                    battle.LogPlayerTwo.Add($"Your {cardTwo.Name} ({cardTwo.Damage}) drew against {cardOne.Name} ({cardOne.Damage}).");
                }
                battle.LogPlayerOne.Add($"--------------------");
                battle.LogPlayerTwo.Add($"--------------------");

                battle.Rounds++;
            }

            if (battle.Rounds == 101)
            {
                battle.Result = ResultType.Draw;
                battle.LogPlayerOne.Add($"The battle against {battle.PlayerTwo.Name} ended in a draw after 100 rounds");
                battle.LogPlayerTwo.Add($"The battle against {battle.PlayerOne.Name} ended in a draw after 100 rounds");
                string currentDeckOne = getCardsAsString(deckOne);
                string currentDeckTwo = getCardsAsString(deckTwo);


                battle.LogPlayerOne.Add($"Your deck now consists of the following cards: {currentDeckOne}");
                battle.LogPlayerTwo.Add($"Your deck now consists of the following cards: {currentDeckTwo}");

            }
            else if (deckOne?.CardList?.Count == 0)
            {
                battle.Result = ResultType.SecondPlayerWon;
                battle.LogPlayerOne.Add($"You lost against {battle.PlayerTwo.Name} after {battle.Rounds} rounds");
                battle.LogPlayerTwo.Add($"You won against {battle.PlayerOne.Name} after {battle.Rounds} rounds");
                battle.LogPlayerOne.Add($"You lost your deck with the following cards: {deckOneOld} and gained a refund of {_refund} coins.");
                battle.LogPlayerTwo.Add($"You won {battle.PlayerOne.Name}'s deck with the following cards: {deckOneOld} ");

            }
            else if (deckTwo?.CardList?.Count == 0)
            {
                battle.Result = ResultType.FirstPlayerWon;
                battle.LogPlayerOne.Add($"You won against {battle.PlayerTwo.Name} after {battle.Rounds} rounds");
                battle.LogPlayerTwo.Add($"You lost against {battle.PlayerOne.Name} after {battle.Rounds} rounds");

                battle.LogPlayerOne.Add($"You won {battle.PlayerTwo.Name}'s deck with the following cards: {deckTwoOld} ");
                battle.LogPlayerTwo.Add($"You lost your deck with the following cards: {deckTwoOld} ");

            }
            battle.PlayerOne.Deck = deckOne;
            battle.PlayerTwo.Deck = deckTwo;

            return battle;
        }

        private string getCardsAsString(Deck? deck)
        {
            if (deck == null)
                return String.Empty;
            string result = String.Empty;
            foreach (Card card in deck!.CardList)
            {
                result += card.Name + $"({card.Damage}), ";
            }
            result = result.Substring(0, result.Length - 2);
            return result;
        }

        private static Card GetRandomCard(List<Card> deck)
        {
            Random rnd = new Random();
            int index = rnd.Next(0, deck.Count);
            return deck[index];
        }

        private static RoundResult? HandleSingleCardFight(Card cardOne, Card cardTwo)
        {
            RoundResult? result = new();
            if (cardOne == null || cardTwo == null)
                throw new Exception("A round did not have any cards to fight!");

            Engine engine = new();
            result = engine.Evaluate(cardOne, cardTwo);

            return result;
        }




    }
}