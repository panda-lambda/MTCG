using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class PackageModel
    {
        /// <summary>This class provides the model for a package.</summary>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Guid PackageID { get; set; }
        public List<Card>? CardList{ get; set; }


    }
}
