using ColorShapeLinks.Common.AI.Examples;
using KD6_37;
using System;

namespace TestKD6_37
{
    class Program
    {
        static void Main(string[] args)
        {
            int runCount = 1;

            if (args.Length >= 1 && !int.TryParse(args[0], out runCount))
            {
                Console.WriteLine("Only the first argument is read and " +
                        "it must be an integer number.");
                return;
            }

            if (runCount < 1 || runCount > 1000)
            {
                Console.WriteLine("Simulations run count must be between 1 and 1000");
                return;
            }

            string whitesName = typeof(MinimaxAIThinker).FullName;
            string redsName = typeof(KD6_37MCTSThinker).FullName; 

            Game game = new Game(whitesName, redsName);

            game.Run(runCount);
        }
    }
}
