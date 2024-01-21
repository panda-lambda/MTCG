using MTCG.HttpServer;
using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Services
{
    public interface IBattleService
    {
        public void StartBattle(HttpSvrEventArgs e);
        public UserStats GetUserStats(HttpSvrEventArgs e);

        public List<UserStats> GetScoreboard(HttpSvrEventArgs e);
    }
}
