using System.Collections.Generic;
using System.Threading;
using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI;
using System;

namespace KD6_37
{
    public class BaseKD637 : AbstractThinker
    {
		private List<FutureMove> possibleMoves;
        private List<FutureMove> nonLosingMoves;
        private Random random;

        public override void Setup(string str)
        {
            possibleMoves = new List<FutureMove>();
            nonLosingMoves = new List<FutureMove>();
            random = new Random();
        }

        public override FutureMove Think(Board board, CancellationToken ct)
        {
            Winner winner;
            PColor colorOfOurAI = board.Turn;

            possibleMoves.Clear();
            nonLosingMoves.Clear();

            for (int col = 0; col < Cols; col++)
            {
                if (board.IsColumnFull(col)) continue;

                for (int shp = 0; shp < 2; shp++)
                {
                    PShape shape = (PShape)shp;

                    if (board.PieceCount(colorOfOurAI, shape) == 0) continue;

                    possibleMoves.Add(new FutureMove(col, shape));

                    board.DoMove(shape, col);

                    winner = board.CheckWinner();

                    // immediately
                    board.UndoMove();

                    if (winner.ToPColor() == colorOfOurAI)
                    {
                        return new FutureMove(col, shape);
                    }
                    else if (winner.ToPColor() != colorOfOurAI.Other())
                    {
                        nonLosingMoves.Add(new FutureMove(col, shape));
                    }
                }
            }

            if (nonLosingMoves.Count > 0)
                return nonLosingMoves[random.Next(nonLosingMoves.Count)];

            return possibleMoves[random.Next(possibleMoves.Count)];

        }
    }
}
