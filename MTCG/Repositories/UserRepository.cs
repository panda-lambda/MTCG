using MTCG.Data;
using MTCG.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MTCG.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public UserRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;

        }
        public UserCredentials? GetUserByUsername(string username)
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


        public bool registerUser(UserCredentials userCredentials)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "CREATE TABLE IF NOT EXISTS USERS (id UUID PRIMARY KEY, name VARCHAR(255), password VARCHAR(255))";
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                    }


                    using (var cmd = connection.CreateCommand())
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
                        Console.WriteLine($"Count: {userCount}");

                        if (userCount > 0)
                        {
                            // User already exists
                            Console.WriteLine($"User with the name {userCredentials.Username} already exists!");

                            cmd.CommandText = "SELECT * FROM USERS";
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Guid id = reader.GetGuid(0);
                                    string name = reader.GetString(1);
                                    string password = reader.GetString(2);

                                    Console.WriteLine($"User ID: {id.ToString()}, Name: {name}, Password: {password}");
                                }
                            }
                            return false;
                        }

                        Console.WriteLine("after existzing check; ");
                    }


                    using (var cmd = connection.CreateCommand())
                    {


                        Guid id = Guid.NewGuid();

                        cmd.CommandText = $"INSERT INTO USERS (ID, NAME, PASSWORD) VALUES (:id, :n, :p)";
                        IDataParameter p = cmd.CreateParameter();
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
                        cmd.Dispose();


                    }

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM USERS";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                Guid id = reader.GetGuid(0);
                                string name = reader.GetString(1);
                                string password = reader.GetString(2);

                                Console.WriteLine($"User ID: {id.ToString()}, Name: {name}, Password: {password}");
                            }
                        }
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exeption in UserRepository:");
                    Console.WriteLine(e.Message);
                    return false;
                }

            }

        }
    }
}
