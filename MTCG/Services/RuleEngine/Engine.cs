using MTCG.Models;

namespace MTCG.Services.RuleEngine
{
    public class Engine
    {
        private readonly List<IRule> _rules = new()
        {
           // new EffectiveVsIneffectiveReversed(),
            new EffectiveVsIneffective(),
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
                if (rule.Applies(cardOne, cardTwo) || rule.Applies(cardTwo, cardOne))
                {
                    return rule.Execute(cardOne, cardTwo);
                } 


            }
            return null; 
        }
    }
}
