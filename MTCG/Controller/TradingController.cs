using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Services;
using MTCG.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Controller
{
    public class TradingController : BaseController
    {
        /// <summary>
        /// Controls the trades and handles related requests.
        /// </summary>
        private ITradingService _tradingService;

        public TradingController(ITradingService tradingService)
        {
            _tradingService = tradingService ?? throw new ArgumentNullException(nameof(tradingService));
        }

        /// <summary>
        /// Handles the request and calls the appropriate method.
        /// </summary>
        /// <param name="e">HttpSvrEvenArgs</param>

        public override void HandleRequest(HttpSvrEventArgs e)
        {
            switch (e.Method)
            {
                case "POST":
                    if (e.Path.StartsWith("/tradings/"))
                    {

                        ExecuteWithExceptionHandling(e, TradeSingleCard);
                     
                    }
                    else if (e.Path.StartsWith("/tradings"))
                    {
                        ExecuteWithExceptionHandling(e, CreateNewTrading);

                    }
                    break;

                case "DELETE":
                    {
                        if (e.Path.StartsWith("/tradings/"))
                        {
                            ExecuteWithExceptionHandling(e, RemoveTradingDeal);
                        }
                        break;
                    }

                case "GET":
                    if (e.Path.StartsWith("/tradings"))
                    {
                        ExecuteWithExceptionHandling(e, GetAvailabeTradingDeals);
                    }
                    break;
                default:
                    e.Reply((int)HttpCodes.BAD_REQUEST, "{\"description\":\"Not a valid Http Request!\"}");
                    break;
            }
        }

        /// <summary>
        /// Trades a single card for another card offered in a trading deal. 
        /// </summary>
        /// <param name="e"></param>
        /// <exception cref="Exception">something went wrong</exception>
        internal void TradeSingleCard(HttpSvrEventArgs e)
        {
            if (_tradingService.TradeSingleCard(e))
                e.Reply((int)HttpCodes.OK, "Trading deal successfully executed.");
            else
            {
                throw new Exception("Internal Error: Something went wrong while swapping cards");
            }

        }
        /// <summary>
        /// createds a new trading deal.
        /// </summary>
        /// <param name="e"></param>

        internal void CreateNewTrading(HttpSvrEventArgs e)
        {
            if (_tradingService.CreateNewTradingDeal(e))
                e.Reply((int)HttpCodes.OK, "{\"description\":\"Trading deal successfully created\"}");
        }

        /// <summary>
        /// removes a trading deal
        /// </summary>
        /// <param name="e"></param>
        /// <exception cref="ForbiddenException">the user is not owner</exception>
        internal void RemoveTradingDeal(HttpSvrEventArgs e)
        {
            if (_tradingService.RemoveTradingDeal(e))
            {
                e.Reply((int)HttpCodes.OK, "{\"description\":\"Trading deal successfully deleted\"}");
            }
            else
            {
                throw new ForbiddenException("The provided deal ID was not found.");
            }
        }


        /// <summary>
        /// gets all the availabe trading deals
        /// </summary>
        /// <param name="e"></param>
        /// <exception cref="NoContentException">there are no trading deals</exception>
        internal void GetAvailabeTradingDeals(HttpSvrEventArgs e)
        {
            List<TradingDeal> deals = _tradingService.GetTradingDeals(e);
            if (deals != null && deals.Count > 0)
            {
                e.Reply((int)HttpCodes.OK, System.Text.Json.JsonSerializer.Serialize(deals, JsonOptions.NullOptions));
            }
            else
            {
                throw new NoContentException("The request was fine, but there are no trading deals available.");
            }
        }




    }
}