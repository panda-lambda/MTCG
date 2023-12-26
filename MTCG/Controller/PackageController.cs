using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Controller
{
    public class PackageController : BaseController
    {

        private IPackageService _packageService;
        public PackageController(IPackageService packageService)
        {
            _packageService = packageService ?? throw new ArgumentNullException(nameof(packageService));
        }


        public override void HandleRequest(HttpSvrEventArgs e)
        {
            if (e.Method == "POST")
            {
                if (e.Path.StartsWith("/packages"))
                {
                    CreateNewCardPackage(e);

                }
                if (e.Path.StartsWith("/transactions/packages"))
                {
                    BuyCardPackage(e);
                }


            }
        }

        public void CreateNewCardPackage(HttpSvrEventArgs e)
        {
            e.Reply((int)HttpCodes.CREATED, "Package and cards successfully created.");
            e.Reply((int)HttpCodes.UNAUTORIZED, "Access token is missing or invalid");
            e.Reply((int)HttpCodes.FORBIDDEN, "Provided user is not \"admin\"");
            e.Reply((int)HttpCodes.CONFLICT, "At least one card in the packages already exists");



        }
        public void BuyCardPackage(HttpSvrEventArgs e)
        {
            e.Reply((int)HttpCodes.OK, "A package has been successfully bought.");
            e.Reply((int)HttpCodes.UNAUTORIZED, "Access token is missing or invalid");//TODO: get response/unauthorized error
            e.Reply((int)HttpCodes.FORBIDDEN, "Not enough money for buying a card package.");
            e.Reply((int)HttpCodes.NOT_FOUND, " No card package available for buying.");
        }


    }
}
