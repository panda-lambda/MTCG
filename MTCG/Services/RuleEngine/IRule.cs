using MTCG.Models;

namespace MTCG.Services.RuleEngine
{
    public interface IRule
    {
        int Priority { get; }
        bool Applies(Card card1, Card card2);
        RoundResult Execute(Card cardOne, Card cardTwo);
    }
}
