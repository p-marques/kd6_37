using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI;
using ColorShapeLinks.Common.Session;
using KD6_37;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace TestKD6_37
{
    internal class Game
    {
        private MatchConfig _matchConfig;
        private Board _board;
        private Player[] _players;
        private int _currentPlayerIndex;

        private Player CurrentPlayer => _players[_currentPlayerIndex];

        public Game(string whitesName, string redsName)
        {
            _matchConfig = new MatchConfig(timeLimitMillis: 200);

            _board = new Board();

            ThinkerPrototype tp = new ThinkerPrototype(whitesName, "0.75", _matchConfig);

            _players = new Player[2];

            _players[0] = new Player(tp.Create(), whitesName);

            tp = new ThinkerPrototype(redsName, "0.75", _matchConfig);

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
            int totalSimulations = 0;
            Console.WriteLine("Starting a new game...");

            ShowBoard();

            CancellationToken ct = new CancellationToken();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            while (true)
            {
                PerformPlayerMove(ct, out int simulations, out int nodeReuses, true);

                totalSimulations += simulations;

                ShowBoard();

                SwitchPlayer();

                if (_board.CheckWinner() == Winner.None)
                    continue;

                Winner winner = _board.CheckWinner();

                Console.Write("\nGame Over! Result: ");

                Console.Write($"{winner}\n");

                break;
            }

            Console.WriteLine($"Simulations: {totalSimulations}");

            bool check = GetAreCachedNodesValid((_players[1].Thinker as KD6_37MCTSThinker).CachedNodes);

            Console.WriteLine($"Valid? {check}");
        }

        private void RunSimulation(int gameCount)
        {
            int count = 0;
            (int white, int red, int draw) = (0, 0, 0);

            Console.WriteLine("Starting games run...");

            for (int i = 0; i < gameCount; i++)
            {
                Winner winner = Simulate();

                count++;

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

                Console.WriteLine($"Record: {red}/{count}");
            }

            Console.WriteLine("Results:");

            Console.WriteLine($"\tWhite: {white}");
            Console.WriteLine($"\tRed: {red}");
            Console.WriteLine($"\tDraws: {draw}");
        }

        private Winner Simulate()
        {
            int totalSimulationsWhite = 0;
            int totalSimulationsRed = 0;
            int totalNodeReuses = 0;

            Winner result = Winner.None;
            
            _board = new Board();

            // Need to reset cached nodes, otherwise it would become better as simulations went along
            (_players[1].Thinker as KD6_37MCTSThinker).ResetCachedNodes();

            _currentPlayerIndex = 0;

            CancellationToken ct = new CancellationToken();

            while (result == Winner.None)
            {
                PerformPlayerMove(ct, out int simulations, out int nodeReuses);

                if (_currentPlayerIndex == 0)
                {
                    totalSimulationsWhite += simulations;
                }
                else
                {
                    totalSimulationsRed += simulations;
                }

                totalNodeReuses += nodeReuses;

                SwitchPlayer();

                result = _board.CheckWinner();
            }

            Console.WriteLine($"-> Simulations: {totalSimulationsWhite} vs {totalSimulationsRed}; Node reuses: {totalNodeReuses}; Winner: {result}");

            return result;
        }

        private void PerformPlayerMove(CancellationToken ct, out int simulations, out int nodeReuses, bool print = false)
        {
            simulations = 0;
            nodeReuses = 0;
            Stopwatch watch = new Stopwatch();
            watch.Start();

            FutureMove move = CurrentPlayer.Thinker.Think(_board, ct);

            watch.Stop();

            KD6_37MCTSThinker kD6_37 = null;
            if (CurrentPlayer.Thinker is KD6_37MCTSThinker)
            {
                kD6_37 = CurrentPlayer.Thinker as KD6_37MCTSThinker;

                nodeReuses = kD6_37.NodeReuses;

                simulations = kD6_37.LastRunSimulations;
            }

            StandardMCTS sMCTS = null;
            if (CurrentPlayer.Thinker is StandardMCTS)
            {
                sMCTS = CurrentPlayer.Thinker as StandardMCTS;

                simulations = sMCTS.LastRunSimulations;
            }

            if (print)
            {
                Console.WriteLine($"-> {CurrentPlayer.Name} plays: {move}. Took {watch.ElapsedMilliseconds}ms.");

                if (kD6_37 != null)
                    Console.WriteLine($"-> Simulations: {kD6_37.LastRunSimulations}; k = {kD6_37.K}; Cached nodes: {kD6_37.ChachedNodesCount}; Reuses: {kD6_37.NodeReuses}");
            }

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

        private bool GetAreCachedNodesValid(Dictionary<long, KD6_37MCSTNode> nodes)
        {
            foreach (KeyValuePair<long, KD6_37MCSTNode> nodeA in nodes)
            {
                foreach (KeyValuePair<long, KD6_37MCSTNode> nodeB in nodes)
                {
                    if (nodeA.Key == nodeB.Key)
                    {
                        if (!nodeA.Value.Board.IsEqual(nodeB.Value.Board))
                        {
                            Console.WriteLine("Found different boards with same hash!!!");

                            return false;
                        }
                    }
                    else
                    {
                        if (nodeA.Value.Board.IsEqual(nodeB.Value.Board))
                        {
                            Console.WriteLine("Found 2 equal boards with different hash!!!");

                            Console.WriteLine();

                            _board = nodeA.Value.Board;

                            ShowBoard();

                            _board = nodeB.Value.Board;

                            ShowBoard();

                            Console.WriteLine();

                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
