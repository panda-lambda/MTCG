using MTCG.Data;
using MTCG.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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

        public bool AddPackage(Package package)
        {
            Console.WriteLine(" in addpackge repo");
            using var connection = _connectionFactory.CreateConnection();


            try
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS CARDS " +
                        "(Id UUID PRIMARY KEY," +
                        " Name VARCHAR(100)," +
                        " Damage DECIMAL(7, 2)," +
                        " Type VARCHAR(50)," +
                        " Description VARCHAR(350)," +
                        " Element VARCHAR(50)   ," +
                        "OwnerId UUID REFERENCES Users(Id))";
                    cmd.ExecuteNonQuery(); //(id UUID PRIMARY KEY, name VARCHAR(255), password VARCHAR(255))
                    cmd.Dispose();
                    Console.WriteLine("nach create cards");
                }

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS Package (Id UUID PRIMARY KEY," +
                        "CardId1 UUID," +
                        "CardId2 UUID," +
                        "CardId3 UUID," +
                        "CardId4 UUID," +
                        "CardId5 UUID," +
                        "FOREIGN KEY (CardId1) REFERENCES CARDS(Id)," +
                        "FOREIGN KEY (CardId2) REFERENCES CARDS(Id)," +
                        "FOREIGN KEY (CardId3) REFERENCES CARDS(Id)," +
                        "FOREIGN KEY (CardId4) REFERENCES CARDS(Id)," +
                        "FOREIGN KEY (CardId5) REFERENCES CARDS(Id))";
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    Console.WriteLine("nach create pakage");
                }

                if (package.CardList != null)
                {


                    using (var transaction = connection.BeginTransaction())
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = transaction;
                        cmd.CommandText = "INSERT INTO CARDS (ID, NAME, DAMAGE, TYPE, DESCRIPTION, ELEMENT) VALUES (:id, :n, :dmg, :t, :desc, :elm)";

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

                        var pDescription = cmd.CreateParameter();
                        pDescription.ParameterName = ":desc";
                        cmd.Parameters.Add(pDescription);

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
                                if (string.IsNullOrEmpty(card.Description))
                                {
                                    pDescription.Value = DBNull.Value;
                                }
                                else
                                {
                                    pDescription.Value = card.Description;
                                }
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
    }
}
