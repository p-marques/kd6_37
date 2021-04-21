using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace KD6_37
{
    public class StandardMCTS : AbstractThinker
    {
        // Default K
        private const float DEFAULT_K = 1.41f;

        // Default % of time to think can actually be used
        private const float DEFAULT_USED_TIME_TO_THINK_PERCENTAGE = 0.98f;

        private Random _random;

        private float _timeToThink;

        private float _k;

        public int LastRunSimulations { get; private set; }

        public float K => _k;

        public override void Setup(string arguments)
        {
            _timeToThink = TimeLimitMillis *
                    DEFAULT_USED_TIME_TO_THINK_PERCENTAGE;

            _k = DEFAULT_K;

            if (!string.IsNullOrEmpty(arguments))
            {
                if (float.TryParse(arguments, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out float inK))
                {
                    _k = inK;
                }
                else
                    throw new ArgumentException("KD6_37 error: bad arguments.");
            }

            _random = new Random();
        }

        public override FutureMove Think(Board board, CancellationToken ct)
        {
            DateTime startTime = DateTime.Now;

            DateTime deadline = startTime + TimeSpan.FromMilliseconds(_timeToThink);

            StandardMCTSNode root = new StandardMCTSNode(board, FutureMove.NoMove);

            StandardMCTSNode selectedNode;

            LastRunSimulations = 0;

            while (DateTime.Now < deadline)
            {
                MCTS(root);
            }

            selectedNode = SelectMovePolicy(root, 0);

            LastRunSimulations += selectedNode.Playouts;

            return selectedNode.Move;
        }

        private void MCTS(StandardMCTSNode root)
        {
            StandardMCTSNode current = root;

            bool selected = false;

            Stack<StandardMCTSNode> moveSequence = new Stack<StandardMCTSNode>();

            moveSequence.Push(current);

            while (!current.IsTerminal && !selected)
            {
                if (current.IsFullyExpanded)
                {
                    current = SelectMovePolicy(current, _k);
                }
                else
                {
                    current = ExpandPolicy(current);
                    selected = true;
                }

                moveSequence.Push(current);
            }

            Winner endState = current.Playout(PlayoutPolicy);

            while (moveSequence.Count > 0)
            {
                StandardMCTSNode node = moveSequence.Pop();

                node.Playouts++;

                if (endState.ToPColor() == node.Turn.Other())
                {
                    node.Wins++;
                }
                else
                {
                    node.Wins--;
                }
            }
        }

        private StandardMCTSNode SelectMovePolicy(StandardMCTSNode node, float k)
        {
            StandardMCTSNode bestChildNode = null;
            float bestUCT = float.NegativeInfinity;

            float lnN = (float)Math.Log(node.Playouts);

            for (int i = 0; i < node.Children.Count; i++)
            {
                StandardMCTSNode childNode = node.Children[i];

                float uct = childNode.Wins / (float)childNode.Playouts
                    + k * (float)Math.Sqrt(lnN / childNode.Playouts);

                if (uct > bestUCT)
                {
                    bestUCT = uct;
                    bestChildNode = childNode;
                }
            }

            return bestChildNode;
        }

        private StandardMCTSNode ExpandPolicy(StandardMCTSNode node)
        {
            IReadOnlyList<FutureMove> untriedMoves = node.UntriedMoves;

            FutureMove move = untriedMoves[_random.Next(untriedMoves.Count)];

            return node.MakeMove(move);
        }

        private FutureMove PlayoutPolicy(IList<FutureMove> availableMoves)
        {
            return availableMoves[_random.Next(availableMoves.Count)];
        }

        public override string ToString() => "G08_KD6-3.7_V2";
    }
}
