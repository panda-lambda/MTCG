using MTCG.Data;
using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace MTCG.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public SessionRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;

        }

        public bool authenticateUser(UserCredentials userCredentials)


        {
            using var connection = _connectionFactory.CreateConnection();

            try
            {

                using (var cmd = connection.CreateCommand())
                {

                    cmd.CommandText = "SELECT ID FROM USERS WHERE NAME = :n AND password =:p";
                    IDataParameter p = cmd.CreateParameter();
                    p.ParameterName = ":n";
                    p.Value = userCredentials.Username;
                    cmd.Parameters.Add(p);


                    IDataParameter n = cmd.CreateParameter();
                    n.ParameterName = ":n";
                    n.Value = userCredentials.Password;
                    cmd.Parameters.Add(n);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exeption in SessionRepository:");

                Console.WriteLine(ex.Message);

            }
            return false;
        }


    }
}
