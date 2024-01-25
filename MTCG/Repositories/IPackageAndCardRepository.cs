﻿using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Repositories
{
    public interface IPackageAndCardRepository
    {
        public bool AddPackage(Package package);
        public List<Card>? BuyPackage(Guid userId);

        public List<Card>? GetCardsByUser(Guid userId);
        public Deck GetDeckByUser(Guid userId);

        public bool ConfigureDeckForUser(List<Guid> cardList, Guid user);

        public void UpdateCardsById(Guid userId, Deck deck);

        bool CreateNewTradingDeal(TradingDeal tradingDeal);

        bool CheckCardForTradingDeal(Guid cardId, Guid userId);
        Guid?  GetTradingDealsByTradingId(Guid tradingId);
        List<TradingDeal> GetAllTradingDeals();
        bool DeleteTradingDeal(Guid tradeId, Guid userId);

        bool TradeSingleCard(Guid userIdOffering, Guid userIdBuying, Guid cardToSell, Guid cardToBuy);

        Card? GetSingleCard(Guid cardId);

        Guid? GetCardToTradeFromTradingDeal(Guid tradeId);

    }
}
