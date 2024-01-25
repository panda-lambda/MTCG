﻿using MTCG.HttpServer;
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

                        ExecuteWithExceptionHandling(e, TradeSingleCard);
                    }

                    if (e.Path.StartsWith("/tradings"))
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
                    e.Reply((int)HttpCodes.BAD_REQUEST, "{\"msg\":\"Not a valid Http Request!\"}");
                    break;
            }
        }

        internal void TradeSingleCard(HttpSvrEventArgs e)
        {
            if(_tradingService.TradeSingleCard(e))
            e.Reply((int)HttpCodes.OK, "Trading deal successfully executed.");

        }
        internal void CreateNewTrading(HttpSvrEventArgs e)
        {
            if (_tradingService.CreateNewTradingDeal(e))
                e.Reply((int)HttpCodes.OK, "{\"description\":\"Trading deal successfully created\"}");
        }
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

        internal void GetAvailabeTradingDeals(HttpSvrEventArgs e)
        {
            List<TradingDeal> deals = _tradingService.GetTradingDeals(e);
            if (deals != null && deals.Count > 0)
            {
                e.Reply((int)HttpCodes.OK, System.Text.Json.JsonSerializer.Serialize(deals, JsonOptions.NullOptions));
            }
            else
            {
                throw new NoAvailableTradingDealsException();
            }
        }




    }
}