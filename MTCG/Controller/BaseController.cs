using MTCG.HttpServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Controller
{
    public abstract class BaseController
    {
        public abstract void HandleRequest(HttpSvrEventArgs e);
    }
}
