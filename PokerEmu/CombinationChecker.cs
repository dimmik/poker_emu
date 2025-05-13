using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerEmu
{
    public class CombinationChecker
    {
        public Combination? BestCombination(Card[] cards)
        {
            // If there are less than 5 cards, return null
            if (cards == null || cards.Length < 5)
                return null;

            // Make a copy of the cards array to avoid modifying the original
            Card[] sortedCards = cards.OrderByDescending(c => (int)c.Value).ToArray();

            // Check for each combination in descending order of value
            Combination combination;

            // Royal Flush (weight: 9000000000)
            if ((combination = CheckRoyalFlush(sortedCards)) != null)
                return combination;

            // Straight Flush (weight: 8000000000 + highest card value)
            if ((combination = CheckStraightFlush(sortedCards)) != null)
                return combination;

            // Four of a Kind (weight: 7000000000 + four of a kind value*10000 + kicker value)
            if ((combination = CheckFourOfAKind(sortedCards)) != null)
                return combination;

            // Full House (weight: 6000000000 + three of a kind value*10000 + pair value)
            if ((combination = CheckFullHouse(sortedCards)) != null)
                return combination;

            // Flush (weight: 5000000000 + values of all 5 cards)
            if ((combination = CheckFlush(sortedCards)) != null)
                return combination;

            // Straight (weight: 4000000000 + highest card value)
            if ((combination = CheckStraight(sortedCards)) != null)
                return combination;

            // Three of a Kind (weight: 3000000000 + three of a kind value*10000 + kicker1*100 + kicker2)
            if ((combination = CheckThreeOfAKind(sortedCards)) != null)
                return combination;

            // Two Pair (weight: 2000000000 + high pair value*10000 + low pair value*100 + kicker)
            if ((combination = CheckTwoPair(sortedCards)) != null)
                return combination;

            // One Pair (weight: 1000000000 + pair value*1000000 + kicker1*10000 + kicker2*100 + kicker3)
            if ((combination = CheckOnePair(sortedCards)) != null)
                return combination;

            // High Card (weight: values of all 5 cards in descending order)
            return CheckHighCard(sortedCards);
        }

        private Combination CheckRoyalFlush(Card[] cards)
        {
            // A royal flush is a straight flush with Ace high
            var straightFlush = CheckStraightFlush(cards);
            if (straightFlush != null && straightFlush.Cards[0].Value == Value.Ace)
            {
                return new Combination
                {
                    Name = "Royal Flush",
                    Description = $"Royal Flush of {straightFlush.Cards[0].Suite}",
                    Weight = 9000000000,
                    Cards = straightFlush.Cards
                };
            }
            return null;
        }

        private Combination CheckStraightFlush(Card[] cards)
        {
            // Group cards by suite
            var suitedCards = cards.GroupBy(c => c.Suite)
                                  .Where(g => g.Count() >= 5)
                                  .Select(g => g.ToArray());

            foreach (var sameSuitCards in suitedCards)
            {
                var straightFlush = CheckStraight(sameSuitCards);
                if (straightFlush != null)
                {
                    return new Combination
                    {
                        Name = "Straight Flush",
                        Description = $"Straight Flush, {straightFlush.Cards[0].Value} high of {straightFlush.Cards[0].Suite}",
                        Weight = 8000000000 + (int)straightFlush.Cards[0].Value,
                        Cards = straightFlush.Cards
                    };
                }
            }
            return null;
        }

        private Combination CheckFourOfAKind(Card[] cards)
        {
            var groups = cards.GroupBy(c => c.Value)
                             .Where(g => g.Count() == 4)
                             .OrderByDescending(g => (int)g.Key)
                             .ToArray();

            if (groups.Length > 0)
            {
                var fourOfAKind = groups[0].ToArray();
                var kicker = cards.Where(c => c.Value != fourOfAKind[0].Value)
                                 .OrderByDescending(c => (int)c.Value)
                                 .FirstOrDefault();

                Card[] combinationCards = new Card[5];
                Array.Copy(fourOfAKind, 0, combinationCards, 0, 4);
                combinationCards[4] = kicker;

                return new Combination
                {
                    Name = "Four of a Kind",
                    Description = $"Four {fourOfAKind[0].Value}s with {kicker.Value} kicker",
                    Weight = 7000000000 + (int)fourOfAKind[0].Value * 10000 + (int)kicker.Value,
                    Cards = combinationCards
                };
            }
            return null;
        }

        private Combination CheckFullHouse(Card[] cards)
        {
            var valueGroups = cards.GroupBy(c => c.Value)
                                  .Select(g => new { Value = g.Key, Cards = g.ToArray() })
                                  .OrderByDescending(g => g.Cards.Length)
                                  .ThenByDescending(g => (int)g.Value)
                                  .ToArray();

            if (valueGroups.Length >= 2 && valueGroups[0].Cards.Length >= 3 && valueGroups[1].Cards.Length >= 2)
            {
                var threeCards = valueGroups[0].Cards.Take(3).ToArray();
                var pairCards = valueGroups[1].Cards.Take(2).ToArray();

                Card[] combinationCards = new Card[5];
                Array.Copy(threeCards, 0, combinationCards, 0, 3);
                Array.Copy(pairCards, 0, combinationCards, 3, 2);

                return new Combination
                {
                    Name = "Full House",
                    Description = $"Full House, {threeCards[0].Value}s over {pairCards[0].Value}s",
                    Weight = 6000000000 + (int)threeCards[0].Value * 10000 + (int)pairCards[0].Value,
                    Cards = combinationCards
                };
            }
            return null;
        }

        private Combination CheckFlush(Card[] cards)
        {
            var suitGroups = cards.GroupBy(c => c.Suite)
                                 .Where(g => g.Count() >= 5)
                                 .Select(g => g.OrderByDescending(c => (int)c.Value).Take(5).ToArray())
                                 .FirstOrDefault();

            if (suitGroups != null)
            {
                // Calculate weight based on all 5 cards
                long weight = 5000000000;
                for (int i = 0; i < 5; i++)
                {
                    weight += (int)suitGroups[i].Value * (long)Math.Pow(100, 4 - i);
                }

                return new Combination
                {
                    Name = "Flush",
                    Description = $"Flush, {suitGroups[0].Value} high of {suitGroups[0].Suite}",
                    Weight = weight,
                    Cards = suitGroups
                };
            }
            return null;
        }

        private Combination CheckStraight(Card[] cards)
        {
            // Remove duplicate values
            var distinctCards = cards.GroupBy(c => c.Value)
                                    .Select(g => g.First())
                                    .OrderByDescending(c => (int)c.Value)
                                    .ToArray();

            if (distinctCards.Length < 5)
                return null;

            // Check for A-5-4-3-2 straight
            if (distinctCards[0].Value == Value.Ace)
            {
                bool isLowStraight = false;
                for (int i = 0; i < distinctCards.Length - 3; i++)
                {
                    if (distinctCards[i].Value == Value.Five &&
                        distinctCards[i + 1].Value == Value.Four &&
                        distinctCards[i + 2].Value == Value.Three &&
                        distinctCards[i + 3].Value == Value.Two)
                    {
                        isLowStraight = true;
                        Card[] straightCards = new Card[5];
                        straightCards[0] = distinctCards[i]; // 5
                        straightCards[1] = distinctCards[i + 1]; // 4
                        straightCards[2] = distinctCards[i + 2]; // 3
                        straightCards[3] = distinctCards[i + 3]; // 2
                                                                 // Find an Ace for the fifth card
                        straightCards[4] = cards.First(c => c.Value == Value.Ace);

                        return new Combination
                        {
                            Name = "Straight",
                            Description = "Straight, 5 high",
                            Weight = 4000000000 + (int)Value.Five,
                            Cards = straightCards
                        };
                    }
                }
            }

            // Check for regular straights
            for (int i = 0; i <= distinctCards.Length - 5; i++)
            {
                if ((int)distinctCards[i].Value - (int)distinctCards[i + 4].Value == 4)
                {
                    Card[] straightCards = new Card[5];
                    for (int j = 0; j < 5; j++)
                    {
                        straightCards[j] = distinctCards[i + j];
                    }

                    return new Combination
                    {
                        Name = "Straight",
                        Description = $"Straight, {straightCards[0].Value} high",
                        Weight = 4000000000 + (int)straightCards[0].Value,
                        Cards = straightCards
                    };
                }
            }

            return null;
        }

        private Combination CheckThreeOfAKind(Card[] cards)
        {
            var valueGroups = cards.GroupBy(c => c.Value)
                                  .Where(g => g.Count() == 3)
                                  .OrderByDescending(g => (int)g.Key)
                                  .Select(g => g.ToArray())
                                  .FirstOrDefault();

            if (valueGroups != null)
            {
                var kickers = cards.Where(c => c.Value != valueGroups[0].Value)
                                  .OrderByDescending(c => (int)c.Value)
                                  .Take(2)
                                  .ToArray();

                Card[] combinationCards = new Card[5];
                Array.Copy(valueGroups, 0, combinationCards, 0, 3);
                Array.Copy(kickers, 0, combinationCards, 3, 2);

                return new Combination
                {
                    Name = "Three of a Kind",
                    Description = $"Three of a Kind, {valueGroups[0].Value}s with {kickers[0].Value}, {kickers[1].Value} kickers",
                    Weight = 3000000000 + (int)valueGroups[0].Value * 10000 + (int)kickers[0].Value * 100 + (int)kickers[1].Value,
                    Cards = combinationCards
                };
            }
            return null;
        }

        private Combination CheckTwoPair(Card[] cards)
        {
            var pairs = cards.GroupBy(c => c.Value)
                            .Where(g => g.Count() >= 2)
                            .OrderByDescending(g => (int)g.Key)
                            .Select(g => g.Take(2).ToArray())
                            .ToArray();

            if (pairs.Length >= 2)
            {
                var highPair = pairs[0];
                var lowPair = pairs[1];

                var kicker = cards.Where(c => c.Value != highPair[0].Value && c.Value != lowPair[0].Value)
                                 .OrderByDescending(c => (int)c.Value)
                                 .FirstOrDefault();

                Card[] combinationCards = new Card[5];
                Array.Copy(highPair, 0, combinationCards, 0, 2);
                Array.Copy(lowPair, 0, combinationCards, 2, 2);
                combinationCards[4] = kicker;

                return new Combination
                {
                    Name = "Two Pair",
                    Description = $"Two Pair, {highPair[0].Value}s and {lowPair[0].Value}s with {kicker.Value} kicker",
                    Weight = 2000000000 + (int)highPair[0].Value * 10000 + (int)lowPair[0].Value * 100 + (int)kicker.Value,
                    Cards = combinationCards
                };
            }
            return null;
        }

        private Combination CheckOnePair(Card[] cards)
        {
            var pair = cards.GroupBy(c => c.Value)
                           .Where(g => g.Count() >= 2)
                           .OrderByDescending(g => (int)g.Key)
                           .Select(g => g.Take(2).ToArray())
                           .FirstOrDefault();

            if (pair != null)
            {
                var kickers = cards.Where(c => c.Value != pair[0].Value)
                                 .OrderByDescending(c => (int)c.Value)
                                 .Take(3)
                                 .ToArray();

                Card[] combinationCards = new Card[5];
                Array.Copy(pair, 0, combinationCards, 0, 2);
                Array.Copy(kickers, 0, combinationCards, 2, 3);

                return new Combination
                {
                    Name = "One Pair",
                    Description = $"Pair of {pair[0].Value}s with {kickers[0].Value}, {kickers[1].Value}, {kickers[2].Value} kickers",
                    Weight = 1000000000 + (int)pair[0].Value * 1000000 +
                             (int)kickers[0].Value * 10000 +
                             (int)kickers[1].Value * 100 +
                             (int)kickers[2].Value,
                    Cards = combinationCards
                };
            }
            return null;
        }

        private Combination CheckHighCard(Card[] cards)
        {
            var topFive = cards.OrderByDescending(c => (int)c.Value).Take(5).ToArray();

            // Calculate weight based on all 5 cards
            long weight = 0;
            for (int i = 0; i < 5; i++)
            {
                weight += (int)topFive[i].Value * (long)Math.Pow(10, 4 - i);
            }

            return new Combination
            {
                Name = "High Card",
                Description = $"High Card, {topFive[0].Value} of {topFive[0].Suite}",
                Weight = weight,
                Cards = topFive
            };
        }
    }
}
