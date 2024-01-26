using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Repositories;
using MTCG.Utilities;
using Newtonsoft.Json;
using Npgsql.NameTranslation;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MTCG.Services
{
    public class PackageAndCardService : IPackageAndCardService
    {

        private ISessionService _sessionService;
        private IPackageAndCardRepository _packageAndCardRepository;
        private IUserRepository _userRepository;
        public PackageAndCardService(ISessionService sessionService, IPackageAndCardRepository packageAndCardRepository, IUserRepository userRepository)
        {
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _packageAndCardRepository = packageAndCardRepository ?? throw new ArgumentNullException(nameof(packageAndCardRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

        }

        public List<Card>? BuyPackage(HttpSvrEventArgs e)
        {

            Guid userId = _sessionService.AuthenticateUserAndSession(e, null);

            return _packageAndCardRepository.BuyPackage((Guid)userId);



        }

        public Deck GetDeckByUser(HttpSvrEventArgs e)
        {
            Guid userId = _sessionService.AuthenticateUserAndSession(e, null);

            return _packageAndCardRepository.GetDeckByUser(userId);
        }



        public List<Card>? GetCardsByUser(HttpSvrEventArgs e)
        {
            Guid userId = _sessionService.AuthenticateUserAndSession(e, null);
            return _packageAndCardRepository.GetCardsByUser((Guid)userId);
        }

        public bool ConfigureDeckForUser(HttpSvrEventArgs e)
        {
            //try
            //{
            List<Guid>? cardIds = JsonConvert.DeserializeObject<List<Guid>>(e.Payload);
            if (cardIds == null || cardIds.Count != 4)
            {
                throw new InvalidCardCountInDeck("The provided deck did not include the required amount of cards.");
            }

            Guid userId = _sessionService.AuthenticateUserAndSession(e, null);


            List<Card>? cardList = _packageAndCardRepository.GetCardsByUser(userId);
            if (cardList == null)
            {
                throw new UserNotCardOwnerException("At least one of the provided cards does not belong to the user or is not available.");
            }

            foreach (Guid id in cardIds)
            {
                bool allIdsExist = false;
                foreach (Card card in cardList)
                {
                    if (card.Id == id && card.Locked == false)
                    {
                        allIdsExist = true;
                        break;
                    }
                }
                if (!allIdsExist)
                {
                    throw new UserNotCardOwnerException("At least one of the provided cards does not belong to the user or is not available.");
                }
            }
            return _packageAndCardRepository.ConfigureDeckForUser(cardIds, userId);




        }

        public bool CheckForValidDeck(Guid userId)
        {
            Deck? deck = _packageAndCardRepository.GetDeckByUser(userId);
            Console.WriteLine("got deck: " + deck.CardList);
            Console.WriteLine("coount: " + deck?.CardList?.Count);

            if (deck?.CardList?.Count == 4)
            {
                return true;
            }
            return false;

        }



        public bool CreateNewPackage(HttpSvrEventArgs e)
        {

            string? token = e.Headers?.FirstOrDefault(header => header.Name == "Authorization")?.Value;
            string userName = string.Empty;
            if ("-mtcgToken" == token?.Substring(token.Length - 10))
            {
                token = token.Replace("-mtcgToken", "");
                userName = token.Replace("Bearer ", "");
                Console.WriteLine("username in createpackage: " + userName);
            }

            if (userName != "admin")
            {
                throw new ForbiddenException("Provided user is not \"admin\".");
            }
            Guid userId = _sessionService.AuthenticateUserAndSession(e, "admin");

            Package cardPackage = new Package { PackageId = Guid.NewGuid() };
            cardPackage.CardList = JsonConvert.DeserializeObject<List<Card>>(e.Payload);

            if (cardPackage.CardList == null || cardPackage.CardList.Count !=5)
            {
                throw new BadRequestException("No cards in Package or invalid card count.");
            }
            List<Card> existingCards = _packageAndCardRepository.GetAllCards();

            if(cardPackage.CardList.Select(card =>card.Id).Intersect(existingCards.Select(card => card.Id)).Any())
            {
                throw new Exception("At least one card in the packages already exists");
            }           
                   

            foreach (Card card in cardPackage.CardList)
            {
                card.Element = EnumHelper.GetEnumByNameFront<ElementType>(card.Name.ToString());
                card.Monster = EnumHelper.GetEnumByNameEnd<MonsterType>(card.Name.ToString());
                card.Type = EnumHelper.GetEnumByNameEnd<CardType>(card.Name.ToString());
                card.Locked = false;
            }

            for (int i = 0; i < cardPackage.CardList.Count; i++)
            {

                Card card = cardPackage.CardList[i];
                Console.WriteLine("card:" + card.Name);
                Console.WriteLine("id: " + card.Id + "");
            }
            return _packageAndCardRepository.AddPackage(cardPackage);
        }

    }
}
