using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MTCG.Data;
using MTCG.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Services
{
    public class DatabaseHelperService : IDatabaseHelperService
    {
        private readonly IConfiguration _configuration;
        private readonly DatabaseHelperRepository _databaseHelperRepository;
        private readonly bool _shouldResetTables;
        private readonly List<string>? _tableNames;

        /// <summary>
        /// gets the tables in the app settings to delete
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="databaseHelperRepository"></param>
        /// <exception cref="Exception"></exception>
        public DatabaseHelperService(IConfiguration configuration, IDatabaseHelperRepository databaseHelperRepository)
        {
            _configuration = configuration;
            _tableNames = _configuration.GetSection("AppSettings:TableNames").Get<List<string>>();
            string? t = _configuration["AppSettings:ResetDB"];

            if (t.IsNullOrEmpty() || _tableNames.IsNullOrEmpty())
            {
                throw new Exception("AppSettings:ResetDB or AppSettings:TableNames not found in appsettings.json");
            }
            else
            {
                _shouldResetTables = bool.Parse(t!);
            }
            _databaseHelperRepository = (DatabaseHelperRepository)databaseHelperRepository;

        }


        /// <summary>
        ///  initializes tables for database
        /// </summary>
        public void InitializeDatabase()
        {
            
            if (_shouldResetTables)
            {
                ResetTables();
            }
            InitializeTables();

        }

        private void ResetTables()
        {
            _databaseHelperRepository.DropTables(_tableNames!);
            Console.WriteLine("Dropped all tables!");
        }
        private void InitializeTables()
        {
            _databaseHelperRepository.CreateTables();
            Console.WriteLine("Created tables!");
        }
    }

}
