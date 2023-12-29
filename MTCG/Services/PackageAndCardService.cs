using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Repositories;
using MTCG.Utilities;
using Newtonsoft.Json;
using Npgsql.NameTranslation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            UserSession? session = _sessionService.AuthenticateUserAndSession(e, null);
            if (session == null || String.IsNullOrEmpty(session.Username))
            {
                throw new UnauthorizedException("could not authenticate in buy package");
            }
            Guid? userId = _userRepository.GetGuidByUserName(session.Username);
            if (userId == null)
            {
                throw new UnauthorizedException("could not authenticate in buy package");
            }
            return _packageAndCardRepository.BuyPackage((Guid)userId);
        }

        public string CreateNewPackage(HttpSvrEventArgs e)
        {
            try
            {
                Console.WriteLine(" in createnewpackage service");
                string? token = e.Headers?.FirstOrDefault(header => header.Name == "Authorization")?.Value;
                string userName = string.Empty;
                if ("-mtcgToken" == token?.Substring(token.Length - 10))
                {
                    token = token.Replace("-mtcgToken", "");
                    userName = token.Replace("Bearer ", "");
                    Console.WriteLine("username in createpackage: " + userName);
                }

                if (userName != "admin")
                    return "admin";
                if (_sessionService.AuthenticateUserAndSession(e, null) == null)
                {
                    Console.WriteLine("no autorization n service!!!!");
                    return "token";
                }
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "409";
            }




        }

    }
}
