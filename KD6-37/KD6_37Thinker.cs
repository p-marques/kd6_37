using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI;
using System;
using System.Threading;

namespace KD6_37
{
    public class KD6_37Thinker : AbstractThinker
    {
        public const int DEFAULT_MAX_DEPTH = 3;

        private int _maxDepth;

        public override void Setup(string str)
        {
            _maxDepth = DEFAULT_MAX_DEPTH;

            if (int.TryParse(str, out int newDepth) && newDepth > 0)
            {
                _maxDepth = newDepth;
            }
        }

        public override FutureMove Think(Board board, CancellationToken ct)
        {
            (FutureMove move, float score) result = 
                Negamax(board, ct, board.Turn, 0);

            return result.move;
        }

        private (FutureMove move, float score) Negamax(
            Board board, CancellationToken ct, 
            PColor turn, int depth)
        {
            (FutureMove bestMove, float bestScore) selectedMove;
            Winner boardStatus;

            if (ct.IsCancellationRequested)
            {
                selectedMove = (FutureMove.NoMove, float.NaN);
            }
            else if ((boardStatus = board.CheckWinner()) != Winner.None)
            {
                if (boardStatus.ToPColor() == turn)
                    selectedMove = (FutureMove.NoMove, float.PositiveInfinity);
                else if (boardStatus == Winner.Draw)
                    selectedMove = (FutureMove.NoMove, 0f);
                else
                    selectedMove = (FutureMove.NoMove, float.NegativeInfinity);
            }
            else if (depth == _maxDepth)
            {
                selectedMove = (FutureMove.NoMove, Evaluate(board, turn));
            }
            else
            {
                selectedMove = (FutureMove.NoMove, float.NegativeInfinity);

                for (int i = 0; i < Cols; i++)
                {
                    if (board.IsColumnFull(i)) continue;

                    for (int j = 0; j < 2; j++)
                    {
                        PShape shape = (PShape)j;

                        if (board.PieceCount(turn, shape) == 0) continue;

                        board.DoMove(shape, i);

                        float lastScore =
                            -Negamax(board, ct, turn.Other(), depth + 1).score;

                        board.UndoMove();

                        if (lastScore > selectedMove.bestScore)
                        {
                            selectedMove = (new FutureMove(i, shape), lastScore);
                        }
                    }
                }
            }

            return selectedMove;
        }

        private float Evaluate(Board board, PColor color)
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

                    if (piece.Value.color == color || piece.Value.shape == color.Shape())
                    {
                        score += maxScoreCenterCenter - Dist(centerRow, centerColumn, i, j);

                        score += centerColumn - Dist(0, centerColumn, i, j);
                    }
                    else
                    {
                        score -= maxScoreCenterCenter - Dist(centerRow, centerColumn, i, j);

                        score -= centerColumn - Dist(0, centerColumn, i, j);
                    }
                }
            }

            return score;
        }

        public override string ToString() => "G08KD6-3.7V1";
    }
}
