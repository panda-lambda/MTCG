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

       


    }
}
