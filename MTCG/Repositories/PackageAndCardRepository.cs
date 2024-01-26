using MTCG.Data;
using MTCG.Models;
using MTCG.Utilities;
using System.Data;
using System.Transactions;

namespace MTCG.Repositories
{
    public class PackageAndCardRepository : IPackageAndCardRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public PackageAndCardRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }


        /// <summary>
        /// gets the card that is offered in a trading deal
        /// </summary>
        /// <param name="tradeId">the trade guid</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException">deal id was not found</exception>
        /// <exception cref="InternalServerErrorException">something went wrong</exception>
        public Guid? GetCardToTradeFromTradingDeal(Guid tradeId)
        {
            using var connection = _connectionFactory.CreateConnection();
            using (var cmd = connection.CreateCommand())
            {
                try
                {
                    cmd.CommandText = $" SELECT CARDTOTRADE FROM TRADES WHERE Id = :id ";
                    IDataParameter cid = cmd.CreateParameter();
                    cid.ParameterName = ":id";
                    cid.Value = tradeId;
                    cmd.Parameters.Add(cid);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            throw new NotFoundException("The provided deal ID was not found");
                        }
                        return reader.GetGuid(0);
                    }
                }
                catch (Exception ex)
                {
                    throw new InternalServerErrorException(ex.Message + " in gettradingdealsbyTradingdeal -packagerepository");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardId">guid of the card</param>
        /// <param name="userId">guid of the supposed owner</param>
        /// <returns></returns>
        /// <exception cref="ForbiddenException">not owner of the card</exception>
        /// <exception cref="ConflictException">card is locked, so its already in a deal</exception>

        public bool CheckCardForTradingDeal(Guid cardId, Guid userId)
        {
            Console.WriteLine($"check if {cardId} belongs to {userId}");

            using var connection = _connectionFactory.CreateConnection();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $" SELECT Id, Locked FROM CARDS WHERE Id = :id AND OwnerId = :uid";
                IDataParameter cid = cmd.CreateParameter();
                cid.ParameterName = ":id";
                cid.Value = cardId;
                cmd.Parameters.Add(cid);

                IDataParameter uid = cmd.CreateParameter();
                uid.ParameterName = ":uid";
                uid.Value = userId;
                cmd.Parameters.Add(uid);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        //if there is no card with that id and that owner, ->trade is forbidden
                        throw new ForbiddenException("The deal contains a card that is not owned by the user or locked in the deck.");
                    }
                    if (reader.GetBoolean(1))
                    {
                        throw new ConflictException("The card is already in a trade.");
                    }

                }

                cmd.Parameters.Clear();

                cmd.CommandText = $" SELECT * FROM DECKS " +
            " WHERE ownerId = :uid AND(CardId1 = :cardId" +
            " OR CardId2 = :cardId " +
           " OR CardId3 = :cardId " +
            "OR CardId4 = :cardId)";


                cid = cmd.CreateParameter();
                cid.ParameterName = ":cardId";
                cid.Value = cardId;
                cmd.Parameters.Add(cid);

                uid = cmd.CreateParameter();
                uid.ParameterName = ":uid";
                uid.Value = userId;
                cmd.Parameters.Add(uid);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        throw new ForbiddenException("The deal contains a card that is not owned by the user or locked in the deck.");
                    }
                }
                return true;
            }


        }
        /// <summary>
        /// //checks if a trading deal with this id exists
        /// </summary>
        /// <param name="tradingId">trading id</param>
        /// <returns>the guid if existent</returns>

        public Guid? GetTradingDealsByTradingId(Guid tradingId)
        {
            using var connection = _connectionFactory.CreateConnection();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $" SELECT * FROM TRADES WHERE Id = :id";
                IDataParameter uid = cmd.CreateParameter();
                uid.ParameterName = ":id";
                uid.Value = tradingId;
                cmd.Parameters.Add(uid);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0)) // chek if there is a trade
                            return (reader.GetGuid(0));
                    }
                }
                return null;
            }


        }

        /// <summary>
        /// gets a single card with some attributes
        /// </summary>
        /// <param name="cardId">guid of the card</param>
        /// <returns>card with most attributes (dmg, type, locked, name, id)</returns>
        /// <exception cref="InternalServerErrorException">something went wrong</exception>

        public Card? GetSingleCard(Guid cardId)
        {
            using var connection = _connectionFactory.CreateConnection();

            {
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = $" SELECT ID, NAME, DAMAGE, TYPE, LOCKED FROM CARDS WHERE Id = :id";
                        IDataParameter uid = cmd.CreateParameter();
                        uid.ParameterName = ":id";
                        uid.Value = cardId;
                        cmd.Parameters.Add(uid);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (!reader.IsDBNull(0)) // chek if there is a card
                                {
                                    Card card = new Card
                                    {
                                        Id = reader.GetGuid(0),
                                        Damage = reader.GetFloat(2),
                                        Locked = reader.GetBoolean(4)
                                    };

                                    string nameString = reader.GetString(1);
                                    if (Enum.TryParse<FactionType>(nameString, out FactionType type))
                                    {
                                        card.Name = type;
                                    }
                                    else
                                    {
                                        throw new InternalServerErrorException("Could not convert faction type to enum");
                                    }

                                    nameString = reader.GetString(3);
                                    if (Enum.TryParse<CardType>(nameString, out CardType cardT))
                                    {
                                        card.Type = cardT;
                                    }
                                    else
                                    {
                                        throw new InternalServerErrorException("Could not convert card type to enum");
                                    }
                                    return card;
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    throw new InternalServerErrorException(ex.Message + " in gettradingdealsbyTradingdeal -packagerepository");
                }
            }
        }


        /// <summary>
        /// Trades single card, ->swaps user
        /// </summary>
        /// <param name="userIdOffering">user who created the deal</param>
        /// <param name="userIdBuying">user who is buying</param>
        /// <param name="cardToSell">card id who is offered</param>
        /// <param name="cardBuying">card id which is buying</param>
        /// <returns></returns>
        /// <exception cref="Exception">something went wrong</exception>
        public bool TradeSingleCard(Guid userIdOffering, Guid userIdBuying, Guid cardToSell, Guid cardBuying)
        {
            Console.WriteLine($"{userIdBuying} gets {cardToSell} and {userIdOffering} gets {cardBuying}");

            using var connection = _connectionFactory.CreateConnection();
            using (var transaction = connection.BeginTransaction())
            {
                var rowsOne = 0;
                var rowsTwo = 0;
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {

                        cmd.CommandText = $"Select * FROM CARDS WHERE (ID = :cts AND OWNERID = :uo) OR ( ID = :cts2 AND OWNERID = :uo2) FOR UPDATE ";


                        IDataParameter oid = cmd.CreateParameter();
                        oid.ParameterName = ":cts";
                        oid.Value = cardToSell;
                        cmd.Parameters.Add(oid);

                        IDataParameter uid = cmd.CreateParameter();
                        uid.ParameterName = ":uo";
                        uid.Value = userIdOffering;
                        cmd.Parameters.Add(uid);

                        oid = cmd.CreateParameter();
                        oid.ParameterName = ":cts2";
                        oid.Value = cardBuying;
                        cmd.Parameters.Add(oid);

                        uid = cmd.CreateParameter();
                        uid.ParameterName = ":uo2";
                        uid.Value = userIdBuying;
                        cmd.Parameters.Add(uid);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                throw new Exception("Something went wrong while trading!");
                            }
                        }
                        cmd.Parameters.Clear();

                        cmd.CommandText = $"UPDATE CARDS SET OWNERID = :ub WHERE ID = :cts AND OWNERID = :uo   ";
                        IDataParameter id = cmd.CreateParameter();
                        id.ParameterName = ":ub";
                        id.Value = userIdBuying;
                        cmd.Parameters.Add(id);

                        oid = cmd.CreateParameter();
                        oid.ParameterName = ":cts";
                        oid.Value = cardToSell;
                        cmd.Parameters.Add(oid);

                        uid = cmd.CreateParameter();
                        uid.ParameterName = ":uo";
                        uid.Value = userIdOffering;
                        cmd.Parameters.Add(uid);

                        rowsOne = cmd.ExecuteNonQuery();

                        cmd.Parameters.Clear();

                        cmd.CommandText = $"UPDATE CARDS SET OWNERID = :ub2 WHERE ID = :cts2 AND OWNERID = :uo2  ";
                        id = cmd.CreateParameter();
                        id.ParameterName = ":ub2";
                        id.Value = userIdOffering;
                        cmd.Parameters.Add(id);

                        oid = cmd.CreateParameter();
                        oid.ParameterName = ":cts2";
                        oid.Value = cardBuying;
                        cmd.Parameters.Add(oid);

                        uid = cmd.CreateParameter();
                        uid.ParameterName = ":uo2";
                        uid.Value = userIdBuying;
                        cmd.Parameters.Add(uid);

                        rowsTwo = cmd.ExecuteNonQuery();

                    }
                    transaction.Commit();
                    return rowsOne > 0 && rowsTwo > 0;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine(ex);
                    throw new Exception("Something went wrong while trading cards!");
                }

            }
        }

        /// <summary>
        /// deletes a trading del
        /// </summary>
        /// <param name="tradeId">the guid of the deal</param>
        /// <param name="userId">the owner of the deal</param>
        /// <returns>true if success</returns>
        /// <exception cref="NotFoundException">deal not found</exception>
        /// <exception cref="ForbiddenException">owner not owner</exception>
        /// <exception cref="Exception"></exception>
        public bool DeleteTradingDeal(Guid tradeId, Guid userId)
        {
            int rowsAffected = 0;
            using var connection = _connectionFactory.CreateConnection();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = $"Select OWNERID, CARDTOTRADE FROM TRADES WHERE ID = :id FOR UPDATE";
                        IDataParameter id = cmd.CreateParameter();
                        id.ParameterName = ":id";
                        id.Value = tradeId;
                        cmd.Parameters.Add(id);

                        IDataParameter oid = cmd.CreateParameter();
                        oid.ParameterName = ":oid";
                        oid.Value = userId;
                        cmd.Parameters.Add(oid);
                        Guid cardId;

                        using (var reader = cmd.ExecuteReader())
                        {
                            Guid userIdReal = Guid.Empty;
                            if (!reader.Read())
                            {
                                throw new NotFoundException("The provided deal ID was not found.");
                            }
                            else
                            {
                                userIdReal = reader.GetGuid(0);
                                cardId = reader.GetGuid(1);

                            }

                            if (userIdReal != userId)
                            {
                                throw new ForbiddenException("The deal contains a card that is not owned by the user.");
                            }

                        }


                        cmd.CommandText = $"UPDATE CARDS SET LOCKED = FALSE WHERE ID = :cardid ";
                        IDataParameter cid = cmd.CreateParameter();
                        cid.ParameterName = ":cardid";
                        cid.Value = cardId;
                        cmd.Parameters.Add(cid);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();

                        cmd.CommandText = $"DELETE FROM TRADES WHERE ID = :id AND OWNERID = :oid";

                        id = cmd.CreateParameter();
                        id.ParameterName = ":id";
                        id.Value = tradeId;
                        cmd.Parameters.Add(id);

                        oid = cmd.CreateParameter();
                        oid.ParameterName = ":oid";
                        oid.Value = userId;
                        cmd.Parameters.Add(oid);

                        rowsAffected = cmd.ExecuteNonQuery();

                    }
                    transaction.Commit();

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Something went wrong while deleting the trade!");
                }
                return rowsAffected > 0;
            }
        }


        /// <summary>
        /// get all the availabe trading deals
        /// </summary>
        /// <returns>list of trading deals</returns>
        /// <exception cref="NoContentException">no trading deals</exception>
        /// <exception cref="InternalServerErrorException"></exception>
        public List<TradingDeal> GetAllTradingDeals()
        {
            using var connection = _connectionFactory.CreateConnection();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $" SELECT ID, OWNERID, CARDTOTRADE, TYPE, MINDAMAGE FROM TRADES";

                using (var reader = cmd.ExecuteReader())
                {
                    List<TradingDeal> deals = new();

                    if (!reader.Read())
                        throw new NoContentException("The request was fine, but there are no trading deals available.");
                    do
                    {
                        string? typeS = null;
                        float? minDamage = null;
                        CardType? type = null;
                        Guid id = reader.GetGuid(0);
                        Guid ownerId = reader.GetGuid(1);
                        Guid cardToTrade = reader.GetGuid(2);
                        if (!reader.IsDBNull(4))
                            minDamage = reader.GetFloat(4);
                        if (!reader.IsDBNull(3))
                        {
                            typeS = reader.GetString(3);

                            if (Enum.TryParse<CardType>(typeS, out CardType cardT))
                            {
                                type = cardT;
                            }
                            else
                            {
                                throw new InternalServerErrorException("Could not convert card type to enum");
                            }
                        }
                        TradingDeal singleDeal = new TradingDeal
                        {
                            Id = id,
                            OwnerId = ownerId,
                            CardToTrade = cardToTrade,
                            Type = type,
                            MinimumDamage = minDamage

                        };
                        deals.Add(singleDeal);


                    } while (reader.Read());
                    return deals;
                }
            }
        }


        /// <summary>
        /// creates a new trading deal
        /// </summary>
        /// <param name="tradingDeal">trading deal model</param>
        /// <returns>true if success</returns>
        /// <exception cref="Exception"></exception>
        public bool CreateNewTradingDeal(TradingDeal tradingDeal)
        {

            Console.WriteLine($"damg in tradingdeal {tradingDeal.MinimumDamage}");
            using var connection = _connectionFactory.CreateConnection();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = $"UPDATE CARDS SET LOCKED = TRUE WHERE ID = :id";

                        IDataParameter ap = cmd.CreateParameter();
                        ap.ParameterName = ":id";
                        ap.Value = tradingDeal.CardToTrade;
                        cmd.Parameters.Add(ap);
                        var lockRowsAffected = cmd.ExecuteNonQuery();

                        cmd.Parameters.Clear();

                        cmd.CommandText = $"INSERT INTO TRADES (Id, OwnerId, CardToTrade, Type, MINDAMAGE) VALUES (:id, :oid, :ctd, :type, :mindmg)";

                        IDataParameter p = cmd.CreateParameter();
                        p.ParameterName = ":id";
                        p.Value = tradingDeal.Id;
                        cmd.Parameters.Add(p);

                        p = cmd.CreateParameter();
                        p.ParameterName = ":oid";
                        p.Value = tradingDeal.OwnerId;
                        cmd.Parameters.Add(p);

                        p = cmd.CreateParameter();
                        p.ParameterName = ":ctd";
                        p.Value = tradingDeal.CardToTrade;
                        cmd.Parameters.Add(p);

                        p = cmd.CreateParameter();
                        p.ParameterName = ":type";
                        p.Value = tradingDeal.Type.ToString();
                        cmd.Parameters.Add(p);

                        p = cmd.CreateParameter();
                        p.ParameterName = ":mindmg";
                        p.Value = tradingDeal.MinimumDamage;
                        cmd.Parameters.Add(p);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        transaction.Commit();
                        return rowsAffected > 0 && lockRowsAffected > 0;
                    }
                }


                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        /// <summary>
        /// gets deck by user id
        /// </summary>
        /// <param name="userId">guid of user</param>
        /// <returns> the deck of the user</returns>
        /// <exception cref="InternalServerErrorException"></exception>
        /// <exception cref="NoContentException">user has no cards in the deck</exception>
        public Deck GetDeckByUser(Guid userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            Deck deck = new();
            deck.CardList = new List<Card>();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText =
            $"SELECT d.Id, c.Id, c.Damage, c.Locked,c.Name, c.Type, c.Monster, c.Element " +
           " FROM Decks d " +
           " LEFT JOIN Cards c ON c.Id IN (d.CardId1, d.CardId2, d.CardId3, d.CardId4) " +
          "  WHERE d.OwnerId = :p";

                IDataParameter uid = cmd.CreateParameter();
                uid.ParameterName = ":p";
                uid.Value = userId;
                cmd.Parameters.Add(uid);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (deck.Id == Guid.Empty)
                        {
                            deck.Id = reader.GetGuid(0);

                        }

                        if (!reader.IsDBNull(2)) // chek if there is a card
                        {
                            Card card = new Card
                            {
                                Id = reader.GetGuid(1),
                                Damage = reader.GetFloat(2),
                                Locked = reader.GetBoolean(3)
                            };

                            string nameString = reader.GetString(4);
                            if (Enum.TryParse<FactionType>(nameString, out FactionType type))
                            {
                                card.Name = type;
                            }
                            else
                            {
                                throw new InternalServerErrorException("Could not convert faction type to enum");
                            }

                            nameString = reader.GetString(5);
                            if (Enum.TryParse<CardType>(nameString, out CardType cardT))
                            {
                                card.Type = cardT;
                            }
                            else
                            {
                                throw new InternalServerErrorException("Could not convert card type to enum");
                            }

                            nameString = reader.GetString(6);
                            if (Enum.TryParse<MonsterType>(nameString, out MonsterType monster))
                            {
                                card.Monster = monster;
                            }
                            else
                            {
                                throw new InternalServerErrorException("Could not convert monster type to enum");
                            }

                            nameString = reader.GetString(7);
                            if (Enum.TryParse<ElementType>(nameString, out ElementType elm))
                            {
                                card.Element = elm;
                            }
                            else
                            {
                                throw new InternalServerErrorException("Could not convert Element type to enum");
                            }

                            deck.CardList.Add(card);
                        }
                    }

                    if (deck.CardList.Count < 1)
                    {
                        throw new NoContentException("The request was fine, but the deck doesn't have any cards.");

                    }
                }
            }
            return deck;
        }


        /// <summary>
        /// updates the deck of a user after a battle
        /// </summary>
        /// <param name="userId">guid of user</param>
        /// <param name="deck">deck after the battle</param>
        /// <exception cref="Exception"></exception>
        public void UpdateCardsById(Guid userId, Deck deck)
        {
            using var connection = _connectionFactory.CreateConnection();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    if (deck != null && deck.CardList != null && deck.CardList.Count == 0)
                    {
                        return;
                    }

                    using (var cmd = connection.CreateCommand())
                    {
                        string inClause = $"SELECT *  FROM CARDS WHERE ID IN (";


                        for (int i = 0; i < deck?.CardList?.Count; i++)
                        {
                            string paramName = ":cardId" + i;
                            inClause += paramName;

                            if (i < deck.CardList.Count - 1)
                            {
                                inClause += ", ";
                            }
                            IDataParameter cardP = cmd.CreateParameter();
                            cardP.ParameterName = paramName;
                            cardP.Value = deck.CardList[i].Id;
                            cmd.Parameters.Add(cardP);
                        }

                        inClause += " )FOR UPDATE";
                        cmd.CommandText = inClause;
                        using (var reader = cmd.ExecuteReader())
                        {

                        }
                        cmd.Parameters.Clear();

                        //--------
                        cmd.CommandText = $"UPDATE DECKS SET CardId1 = NULL, CardId2 = NULL, CardId3 = NULL, CardId4 = NULL WHERE OwnerId = :uid";
                        IDataParameter p = cmd.CreateParameter();
                        p.ParameterName = ":uid";
                        p.Value = userId;
                        cmd.Parameters.Add(p);
                        cmd.ExecuteNonQuery();

                        cmd.Parameters.Clear();
                        //now delete cards from user
                        inClause = "UPDATE CARDS SET OwnerId = :uid WHERE Id IN (";

                        for (int i = 0; i < deck?.CardList?.Count; i++)
                        {
                            string paramName = ":cardId" + i;
                            inClause += paramName;

                            if (i < deck.CardList.Count - 1)
                            {
                                inClause += ", ";
                            }
                            IDataParameter cardP = cmd.CreateParameter();
                            cardP.ParameterName = paramName;
                            cardP.Value = deck.CardList[i].Id;
                            cmd.Parameters.Add(cardP);
                        }

                        inClause += ")";
                        cmd.CommandText = inClause;

                        IDataParameter uid = cmd.CreateParameter();
                        uid.ParameterName = ":uid";
                        uid.Value = userId;
                        cmd.Parameters.Add(uid);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                    transaction.Commit();

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine(ex);
                    throw new Exception("Something went wrong while updating the cards after the battle!");
                }
            }
        }

        /// <summary>
        /// changes ownership to null for sold cards
        /// </summary>
        /// <param name="cardIds">List of cards ids</param>
        /// <param name="userId">owner id </param>
        /// <returns>count of sold cards</returns>
        /// <exception cref="Exception"></exception>
        public int SellCards(List<Guid> cardIds, Guid userId)
        {
            int affectedRows = 0;
            using var connection = _connectionFactory.CreateConnection();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    if (cardIds == null || cardIds.Count == 0)
                    {
                        return 0;
                    }
                    using (var cmd = connection.CreateCommand())
                    {

                        string selectCommand = $"SELECT *  FROM CARDS WHERE OWNERID = :uid AND LOCKED = FALSE AND ";



                        IDataParameter uid = cmd.CreateParameter();
                        uid.ParameterName = ":uid";
                        uid.Value = userId;
                        cmd.Parameters.Add(uid);

                        string inClause = "ID IN (";

                        for (int i = 0; i < cardIds.Count; i++)
                        {
                            string paramName = ":cardId" + i;
                            inClause += paramName;

                            if (i < cardIds.Count - 1)
                            {
                                inClause += ", ";
                            }
                            IDataParameter cardP = cmd.CreateParameter();
                            cardP.ParameterName = paramName;
                            cardP.Value = cardIds[i];
                            cmd.Parameters.Add(cardP);
                        }

                        inClause += " )";
                        selectCommand += inClause + " FOR UPDATE";
                        cmd.CommandText = selectCommand;
                        using (var reader = cmd.ExecuteReader())
                        {
                            int counter = 0;
                            while (reader.Read())
                            {

                                counter++;
                            }
                            if (counter != cardIds.Count)
                            {
                                return 0;
                            }
                        }
                      


                        string updateCmd = $"UPDATE CARDS SET OWNERID = NULL WHERE OWNERID = :uid AND LOCKED = FALSE AND " + inClause;


                        cmd.CommandText = updateCmd;
                        affectedRows = cmd.ExecuteNonQuery();

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("exception in selling cards repository");
                    Console.WriteLine(ex);
                    transaction.Rollback();
                    throw new Exception("Something went wrong while selling cards!");
                }
                if (affectedRows != cardIds.Count)
                {
                    transaction.Rollback();
                    return 0;
                }
                transaction.Commit();
                return affectedRows;
            }
        }




        /// <summary>
        /// gets all the cards of a user
        /// </summary>
        /// <param name="userId">owner</param>
        /// <returns>List of cards</returns>
        /// <exception cref="InternalServerErrorException"></exception>
        public List<Card>? GetCardsByUser(Guid userId)
        {
            Console.WriteLine(" in getcardsbyuser repo");
            Console.WriteLine("user id in get cards by user : " + userId);
            using var connection = _connectionFactory.CreateConnection();


            Console.WriteLine("uid: " + userId.ToString());
            using (var cmd = connection.CreateCommand())
            {
                List<Card>? cardList = new List<Card>();
                cmd.CommandText = "SELECT Id, Name, Damage, Locked FROM Cards WHERE OwnerId = :uid";

                IDataParameter uid = cmd.CreateParameter();
                uid.ParameterName = ":uid";
                uid.Value = userId;
                cmd.Parameters.Add(uid);

                cmd.ExecuteNonQuery();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Card card = new Card();

                        card.Id = reader.GetGuid(0);
                        Console.WriteLine("card with " + card.Id.ToString());
                        if (Enum.TryParse<FactionType>(reader.GetString(1), out FactionType type))
                        {
                            card.Name = type;
                        }
                        else
                        {
                            throw new InternalServerErrorException("could not convert factiontype to enum");
                        }

                        card.Damage = reader.GetFloat(2);
                        card.Locked = reader.GetBoolean(3);

                        cardList.Add(card);
                    }

                    Console.WriteLine("in get cards by user");
                    Console.WriteLine(" count cards: " + cardList.Count());

                    return cardList;
                }
            }

        }

        /// <summary>
        /// get all cards
        /// </summary>
        /// <returns>list of all card ids</returns>

        public List<Card> GetAllCards()
        {
            using var connection = _connectionFactory.CreateConnection();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $"SELECT Id FROM Cards";

                using (var reader = cmd.ExecuteReader())
                {
                    List<Card> cards = new();

                    while (reader.Read())
                    {
                        cards.Add(new Card { Id = reader.GetGuid(0) });
                    }
                    return cards;

                }
            }
        }


        /// <summary>
        /// add cards into cards and package to buy
        /// </summary>
        /// <param name="package"> package consisting of 5 cards</param>
        /// <returns>true if success</returns>

        public bool AddPackage(Package package)
        {
            Console.WriteLine(" in addpackge repo");
            using var connection = _connectionFactory.CreateConnection();
            try
            {
                if (package.CardList != null)
                {


                    using (var transaction = connection.BeginTransaction())
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = transaction;
                        cmd.CommandText = "INSERT INTO CARDS (ID, NAME, DAMAGE, TYPE, LOCKED, ELEMENT, MONSTER) VALUES (:id, :n, :dmg, :t, :lock, :elm, :mon)";

                        IDataParameter pId = cmd.CreateParameter();
                        pId.ParameterName = ":id";
                        cmd.Parameters.Add(pId);

                        IDataParameter pName = cmd.CreateParameter();
                        pName.ParameterName = ":n";
                        cmd.Parameters.Add(pName);

                        IDataParameter pDamage = cmd.CreateParameter();
                        pDamage.ParameterName = ":dmg";
                        cmd.Parameters.Add(pDamage);

                        IDataParameter pType = cmd.CreateParameter();
                        pType.ParameterName = ":t";
                        cmd.Parameters.Add(pType);

                        IDataParameter pLocked = cmd.CreateParameter();
                        pLocked.ParameterName = ":lock";
                        cmd.Parameters.Add(pLocked);

                        IDataParameter pElement = cmd.CreateParameter();
                        pElement.ParameterName = ":elm";
                        cmd.Parameters.Add(pElement);
                        IDataParameter pMonster = cmd.CreateParameter();
                        pMonster.ParameterName = ":mon";
                        cmd.Parameters.Add(pMonster);

                        try
                        {
                            foreach (Card card in package.CardList)
                            {
                                pId.Value = card.Id;
                                pName.Value = card.Name.ToString();
                                pDamage.Value = card.Damage;
                                pType.Value = card.Type.ToString();
                                pLocked.Value = card.Locked;
                                pElement.Value = card.Element.ToString();
                                pMonster.Value = card.Monster.ToString();

                                cmd.ExecuteNonQuery();
                            }

                            Console.WriteLine("nach cards einfügen");

                            Guid id = Guid.NewGuid();

                            cmd.CommandText = $"INSERT INTO PACKAGE (ID, CARDID1, CARDID2 , CARDID3, CARDID4, CARDID5) VALUES (:idp, :f, :s, :th, :fo, :fi )";
                            var p = cmd.CreateParameter();
                            p.ParameterName = ":idp";
                            p.Value = id;
                            cmd.Parameters.Add(p);

                            p = cmd.CreateParameter();
                            p.ParameterName = ":f";
                            p.Value = package.CardList[0].Id;
                            cmd.Parameters.Add(p);

                            p = cmd.CreateParameter();
                            p.ParameterName = ":s";
                            p.Value = package.CardList[1].Id;
                            cmd.Parameters.Add(p);

                            p = cmd.CreateParameter();
                            p.ParameterName = ":th";
                            p.Value = package.CardList[2].Id;
                            cmd.Parameters.Add(p);

                            p = cmd.CreateParameter();
                            p.ParameterName = ":fo";
                            p.Value = package.CardList[3].Id;
                            cmd.Parameters.Add(p);

                            p = cmd.CreateParameter();
                            p.ParameterName = ":fi";
                            p.Value = package.CardList[4].Id;
                            cmd.Parameters.Add(p);

                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("exc in adding cards and package");
                            Console.WriteLine(ex.Message);
                            transaction.Rollback();
                            throw new Exception("Something went wrong while adding cards to the packages!");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("exception in package repo addpackage");
                Console.WriteLine(ex.Message);
                return false;
            }


            return false;
        }


        /// <summary>
        /// buys a package, changes ownership of cards, removes the package and removes 5 coins from user
        /// </summary>
        /// <param name="userId">user </param>
        /// <returns></returns>
        /// <exception cref="ForbiddenException">not enough money</exception>
        /// <exception cref="NotFoundException">no package available</exception>
        /// <exception cref="InternalServerErrorException"></exception>

        public List<Card>? BuyPackage(Guid userId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        Int32 coinCount = 0;
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = $"SELECT COINS FROM USERDATA WHERE Id = :uid";
                            IDataParameter userIdParam = cmd.CreateParameter();
                            userIdParam.ParameterName = ":uid";
                            userIdParam.Value = userId;
                            cmd.Parameters.Add(userIdParam);

                            using (var reader = cmd.ExecuteReader())
                            {

                                if (reader.Read())
                                {
                                    coinCount = reader.GetInt32(0);
                                }
                                if (coinCount < 5)
                                {
                                    Console.WriteLine("no money to buy");
                                    throw new ForbiddenException("Not enough money for buying a card package");


                                }
                            }
                        }

                        using (var cmda = connection.CreateCommand())
                        {
                            cmda.CommandText = $"UPDATE UserData SET Coins = :coins WHERE Id = :userId";

                            IDataParameter q = cmda.CreateParameter();
                            q.ParameterName = ":coins";
                            q.Value = coinCount - 5;
                            cmda.Parameters.Add(q);

                            IDataParameter k = cmda.CreateParameter();
                            k.ParameterName = ":userId";
                            k.Value = userId;
                            cmda.Parameters.Add(k);
                            cmda.ExecuteNonQuery();

                        }



                        var cardIds = new List<Guid>();
                        Guid? packageId = null;
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = $"SELECT Id, CardId1, CardId2, CardId3, CardId4, CardId5 FROM Package LIMIT 1";
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    packageId = reader.GetGuid(0);

                                    for (int i = 1; i <= 5; i++)
                                    {
                                        if (!reader.IsDBNull(i))
                                        {
                                            cardIds.Add(reader.GetGuid(i));
                                        }
                                    }
                                }
                            }
                        }
                        if (cardIds.Count != 5 || packageId == null)
                        {
                            Console.WriteLine("no package to buy");
                            throw new NotFoundException("No card package available for buying");
                        }

                        foreach (var cardId in cardIds)
                        {
                            using (var cmda = connection.CreateCommand())
                            {
                                cmda.CommandText = $"UPDATE Cards SET OwnerId = :userId WHERE Id = :cardId";

                                IDataParameter q = cmda.CreateParameter();
                                q.ParameterName = ":userId";
                                q.Value = userId;
                                cmda.Parameters.Add(q);

                                IDataParameter k = cmda.CreateParameter();
                                k.ParameterName = ":cardId";
                                k.Value = cardId;
                                cmda.Parameters.Add(k);
                                cmda.ExecuteNonQuery();

                            }
                        }

                        List<Card> cardList = new();


                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "SELECT Id, Name, Damage FROM Cards WHERE Id IN (";

                            for (int i = 0; i < 5; i++)
                            {
                                string paramName = "@id" + i;
                                cmd.CommandText += paramName + (i < 5 - 1 ? ", " : "");

                                var param = cmd.CreateParameter();
                                param.ParameterName = paramName;
                                param.Value = cardIds[i];
                                cmd.Parameters.Add(param);
                            }

                            cmd.CommandText += ")";

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Card card = new Card();

                                    card.Id = reader.GetGuid(0);
                                    if (Enum.TryParse<FactionType>(reader.GetString(1), out FactionType type))
                                    {
                                        card.Name = type;
                                    }
                                    else
                                    {
                                        throw new InternalServerErrorException("could not convert factiontype to enum");
                                    }

                                    card.Damage = reader.GetFloat(2);

                                    cardList.Add(card);
                                }
                            }
                        }



                        foreach (Card card in cardList)
                        {
                            Console.WriteLine(card.Id + " " + card.Name);
                        }

                        using (var cmdb = connection.CreateCommand())
                        {
                            cmdb.CommandText = $"DELETE FROM Package WHERE Id = :packageId";
                            IDataParameter p = cmdb.CreateParameter();
                            p.ParameterName = ":packageId";
                            p.Value = packageId;
                            cmdb.Parameters.Add(p);
                            cmdb.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return cardList;
                    }


                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine(ex.Message);
                        throw;

                    }
                }
            }
        }


        /// <summary>
        /// configure the deck of a user
        /// </summary>
        /// <param name="cardList">list of cards</param>
        /// <param name="user">owner id</param>
        /// <returns>true if success</returns>
        /// <exception cref="Exception"></exception>

        public bool ConfigureDeckForUser(List<Guid> cardList, Guid user)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        Console.WriteLine("in configure deck");

                        using (var cmd = connection.CreateCommand())
                        {
                            Guid newId = Guid.NewGuid();

                            cmd.CommandText =
                $"INSERT INTO DECKS (Id, OwnerId, CardId1, CardId2, CardId3, CardId4) " +
               " VALUES (:id,:owner, :card1, :card2, :card3, :card4) " +
                "ON CONFLICT (OwnerId) DO UPDATE " +
                "SET CardId1 = EXCLUDED.CardId1, " +
                    "CardId2 = EXCLUDED.CardId2, " +
                    "CardId3 = EXCLUDED.CardId3, " +
                    "CardId4 = EXCLUDED.CardId4";

                            IDbDataParameter idP = cmd.CreateParameter();
                            idP.ParameterName = ":id";
                            idP.Value = newId;
                            cmd.Parameters.Add(idP);

                            IDbDataParameter ownerP = cmd.CreateParameter();
                            ownerP.ParameterName = ":owner";
                            ownerP.Value = user;
                            cmd.Parameters.Add(ownerP);

                            for (int i = 0; i < 4; i++)
                            {
                                IDbDataParameter cardParam = cmd.CreateParameter();
                                cardParam.ParameterName = $":card{i + 1}";
                                cardParam.Value = cardList[i];
                                cmd.Parameters.Add(cardParam);
                            }

                            cmd.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        return true;
                    }



                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Fehler: " + ex.Message);
                        throw new Exception("Something went wrong while updating the deck with your cards");

                    }

                }

            }
        }
    }
}
