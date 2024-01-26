using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Repositories;
using MTCG.Utilities;
using Newtonsoft.Json;

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


        /// <summary>
        /// auth and creates a new trading deal 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedException">invalid token</exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ConflictException">deal already exists</exception>
        public bool CreateNewTradingDeal(HttpSvrEventArgs e)
        {
            Guid userId = _sessionService.AuthenticateUserAndSession(e, null);
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedException();
            }

            Console.WriteLine($"in create new trading deal payload: {e.Payload}");
            
            TradingDeal? tradingDeal = JsonConvert.DeserializeObject<TradingDeal>(e.Payload) ?? throw new Exception(" no trading deal in service");
            tradingDeal.OwnerId = userId;
            if (_packageAndCardRepository.GetTradingDealsByTradingId(tradingDeal.Id) != null)
            {
                throw new ConflictException("A deal with this deal ID already exists.");
            }
            _packageAndCardRepository.CheckCardForTradingDeal(tradingDeal.CardToTrade, userId);
            return _packageAndCardRepository.CreateNewTradingDeal(tradingDeal);
        }


        /// <summary>
        /// auth + take on a offer 
        /// </summary>
        /// <param name="e"></param>
        /// <returns>true if success</returns>
        /// <exception cref="UnauthorizedException">invalid token</exception>
        /// <exception cref="NotFoundException">trade deal not found</exception>
        /// <exception cref="ForbiddenException">user not owner/self trade </exception>
        /// <exception cref="Exception"></exception>
        public bool TradeSingleCard(HttpSvrEventArgs e)
        {
            Guid userId = _sessionService.AuthenticateUserAndSession(e, null);
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedException();
            }
            string? tradeIdString = e.Path.Replace("/tradings/", "");
            Guid? tradeId = Guid.Parse(tradeIdString);
            if (tradeId == null)
            {
                throw new NotFoundException("The provided deal ID was not found.");
            }

            string cardBuyingIdString = e.Payload.Replace("\"","");
            Console.WriteLine("nach replace :" +cardBuyingIdString);

            if (!Guid.TryParse(cardBuyingIdString, out Guid cardToMeetReqId))
            {
                throw new ForbiddenException("The offered card is not owned by the user, or the requirements are not met (Type, MinimumDamage), or the offered card is locked in the deck.");
            }

            List<TradingDeal> dealList = _packageAndCardRepository.GetAllTradingDeals();

            foreach (TradingDeal deal in dealList)
            {
                if (deal.Id == tradeId)
                {
                    if (deal.OwnerId == userId)
                    {
                        throw new ForbiddenException("You cannot trade with yourself.");
                    }
                    else
                    {
                        Card? cardToMeetReq = _packageAndCardRepository.GetSingleCard(cardToMeetReqId) ?? throw new Exception("no card to trade in service");

                        if (cardToMeetReq.Damage < deal.MinimumDamage || cardToMeetReq.Type != deal.Type)
                        {
                            throw new ForbiddenException("The offered card is not owned by the user, or the requirements are not met (Type, MinimumDamage), or the offered card is locked in the deck.");
                        }

                        if (_packageAndCardRepository.DeleteTradingDeal((Guid)tradeId, deal.OwnerId))
                        {
                            return _packageAndCardRepository.TradeSingleCard(deal.OwnerId, userId, deal.CardToTrade, cardToMeetReqId);
                        
                        }
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// auth and deletes a trading deal
        /// </summary>
        /// <param name="e"></param>
        /// <returns>true if success</returns>
        /// <exception cref="NotFoundException">tradeing deal not found</exception>
        /// <exception cref="ForbiddenException">user not owner</exception>
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
                throw new ForbiddenException("Not the owner of the deal.");
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
