using Microsoft.Extensions.Logging;
using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Services;
using MTCG.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

            if (e.Method == "GET")
            {
                if (e.Path.StartsWith("/cards"))
                {
                    GetCardsByUser(e);
                }

                if (e.Path.StartsWith("/deck"))
                {
                    GetDeckByUser(e);
                }

            }

            if (e.Method == "PUT")
            {
                if (e.Path.StartsWith("/deck"))
                {
                    ExecuteWithExceptionHandling(e, ConfigureDeck);
                }
            }
        }

        private void ConfigureDeck(HttpSvrEventArgs e)
        {
            if (_packageService.ConfigureDeckForUser(e))
            {
                e.Reply((int)HttpCodes.OK, "{\"description\":\" The deck has been successfully configured.\"}");
            }
            else
            {
                throw new InternalServerErrorException("Something went wrong!");
            }
        }

        private void GetDeckByUser(HttpSvrEventArgs e)
        {
            try
            {
                Deck? deck = _packageService.GetDeckByUser(e);
                if (deck == null)
                {
                    throw new UserHasNoCardsException("deck in controller null");
                }
                e.Reply((int)HttpCodes.OK, System.Text.Json.JsonSerializer.Serialize(deck.CardList, JsonOptions.DefaultOptions));
            }
            catch (UserHasNoCardsException)
            {
                e.Reply((int)HttpCodes.NO_CONTENT, "{\"description\":\"The request was fine, but the deck doesn't have any cards.\"}");
            }

            catch (UnauthorizedException ex)
            {
                Console.WriteLine(ex.Message + "   in getdeckbyuser controller");
                e.Reply((int)HttpCodes.UNAUTORIZED, "{\"description\":\"Access token is missing or invalid.\"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " in controller");
                e.Reply((int)HttpCodes.INTERNAL_SERVER_ERROR, "{\"msg\":\"Something went wrong.\"}");

            }

        }

        private void GetCardsByUser(HttpSvrEventArgs e)
        {
            try
            {
                List<Card>? userCardList = _packageService.GetCardsByUser(e);
                if (userCardList == null || userCardList.Count == 0)
                {
                    throw new UserHasNoCardsException("user hat keine karten, im controller");
                }
                e.Reply((int)HttpCodes.OK, System.Text.Json.JsonSerializer.Serialize(userCardList, JsonOptions.DefaultOptions));

            }
            catch (UserHasNoCardsException)
            {
                e.Reply((int)HttpCodes.NO_CONTENT, "{\"description\":\"The request was fine, but the user doesn't have any cards.\"}");
            }

            catch (UnauthorizedException ex)
            {
                Console.WriteLine(ex.Message + "   in buycardpackage controller");
                e.Reply((int)HttpCodes.UNAUTORIZED, "{\"description\":\"Access token is missing or invalid.\"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " in controller");
                e.Reply((int)HttpCodes.INTERNAL_SERVER_ERROR, "{\"msg\":\"Something went wrong.\"}");

            }
        }

        private void CreateNewCardPackage(HttpSvrEventArgs e)
        {
            try
            {
                Console.WriteLine("in create newcardpackage controller");
                _packageService.CreateNewPackage(e);

                e.Reply((int)HttpCodes.OK, "{\"description\":\"Package and cards successfully created.\"}");

            }
            catch (UserNotAdminException ex)
            {
                Console.WriteLine(ex.Message);
                e.Reply((int)HttpCodes.FORBIDDEN, "{\"description\":\"Provided user is not admin.\"}");

            }
            catch (UnauthorizedException ex)
            {
                Console.WriteLine(ex.Message);
                e.Reply((int)HttpCodes.UNAUTORIZED, "{\"description\":\"Access token is missing or invalid.\"}");

            }
            catch (CardExistsAlreadyException ex)
            {
                Console.WriteLine(ex.Message);
                e.Reply((int)HttpCodes.CONFLICT, "{\"description\":\"At least one card in the packages already exists\"}");
            }
            catch (Exception)
            {
                e.Reply((int)HttpCodes.INTERNAL_SERVER_ERROR, "{\"msg\":\"User could not be created. Something went wrong\"}");
            }
        }
        private void BuyCardPackage(HttpSvrEventArgs e)
        {
            try
            {
                List<Card>? package = _packageService.BuyPackage(e);
                if (package == null)
                {
                    throw new InternalServerErrorException("package in controller null");
                }
                string testo = System.Text.Json.JsonSerializer.Serialize(package, JsonOptions.DefaultOptions);
                Console.WriteLine("package and cards: " + testo);
                e.Reply((int)HttpCodes.OK, System.Text.Json.JsonSerializer.Serialize(package, JsonOptions.DefaultOptions));
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                e.Reply((int)HttpCodes.INTERNAL_SERVER_ERROR, "{\"msg\":\"Something went wrong.\"}");
            }
        }



    }
}
