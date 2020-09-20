using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace MinesweeperSolver
{
    public class MoveCalculatorSafe
    {
        private Board gameBoard;
        private Square[,] grid;

        public MoveCalculatorSafe(Board gameBoard)
        {
            this.gameBoard = gameBoard;
            this.grid = gameBoard.Grid;
        }

        public IEnumerable<Square> GetSafeSquares()
        {
            var safeSquares = new List<Square>();
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int v = 0; v < grid.GetLength(1); v++)
                {
                    Square currentSquare = grid[i, v];
                    if (currentSquare.Type == SquareType.Number)
                    {
                        // Get the total number of flags in squares adjacent to current square
                        int flagCount = GetFlagCount(currentSquare.AdjacentUnclicked);

                        // Get list of safe squares
                        if (currentSquare.Number <= flagCount && currentSquare.AdjacentUnclicked.Count > flagCount)
                        {
                            foreach (var square in currentSquare.AdjacentUnclicked)
                            {
                                if (square.Threat == ThreatType.Unknown)
                                {
                                    safeSquares.Add(square);
                                }
                            }
                        }
                    }
                }
            }
            return safeSquares;
        }

        public void PerformMoves(IEnumerable<Square> safeSquares)
        {
            foreach (var square in safeSquares)
            {
                int safestClickX = square.PosX;
                int safestClickY = square.PosY;
                MouseOperations.PerformLeftClick(
                    gameBoard.WindowRect.left + Solver.GRID_OFFSET_X + (safestClickX * Square.PIXEL_WIDTH) + 8,
                    gameBoard.WindowRect.top + Solver.GRID_OFFSET_Y + (safestClickY * Square.PIXEL_HEIGHT) + 8);
            }
        }

        private int GetFlagCount(IEnumerable<Square> adjacentSquares)
        {
            int flaggedCount = 0;
            foreach (var square in adjacentSquares)
            {
                if (square.Threat == ThreatType.Flag)
                {
                    flaggedCount += 1;
                }
            }
            return flaggedCount;
        }
    }
}
