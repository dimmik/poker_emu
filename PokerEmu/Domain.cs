using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerEmu
{
    public enum Suite
    {
        Spades, Clubs, Heart, Diamonds
    }
    public enum Value
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14,
    }
    public record Card(Suite Suite, Value Value);
    public class Combination
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public long Weight { get; set; }
        public Card[] Cards { get; set; } = [];
    }

    public record Player(Card[] Cards)
    {
        public override string ToString()
        {
            return string.Join(",", Cards.Select(c => c.ToString()));
        }
    }
}
