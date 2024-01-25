using MTCG.HttpServer;
using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Services
{
    public interface ITradingService
    {

        bool CreateNewTradingDeal(HttpSvrEventArgs e);
        List<TradingDeal> GetTradingDeals(HttpSvrEventArgs e);

        bool RemoveTradingDeal(HttpSvrEventArgs e);
        bool TradeSingleCard(HttpSvrEventArgs e);
    }
}
