using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;

namespace MinesweeperSolver
{
    public class Solver
    {
        public const int GRID_OFFSET_X = 15;
        public const int GRID_OFFSET_Y = 100;

        private OperatingSystem OS { get; set; }

        public Solver(OperatingSystem os)
        {
            OS = os;
        }

        /// <summary>
        /// Prepares game board and executes calculation logic
        /// </summary>
        /// <param name="windowRect"></param>
        /// <param name="os"></param>
        public void BeginSolving(User32.Rect windowRect)
        {
            // Get height and width properties of process
            int windowWidth = windowRect.right - windowRect.left;
            int windowHeight = windowRect.bottom - windowRect.top;

            while (true)
            {
                {
                    Board gameBoard = CreateGameBoard(windowRect, windowWidth, windowHeight);
                    if (gameBoard == null)
                    {
                        continue;
                    }

                    bool gameRestarted = false;
                    if (!gameBoard.IsSmiling)
                    {
                        Thread.Sleep(5000);
                        // Click restart button
                        MouseOperations.PerformLeftClick(windowRect.left + (windowWidth / 2) + 2, windowRect.top + 77);
                        gameRestarted = true;
                    }

                    // If the game has been restarted, make initial click in the corner (statistically safest move)
                    if (gameRestarted)
                    {
                        MouseOperations.PerformLeftClick(
                            gameBoard.WindowRect.left + Solver.GRID_OFFSET_X + (gameBoard.Grid[gameBoard.Grid.GetLength(0) - 1, 0].PosX * Square.PIXEL_WIDTH) + 8,
                            gameBoard.WindowRect.top + Solver.GRID_OFFSET_Y + (gameBoard.Grid[gameBoard.Grid.GetLength(0) - 1, 0].PosY * Square.PIXEL_HEIGHT) + 8);
                        Thread.Sleep(100);
                    }

                    // Calculate optimal move and apply click
                    MoveCalculator moveCalculator = new MoveCalculator(gameBoard);
                    moveCalculator.CalculateMove();
                }
            }
        }

        private Board CreateGameBoard(User32.Rect windowRect, int windowWidth, int windowHeight)
        {
            Thread.Sleep(30);
            var bmp = new Bitmap(windowWidth, windowHeight, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bmp);
            graphics.CopyFromScreen(windowRect.left, windowRect.top, 0, 0, new Size(windowWidth, windowHeight), CopyPixelOperation.SourceCopy);

            int boardWidth = (bmp.Width - 10 - GRID_OFFSET_X) / Square.PIXEL_WIDTH;
            int boardHeight = (bmp.Height - 10 - GRID_OFFSET_Y) / Square.PIXEL_HEIGHT;

            // Get grid of colors representing the values of each square
            Color[,] colorGrid = GetColorGrid(bmp, boardWidth, boardHeight);

            bool isSmiling = true;
            if (OS.Platform == PlatformID.Win32NT && OS.Version.Major == 6)
            {
                isSmiling = bmp.GetPixel((windowWidth / 2) - 2, 75).Equals(Color.FromArgb(255, 0, 0, 0));
            }
            if (OS.Platform == PlatformID.Win32NT && OS.Version.Major == 6 && OS.Version.Minor == 2)
            {
                isSmiling = bmp.GetPixel((windowWidth / 2), 78).Equals(Color.FromArgb(255, 0, 0, 0));
            }
            if (OS.Platform == PlatformID.Win32NT && OS.Version.Major == 10)
            {
                isSmiling = bmp.GetPixel((windowWidth / 2), 78).Equals(Color.FromArgb(255, 0, 0, 0));
            }

            Board gameBoard = new Board(windowRect, isSmiling);
            gameBoard.Grid = new Square[boardHeight, boardWidth];

            try
            {
                gameBoard.CreateGrid(boardWidth, boardHeight, colorGrid);
            }
            catch (Exception)
            {
                return null;
            }
            return gameBoard;
        }

        /// <summary>
        /// Get a grid of color values from minesweeper game board
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="gridWidth"></param>
        /// <param name="gridHeight"></param>
        /// <returns></returns>
        private Color[,] GetColorGrid(Bitmap bmp, int gridWidth, int gridHeight)
        {
            Color[,] colorGrid = new Color[gridWidth, gridHeight];
            for (int i = 0; i < gridWidth; i++)
            {
                for (int v = 0; v < gridHeight; v++)
                {
                    //Get a set of colors from the current square
                    var colors = GetSquareColors(bmp, v, i);

                    //Loop through each color until we find one that is not gray and
                    //assign it to the color grid element
                    foreach (var color in colors)
                    {
                        if (!color.Equals(Color.FromArgb(255, 192, 192, 192)))
                        {
                            colorGrid[i, v] = color;

                            if (color.Equals(Color.FromArgb(255, 255, 0, 0)))
                            {
                                if (colors.Where(_ => _.R == 0).Count() > 0)
                                {
                                    colorGrid[i, v] = Color.FromArgb(255, 1, 1, 1);
                                }
                            }

                            break;
                        }
                        else
                        {
                            //Get color of top left pixel in square (differentiates between empty block
                            //and unclicked block)
                            colorGrid[i, v] = bmp.GetPixel(
                                GRID_OFFSET_X + (i * Square.PIXEL_WIDTH) + 1,
                                GRID_OFFSET_Y + (v * Square.PIXEL_HEIGHT) + 1);
                        }
                    }
                    //If color is never assigned, set color grid element to gray
                    if (colorGrid[i, v].IsEmpty)
                        colorGrid[i, v] = Color.FromArgb(255, 192, 192, 192);
                }
            }
            return colorGrid;
        }

        private IEnumerable<Color> GetSquareColors(Bitmap bmp, int row, int column)
        {
            Color pixelCenterTop = bmp.GetPixel(
                GRID_OFFSET_X + (column * Square.PIXEL_WIDTH) + (Square.PIXEL_WIDTH / 2),
                GRID_OFFSET_Y + (row * Square.PIXEL_HEIGHT) + ((Square.PIXEL_HEIGHT / 2) - 3));

            Color pixelTopLeft = bmp.GetPixel(
                GRID_OFFSET_X + (column * Square.PIXEL_WIDTH) + (Square.PIXEL_WIDTH / 4),
                GRID_OFFSET_Y + (row * Square.PIXEL_HEIGHT) + (Square.PIXEL_HEIGHT / 4));

            Color pixelBottomLeft = bmp.GetPixel(
                GRID_OFFSET_X + (column * Square.PIXEL_WIDTH) + (Square.PIXEL_WIDTH / 4),
                GRID_OFFSET_Y + (row * Square.PIXEL_HEIGHT) + (Square.PIXEL_HEIGHT * 3 / 4));

            Color pixelTopRight = bmp.GetPixel(
                GRID_OFFSET_X + (column * Square.PIXEL_WIDTH) + (Square.PIXEL_WIDTH * 3 / 4),
                GRID_OFFSET_Y + (row * Square.PIXEL_HEIGHT) + (Square.PIXEL_HEIGHT / 2));

            Color pixelBottomRight = bmp.GetPixel(
                GRID_OFFSET_X + (column * Square.PIXEL_WIDTH) + (Square.PIXEL_WIDTH * 3 / 4),
                GRID_OFFSET_Y + (row * Square.PIXEL_HEIGHT) + (Square.PIXEL_HEIGHT * 3 / 4));

            Color pixelCenterBottom = bmp.GetPixel(
                GRID_OFFSET_X + (column * Square.PIXEL_WIDTH) + (Square.PIXEL_WIDTH / 2),
                GRID_OFFSET_Y + (row * Square.PIXEL_HEIGHT) + (Square.PIXEL_HEIGHT / 2));

            var colors = new[] { pixelCenterTop, pixelTopLeft, pixelBottomLeft, pixelTopRight, pixelBottomRight, pixelCenterBottom };
            return colors;
        }
    }
}