using ColorShapeLinks.Common.AI;

namespace TestKD6_37
{
    internal struct Player
    {
        public IThinker Thinker { get; }
        public string Name { get; }

        public Player(IThinker thinker, string name)
        {
            Thinker = thinker;
            Name = name;
        }
    }
}
