
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Security.Cryptography;

namespace PokerEmu {

    public class Program
    {
        static string fn = @"c:\tmp\poker_values_100000.tsv";
        static void print(string s)
        {
            Console.WriteLine(s);
            File.AppendAllLines(fn, [s]);
        }
        static void Main()
        {

            int max = 100000;
            print("hane\twin rate");
            var vals = Enum.GetValues<Value>().ToArray();
            for (int i1 = 0; i1 < vals.Length; i1++)
            {
                for (int i2 = i1; i2 < vals.Length; i2++)
                {
                    var v1 = vals[i1]; 
                    var v2 = vals[i2];

                    if (v1 == v2)
                    {
                        // pair
                        var c1 = new Card(Suite.Spades, v1);
                        var c2 = new Card(Suite.Clubs, v2);
                        SpecificGame(c1, c2, max);
                    }
                    else
                    {
                        // suited
                        var c1 = new Card(Suite.Clubs, v1);
                        var c2 = new Card(Suite.Clubs, v2);
                        SpecificGame(c1, c2, max);
                        // off-suite
                        c1 = new Card(Suite.Heart, v1);
                        c2 = new Card(Suite.Clubs, v2);
                        SpecificGame(c1, c2, max);
                    }
                }
            }



            //Console.WriteLine($"my w = {myW.Name} [{myW.Weight}, {myW.Description}], oppoW = {oppoW.Name} [{oppoW.Weight}, {oppoW.Description}], winner: {winner}");
        }

        private static void SpecificGame(Card c1, Card c2, int max)
        {
            Dictionary<int, int> results = new Dictionary<int, int>();
            Dictionary<string, int> winningCombos = new Dictionary<string, int>();
            for (int i = 0; i < max; i++)
            {
                var (myW, oppoW) = RunGame(c1, c2, randomMyCards: false);
                int winner = myW.Weight > oppoW.Weight ? 1 : oppoW.Weight > myW.Weight ? -1 : 0;
                results[winner] = results.GetValueOrDefault(winner, 0) + 1;
                //if (i % 50000 == 0)
                //{
                //   Console.WriteLine($"{i} of {max}");
                //}
                if (winner == 1) // all
                {
                    var c = (winner > 0 ? myW : oppoW).Name;
                    winningCombos[c] = winningCombos.GetValueOrDefault(c, 0) + 1;
                }
            }
            var combos = string.Join("\t", winningCombos.OrderByDescending(x => x.Value).Select(x => x.Key));
            print($"{c1.Value.ToS()}{c2.Value.ToS()}{(c1.Suite == c2.Suite ? "s" : "o")}\t{(results[1] * 100.0 / max):0.00}%\t{combos}");
        }

        private static void RandomGame(Dictionary<int, int> results, Dictionary<string, int> winningCombos, Card c1, Card c2, int max)
        {
            for (int i = 0; i < max; i++)
            {
                var (myW, oppoW) = RunGame(c1, c2, randomMyCards: true);
                int winner = myW.Weight > oppoW.Weight ? 1 : oppoW.Weight > myW.Weight ? -1 : 0;
                results[winner] = results.GetValueOrDefault(winner, 0) + 1;
                if (i % 50000 == 0)
                {
                    Console.WriteLine($"{i} of {max}");
                }
                if (winner > -2) // all
                {
                    var c = (winner > 0 ? myW : oppoW).Name;
                    winningCombos[c] = winningCombos.GetValueOrDefault(c, 0) + 1;
                }
            }

            foreach (int i in results.Keys)
            {
                Console.WriteLine($"{i}: {results[i]} ({(int)(results[i] * 100 / max)}%)");
            }
            Console.WriteLine();
            foreach (var kv in winningCombos.OrderByDescending(x => x.Value))
            {
                Console.WriteLine($"{kv.Key}: {kv.Value}");
            }
        }

        private static (Combination myW, Combination oppoW) RunGame(Card c1, Card c2, bool randomMyCards = false)
        {
            var deck = new Deck();
            deck.Shuffle();

            var me = new Player(randomMyCards ? [deck.GetNext(), deck.GetNext()] : [deck.GetCard(c1), deck.GetCard(c2)]);
            var oppo = new Player([deck.GetNext(), deck.GetNext()]);

            Card[] onTable = [deck.GetNext(), deck.GetNext(), deck.GetNext(), deck.GetNext(), deck.GetNext()];

            var myCards = me.Cards.Concat(onTable).ToArray();
            var oppoCards = oppo.Cards.Concat(onTable).ToArray();

            var cc = new CombinationChecker();

            var mw = cc.BestCombination(myCards);
            var ow = cc.BestCombination(oppoCards);
            if (mw == null || ow == null) throw new Exception("null combination");
            return (mw, ow);
        }
    }
}