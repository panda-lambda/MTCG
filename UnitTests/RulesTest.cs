using MTCG.Models;
using System.Net.Sockets;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using MTCG.Services;
using MTCG.Services.RuleEngine;
namespace UnitTests
{
    public class RulesTest
    {
        private readonly Engine engine = new();

        [SetUp]
        public void Setup()
        {
                    }


        [Test]
        public void effivsineffiTest()
        {
            var cardOne = new Card { Type = CardType.Spell, Element = ElementType.Water, Damage = 10 };
            var cardTwo = new Card { Type = CardType.Spell, Element = ElementType.Fire, Damage = 10 };

            RoundResult? res = engine.Evaluate(cardOne, cardTwo);

            Assert.That(res, Is.Not.EqualTo(null), "res is null");
            Assert.That(res?.Result, Is.EqualTo(ResultType.FirstPlayerWon), "res null bei ");

        }

        [Test]
        [TestCase(CardType.Spell, CardType.Monster, ExpectedResult = ResultType.FirstPlayerWon)]
        [TestCase(CardType.Spell, CardType.Spell, ExpectedResult = ResultType.FirstPlayerWon)]
        [TestCase(CardType.Monster, CardType.Monster, ExpectedResult = ResultType.Draw)]
        [TestCase(CardType.Monster, CardType.Spell, ExpectedResult = ResultType.FirstPlayerWon)]
        public ResultType effivsineffiNotAppliedFirstPlayerTest(CardType firstType, CardType secondType)
        {
            var cardOne = new Card { Type = firstType, Element = ElementType.Water, Damage = 10 };
            var cardTwo = new Card { Type = secondType, Element = ElementType.Fire, Damage = 10 };

            RoundResult? res = engine.Evaluate(cardOne, cardTwo);

            Assert.That(res, Is.Not.EqualTo(null), "res is null");


            return (ResultType)res.Result;
        }

        [Test]
        [TestCase(CardType.Spell, CardType.Monster, ExpectedResult = ResultType.SecondPlayerWon)]
        [TestCase(CardType.Spell, CardType.Spell, ExpectedResult = ResultType.SecondPlayerWon)]
        [TestCase(CardType.Monster, CardType.Monster, ExpectedResult = ResultType.Draw)]
        [TestCase(CardType.Monster, CardType.Spell, ExpectedResult = ResultType.SecondPlayerWon)]
        public ResultType effivsineffiNotAppliedSecondPlayerTest(CardType firstType, CardType secondType)
        {
            var cardOne = new Card { Type = firstType, Element = ElementType.Water, Damage = 10 };
            var cardTwo = new Card { Type = secondType, Element = ElementType.Fire, Damage = 10 };

            RoundResult? res = engine.Evaluate(cardTwo, cardOne);

            Assert.That(res, Is.Not.EqualTo(null), "res is null");
            return (ResultType)res.Result;

        }


        [Test]
        [TestCase(CardType.Spell, CardType.Monster, MonsterType.None, MonsterType.Knight, ExpectedResult = ResultType.FirstPlayerWon)]
        [TestCase(CardType.Monster, CardType.Spell, MonsterType.Knight, MonsterType.None, ExpectedResult = ResultType.SecondPlayerWon)]
        public ResultType ImmunityMonsterFights(CardType firstType, CardType secondType, MonsterType mOneType, MonsterType mTwoType)
        {

            var cardOne = new Card { Type = firstType, Element = ElementType.Water, Monster = mOneType, Damage = 10 };
            var cardTwo = new Card { Type = secondType, Element = ElementType.Water, Monster = mTwoType, Damage = 10 };

            RoundResult? res = engine.Evaluate(cardOne, cardTwo);

            Assert.That(res, Is.Not.EqualTo(null), "res is null");
            return (ResultType)res.Result;

        }

        [Test]
        [TestCase(CardType.Monster, CardType.Monster, MonsterType.Goblin, MonsterType.Dragon, ExpectedResult = ResultType.SecondPlayerWon)]
        [TestCase(CardType.Monster, CardType.Monster, MonsterType.Dragon, MonsterType.Goblin, ExpectedResult = ResultType.FirstPlayerWon)]
        [TestCase(CardType.Monster, CardType.Monster, MonsterType.Elf, MonsterType.Dragon, ExpectedResult = ResultType.FirstPlayerWon)]
        [TestCase(CardType.Monster, CardType.Monster, MonsterType.Dragon, MonsterType.Elf, ExpectedResult = ResultType.Draw)]
        [TestCase(CardType.Monster, CardType.Spell, MonsterType.Knight, MonsterType.None, ExpectedResult = ResultType.SecondPlayerWon)]

        public ResultType RegularMonsterFight(CardType firstType, CardType secondType, MonsterType mOneType, MonsterType mTwoType)
        {
            var cardOne = new Card { Type = firstType, Element = ElementType.Fire, Monster = mOneType, Damage = 10 };
            var cardTwo = new Card { Type = secondType, Element = ElementType.Water, Monster = mTwoType, Damage = 10 };

            RoundResult? res = engine.Evaluate(cardOne, cardTwo);

            Assert.That(res, Is.Not.EqualTo(null), "res is null");
            return (ResultType)res.Result;
        }


        [Test]
        public void CardNotFilledShouldDraw()
        {

            Card cardOne = new();
            Card cardTwo = new();
     
            RoundResult? res = engine.Evaluate(cardOne, cardTwo);
            Console.WriteLine(res.Result);
            Assert.That(res, Is.Not.EqualTo(null), "res is null");
            Assert.That(res.Result, Is.Not.EqualTo(null), "res is null");
            Assert.That(res.Result, Is.EqualTo(ResultType.Draw));
        }




    }
}