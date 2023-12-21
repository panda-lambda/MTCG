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
