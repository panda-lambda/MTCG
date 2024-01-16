using System;
using MTCG.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Services
{
    public class GameService : IGameService
    {
        private ConcurrentQueue<Player> playerQueue = new ConcurrentQueue<Player>();
    }
}
