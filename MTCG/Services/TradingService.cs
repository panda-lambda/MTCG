using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Repositories;
using MTCG.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Services
{
    public class TradingService : ITradingService
    {
        private ISessionService _sessionService;
        private IPackageAndCardRepository _packageAndCardRepository;
        private IUserRepository _userRepository;

        public TradingService(ISessionService sessionService, IPackageAndCardRepository packageAndCardRepository, IUserRepository userRepository)
        {
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _packageAndCardRepository = packageAndCardRepository ?? throw new ArgumentNullException(nameof(packageAndCardRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

        }

        public bool CreateNewTradingDeal(HttpSvrEventArgs e)
        {
            Guid userId = _sessionService.AuthenticateUserAndSession(e, null);
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException();
            }
            TradingDeal? tradingDeal = JsonConvert.DeserializeObject<TradingDeal>(e.Payload) ?? throw new Exception(" no trading deal in service");
            tradingDeal.OwnerId = userId;
            if (_packageAndCardRepository.GetTradingDealsByTradingId(tradingDeal.Id) != null)
            {
                throw new DealAlreadyExistsException();
            }
            _packageAndCardRepository.CheckCardForTradingDeal(tradingDeal.CardToTrade, userId);
            return _packageAndCardRepository.CreateNewTradingDeal(tradingDeal);
        }

        public bool TradeSingleCard(HttpSvrEventArgs e)
        {
            Guid userId = _sessionService.AuthenticateUserAndSession(e, null);
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException();
            }
            string? tradeIdString = e.Path.Replace("/tradings/", "");
            Guid? tradeId = Guid.Parse(tradeIdString);
           return _packageAndCardRepository.TradeSingleCard((Guid) tradeId, userId);

        }

        public bool RemoveTradingDeal(HttpSvrEventArgs e)
        {
            Guid userId = _sessionService.AuthenticateUserAndSession(e, null);
            string? tradeIdString = e.Path.Replace("/tradings/", "");
            Guid? tradeId = Guid.Parse(tradeIdString);
            if (tradeId == null)
            {
                throw new NotFoundException("The provided deal ID was not found.");
            }
            if (_packageAndCardRepository.DeleteTradingDeal((Guid)tradeId, userId))
            {
                return true;
            }
            else
            {
                throw new ForbiddenException("The provided deal ID was not found.");
            }

        }

        public List<TradingDeal> GetTradingDeals(HttpSvrEventArgs e)
        {
            _sessionService.AuthenticateUserAndSession(e, null);

            List<TradingDeal> deals = _packageAndCardRepository.GetAllTradingDeals();

            return deals;

        }
    }
}
