using System;
using System.Collections.Generic;
using System.Linq;

namespace MinesweeperSolver
{
    public class MoveCalculatorDangerous
    {
        private Board gameBoard;
        private Square[,] grid;
        private Random rand;

        public MoveCalculatorDangerous(Board gameBoard)
        {
            this.gameBoard = gameBoard;
            this.grid = gameBoard.Grid;
            this.rand = new Random();
        }

        public void PerformMove()
        {
            var probabilityCruxes = new List<ProbabilityCrux>();

            // Find groups of linked cruxes (across, not adjacent). They are separated
            // into different groups because they are statistically independent, so we
            // want to perform the calculations separately to vastly speed up the program
            var cruxGroups = FindCruxGroups();

            foreach (var cruxGroup in cruxGroups)
            {
                var numbers = FindAllAdjacentNumbers(cruxGroup);

                // Calculate the safest crux to click in this group and add it to a list
                probabilityCruxes.Add(GetSafeProbabilityCrux(numbers, cruxGroup));
            }

            // Find the safest crux out of all of the groups
            var safestCrux = probabilityCruxes.OrderBy(_ => _.MineProbability).First();

            MouseOperations.PerformLeftClick(
                gameBoard.WindowRect.left + Solver.GRID_OFFSET_X + (safestCrux.Crux.PosX * Square.PIXEL_WIDTH) + 8,
                gameBoard.WindowRect.top + Solver.GRID_OFFSET_Y + (safestCrux.Crux.PosY * Square.PIXEL_HEIGHT) + 8);
            LogStatistics(safestCrux);
        }

        /// <summary>
        /// Generate a random sampling of flags for a group of crux squares and find the crux square with the
        /// least amount of flags
        /// </summary>
        /// <param name="numbers"></param>
        /// <param name="cruxGroup"></param>
        /// <returns></returns>
        private ProbabilityCrux GetSafeProbabilityCrux(IEnumerable<Square> numbers, IEnumerable<Square> cruxGroup)
        {
            int count = 0;
            int totalFlags = 0;
            while (count < 200)
            {
                count += 1;
                numbers = Shuffle(numbers);
                foreach (var number in numbers)
                {
                    number.AdjacentUnclicked = Shuffle(number.AdjacentUnclicked).ToList();
                }
                PlaceFlags(numbers);
                totalFlags += TallyPotentialFlags(cruxGroup);
            }

            var safeCrux = cruxGroup.OrderBy(_ => _.FlagTotal).First();
            decimal safeMineProbability = CalculateMineProbability(totalFlags, safeCrux.FlagTotal);

            return new ProbabilityCrux()
            {
                Crux = safeCrux,
                MineProbability = safeMineProbability,
                TotalFlags = totalFlags
            };
        }

        /// <summary>
        /// Calculate chances of a mine, given a crux square's total flags and the overall total flags placed
        /// </summary>
        /// <param name="totalFlags"></param>
        /// <param name="cruxTotalFlags"></param>
        /// <returns></returns>
        private decimal CalculateMineProbability(int totalFlags, int cruxTotalFlags)
        {
            decimal safeMineProbability;
            if (totalFlags > 0)
            {
                safeMineProbability = (decimal)cruxTotalFlags / totalFlags;
            }
            else
            {
                safeMineProbability = 0;
            }

            return safeMineProbability;
        }

        /// <summary>
        /// Tally up all flag totals in a group of crux squares and reset them
        /// </summary>
        /// <param name="cruxGroup"></param>
        /// <returns></returns>
        private int TallyPotentialFlags(IEnumerable<Square> cruxGroup)
        {
            int totalFlags = 0;
            foreach (var crux in cruxGroup)
            {
                if (crux.IsPotentialFlag)
                {
                    totalFlags += 1;
                    crux.FlagTotal += 1;
                    crux.IsPotentialFlag = false;
                    crux.IsGrouped = false;
                }
            }
            return totalFlags;
        }

        /// <summary>
        /// Find groups of crux squares
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IEnumerable<Square>> FindCruxGroups()
        {
            var cruxGroups = new List<IEnumerable<Square>>();
            while (true)
            {
                var crux = FindUngroupedCrux();
                if (crux != null)
                {
                    cruxGroups.Add(FindCruxGroup());
                }
                else
                {
                    return cruxGroups;
                }
            }
        }

        /// <summary>
        /// Find group of crux squares that are all across from each other
        /// </summary>
        private IEnumerable<Square> FindCruxGroup(List<Square> group = null)
        {
            if (group == null || group.Count == 0)
            {
                group = new List<Square>();
                var crux = FindUngroupedCrux();
                crux.IsGrouped = true;
                if (crux == null)
                {
                    return null;
                }
                group.Add(crux);
            }

            for (int i = 0; i < group.Count; i++)
            {
                if (group[i].Threat == ThreatType.Crux)
                {
                    foreach (var acrossUnclicked in group[i].AcrossUnclicked)
                    {
                        if (acrossUnclicked.Threat == ThreatType.Crux && !group.Contains(acrossUnclicked))
                        {
                            group.Add(acrossUnclicked);
                            acrossUnclicked.IsGrouped = true;
                            group = FindCruxGroup(group).ToList();
                        }
                    }
                }
            }
            return group;
        }

        /// <summary>
        /// Find the first crux square that is not in a group
        /// </summary>
        /// <returns></returns>
        private Square FindUngroupedCrux()
        {
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int v = 0; v < grid.GetLength(1); v++)
                {
                    var currentSquare = grid[i, v];
                    if (currentSquare.Threat == ThreatType.Crux && !currentSquare.IsGrouped)
                    {
                        return currentSquare;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find all number squares that are adjacent to a collection of squares
        /// </summary>
        /// <param name="unclickedSquares"></param>
        /// <returns></returns>
        private IEnumerable<Square> FindAllAdjacentNumbers(IEnumerable<Square> squares)
        {
            var adjacentNumbers = new List<Square>();
            foreach (var square in squares)
            {
                foreach (var number in square.AdjacentNumbers)
                {
                    if (!adjacentNumbers.Contains(number))
                    {
                        adjacentNumbers.Add(number);
                    }
                }
            }
            return adjacentNumbers;
        }

        /// <summary>
        /// Set a potential flag configuration on all unclicked squares that are adjacent
        /// to a collection of number squares
        /// </summary>
        /// <param name="numbers"></param>
        private void PlaceFlags(IEnumerable<Square> numbers)
        {
            if (numbers.Count() == 0)
            {
                return;
            }

            Square number = numbers.First();
            bool placedFlag = false;
            foreach (var unclicked in number.AdjacentUnclicked)
            {
                if (CanPlaceFlag(unclicked))
                {
                    placedFlag = true;
                    unclicked.IsPotentialFlag = true;
                    PlaceFlags(numbers.Skip(1));
                }
            }

            if (!placedFlag)
            {
                PlaceFlags(numbers.Skip(1));
            }
        }

        /// <summary>
        /// Determine if a flag can validly be placed on an unclicked square
        /// </summary>
        /// <param name="square"></param>
        /// <returns></returns>
        private bool CanPlaceFlag(Square square)
        {
            // Get the total number of adjacent numbers that have reached their adjacent flag limit. For example, if there was one
            // adjacent number 3 with three adjacent flags, this expression would return one
            var numbersWithMaxFlags = square.AdjacentNumbers.Count(n => n.AdjacentUnclicked.Count(u => u.Threat == ThreatType.Flag || u.IsPotentialFlag) >= n.Number);

            if (numbersWithMaxFlags == 0 &&
                square.Threat != ThreatType.Flag &&
                !square.IsPotentialFlag)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Randomize the order of a collection of elements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        private IEnumerable<T> Shuffle<T>(IEnumerable<T> source)
        {
            T[] elements = source.ToArray();
            for (int i = elements.Length - 1; i >= 0; i--)
            {
                int swapIndex = rand.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }

        private static void LogStatistics(ProbabilityCrux safestCrux)
        {
            Statistics.LastProbability = safestCrux.MineProbability;
            Statistics.LastSquare = safestCrux.Crux;
            Statistics.LastTotalFlags = safestCrux.TotalFlags;
        }
    }
}
