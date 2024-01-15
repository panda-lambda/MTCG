﻿using MTCG.Data;
using MTCG.Models;
using MTCG.Utilities;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
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


        public Deck GetDeckByUser(Guid userId)
        {

            using var connection = _connectionFactory.CreateConnection();
            try
            {


                Deck deck = new Deck();

                Console.WriteLine("uid: " + userId.ToString());
                using (var cmd = connection.CreateCommand())
                {
                    List<Guid>? cardIds = new List<Guid>();

                    deck.CardList = new List<Card>();


                    cmd.CommandText = $"SELECT Id, Description CardId1, CardId2, CardId3, CardId4 FROM DECKS WHERE OWNERID = :p";

                    IDataParameter uid = cmd.CreateParameter();
                    uid.ParameterName = ":p";
                    uid.Value = userId;
                    cmd.Parameters.Add(uid);


                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {

                            deck.Id = reader.GetGuid(0);
                            if (!reader.IsDBNull(1))
                            {
                                deck.Description = reader.GetString(1);
                            }

                            for (int i = 2; i < 5; i++)
                            {
                                if (!reader.IsDBNull(i))
                                {
                                    cardIds.Add(reader.GetGuid(i));
                                }
                            }
                        }
                        else
                        {
                            throw new UserHasNoCardsException("The request was fine, but the deck doesn't have any cards");
                        }
                    }

                    Console.WriteLine("lesen des decks");

                    cmd.CommandText = "SELECT Id, Name, Damage, Locked FROM Cards WHERE Id IN (";

                    for (int i = 0; i < cardIds.Count; i++)
                    {
                        string paramName = "@id" + i;
                        cmd.CommandText += paramName;

                        // Füge ein Komma hinzu, außer nach dem letzten Parameter
                        if (i < cardIds.Count - 1)
                        {
                            cmd.CommandText += ", ";
                        }

                        var param = cmd.CreateParameter();
                        param.ParameterName = paramName;
                        param.Value = cardIds[i];
                        cmd.Parameters.Add(param);
                    }

                    cmd.CommandText += ")";

                    using (var reader = cmd.ExecuteReader())

                    {
                        Console.WriteLine("nach execture reader select");
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
                            card.Locked = reader.GetBoolean(3);

                            deck.CardList.Add(card);
                        }
                        if (deck.CardList.Count < 1)
                        {
                            throw new InvalidCardCountInDeck("invalid card count in deck");
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
                throw new InternalServerErrorException(ex.Message + " in GetCrdsbyuser repository");
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
                        cmd.CommandText = "INSERT INTO CARDS (ID, NAME, DAMAGE, TYPE, LOCKED, ELEMENT) VALUES (:id, :n, :dmg, :t, :lock, :elm)";

                        var pId = cmd.CreateParameter();
                        pId.ParameterName = ":id";
                        cmd.Parameters.Add(pId);

                        var pName = cmd.CreateParameter();
                        pName.ParameterName = ":n";
                        cmd.Parameters.Add(pName);

                        var pDamage = cmd.CreateParameter();
                        pDamage.ParameterName = ":dmg";
                        cmd.Parameters.Add(pDamage);

                        var pType = cmd.CreateParameter();
                        pType.ParameterName = ":t";
                        cmd.Parameters.Add(pType);

                        var pLocked = cmd.CreateParameter();
                        pLocked.ParameterName = ":lock";
                        cmd.Parameters.Add(pLocked);

                        var pElement = cmd.CreateParameter();
                        pElement.ParameterName = ":elm";
                        cmd.Parameters.Add(pElement);

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
                    Console.WriteLine( "in configure deck");

                    using (var cmd = connection.CreateCommand())
                    {
                        Guid newId = Guid.NewGuid();

                        cmd.CommandText = @"
            INSERT INTO DECKS (Id, OwnerId, CardId1, CardId2, CardId3, CardId4) 
            VALUES (:id,:owner, :card1, :card2, :card3, :card4)
            ON CONFLICT (OwnerId) DO UPDATE 
            SET CardId1 = EXCLUDED.CardId1, 
                CardId2 = EXCLUDED.CardId2, 
                CardId3 = EXCLUDED.CardId3, 
                CardId4 = EXCLUDED.CardId4";

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
