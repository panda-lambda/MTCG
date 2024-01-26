using MTCG.HttpServer;
using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Services
{
    public interface IPackageAndCardService
    {
         bool CreateNewPackage(HttpSvrEventArgs e);
        List<Card>? BuyPackage(HttpSvrEventArgs e);
        List<Card>? GetCardsByUser(HttpSvrEventArgs e);
        Deck GetDeckByUser(HttpSvrEventArgs e);
        bool ConfigureDeckForUser(HttpSvrEventArgs e);

        bool CheckForValidDeck( Guid userId);
        int SellCards(HttpSvrEventArgs e);






    }
}
