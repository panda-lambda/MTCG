using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Controller
{
    public class TradingController : BaseController
    {
        private ITradingService _tradingService;

        public TradingController(ITradingService tradingService)
        {
            _tradingService = tradingService ?? throw new ArgumentNullException(nameof(tradingService));
        }


        public override void HandleRequest(HttpSvrEventArgs e)
        {
            switch (e.Method)
            {
                case "POST":
                    if (e.Path.StartsWith("/tradings/"))
                    {
                        TradeSingleCard(e);
                    }

                    if (e.Path.StartsWith("/tradings"))
                    {
                        CreateNewTrading(e);
                    }
                    break;

                case "DELETE":
                    {
                        if (e.Path.StartsWith("/tradings/"))
                        {
                            RemoveTradingDeal(e);
                        }
                        break;
                    }

                case "GET":
                    if (e.Path.StartsWith("/tradings"))
                    {
                        GetAvailabeTradingDeals(e);
                    }
                    break;
                default:
                    e.Reply((int)HttpCodes.BAD_REQUEST, "{\"msg\":\"Not a valid Http Request!\"}");
                    break;
            }
        }

        internal void TradeSingleCard(HttpSvrEventArgs e)
        {
            e.Reply((int)HttpCodes.OK, "Trading deal successfully executed.");
            e.Reply((int)HttpCodes.UNAUTORIZED, "{\"description\":\"Access token is missing or invalid\"}");
            e.Reply((int)HttpCodes.FORBIDDEN, "The offered card is not owned by the user, or the requirements are not met (Type, MinimumDamage), or the offered card is locked in the deck.");
            e.Reply((int)HttpCodes.NOT_FOUND, "The provided deal ID was not found.");
        }
        internal void CreateNewTrading(HttpSvrEventArgs e)
        {
            e.Reply((int)HttpCodes.OK, "Trading deal successfully created.");
            e.Reply((int)HttpCodes.UNAUTORIZED, "{\"description\":\"Access token is missing or invalid\"}");
            e.Reply((int)HttpCodes.FORBIDDEN, "The deal contains a card that is not owned by the user or locked in the deck.");
            e.Reply((int)HttpCodes.CONFLICT, "A deal with this deal ID already exists.");
        }
        internal void RemoveTradingDeal(HttpSvrEventArgs e)
        {
            e.Reply((int)HttpCodes.OK, "Trading deal successfully deleted.");
            e.Reply((int)HttpCodes.UNAUTORIZED, "{\"description\":\"Access token is missing or invalid\"}");
            e.Reply((int)HttpCodes.FORBIDDEN, "The deal contains a card that is not owned by the user.");
            e.Reply((int)HttpCodes.NOT_FOUND, "The provided deal ID was not found.");
            e.Reply((int)HttpCodes.CONFLICT, "A deal with this deal ID already exists.");
        }
        internal void GetAvailabeTradingDeals(HttpSvrEventArgs e)
        {
            e.Reply((int)HttpCodes.OK, "There are trading deals available, the response contains these");
            //content:
            //  application/json:
            //    schema:
            //      type: array
            //      items:
            //        $ref: '#/components/schemas/TradingDeal'");
            e.Reply((int)HttpCodes.NO_CONTENT, "The request was fine, but there are no trading deals available.");
            e.Reply((int)HttpCodes.UNAUTORIZED, "{\"description\":\"Access token is missing or invalid\"}");


        }




    }
}