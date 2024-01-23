using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Services.RuleEngine
{
    public class Engine
    {
        private readonly List<IRule> _rules = new()
        {
            new EffectiveVsIneffectiveReversed(),
            new EffectiveVsIneffective(),
            new ImmunityReversed(),
            new InstantKillReversed(),
            new InstantKill(),
            new Immunity(),
            new DefaultFight()
        };

        public Engine()
        {
            _rules = _rules.OrderBy(Rule => Rule.Priority).ToList();
        }


        public RoundResult? Evaluate(Card cardOne, Card cardTwo)
        {

            foreach (IRule rule in _rules)
            {
                if (rule.Applies(cardOne, cardTwo))
                {
                    return rule.Execute(cardOne, cardTwo);
                }
            }
            return null; 
        }
    }
}
