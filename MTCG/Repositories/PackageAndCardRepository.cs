using MTCG.Data;
using MTCG.Models;
using MTCG.Utilities;
using Npgsql;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;

namespace MTCG.Repositories
{
    public class PackageAndCardRepository : IPackageAndCardRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public PackageAndCardRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

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

        public Card? GetSingleCard(Guid cardId)
        {
            using var connection = _connectionFactory.CreateConnection();

            {
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = $" SELECT ID, NAME, DAMAGE,TYPE, LOCKED FROM TRADES WHERE Id = :id";
                        IDataParameter uid = cmd.CreateParameter();
                        uid.ParameterName = ":p";
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
                        cmd.CommandText = $"UPDATE CARDS SET OWNERID = :ub WHERE ID = :cts AND OWNERID = :uo  ";
                        IDataParameter id = cmd.CreateParameter();
                        id.ParameterName = ":ub";
                        id.Value = userIdBuying;
                        cmd.Parameters.Add(id);

                        IDataParameter oid = cmd.CreateParameter();
                        oid.ParameterName = ":cts";
                        oid.Value = cardToSell;
                        cmd.Parameters.Add(oid);

                        IDataParameter uid = cmd.CreateParameter();
                        uid.ParameterName = ":uo";
                        uid.Value = userIdOffering;
                        cmd.Parameters.Add(uid);

                        rowsOne = cmd.ExecuteNonQuery();


                        cmd.CommandText = $"UPDATE CARDS SET OWNERID = :ub WHERE ID = :cts AND OWNERID = :uo  ";
                        id = cmd.CreateParameter();
                        id.ParameterName = ":ub";
                        id.Value = userIdOffering;
                        cmd.Parameters.Add(id);

                        oid = cmd.CreateParameter();
                        oid.ParameterName = ":cts";
                        oid.Value = cardBuying;
                        cmd.Parameters.Add(oid);

                        uid = cmd.CreateParameter();
                        uid.ParameterName = ":uo";
                        uid.Value = userIdBuying;
                        cmd.Parameters.Add(uid);

                        rowsTwo = cmd.ExecuteNonQuery();
                        return rowsOne > 0 && rowsTwo > 0;
                    }
                }
                catch (Exception ex)
                {
                    throw new InternalServerErrorException(ex.Message + " in gettradingdealsbyTradingdeal -packagerepository");
                }
            }
        }
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
                        cmd.CommandText = $"Select OWNERID, CARDTOTRACE FROM TRADES WHERE ID = :id ";
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

                            cmd.CommandText = $"UPDATE CARDS SET LOCKED = FALSE WHERE ID = :cardid ";
                            IDataParameter cid = cmd.CreateParameter();
                            cid.ParameterName = ":cardid";
                            cid.Value = cardId;
                            cmd.Parameters.Add(cid);
                            cmd.ExecuteNonQuery();


                        }
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
                    throw new InternalServerErrorException(ex.Message + " removetradingdeal -packagerepository");
                }
                return rowsAffected > 0;
            }
        }

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
                        throw new NoContentException();
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
                        p.Value = tradingDeal.Type.ToString() ?? null;
                        cmd.Parameters.Add(p);

                        p = cmd.CreateParameter();
                        p.ParameterName = ":mindmg";
                        p.Value = tradingDeal.MinimumDamage ?? 0;
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


        public Deck GetDeckByUser(Guid userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            try
            {
                Deck deck = new Deck();
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
                            throw new InvalidCardCountInDeck("Invalid card count in deck");
                        }
                    }
                }

                return deck;
            }
            catch (UserHasNoCardsException)
            {
                throw;
            }
            catch (InvalidCardCountInDeck)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InternalServerErrorException(ex.Message + " in GetDeckByUser repository");
            }

        }

        public void UpdateCardsById(Guid userId, Deck deck)
        {
            using var connection = _connectionFactory.CreateConnection();
            try
            {
                if (deck != null && deck.CardList != null && deck.CardList.Count == 0)
                {
                    return;
                }
                using (var transaction = connection.BeginTransaction())
                {

                    using (var cmd = connection.CreateCommand())
                    {

                        cmd.CommandText = $"UPDATE DECKS SET CardId1 = NULL, CardId2 = NULL, CardId3 = NULL, CardId4 = NULL WHERE OwnerId = :uid";
                        IDataParameter p = cmd.CreateParameter();
                        p.ParameterName = ":uid";
                        p.Value = userId;
                        cmd.Parameters.Add(p);
                        cmd.ExecuteNonQuery();

                        cmd.Parameters.Clear();
                        //now delete cards from user
                        string inClause = "UPDATE CARDS SET OwnerId = :uid WHERE Id IN (";

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
            }
            catch (Exception ex)
            {
                throw new InternalServerErrorException(ex.Message + " in updatecardsbyid -packagerepository");
            }
        }

        public List<Card>? GetCardsByUser(Guid userId)
        {
            Console.WriteLine(" in getcardsbyuser repo");
            Console.WriteLine("user id in get cards by user : " + userId);
            using var connection = _connectionFactory.CreateConnection();
            try
            {

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


            catch (Exception ex)
            {
                throw new InternalServerErrorException(ex.Message + " in GetCardsbyuser repository");
            }
        }


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
                            throw;
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
                                    throw new InsufficientCoinsException("not enough coins!");
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
                            Console.WriteLine("card1 " + cardIds[1]);
                            throw new NoAvailableCardsException("No cards avail or no package found!");
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

                        List<Card> cardList = new List<Card>();




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

                    catch (InsufficientCoinsException)
                    {
                        throw;
                    }
                    catch (NoAvailableCardsException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine(ex.Message);
                        return null;
                    }
                }
            }
        }

        public bool ConfigureDeckForUser(List<Guid> cardList, Guid user)
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
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
                    }
                }
                return true;





            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Innere Ausnahme: " + ex.InnerException.Message);
                }
                Console.WriteLine("StackTrace: " + ex.StackTrace);
                return false;
            }



        }
    }
}
