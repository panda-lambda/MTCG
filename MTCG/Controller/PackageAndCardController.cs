using Microsoft.Extensions.FileProviders;
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
        /// <summary>
        /// Controls everything related to cards and packages and handles related requests.
        /// </summary>
        private IPackageAndCardService _packageService;
        private ISessionService _sessionService;

        public PackageAndCardController(IPackageAndCardService packageAndCardService, ISessionService sessionService)
        {
            _packageService = packageAndCardService ?? throw new ArgumentNullException(nameof(packageAndCardService));
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        }


        /// <summary>
        /// Handles the request and calls the appropriate method.
        /// </summary>
        /// <param name="e">HttpSvrEvenArgs</param>
        public override void HandleRequest(HttpSvrEventArgs e)
        {
            if (e.Method == "POST")
            {
                if (e.Path.StartsWith("/packages"))
                {
                    ExecuteWithExceptionHandling(e, CreateNewCardPackage);
                }
                if (e.Path.StartsWith("/transactions/packages"))
                {
                    ExecuteWithExceptionHandling(e, BuyCardPackage);
                }
                if (e.Path.StartsWith("/cards"))
                {
                    ExecuteWithExceptionHandling(e, SellCards);
                }
            }

            else if (e.Method == "GET")
            {
                if (e.Path.StartsWith("/cards"))
                {
                    ExecuteWithExceptionHandling(e, GetCardsByUser);
                }

                if (e.Path.StartsWith("/deck"))
                {
                    ExecuteWithExceptionHandling(e, GetDeckByUser);
                }

            }

            else if (e.Method == "PUT")
            {
                if (e.Path.StartsWith("/deck"))
                {
                    ExecuteWithExceptionHandling(e, ConfigureDeck);
                }
                
            }

            else { e.Reply((int)HttpCodes.BAD_REQUEST, "{\"description\":\"Not a valid Http Request!\"}"); }

        }
        

        /// <summary>
        /// Adds cards to the deck of the user. 
        /// </summary>
        /// <param name="e"></param>
        /// <exception cref="InternalServerErrorException"> the deck can not be updated</exception>

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
        
        /// <summary>
        /// unique feature: sells x cards for x/2 coins. 
        /// </summary>
        /// <param name="e">httpsvreventargs</param>
        
        private void SellCards(HttpSvrEventArgs e)
        {
            int count = 0 ;
            count = _packageService.SellCards(e);
            if (count != 0)
            {
                e.Reply((int)HttpCodes.OK, "{\"description\":\" You sold "+count+ " cards for "+Math.Floor(count/2.0)+" coin(s).\"}");
            }
            else
            {
                e.Reply((int)HttpCodes.CONFLICT, "{\"description\":\"The cards you offered did not exist or you are not the owner!\"}");
            }
        }

        /// <summary>
        /// Gets the deck of a user
        /// </summary>
        /// <param name="e"></param>
        /// <exception cref="NoContentException">there are no cards in the user's deck</exception>
        private void GetDeckByUser(HttpSvrEventArgs e)
        {
            Deck? deck = _packageService.GetDeckByUser(e);
            if (deck == null || deck.CardList == null || deck.CardList?.Count == 0)
            {
                throw new NoContentException("The request was fine, but the deck doesn't have any cards.");
            }
            if (e.Path.StartsWith("/deck?format=plain"))
            {
                int totalCards = deck!.CardList!.Count;
                int monsterCount = deck.CardList.Count(card => card.Type == CardType.Monster);
                int spellCount = deck.CardList.Count(card => card.Type == CardType.Spell);
                float maxDamage = deck.CardList.Max(card => card.Damage);
                float minDamage = deck.CardList.Min(card => card.Damage);
                int countFire = deck.CardList.Count(card => card.Element == ElementType.Fire);
                int countRegular = deck.CardList.Count(card => card.Element == ElementType.Normal);
                int countWater = deck.CardList.Count(card => card.Element == ElementType.Water);

                string plainDescription = $"The deck consists of {totalCards} cards.\n" +
                          $"It features {monsterCount} monsters and {spellCount} spells.\n" +
                          $"Your strongest card does {maxDamage} damage and your weakest {minDamage} damage.\n" +
                          $"Elements are the following: {countFire} fire, {countRegular} regular, {countWater} water.";

                e.Reply((int)HttpCodes.OK, "{\"description\":\"" + plainDescription + "\"}");
            }
            else
            {
                e.Reply((int)HttpCodes.OK, System.Text.Json.JsonSerializer.Serialize(deck.CardList, JsonOptions.DefaultOptions));
            }
        }

        /// <summary>
        /// Get all the aquired cards of a user
        /// </summary>
        /// <param name="e"></param>
        /// <exception cref="NoContentException"> user has no cards aquired yet</exception>
        private void GetCardsByUser(HttpSvrEventArgs e)
        {

            List<Card>? userCardList = _packageService.GetCardsByUser(e);
            if (userCardList == null || userCardList.Count == 0)
            {
                throw new NoContentException("The request was fine, but the user doesn't have any cards.");
            }
            e.Reply((int)HttpCodes.OK, System.Text.Json.JsonSerializer.Serialize(userCardList, JsonOptions.DefaultOptions));

        }

        /// <summary>
        /// Creates a new package that can be bought
        /// </summary>
        /// <param name="e"></param>
        private void CreateNewCardPackage(HttpSvrEventArgs e)
        {

            Console.WriteLine("in create newcardpackage controller");
            _packageService.CreateNewPackage(e);

            e.Reply((int)HttpCodes.OK, "{\"description\":\"Package and cards successfully created.\"}");



        }
        /// <summary>
        /// Changes the ownership of the cards to the user and deletes the package. 
        /// </summary>
        /// <param name="e"></param>
        /// <exception cref="NotFoundException">package can not be found</exception>
        private void BuyCardPackage(HttpSvrEventArgs e)
        {
            List<Card>? package = _packageService.BuyPackage(e);
            if (package == null)
            {
                throw new NotFoundException("No card package available for buying.");
            }
            string testo = System.Text.Json.JsonSerializer.Serialize(package, JsonOptions.DefaultOptions);
            Console.WriteLine("package and cards: " + testo);
            e.Reply((int)HttpCodes.OK, System.Text.Json.JsonSerializer.Serialize(package, JsonOptions.DefaultOptions));
        }



    }
}
