namespace MTCG.Models
{
    public  class Deck
    {
        /// <summary>This class provides the model for the decks.</summary>

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Guid Id { get; set; }
        public Guid? OwnerId { get; set; }
        public string? Description { get; set; }
        public List<Card>? CardList { get; set; }

    }
}
