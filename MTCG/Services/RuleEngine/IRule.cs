using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Services.RuleEngine
{
    public interface IRule
    {
        int Priority { get; }
        bool Applies(Card card1, Card card2);
        RoundResult Execute(Card cardOne, Card cardTwo);
    }
}
