using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public enum CardType
    {
        /// <summary>This enum provides the card type.</summary>
        Monster,
        Spell,
        
    }


    public enum MonsterType
    {
        None,
        Elf, 
        Ork,
        Dragon, 
        Knight,
        Troll, 
        Goblin,
        Kraken, 
        Wizard
    }
    public enum ElementType
    {
        /// <summary>This enum provides the element type of a card.</summary>
        
        Normal,
        Fire,
        Water,
        Earth,
        Dark,
        Light
    }

    public enum ResultType
    {
        /// <summary>This enum provides the result type of a battle.</summary>

        FirstPlayerWon,
        SecondPlayerWon,
        Draw
    }


    public enum FactionType
    {
        /// <summary>This enum provides the faction type of a card.</summary>

        WaterGoblin, FireGoblin, RegularGoblin, WaterTroll, FireTroll, RegularTroll, WaterElf, FireElf,
        RegularElf, WaterSpell, FireSpell, RegularSpell, Knight, Dragon, Ork, Kraken
    }


    public enum HttpCodes
    {
        OK = 200,
        CREATED = 200,
        NO_CONTENT= 204,
        BAD_REQUEST= 400,
        UNAUTORIZED = 401,
        FORBIDDEN = 403,
        NOT_FOUND = 404, 
        METHOD_NOT_ALLOWED = 405, 
        CONFLICT = 409,
        INTERNAL_SERVER_ERROR = 500
    }
}
