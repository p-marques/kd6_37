using ColorShapeLinks.Common;
using System;

namespace KD6_37
{
    public class ZobristHashing
    {
        private Random _random;
        private int _size;
        private int[] _zobristKey;

        public ZobristHashing(Board board)
        {
            _size = board.cols * board.rows;

            _random = new Random();

            _zobristKey = new int[_size * 4];

            for (int i = 0; i < _size * 4; i++)
            {
                _zobristKey[i] = _random.Next();
            }
        }

        public long Hash(Board board)
        {
            
            long result = 0;

            for (int i = 0; i < _size; i++)
            {
                Piece? piece = board.GetPieceAt(i);

                if (piece.HasValue)
                {
                    Piece a = piece.Value;

                    int pieceKey;

                    if (a.shape == PShape.Round)
                    {
                        if (a.color == PColor.White)
                        {
                            pieceKey = 0;
                        }
                        else
                        {
                            pieceKey = 1;
                        }
                    }
                    else
                    {
                        if (a.color == PColor.White)
                        {
                            pieceKey = 2;
                        }
                        else
                        {
                            pieceKey = 3;
                        }
                    }

                    result ^= _zobristKey[i * 4 + pieceKey];
                }
            }

            return result;
        }
    }
}
