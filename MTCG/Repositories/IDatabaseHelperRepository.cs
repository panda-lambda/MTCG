using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Repositories
{
    public interface IDatabaseHelperRepository
    {
        void DropTables(List<string> tableNames);
        void CreateTables();

    }
}
