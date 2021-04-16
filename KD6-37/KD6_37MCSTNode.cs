using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI;
using System;
using System.Collections.Generic;

namespace KD6_37
{
    public class KD6_37MCSTNode
    {
        private Board _board;

        private IList<FutureMove> _validMoves;

        private List<FutureMove> _untriedMoves;

        public FutureMove Move { get; }

        public bool IsTerminal => _board.CheckWinner() != Winner.None;

        public bool IsFullyExpanded => UntriedMoves.Count == 0;

        public PColor Turn => _board.Turn;

        public Board Board => _board;

        public IEnumerable<FutureMove> ValidMoves
        {
            get
            {
                if (_validMoves == null)
                {
                    _validMoves = DiscernValidMoves(_board);
                }
                
                return _validMoves;
            }
        }

        public IReadOnlyList<FutureMove> UntriedMoves
        {
            get
            {
                if (_untriedMoves == null)
                {
                    _untriedMoves = new List<FutureMove>(ValidMoves);
                }

                return _untriedMoves;
            }
        }

        public int Wins { get; set; }

        public int Playouts { get; set; }

        public IList<KD6_37MCSTNode> Children { get; }

        public KD6_37MCSTNode(Board board, FutureMove move)
        {
            _board = board;

            Move = move;

            Children = new List<KD6_37MCSTNode>();
        }

        public KD6_37MCSTNode MakeMove(FutureMove move)
        {
            Board newBoard = _board.Copy();

            newBoard.DoMove(move.shape, move.column);

            KD6_37MCSTNode child = new KD6_37MCSTNode(newBoard, move);

            Children.Add(child);

            _untriedMoves.Remove(move);

            return child;
        }

        public Winner Playout(Func<IList<FutureMove>, FutureMove> strategy)
        {
            Board boardCopy = _board.Copy();

            while (boardCopy.CheckWinner() == Winner.None)
            {
                FutureMove move = strategy(DiscernValidMoves(boardCopy));

                boardCopy.DoMove(move.shape, move.column);
            }

            return boardCopy.CheckWinner();
        }

        private IList<FutureMove> DiscernValidMoves(Board board)
        {
            List<FutureMove> validMoves = new List<FutureMove>();

            for (int i = 0; i < _board.cols; i++)
            {
                if (board.IsColumnFull(i)) continue;

                for (int j = 0; j < 2; j++)
                {
                    PShape shape = (PShape)j;

                    if (board.PieceCount(board.Turn, shape) == 0) continue;

                    validMoves.Add(new FutureMove(i, shape));
                }
            }

            return validMoves;
        }

        
    }
}
