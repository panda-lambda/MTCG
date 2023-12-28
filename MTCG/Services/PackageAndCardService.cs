using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Repositories;
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
        public PackageAndCardService(ISessionService sessionService, IPackageAndCardRepository packageAndCardRepository)
        {
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _packageAndCardRepository = packageAndCardRepository ?? throw new ArgumentNullException(nameof(packageAndCardRepository));

        }

        public List<Card>? BuyPackage(HttpSvrEventArgs e) {


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


            return null;
        }
        public string CreateNewPackage(HttpSvrEventArgs e)
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
            if (_sessionService.AuthenticateUserAndSession(e, null) == null )
            {
                Console.WriteLine("no autorization n service!!!!");
                return "token";
            }

            try
            {
                Package cardPackage = new Package { PackageId = Guid.NewGuid() };
                cardPackage.CardList = JsonConvert.DeserializeObject<List<Card>>(e.Payload);
                if (cardPackage.CardList != null) { }
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
