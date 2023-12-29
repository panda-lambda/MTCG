using MTCG.HttpServer;
using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Services
{
    public interface IPackageAndCardService
    {
        public string CreateNewPackage(HttpSvrEventArgs e);
        public List<Card>? BuyPackage(HttpSvrEventArgs e);
    }
}
