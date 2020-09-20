using System;
using System.Drawing;

namespace MinesweeperSolver
{
    /// <summary>
    /// Keeps track of the state of the board
    /// </summary>
    public class Board
    {
        public User32.Rect WindowRect { get; set; }
        public Square[,] Grid { get; set; }
        public bool IsSmiling { get; set; }

        public Board(User32.Rect windowrect, bool isSmiling)
        {
            WindowRect = windowrect;
            IsSmiling = isSmiling;
        }

        public void CreateGrid(int width, int height, Color[,] colorGrid)
        {
            Grid = new Square[width, height];

            CreateSquares(width, height, colorGrid);

            // Find adjacent blocks for each square in the grid and associate them
            for (int i = 0; i < Grid.GetLength(0); i++)
            {
                for (int v = 0; v < Grid.GetLength(1); v++)
                {
                    SetNumbers(i, v);
                    SetUnclicked(i, v);
                }
            }
        }

        public bool NumberExists(int currentY, int currentX)
        {
            if (currentX >= 0 && currentX < Grid.GetLength(1) &&
                currentY >= 0 && currentY < Grid.GetLength(0) &&
                Grid[currentY, currentX].Type == SquareType.Number)
            {
                return true;
            }
            else return false;
        }

        private void CreateSquares(int width, int height, Color[,] colorGrid)
        {
            for (int i = 0; i < width; i++)
            {
                for (int v = 0; v < height; v++)
                {
                    if (colorGrid[i, v].Equals(Color.FromArgb(255, 192, 192, 192)))
                        Grid[i, v] = new Square(SquareType.Empty, ThreatType.None, i, v, null);
                    else if (colorGrid[i, v].Equals(Color.FromArgb(255, 255, 255, 255)))
                        Grid[i, v] = new Square(SquareType.Unclicked, ThreatType.Unknown, i, v, null);
                    else if (colorGrid[i, v].Equals(Color.FromArgb(255, 1, 1, 1)))
                        Grid[i, v] = new Square(SquareType.Unclicked, ThreatType.Flag, i, v, null);
                    else if (colorGrid[i, v].Equals(Color.FromArgb(255, 0, 0, 0)))
                        Grid[i, v] = new Square(SquareType.Unclicked, ThreatType.Crux, i, v, null);
                    else if (colorGrid[i, v].Equals(Color.FromArgb(255, 0, 0, 255)))
                        Grid[i, v] = new Square(SquareType.Number, ThreatType.None, i, v, 1);
                    else if (colorGrid[i, v].Equals(Color.FromArgb(255, 0, 128, 0)))
                        Grid[i, v] = new Square(SquareType.Number, ThreatType.None, i, v, 2);
                    else if (colorGrid[i, v].Equals(Color.FromArgb(255, 255, 0, 0)))
                        Grid[i, v] = new Square(SquareType.Number, ThreatType.None, i, v, 3);
                    else if (colorGrid[i, v].Equals(Color.FromArgb(255, 0, 0, 128)))
                        Grid[i, v] = new Square(SquareType.Number, ThreatType.None, i, v, 4);
                    else if (colorGrid[i, v].Equals(Color.FromArgb(255, 128, 0, 0)))
                        Grid[i, v] = new Square(SquareType.Number, ThreatType.None, i, v, 5);
                    else if (colorGrid[i, v].Equals(Color.FromArgb(255, 0, 128, 128)))
                        Grid[i, v] = new Square(SquareType.Number, ThreatType.None, i, v, 6);
                    else if (colorGrid[i, v].Equals(Color.FromArgb(255, 132, 0, 132)))
                        Grid[i, v] = new Square(SquareType.Number, ThreatType.None, i, v, 7);
                    else if (colorGrid[i, v].Equals(Color.FromArgb(255, 128, 128, 128)))
                        Grid[i, v] = new Square(SquareType.Number, ThreatType.None, i, v, 8);
                    else
                        throw new Exception($"Unable to interpret color {colorGrid[i, v].ToString()}");
                }
            }
        }

        private void SetNumbers(int column, int row)
        {
            // Set top left
            if (column - 1 >= 0 && row - 1 >= 0 && Grid[column - 1, row - 1].Type == SquareType.Number)
            {
                Grid[column, row].AdjacentNumbers.Add(Grid[column - 1, row - 1]);
            }
            // Set top
            if (row - 1 >= 0 && Grid[column, row - 1].Type == SquareType.Number)
            {
                Grid[column, row].AdjacentNumbers.Add(Grid[column, row - 1]);
                Grid[column, row].AcrossNumbers.Add(Grid[column, row - 1]);
            }
            // Set top right
            if (column + 1 < Grid.GetLength(0) && row - 1 >= 0 && Grid[column + 1, row - 1].Type == SquareType.Number)
            {
                Grid[column, row].AdjacentNumbers.Add(Grid[column + 1, row - 1]);
            }
            // Set right
            if (column + 1 < Grid.GetLength(0) && Grid[column + 1, row].Type == SquareType.Number)
            {
                Grid[column, row].AdjacentNumbers.Add(Grid[column + 1, row]);
                Grid[column, row].AcrossNumbers.Add(Grid[column + 1, row]);
            }
            // Set bottom right
            if (column + 1 < Grid.GetLength(0) && row + 1 < Grid.GetLength(1) && Grid[column + 1, row + 1].Type == SquareType.Number)
            {
                Grid[column, row].AdjacentNumbers.Add(Grid[column + 1, row + 1]);
            }
            // Set bottom
            if (row + 1 < Grid.GetLength(1) && Grid[column, row + 1].Type == SquareType.Number)
            {
                Grid[column, row].AdjacentNumbers.Add(Grid[column, row + 1]);
                Grid[column, row].AcrossNumbers.Add(Grid[column, row + 1]);
            }
            // Set bottom left
            if (column - 1 >= 0 && row + 1 < Grid.GetLength(1) && Grid[column - 1, row + 1].Type == SquareType.Number)
            {
                Grid[column, row].AdjacentNumbers.Add(Grid[column - 1, row + 1]);
            }
            // Set left
            if (column - 1 >= 0 && Grid[column - 1, row].Type == SquareType.Number)
            {
                Grid[column, row].AdjacentNumbers.Add(Grid[column - 1, row]);
                Grid[column, row].AcrossNumbers.Add(Grid[column - 1, row]);
            }
        }

        private void SetUnclicked(int column, int row)
        {
            // Set top left
            if (column - 1 >= 0 && row - 1 >= 0 && Grid[column - 1, row - 1].Type == SquareType.Unclicked)
            {
                Grid[column, row].AdjacentUnclicked.Add(Grid[column - 1, row - 1]);
            }
            // Set top
            if (row - 1 >= 0 && Grid[column, row - 1].Type == SquareType.Unclicked)
            {
                Grid[column, row].AdjacentUnclicked.Add(Grid[column, row - 1]);
                Grid[column, row].AcrossUnclicked.Add(Grid[column, row - 1]);
            }
            // Set top right
            if (column + 1 < Grid.GetLength(0) && row - 1 >= 0 && Grid[column + 1, row - 1].Type == SquareType.Unclicked)
            {
                Grid[column, row].AdjacentUnclicked.Add(Grid[column + 1, row - 1]);
            }
            // Set right
            if (column + 1 < Grid.GetLength(0) && Grid[column + 1, row].Type == SquareType.Unclicked)
            {
                Grid[column, row].AdjacentUnclicked.Add(Grid[column + 1, row]);
                Grid[column, row].AcrossUnclicked.Add(Grid[column + 1, row]);
            }
            // Set bottom right
            if (column + 1 < Grid.GetLength(0) && row + 1 < Grid.GetLength(1) && Grid[column + 1, row + 1].Type == SquareType.Unclicked)
            {
                Grid[column, row].AdjacentUnclicked.Add(Grid[column + 1, row + 1]);
            }
            // Set bottom
            if (row + 1 < Grid.GetLength(1) && Grid[column, row + 1].Type == SquareType.Unclicked)
            {
                Grid[column, row].AdjacentUnclicked.Add(Grid[column, row + 1]);
                Grid[column, row].AcrossUnclicked.Add(Grid[column, row + 1]);
            }
            // Set bottom left
            if (column - 1 >= 0 && row + 1 < Grid.GetLength(1) && Grid[column - 1, row + 1].Type == SquareType.Unclicked)
            {
                Grid[column, row].AdjacentUnclicked.Add(Grid[column - 1, row + 1]);
            }
            // Set left
            if (column - 1 >= 0 && Grid[column - 1, row].Type == SquareType.Unclicked)
            {
                Grid[column, row].AdjacentUnclicked.Add(Grid[column - 1, row]);
                Grid[column, row].AcrossUnclicked.Add(Grid[column - 1, row]);
            }
        }
    }
}