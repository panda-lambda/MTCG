using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Data
{
    public interface IDatabaseConnectionFactory
    {

        IDbConnection CreateConnection();
    }
}
