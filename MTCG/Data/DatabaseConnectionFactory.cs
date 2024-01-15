using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace MTCG.Data
{
    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly string? _connectionString;

        public DatabaseConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("postgres");
        }
        public IDbConnection CreateConnection()
        {
            IDbConnection dbConnection = new NpgsqlConnection(_connectionString);
            dbConnection.Open();

            //Console.WriteLine("Connected to DB");

            return dbConnection;
        }
    }
}
