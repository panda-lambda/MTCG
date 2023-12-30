using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Repositories
{
    public interface IPackageAndCardRepository
    {
        public bool AddPackage(Package package);
        public List<Card>? BuyPackage(Guid userId);

        public List<Card>? GetCardsByUser(Guid userId);
        public List<Card>? GetDeckByUser(Guid userId);
    }
}
