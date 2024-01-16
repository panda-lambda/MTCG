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
            try
            {
                Guid userId = _sessionService.AuthenticateUserAndSession(e, null);

                return _packageAndCardRepository.BuyPackage((Guid)userId);
            }
            catch (InsufficientCoinsException)
            {
                throw;
            }
            catch (NoAvailableCardsException)
            {
                throw;
            }
            catch (UnauthorizedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public Deck GetDeckByUser(HttpSvrEventArgs e)
        {
            try
            {

                Guid userId = _sessionService.AuthenticateUserAndSession(e, null);
                Deck deck = _packageAndCardRepository.GetDeckByUser((Guid)userId);
                return deck;
            }

            catch (UserHasNoCardsException)
            {
                throw;
            }
            catch (InvalidCardCountInDeck ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            catch (UnauthorizedException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }



        public List<Card>? GetCardsByUser(HttpSvrEventArgs e)
        {
            try
            {
                Guid userId = _sessionService.AuthenticateUserAndSession(e, null);

                List<Card>? cardList = _packageAndCardRepository.GetCardsByUser((Guid)userId);

                return cardList;
            }
            catch (UnauthorizedException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
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

        public bool CheckForValidDeck(HttpSvrEventArgs e)
        {
            Guid userId = _sessionService.AuthenticateUserAndSession(e, null);

            Deck? deck = _packageAndCardRepository.GetDeckByUser(userId);
            if (deck?.CardList?.Count == 4)
            {
                return true;
            }
            return false;

        }



        public string CreateNewPackage(HttpSvrEventArgs e)
        {
            try
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
                    throw new UserNotAdminException("user not admin in crate new packge");
                }
                Guid userId = _sessionService.AuthenticateUserAndSession(e, null);

                Package cardPackage = new Package { PackageId = Guid.NewGuid() };
                cardPackage.CardList = JsonConvert.DeserializeObject<List<Card>>(e.Payload);
                if (cardPackage.CardList == null)
                {
                    throw new Exception(" no card in package in service");
                }
                foreach (Card card in cardPackage.CardList)
                {
                    if (card.Name.ToString().StartsWith("Fire"))
                        card.Element = ElementType.Fire;
                    if (card.Name.ToString().StartsWith("Water"))
                        card.Element = ElementType.Water;
                }
                for (int i = 0; i < cardPackage.CardList?.Count; i++)
                {

                    Card card = cardPackage.CardList[i];
                    Console.WriteLine("card:" + card.Name);
                    Console.WriteLine("id: " + card.Id + "");
                }
                if (_packageAndCardRepository.AddPackage(cardPackage))
                    return "success";
                else
                    return "409";



            }
            catch (Exception)
            {
                throw;
            }




        }

    }
}
