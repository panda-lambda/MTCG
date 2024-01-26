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
                    ExecuteWithExceptionHandling(e, CreateNewCardPackage);
                }
                if (e.Path.StartsWith("/transactions/packages"))
                {
                    ExecuteWithExceptionHandling(e, BuyCardPackage);
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

        private void GetCardsByUser(HttpSvrEventArgs e)
        {

            List<Card>? userCardList = _packageService.GetCardsByUser(e);
            if (userCardList == null || userCardList.Count == 0)
            {
                throw new NoContentException("The request was fine, but the user doesn't have any cards.");
            }
            e.Reply((int)HttpCodes.OK, System.Text.Json.JsonSerializer.Serialize(userCardList, JsonOptions.DefaultOptions));

        }

        private void CreateNewCardPackage(HttpSvrEventArgs e)
        {

            Console.WriteLine("in create newcardpackage controller");
            _packageService.CreateNewPackage(e);

            e.Reply((int)HttpCodes.OK, "{\"description\":\"Package and cards successfully created.\"}");



        }
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
