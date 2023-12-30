using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public  class Deck
    {

        public Guid Id { get; set; }
        public Guid? OwnerId { get; set; }
        public string? Description { get; set; }
        public List<Card>? CardList { get; set; }

    }
}
