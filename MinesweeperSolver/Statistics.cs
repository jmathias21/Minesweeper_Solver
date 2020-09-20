using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver
{
    public static class Statistics
    {
        public static decimal LastProbability { get; set; }
        public static Square LastSquare { get; set; }
        public static int LastTotalFlags { get; set; }
    }
}
