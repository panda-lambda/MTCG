﻿namespace MTCG.Models
{
    public class Card
    {
        /// <summary>This class provides the model for a card.</summary>

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Guid Id { get; set; }
        public FactionType Name { get; set; }
        public float Damage { get; set; }
        public CardType Type { get; set; }
        public string? Description { set; get; }
        public ElementType Element { get; set; }

    }
}