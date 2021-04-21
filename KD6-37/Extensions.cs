using ColorShapeLinks.Common;
using System;

namespace KD6_37
{
    public static class Extensions
    {
        /// <summary>
        /// Look up piece from 1 dimensional array index.
        /// </summary>
        /// <param name="board">Board to look</param>
        /// <param name="index">1D index of piece</param>
        /// <returns></returns>
        public static Piece? GetPieceAt(this Board board, int index)
        {
            int row = index % board.rows;
            int col = index / board.rows;

            return board[row, col];

            throw new ArgumentException("Bad index!");
        }

        public static bool IsEqual(this Board board, Board otherBord)
        {
            for (int i = 0; i < board.rows; i++)
            {
                for (int j = 0; j < board.cols; j++)
                {
                    Piece? pieceA = board[i, j];

                    Piece? pieceB = otherBord[i, j];

                    if (!pieceA.HasValue && pieceB.HasValue
                        || pieceA.HasValue && !pieceB.HasValue)
                    {
                        return false;
                    }

                    if (pieceA.HasValue && pieceB.HasValue)
                    {
                        if (pieceA.Value.color != pieceB.Value.color
                            || pieceA.Value.shape != pieceB.Value.shape)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
