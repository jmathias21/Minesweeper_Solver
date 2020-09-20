using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MinesweeperSolver
{
    public class MoveCalculator
    {
        private Board gameBoard;
        private Square[,] grid;

        public MoveCalculator(Board gameBoard)
        {
            this.gameBoard = gameBoard;
            this.grid = gameBoard.Grid;
        }

        public void CalculateMove()
        {
            // Virtually flag all squares that we know must have a mine in them because
            // the adjacent number is equal to its adjacent square count
            SetFlags(gameBoard.Grid);

            var moveCalculatorSafe = new MoveCalculatorSafe(gameBoard);
            var safeSquares = moveCalculatorSafe.GetSafeSquares();

            if (safeSquares.Count() > 0)
            {
                moveCalculatorSafe.PerformMoves(safeSquares);
            }
            else
            {
                // A crux is equivalent to a question mark in a normal game of minesweeper.
                // It indicates that we don't know for sure if that square contains a mine.
                // We go through and mark every crux so that future calculations know which
                // squares we are unsure about
                SetCruxes(gameBoard.Grid);

                // This move calculator attempts to find and click the safest crux square
                var moveCalculatorDangerous = new MoveCalculatorDangerous(gameBoard);
                moveCalculatorDangerous.PerformMove();
            }
        }

        /// <summary>
        /// Mark all flags on the game grid
        /// </summary>
        /// <param name="grid"></param>
        private void SetFlags(Square[,] grid)
        {
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int v = 0; v < grid.GetLength(1); v++)
                {
                    if (grid[i, v].Type == SquareType.Number)
                    {
                        if (grid[i, v].AdjacentUnclicked.Count <= grid[i, v].Number)
                        {
                            foreach (var square in grid[i, v].AdjacentUnclicked)
                            {
                                if (square.Threat != ThreatType.Flag)
                                {
                                    square.Threat = ThreatType.Flag;
                                    MouseOperations.PerformFlagClick(
                                        gameBoard.WindowRect.left + Solver.GRID_OFFSET_X + (square.PosX * Square.PIXEL_WIDTH) + 8,
                                        gameBoard.WindowRect.top + Solver.GRID_OFFSET_Y + (square.PosY * Square.PIXEL_HEIGHT) + 8);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Mark all crux squares on the game grid
        /// </summary>
        /// <param name="grid"></param>
        private void SetCruxes(Square[,] grid)
        {
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int v = 0; v < grid.GetLength(1); v++)
                {
                    var currentSquare = grid[i, v];
                    if (currentSquare.Type == SquareType.Number)
                    {
                        var adjacentUnknown = currentSquare.AdjacentUnclicked.Where(_ => _.Threat != ThreatType.Flag);
                        var adjacentFlags = currentSquare.AdjacentUnclicked.Where(_ => _.Threat == ThreatType.Flag);
                        if (adjacentUnknown.Count() > currentSquare.Number - adjacentFlags.Count())
                        {
                            foreach (var square in adjacentUnknown)
                            {
                                if (square.Threat != ThreatType.Crux)
                                {
                                    square.Threat = ThreatType.Crux;
                                    MouseOperations.PerformQuestionClick(
                                        gameBoard.WindowRect.left + Solver.GRID_OFFSET_X + (square.PosX * Square.PIXEL_WIDTH) + 8,
                                        gameBoard.WindowRect.top + Solver.GRID_OFFSET_Y + (square.PosY * Square.PIXEL_HEIGHT) + 8);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
