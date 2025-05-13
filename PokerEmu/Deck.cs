namespace PokerEmu
{
    public class Deck
    {
        public List<Card> Cards;
        public Deck()
        {
            Cards = new List<Card>();
            foreach (var s in Enum.GetValues<Suite>())
            {
                foreach (var v in Enum.GetValues<Value>())
                {
                    Cards.Add(new Card(s, v));
                }
            }
        }
        public void Shuffle()
        {
            Cards = Cards.OrderBy(c => System.Guid.NewGuid()).ToList();
        }
        public Card GetNext(bool keep = false)
        {
            if (Cards.Count == 0) throw new Exception("No next card");
            var c = Cards[0];
            if (!keep) Cards.Remove(c);
            return c;
        }
        public Card GetCard(Card c, bool keep = false)
        {
            var idx = Cards.IndexOf(c);
            if (idx == -1)  throw new Exception($"{c} is not in the deck");
            else
            {
                c = Cards[idx];
                if (!keep) Cards.Remove(c);
                return c;
            }
        }
    }
}
