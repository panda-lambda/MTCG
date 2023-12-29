using Microsoft.Extensions.Logging;
using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Services;
using MTCG.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MTCG.Controller
{
    public class PackageAndCardController : BaseController
    {

        private IPackageAndCardService _packageService;
        private ISessionService _sessionService;
        public PackageAndCardController(IPackageAndCardService packageAndCardService, ISessionService sessionService)
        {
            _packageService = packageAndCardService ?? throw new ArgumentNullException(nameof(packageAndCardService));
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        }


        public override void HandleRequest(HttpSvrEventArgs e)
        {
            if (e.Method == "POST")
            {
                if (e.Path.StartsWith("/packages"))
                {
                    CreateNewCardPackage(e);

                }
                if (e.Path.StartsWith("/transactions/packages"))
                {
                    BuyCardPackage(e);
                }
            }
        }

        public void CreateNewCardPackage(HttpSvrEventArgs e)
        {

            Console.WriteLine("in create newcardpackage controller");
            switch (_packageService.CreateNewPackage(e))
            {
                case ("success"):
                    Console.WriteLine("Package and crrds succes created");

                    e.Reply((int)HttpCodes.OK, "{\"description\":\"Package and cards successfully created.\"}");

                    break;
                case ("token"):
                    e.Reply((int)HttpCodes.INTERNAL_SERVER_ERROR, "{\"msg\":\"User could not be created. Something went wrong\"}");

                    //e.Reply((int)HttpCodes.UNAUTORIZED,"{\"msg\":\"Access token is missing or invalid.\"}");
                    break;
                case ("admin"):
                    e.Reply((int)HttpCodes.FORBIDDEN, "{\"description\":\"Provided user is not admin.\"}");
                    break;
                case ("409"):
                    e.Reply((int)HttpCodes.CONFLICT, "{\"description\":\"At least one card in the packages already exists\"}");
                    break;
            }
        }
        public void BuyCardPackage(HttpSvrEventArgs e)
        {
            try
            {
                List<Card>? package = _packageService.BuyPackage(e);
                if (package == null)
                {
                    throw new InternalServerErrorException("package in controller null");
                }

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
             
                string testo = System.Text.Json.JsonSerializer.Serialize(package,options);
                Console.WriteLine("package and cards: "+testo);
                e.Reply((int)HttpCodes.OK, System.Text.Json.JsonSerializer.Serialize(package,options));
            }
            catch (UnauthorizedException ex)
            {
                Console.WriteLine(ex.Message + "   in buycardpackage controller");
                e.Reply((int)HttpCodes.UNAUTORIZED, "{\"description\":\"Access token is missing or invalid.\"}");
            }
            catch (InternalServerErrorException ex)
            {
                Console.WriteLine(ex.Message);
                e.Reply((int)HttpCodes.INTERNAL_SERVER_ERROR, "{\"msg\":\"Something went wrong.\"}");
            }
            catch (NoAvailableCardsException)
            {
                e.Reply((int)HttpCodes.NOT_FOUND, "{\"description\":\" No card package available for buying.\"}");
            }
            catch (InsufficientCoinsException)
            {
                e.Reply((int)HttpCodes.FORBIDDEN, "{\"description\":\"Not enough money for buying a card package.\"}");

            }
        }


    }
}
