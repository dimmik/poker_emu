
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Net;
using System.Security.Cryptography;

namespace PokerEmu {

    public class Program
    {
        static string fn = @"c:\tmp\poker_t_values_1000_1.tsv";
        static void print(string s)
        {
            Console.WriteLine(s);
            File.AppendAllLines(fn, [s]);
        }
        static void Main()
        {

            int max = 1000;
            print("hand\twin rate");
            var vals = Enum.GetValues<Value>().ToArray();
            var winTable = new Dictionary<(Card, Card), int>();
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
                        int res = SpecificGame(c1, c2, max);
                        winTable[(c1, c2)] = res;
                    }
                    else
                    {
                        // suited
                        var c1 = new Card(Suite.Clubs, v1);
                        var c2 = new Card(Suite.Clubs, v2);
                        int res = SpecificGame(c1, c2, max);
                        winTable[(c1, c2)] = res;
                        // off-suite
                        c1 = new Card(Suite.Spades, v1);
                        c2 = new Card(Suite.Clubs, v2);
                        res = SpecificGame(c1, c2, max);
                        winTable[(c1, c2)] = res;
                    }
                }
            }
            print("\n\nWin Table");
            print($"o\\s\t{string.Join("\t", vals.Reverse().Select(v => v.ToS()))}");
            for (int i1 = vals.Length - 1; i1 >= 0; i1--)
            {
                var str = $"{vals[i1].ToS()}";
                for (int i2 = vals.Length - 1; i2 >= 0; i2--)
                {
                    (Card c1, Card c2) tuple;
                    if (i2 < i1) tuple = (new(Suite.Clubs, vals[i2]), new(Suite.Clubs, vals[i1]));
                    else if (i2 > i1) tuple = (new(Suite.Spades, vals[i1]), new(Suite.Clubs, vals[i2]));
                    else tuple = (new(Suite.Spades, vals[i1]), new(Suite.Clubs, vals[i2]));
                    int res = winTable[tuple];
                    str += $"\t{res * 100.0/max:0.00}%";
                }
                print(str);
            }



            //Console.WriteLine($"my w = {myW.Name} [{myW.Weight}, {myW.Description}], oppoW = {oppoW.Name} [{oppoW.Weight}, {oppoW.Description}], winner: {winner}");
        }

        private static int SpecificGame(Card c1, Card c2, int max)
        {
            Dictionary<int, int> results = new Dictionary<int, int>()
            {
                { 0, 0 },
                { 1, 0 },
                { -1, 0 },
            };
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
            var combos = string.Join("\t", winningCombos.OrderByDescending(x => x.Value).Select(x => $"{x.Key}\t{x.Value * 100.0 / results[1]:0.00}%"));
            print($"{c1.Value.ToS()}{c2.Value.ToS()}{(c1.Suite == c2.Suite ? "s" : "o")}\t{(results[1] * 100.0 / max):0.00}%\t{combos}");
            return results[1];
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