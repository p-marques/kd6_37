using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI;
using ColorShapeLinks.Common.Session;
using System;
using System.Diagnostics;
using System.Threading;

namespace TestKD6_37
{
    internal class Game
    {
        private const int MAX_ALLOWED_TIME_IN_MS = 10000;

        private MatchConfig _matchConfig;
        private Board _board;
        private Player[] _players;
        private int _currentPlayerIndex;

        private Player CurrentPlayer => _players[_currentPlayerIndex];

        public Game(string whitesName, string redsName)
        {
            _matchConfig = new MatchConfig(timeLimitMillis: 200);

            _board = new Board();

            ThinkerPrototype tp = new ThinkerPrototype(whitesName, "", _matchConfig);

            _players = new Player[2];

            _players[0] = new Player(tp.Create(), whitesName);

            tp = new ThinkerPrototype(redsName, "", _matchConfig);

            _players[1] = new Player(tp.Create(), redsName);

            // whites play first
            _currentPlayerIndex = 0;
        }

        public void Run(int gameCount = 1)
        {
            if (gameCount == 1)
                RunSingleGame();
            else
                RunSimulation(gameCount);
        }

        private void RunSingleGame()
        {
            Console.WriteLine("Starting a new game...");

            ShowBoard();

            CancellationToken ct = new CancellationToken();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            while (watch.ElapsedMilliseconds < MAX_ALLOWED_TIME_IN_MS)
            {
                PerformPlayerMove(ct, true);

                ShowBoard();

                SwitchPlayer();

                if (_board.CheckWinner() == Winner.None)
                    continue;

                Winner winner = _board.CheckWinner();

                Console.Write("\nGame Over! Result: ");

                Console.Write($"{winner}\n");

                break;
            }

            Console.WriteLine($"\nTest over in {watch.ElapsedMilliseconds}ms");
        }

        private void RunSimulation(int gameCount)
        {
            (int white, int red, int draw) = (0, 0, 0);

            Console.WriteLine("Starting games run...");

            for (int i = 0; i < gameCount; i++)
            {
                Winner winner = Simulate();

                switch (winner)
                {
                    case Winner.Draw:
                        draw++;
                        break;
                    case Winner.White:
                        white++;
                        break;
                    case Winner.Red:
                        red++;
                        break;
                    default:
                        throw new ArgumentException(
                            "Unnexpected game over state.");
                }
            }

            Console.WriteLine("Results:");

            Console.WriteLine($"\tWhite: {white}");
            Console.WriteLine($"\tRed: {red}");
            Console.WriteLine($"\tDraws: {draw}");
        }

        private Winner Simulate()
        {
            Winner result = Winner.None;
            
            _board = new Board();

            _currentPlayerIndex = 0;

            CancellationToken ct = new CancellationToken();

            while (result == Winner.None)
            {
                PerformPlayerMove(ct);

                SwitchPlayer();

                result = _board.CheckWinner();
            }

            return result;
        }

        private void PerformPlayerMove(CancellationToken ct, bool print = false)
        {
            FutureMove move = CurrentPlayer.Thinker.Think(_board, ct);

            if (print)
                Console.WriteLine($"-> {CurrentPlayer.Name} plays: {move}");

            _board.DoMove(move.shape, move.column);
        }

        private void SwitchPlayer() => 
            _currentPlayerIndex = _currentPlayerIndex == 0 ? 1 : 0;

        // Helper method to show a board
        private void ShowBoard()
        {
            for (int r = _board.rows - 1; r >= 0; r--)
            {
                for (int c = 0; c < _board.cols; c++)
                {
                    char pc = '.';
                    Piece? p = _board[r, c];
                    if (p.HasValue)
                    {
                        if (p.Value.Is(PColor.White, PShape.Round))
                        {
                            pc = 'w';
                        }
                        else if (p.Value.Is(PColor.White, PShape.Square))
                        {
                            pc = 'W';
                        }
                        else if (p.Value.Is(PColor.Red, PShape.Round))
                        {
                            pc = 'r';
                        }
                        else if (p.Value.Is(PColor.Red, PShape.Square))
                        {
                            pc = 'R';
                        }
                        else
                        {
                            throw new ArgumentException(
                                $"Invalid piece '{p.Value}'");
                        }
                    }
                    Console.Write(pc);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
