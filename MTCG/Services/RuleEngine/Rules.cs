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

    public class EffectiveVsIneffectiveReversed : IRule
    {
        public int Priority { get; } = 3;
        public bool Applies(Card cardOne, Card cardTwo)
        {
            return ((cardOne.Type == CardType.Spell || cardOne.Type == CardType.Spell)
                && ((cardOne.Element == ElementType.Fire && cardTwo.Element == ElementType.Water) ||
                    (cardOne.Element == ElementType.Fire && (cardTwo.Element == ElementType.Normal || cardTwo.Element == ElementType.None)) ||
                   ((cardOne.Element == ElementType.None || cardOne.Element == ElementType.Normal) && cardTwo.Element == ElementType.Water)));


        }
        public RoundResult Execute(Card cardOne, Card cardTwo)
        {
            RoundResult result = new RoundResult();
            return result;
        }
    }

    public class EffectiveVsIneffective : IRule
    {
        public int Priority { get; } = 3;
        public bool Applies(Card cardOne, Card cardTwo)
        {
            return (cardOne.Element == ElementType.Water && cardTwo.Element == ElementType.Fire);

        }

        public RoundResult Execute(Card cardOne, Card cardTwo)
        {
            RoundResult result = new RoundResult();

            result.Result = ResultType.FirstPlayerWon;
            result.LogRoundPlayerTwo = $"You lost due to the attribute  of {cardOne.Name} with your {cardTwo.Name}";
            result.LogRoundPlayerOne = $"You won due to the attribute fire of {cardOne.Name} against your {cardTwo.Name}";

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
            RoundResult result = new RoundResult();

            result.Result = ResultType.FirstPlayerWon;
            result.LogRoundPlayerTwo = $"Your {cardTwo.Name} cannot attack {cardOne.Name} because of immunity!";
            result.LogRoundPlayerOne = $"Your {cardOne.Name} is immune against {cardTwo.Name}!";

            return result;

        }
    }

    public class ImmunityReversed : IRule
    {
        public int Priority { get; } = 1;
        public bool Applies(Card cardOne, Card cardTwo)
        {
            return ((cardTwo.Monster == MonsterType.Dragon && cardOne.Monster == MonsterType.Goblin) ||
                    (cardTwo.Monster == MonsterType.Wizard && cardOne.Monster == MonsterType.Ork) ||
                    (cardTwo.Monster == MonsterType.Kraken && cardOne.Type == CardType.Spell) ||
                    (cardTwo.Monster == MonsterType.Elf && cardTwo.Element == ElementType.Fire && cardOne.Monster == MonsterType.Dragon)
                    );
        }

        public RoundResult Execute(Card cardOne, Card cardTwo)
        {
            RoundResult result = new RoundResult();
            result.Result = ResultType.SecondPlayerWon;
            result.LogRoundPlayerOne = $"Your {cardTwo.Name} cannot attack {cardOne.Name} because of immunity!";
            result.LogRoundPlayerTwo = $"Your {cardOne.Name} is immune against {cardTwo.Name}!";
            return result;
        }
    }

    public class InstantKillReversed : IRule
    {
        public int Priority { get; } = 1;
        public bool Applies(Card cardOne, Card cardTwo)
        {
            return (cardOne.Monster == MonsterType.Knight && cardTwo.Type == CardType.Spell && cardTwo.Element == ElementType.Water);
        }

        public RoundResult Execute(Card cardOne, Card cardTwo)
        {
            RoundResult result = new RoundResult();
            result.Result = ResultType.SecondPlayerWon;
            result.LogRoundPlayerOne = $"Your {cardOne.Name} is too heavy, it instantly drowned against {cardTwo.Name}!";
            result.LogRoundPlayerTwo = $"Your {cardTwo.Name} instantly drowned {cardOne.Name} !";
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
            RoundResult result = new RoundResult();

            result.Result = ResultType.FirstPlayerWon;
            result.LogRoundPlayerOne = $"Your {cardOne.Name} is too heavy,it instantly drowned against {cardTwo.Name}!";
            result.LogRoundPlayerTwo = $"Your {cardTwo.Name} instantly drowned {cardOne.Name}!";


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
            RoundResult result = new RoundResult();
            if (cardOne.Damage > cardTwo.Damage)
            {
                result.Result = ResultType.FirstPlayerWon;

                result.LogRoundPlayerTwo = $"Your {cardTwo.Name} lost with {cardTwo.Damage} damage against {cardOne.Name} with {cardOne.Damage} damage!";
                result.LogRoundPlayerOne = $"Your {cardOne.Name} won with {cardOne.Damage} damage against {cardTwo.Name} with {cardTwo.Damage} damage!";
            }
            else if (cardOne.Damage < cardTwo.Damage)
            {
                result.Result = ResultType.SecondPlayerWon;

                result.LogRoundPlayerTwo = $"Your {cardTwo.Name} won with {cardTwo.Damage} damage against {cardOne.Name} with {cardOne.Damage} damage!";
                result.LogRoundPlayerOne = $"Your {cardOne.Name} lost with {cardOne.Damage} damage against {cardTwo.Name} with {cardTwo.Damage} damage!";
            }
            else
            {
                result.Result = ResultType.Draw;

                result.LogRoundPlayerTwo = $"Your {cardTwo.Name} draw with {cardTwo.Damage} damage against {cardOne.Name} with {cardOne.Damage} damage!";
                result.LogRoundPlayerOne = $"Your {cardOne.Name} draw with {cardOne.Damage} damage against {cardTwo.Name} with {cardTwo.Damage} damage!";
            }

            return result;

        }
    }
}
