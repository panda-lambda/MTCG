using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MTCG.Services.RuleEngine
{

    public class FireVsWaterRule : IRule
    {
        public int Priority { get; } = 2;
        public bool Applies(Card cardOne, Card cardTwo)
        {
            return (cardOne.Element == ElementType.Fire && cardTwo.Element == ElementType.Water);

        }


        public RoundResult Execute(Card cardOne, Card cardTwo)
        {
            RoundResult result = new RoundResult();
            return result;
        }
    }

    public class WaterVsFireRule : IRule
    {
        public int Priority { get; } = 2;
        public bool Applies(Card cardOne, Card cardTwo)
        {
            return (cardOne.Element == ElementType.Water && cardTwo.Element == ElementType.Fire);

        }

        public RoundResult Execute(Card cardOne, Card cardTwo)
        {
            RoundResult result = new RoundResult();

            result.Result = ResultType.FirstPlayerWon;
            result.LogRoundPlayerTwo= $"You lost to the attribute fire of {cardOne.Name} with your {cardTwo.Name}";
            result.LogRoundPlayerOne = $"You won due to the attribute fire of {cardOne.Name} against {cardTwo.Name}";

            return result;

        }
    }
}
