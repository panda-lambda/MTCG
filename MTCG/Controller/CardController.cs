using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MTCG.Controller
{
    public class CardController : BaseController
    {

        private ICardService _cardService;
        public CardController(ICardService cardService)        {
            _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
        }


        public override void HandleRequest(HttpSvrEventArgs e)
        {

            if (e.Path.StartsWith("/cards") && e.Method == "POST")
            {
                GetAllCardsByUser(e);
            }
            if (e.Path.StartsWith("/deck") && e.Method == "GET")
            {
                ShowCurrentDeckByUser(e);
            }
            if (e.Path.StartsWith("/deck") && e.Method == "PUT")
            {
               CreateCurrentDeckByUser(e);
            }
        }

       

        

        private void GetAllCardsByUser(HttpSvrEventArgs e)
        {
            e.Reply((int)HttpCodes.NO_CONTENT, "The request was fine, but the user doesn't have any cards.");
            e.Reply((int)HttpCodes.UNAUTORIZED, "Access token is missing or invalid.");

        }

        private void ShowCurrentDeckByUser(HttpSvrEventArgs e)
        {
            e.Reply((int)HttpCodes.OK, "content");
            //    application / json:
            //      schema:
            //type: array
            //items:
            //          $ref: '#/components/schemas/Card'
            //    text / plain:
            //      schema:
            //type: string
            //description: The textual deck description.
            e.Reply((int)HttpCodes.NO_CONTENT, "The request was fine, but the deck doesn't have any cards.");

            e.Reply((int)HttpCodes.UNAUTORIZED, "Access token is missing or invalid.");

        }
        private void CreateCurrentDeckByUser(HttpSvrEventArgs e)
        {

            e.Reply((int)HttpCodes.OK, "content");

            //requestBody:
            //content:
            //    application / json:
            //    schema:
            //type: array
            //items:
            //        type: string
            //        format: uuid
            //      minItems: 4
            //      maxItems: 4
            //      uniqueItems: true
            //required: true
            
            e.Reply((int)HttpCodes.BAD_REQUEST, "The provided deck did not include the required amount of cards.");
            e.Reply((int)HttpCodes.UNAUTORIZED, "Access token is missing or invalid.");
            e.Reply((int)HttpCodes.FORBIDDEN, "At least one of the provided cards does not belong to the user or is not available.");


        }
    }
}


