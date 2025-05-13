
using System.ComponentModel;
using System.Security.Cryptography;

namespace PokerEmu {

    public class Program
    {

        static void Main()
        {

            Console.WriteLine("Hello, World!");


            Dictionary<int, int> results = new Dictionary<int, int>();
            Dictionary<string, int> winningCombos = new Dictionary<string, int>();

            var c1 = new Card(Suite.Clubs, Value.Queen);
            var c2 = new Card(Suite.Diamonds, Value.Seven);
            Combination myW, oppoW;
            int max = 100000;
            for (int i = 0; i < max; i++)
            {
                (myW, oppoW) = RunGame(c1, c2, randomMyCards: true);
                int winner = myW.Weight > oppoW.Weight ? 1 : oppoW.Weight > myW.Weight ? -1 : 0;
                results[winner] = results.GetValueOrDefault(winner, 0) + 1;
                if (i%50000 == 0)
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
                Console.WriteLine($"{i}: {results[i]} ({(int)(results[i]*100/max)}%)");
            }
            Console.WriteLine();
            foreach (var kv in winningCombos.OrderByDescending(x => x.Value))
            {
                Console.WriteLine($"{kv.Key}: {kv.Value}");
            }


            //Console.WriteLine($"my w = {myW.Name} [{myW.Weight}, {myW.Description}], oppoW = {oppoW.Name} [{oppoW.Weight}, {oppoW.Description}], winner: {winner}");
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