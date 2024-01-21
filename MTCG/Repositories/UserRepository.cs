using MTCG.Data;
using MTCG.Models;
using MTCG.Utilities;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace MTCG.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public UserRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;

        }

        public UserStats GetUserStats(Guid userId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM USERSTATS WHERE ID= :n";
                        IDataParameter n = cmd.CreateParameter();
                        n.ParameterName = ":n";
                        n.Value = userId;
                        cmd.Parameters.Add(n);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                _ = reader.GetGuid(0);
                                string name = reader.GetString(1);
                                int elo = reader.GetInt32(2);
                                int wins = reader.GetInt32(3);
                                int losses = reader.GetInt32(4);


                                return new UserStats
                                {
                                    Name = name,
                                    Elo = elo,
                                    Wins = wins,
                                    Losses = losses
                                };
                            }
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("exception in userrepository getting hashed pw");

                }
                return null;

            }
        }

        public string GetNameByGuid(Guid? userId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT NAME FROM USERS WHERE ID= :n";
                        IDataParameter n = cmd.CreateParameter();
                        n.ParameterName = ":n";
                        n.Value = userId;
                        cmd.Parameters.Add(n);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string name = reader.GetString(0);
                                return name;
                            }
                        }
                    }
                    return String.Empty;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("exception in userrepository getting hashed pw");

                }
                return String.Empty;

            }
        }           



        public List<UserStats>? GetScoreboard()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        List<UserStats> scoreboard = new();
                        cmd.CommandText = "SELECT * FROM USERSTATS ORDER BY ELO DESC";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var userStats = new UserStats
                                {
                                    Id = reader.GetGuid(0),
                                    Name = reader.GetString(1),
                                    Elo = reader.GetInt32(2),
                                    Wins = reader.GetInt32(3),
                                    Losses = reader.GetInt32(4)
                                };
                                scoreboard.Add(userStats);
                            }
                        }
                        return scoreboard;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("exception in userrepository getting hashed pw");

                }
                return null;

            }
        }

        public UserCredentials? GetHashByUsername(string username)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                Console.WriteLine("im userrepo für die pw ");
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM USERS WHERE NAME= :n";
                        IDataParameter n = cmd.CreateParameter();
                        n.ParameterName = ":n";
                        n.Value = username;
                        cmd.Parameters.Add(n);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Guid id = reader.GetGuid(0);
                                string name = reader.GetString(1);
                                string password = reader.GetString(2);

                                Console.WriteLine("token in user repo " + password);
                                return new UserCredentials
                                {
                                    Id = id,
                                    Username = name,
                                    Password = password
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("exception in userrepository getting hashed pw");
                }
                return null;

            }
        }



        public int? GetCoinsByUser(string username)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {

                        cmd.CommandText = "SELECT COINS FROM USERDATA WHERE NAME= :n";
                        IDataParameter n = cmd.CreateParameter();
                        n.ParameterName = ":n";
                        n.Value = username;
                        cmd.Parameters.Add(n);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int coins = reader.GetInt32(0);
                                return coins;
                            }
                        }
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                    return null;
                }
            }
        }

        public bool SetCoinsByUser(string username, int amount)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE USERDATA SET COINS = :coins WHERE NAME = :user";


                        IDataParameter c = cmd.CreateParameter();
                        c.ParameterName = ":coins";
                        c.Value = amount;
                        cmd.Parameters.Add(c);

                        IDataParameter u = cmd.CreateParameter();
                        u.ParameterName = ":user";
                        u.Value = username;
                        cmd.Parameters.Add(u);


                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0; //check if any rows where affected
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                    return false;
                }
            }
        }

        public bool UpdateUserData(Guid userId, UserData userData)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {

                        cmd.CommandText = @"
             INSERT INTO USERDATA(Id, Name, Bio, Image)
             VALUES(:id, :name, :bio, :img)
             ON CONFLICT(Id) DO UPDATE
           SET Name = CASE WHEN EXCLUDED.Name IS NOT NULL THEN EXCLUDED.Name ELSE USERDATA.Name END, 
        Bio = CASE WHEN EXCLUDED.Bio IS NOT NULL THEN EXCLUDED.Bio ELSE USERDATA.Bio END, 
        Image = CASE WHEN EXCLUDED.Image IS NOT NULL THEN EXCLUDED.Image ELSE USERDATA.Image END";

                        IDbDataParameter idP = cmd.CreateParameter();
                        idP.ParameterName = ":id";
                        idP.Value = userId;
                        cmd.Parameters.Add(idP);

                        IDbDataParameter name = cmd.CreateParameter();
                        name.ParameterName = ":name";
                        name.Value = userData.Name;
                        cmd.Parameters.Add(name);

                        IDbDataParameter bio = cmd.CreateParameter();
                        bio.ParameterName = ":bio";
                        bio.Value = userData.Bio;
                        cmd.Parameters.Add(bio);

                        IDbDataParameter img = cmd.CreateParameter();
                        img.ParameterName = ":img";
                        img.Value = userData.Image;
                        cmd.Parameters.Add(img);

                        cmd.ExecuteNonQuery();

                        Console.WriteLine("user updated");
                        Console.WriteLine(userData);

                        return true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exeption in UserRepository UpdateUserdata:");
                    Console.WriteLine(e.Message);
                    return false;
                }

            }
        }

        public Guid? GetGuidByUserName(string userName)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM USERS WHERE NAME= :n";
                        IDataParameter n = cmd.CreateParameter();
                        n.ParameterName = ":n";
                        n.Value = userName;
                        cmd.Parameters.Add(n);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Guid id = reader.GetGuid(0);
                                return id;
                            }
                        }
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exeption in UserRepository getGuidByUserName:");
                    Console.WriteLine(e.Message);
                    return null;
                }

            }


        }
        public UserData? GetUserData(Guid userId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM USERDATA WHERE ID= :n";
                        IDataParameter n = cmd.CreateParameter();
                        n.ParameterName = ":n";
                        n.Value = userId;
                        cmd.Parameters.Add(n);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Guid id = reader.GetGuid(0);

                                string name = reader.GetString(1);
                                string? bio = null;
                                string? img = null;
                                int coins = 0;

                                if (!reader.IsDBNull(2))
                                {
                                    bio = reader.GetString(2);
                                }

                                if (!reader.IsDBNull(3))
                                {
                                    img = reader.GetString(3);
                                }
                                if (!reader.IsDBNull(4))
                                {
                                    coins = reader.GetInt32(4);
                                }
                                Console.WriteLine("coins hat " + coins + " !");
                                return new UserData
                                {
                                    Name = name,
                                    Bio = bio,
                                    Image = img,
                                    Coins = coins
                                };
                            }
                        }
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw new UserNotFoundException("User not found");


                }
            }
        }

        public bool registerUser(UserCredentials userCredentials)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {


                using (var transaction = connection.BeginTransaction())
                using (var cmd = connection.CreateCommand())
                {

                    try
                    {


                        cmd.CommandText = "SELECT COUNT(*) FROM USERS WHERE NAME = :n";
                        IDataParameter p = cmd.CreateParameter();
                        p.ParameterName = ":n";
                        p.Value = userCredentials.Username;
                        cmd.Parameters.Add(p);
                        long userCount = 0;
                        object? count = cmd.ExecuteScalar();
                        if (count != null)
                            userCount = (long)count;
                        if (userCount > 0)
                        {
                            Console.WriteLine($"User with the name {userCredentials.Username} already exists!");
                            return false;
                        }
                        Guid id = Guid.NewGuid();
                        //users
                        cmd.CommandText = $"INSERT INTO USERS (ID, NAME, PASSWORD) VALUES (:id, :n, :p)";
                        p = cmd.CreateParameter();
                        p.ParameterName = ":id";
                        p.Value = id;
                        cmd.Parameters.Add(p);

                        p = cmd.CreateParameter();
                        p.ParameterName = ":n";
                        p.Value = userCredentials.Username;
                        cmd.Parameters.Add(p);

                        p = cmd.CreateParameter();
                        p.ParameterName = ":p";
                        p.Value = userCredentials.Password;
                        cmd.Parameters.Add(p);

                        cmd.ExecuteNonQuery();
                        //userdata
                        cmd.CommandText = $"INSERT INTO USERDATA (ID, NAME, COINS) VALUES (:id, :n, :k)";
                        p = cmd.CreateParameter();
                        p.ParameterName = ":id";
                        p.Value = id;
                        cmd.Parameters.Add(p);

                        p = cmd.CreateParameter();
                        p.ParameterName = ":n";
                        p.Value = userCredentials.Username;
                        cmd.Parameters.Add(p);

                        IDataParameter k = cmd.CreateParameter();
                        k.ParameterName = ":k";
                        k.Value = 20;
                        cmd.Parameters.Add(k);
                        cmd.ExecuteNonQuery();
                        //userstats
                        cmd.CommandText = $"INSERT INTO USERSTATS (ID, NAME, ELO, WINS, LOSSES) VALUES (:id, :n, :k, :wins, :loss)";
                        p = cmd.CreateParameter();
                        p.ParameterName = ":id";
                        p.Value = id;
                        cmd.Parameters.Add(p);

                        p = cmd.CreateParameter();
                        p.ParameterName = ":n";
                        p.Value = userCredentials.Username;
                        cmd.Parameters.Add(p);

                        k = cmd.CreateParameter();
                        k.ParameterName = ":k";
                        k.Value = 1000;
                        cmd.Parameters.Add(k);

                        k = cmd.CreateParameter();
                        k.ParameterName = ":wins";
                        k.Value = 0;
                        cmd.Parameters.Add(k);

                        k = cmd.CreateParameter();
                        k.ParameterName = ":loss";
                        k.Value = 0;
                        cmd.Parameters.Add(k);

                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                        return true;

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("exception in register user repository");
                        Console.WriteLine(ex.Message);
                        transaction.Rollback();
                        throw;
                    }



                }

            }
        }
    }
}
