using MTCG.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MTCG.Services.RuleEngine
{


    public class EffectiveVsIneffective : IRule
    {
        public int Priority { get; } = 5;
        public bool Applies(Card cardOne, Card cardTwo)
        {
            return ((cardOne.Type == CardType.Spell || cardTwo.Type == CardType.Spell) &&
                ((cardOne.Element == ElementType.Water && cardTwo.Element == ElementType.Fire) ||
                (cardOne.Element == ElementType.Fire && cardTwo.Element == ElementType.Normal) ||
                (cardOne.Element == ElementType.Normal && cardTwo.Element == ElementType.Water)));

        }

        public RoundResult Execute(Card cardOne, Card cardTwo)

        {
            Console.WriteLine($"effectivness fight with {cardOne.Name} and {cardTwo.Name}");
            RoundResult result = new();

            if (Applies(cardOne, cardTwo))
            {
                result.LogRoundPlayerOne = $"Your {cardOne.Name} did double the damage ({cardOne.Damage * 2}) due to its {cardOne.Element} element against {cardTwo.Name} with {cardTwo.Element} elementm, which did half the damage ({cardTwo.Damage * 0.5})!";
                result.LogRoundPlayerTwo = $"Your {cardTwo.Name} did half the damage ({cardTwo.Damage * 0.5}) due to its {cardTwo.Element} element against {cardOne.Name} with {cardOne.Element} element, which did double the damage ({cardOne.Damage * 2})!";
                if (cardOne.Damage * 2 > cardTwo.Damage * 0.5)
                {
                    result.Result = ResultType.FirstPlayerWon;
                }
                else if (cardOne.Damage * 2 < cardTwo.Damage * 0.5)
                {
                    result.Result = ResultType.SecondPlayerWon;
                }
                else if (cardOne.Damage * 2 == cardTwo.Damage * 0.5)
                {
                    result.Result = ResultType.Draw;
                }
            }

            if (Applies(cardTwo, cardOne))
            {
                result.LogRoundPlayerOne = $"Your {cardOne.Name} did half the damage ({cardOne.Damage * 0.5}) due to its {cardOne.Element} element against {cardTwo.Name} with {cardTwo.Element} element, which did double the damage ({cardTwo.Damage * 2})!";
                result.LogRoundPlayerTwo = $"Your {cardTwo.Name} did double the damage ({cardTwo.Damage * 2}) due to its {cardTwo.Element} element against {cardOne.Name} with {cardOne.Element} element, which did half the damage ({cardOne.Damage * 0.5})!";

                if (cardOne.Damage * 0.5 > cardTwo.Damage * 2)
                {
                    result.Result = ResultType.FirstPlayerWon;
                }
                else if (cardOne.Damage * 0.5 < cardTwo.Damage * 2)
                {
                    result.Result = ResultType.SecondPlayerWon;
                }
                else if (cardOne.Damage * 0.5 == cardTwo.Damage * 2)
                {
                    result.Result = ResultType.Draw;
                }
            }
            return result;
        }
    }


    public class Immunity : IRule
    {
        public int Priority { get; } = 1;
        public bool Applies(Card cardOne, Card cardTwo)
        {
            return ((cardOne.Monster == MonsterType.Dragon && cardTwo.Monster == MonsterType.Goblin) ||
                    (cardOne.Monster == MonsterType.Wizard && cardTwo.Monster == MonsterType.Ork) ||
                    (cardOne.Monster == MonsterType.Kraken && cardTwo.Type == CardType.Spell) ||
                    (cardOne.Monster == MonsterType.Elf && cardOne.Element == ElementType.Fire && cardTwo.Monster == MonsterType.Dragon)
                    );
        }

        public RoundResult Execute(Card cardOne, Card cardTwo)
        {
            RoundResult result = new();
            Console.WriteLine($"immunity fight with {cardOne.Name} and {cardTwo.Name}");

            if (Applies(cardOne, cardTwo))
            {
                result.Result = ResultType.FirstPlayerWon;
                result.LogRoundPlayerOne = $"Your {cardOne.Name} is immune against {cardTwo.Name}!";
                result.LogRoundPlayerTwo = $"Your {cardTwo.Name} cannot attack {cardOne.Name} because of immunity!";
            }
            else if (Applies(cardTwo, cardOne))
            {
                result.Result = ResultType.SecondPlayerWon;
                result.LogRoundPlayerOne = $"Your {cardOne.Name} cannot attack {cardTwo.Name} because of immunity!";
                result.LogRoundPlayerTwo = $"Your {cardTwo.Name} is immune against {cardOne.Name}!";
            }
            return result;

        }
    }
    public class InstantKill : IRule
    {
        public int Priority { get; } = 1;
        public bool Applies(Card cardOne, Card cardTwo)
        {
            return (cardOne.Type == CardType.Spell && cardOne.Element == ElementType.Water && cardTwo.Monster == MonsterType.Knight);
        }

        public RoundResult Execute(Card cardOne, Card cardTwo)
        {
            Console.WriteLine($"Instant kill with {cardOne.Name} and {cardTwo.Name}");
            RoundResult result = new RoundResult();
            if (Applies(cardOne, cardTwo))
            {
                result.Result = ResultType.FirstPlayerWon;
                result.LogRoundPlayerOne = $"Your {cardOne.Name} instantly drowned {cardTwo.Name} !";
                result.LogRoundPlayerTwo = $"Your {cardTwo.Name} is too heavy, it instantly drowned against {cardOne.Name}!";
            }

            else if (Applies(cardTwo, cardOne))
            {
                result.Result = ResultType.SecondPlayerWon;
                result.LogRoundPlayerOne = $"Your {cardOne.Name} is too heavy,it instantly drowned against {cardTwo.Name}!";
                result.LogRoundPlayerTwo = $"Your {cardTwo.Name} instantly drowned {cardOne.Name}!";
            }
            return result;
        }
    }

    public class DefaultFight : IRule
    {
        public int Priority { get; } = 10;
        public bool Applies(Card cardOne, Card cardTwo)
        {
            return true;
        }

        public RoundResult Execute(Card cardOne, Card cardTwo)
        {
            Console.WriteLine($"Default fight with {cardOne.Name} and {cardTwo.Name}");
            RoundResult result = new RoundResult();
            if (cardOne.Damage > cardTwo.Damage)
            {
                result.Result = ResultType.FirstPlayerWon;
            }
            else if (cardOne.Damage < cardTwo.Damage)
            {
                result.Result = ResultType.SecondPlayerWon;
            }
            else
            {
                result.Result = ResultType.Draw;
            }
            return result;
        }
    }
}
