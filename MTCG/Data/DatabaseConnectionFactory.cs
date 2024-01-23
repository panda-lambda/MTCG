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
            try
            {
                IDbConnection dbConnection = new NpgsqlConnection(_connectionString);
                dbConnection.Open();
                return dbConnection;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Could not connect to database!");
                throw;
            }
            

            //Console.WriteLine("Connected to DB");}

          
        }
    }
}
