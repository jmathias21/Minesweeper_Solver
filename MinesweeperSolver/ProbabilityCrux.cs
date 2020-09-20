using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver
{
    public class ProbabilityCrux
    {
        public Square Crux { get; set; }
        public decimal MineProbability { get; set; }
        public int TotalFlags { get; set; }
    }
}
