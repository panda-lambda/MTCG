using System.Data;
using System.Data.Common;
using Npgsql;

namespace MTCG.Data
{
    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        public IDbConnection CreateConnection()
        {
            string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=root;Database=postgres;";
            IDbConnection dbConnection = new NpgsqlConnection(connectionString);
            dbConnection.Open();

            Console.WriteLine("Connected to DB");

            return dbConnection;
        }
    }
}
