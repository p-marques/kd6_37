using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI;
using System;
using System.Collections.Generic;
using System.Threading;

namespace KD6_37
{
    public class KD6_37MCTSThinker : AbstractThinker
    {
        // Default time available to think
        private const float DEFAULT_TIME_TO_THINK = 0.2f;

        // Default % of time to think can actually be used
        private const float DEFAULT_USED_TIME_TO_THINK_PERCENTAGE = 0.98f;

        private Random _random;

        private float _timeToThink;

        private float _k;

        private float _c;

        public int LastRunSimulations { get; private set; }

        public float K => _k;

        public float C => _c;

        public override void Setup(string str)
        {
            if (float.TryParse(str, out float value) && value > 0f)
            {
                _timeToThink = value * DEFAULT_USED_TIME_TO_THINK_PERCENTAGE;
            }
            else
            {
                _timeToThink = DEFAULT_TIME_TO_THINK *
                    DEFAULT_USED_TIME_TO_THINK_PERCENTAGE;
            }

            _k = (2 / (float)Math.Sqrt(2));
            _c = 0.1f;

            _random = new Random();
        }

        public override FutureMove Think(Board board, CancellationToken ct)
        {
            DateTime startTime = DateTime.Now;

            DateTime deadline = startTime + TimeSpan.FromSeconds(_timeToThink);

            KD6_37MCSTNode root = new KD6_37MCSTNode(board, FutureMove.NoMove);

            KD6_37MCSTNode selectedNode;

            LastRunSimulations = 0;

            while (DateTime.Now < deadline)
            {
                MCTS(root);
            }

            selectedNode = SelectMovePolicy(root, 0, 0);

            LastRunSimulations += selectedNode.Playouts;

            return selectedNode.Move;
        }

        private void MCTS(KD6_37MCSTNode root)
        {
            KD6_37MCSTNode current = root;

            bool selected = false;

            Stack<KD6_37MCSTNode> moveSequence = new Stack<KD6_37MCSTNode>();

            moveSequence.Push(current);

            while (!current.IsTerminal && !selected)
            {
                if (current.IsFullyExpanded)
                {
                    current = SelectMovePolicy(current, _k, _c);
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
                KD6_37MCSTNode node = moveSequence.Pop();

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

        private KD6_37MCSTNode SelectMovePolicy(KD6_37MCSTNode node, float k, float c)
        {
            KD6_37MCSTNode bestChildNode = null;
            float bestUCT = float.NegativeInfinity;

            float lnN = (float)Math.Log(node.Playouts);

            for (int i = 0; i < node.Children.Count; i++)
            {
                KD6_37MCSTNode childNode = node.Children[i];

                float uct = childNode.Wins / (float)childNode.Playouts
                    + k * (float)Math.Sqrt(lnN / childNode.Playouts)
                    + c * GetBoardValue(childNode.Board, node.Turn);

                if (uct > bestUCT)
                {
                    bestUCT = uct;
                    bestChildNode = childNode;
                }
            }

            return bestChildNode;
        }

        private KD6_37MCSTNode ExpandPolicy(KD6_37MCSTNode node)
        {
            IReadOnlyList<FutureMove> untriedMoves = node.UntriedMoves;

            FutureMove move = untriedMoves[_random.Next(untriedMoves.Count)];

            return node.MakeMove(move);
        }

        private FutureMove PlayoutPolicy(IList<FutureMove> availableMoves)
        {
            return availableMoves[_random.Next(availableMoves.Count)];
        }

        private float GetBoardValue(Board board, PColor turn)
        {
            float Dist(float x1, float y1, float x2, float y2)
            {
                return (float)Math.Sqrt(
                    Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
            }

            float centerColumn = board.cols / 2f;
            float centerRow = board.rows / 2;

            float maxScoreCenterCenter = Dist(centerRow, centerColumn, 0, 0);

            float score = 0;

            for (int i = 0; i < board.rows; i++)
            {
                for (int j = 0; j < board.cols; j++)
                {
                    Piece? piece = board[i, j];

                    if (!piece.HasValue) continue;

                    if (piece.Value.color == turn || piece.Value.shape == turn.Shape())
                    {
                        score += maxScoreCenterCenter - Dist(centerRow, centerColumn, i, j);
                    }
                    else
                    {
                        score -= maxScoreCenterCenter - Dist(centerRow, centerColumn, i, j);
                    }
                }
            }

            return score;
        }

        public override string ToString() => "G08KD6-3.7V2";
    }
}
