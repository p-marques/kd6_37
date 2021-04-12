using System.Collections.Generic;
using System.Threading;
using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI;
using System;

namespace KD6_37
{
    public class RandomThinker : AbstractThinker
    {
		private List<FutureMove> _possibleMoves;
        private List<FutureMove> _nonLosingMoves;
        private Random _random;

        public override void Setup(string str)
        {
            _possibleMoves = new List<FutureMove>();
            _nonLosingMoves = new List<FutureMove>();
            _random = new Random();
        }

        public override FutureMove Think(Board board, CancellationToken ct)
        {
            Winner winner;
            PColor colorOfOurAI = board.Turn;

            _possibleMoves.Clear();
            _nonLosingMoves.Clear();

            for (int col = 0; col < Cols; col++)
            {
                if (board.IsColumnFull(col)) continue;

                for (int shp = 0; shp < 2; shp++)
                {
                    PShape shape = (PShape)shp;

                    if (board.PieceCount(colorOfOurAI, shape) == 0) continue;

                    _possibleMoves.Add(new FutureMove(col, shape));

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
                        _nonLosingMoves.Add(new FutureMove(col, shape));
                    }
                }
            }

            if (_nonLosingMoves.Count > 0)
                return _nonLosingMoves[_random.Next(_nonLosingMoves.Count)];

            return _possibleMoves[_random.Next(_possibleMoves.Count)];

        }
    }
}
