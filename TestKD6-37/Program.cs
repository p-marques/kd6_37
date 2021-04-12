using ColorShapeLinks.Common.AI.Examples;
using KD6_37;

namespace TestKD6_37
{
    class Program
    {
        static void Main(string[] args)
        {
            string whitesName = typeof(RandomThinker).FullName;
            string redsName = typeof(MinimaxAIThinker).FullName;
            Game game = new Game(whitesName, redsName);

            game.Play();
        }
    }
}
