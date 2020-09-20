using System.Collections.Generic;

namespace MinesweeperSolver
{
    public class Square
    {
        public const int PIXEL_WIDTH = 16;
        public const int PIXEL_HEIGHT = 16;

        public int? Number { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public SquareType Type { get; set; }
        public ThreatType Threat { get; set; }
        public List<Square> AdjacentUnclicked { get; set; } = new List<Square>();
        public List<Square> AdjacentNumbers { get; set; } = new List<Square>();
        public List<Square> AcrossUnclicked { get; set; } = new List<Square>();
        public List<Square> AcrossNumbers { get; set; } = new List<Square>();
        public int FlagTotal { get; set; }
        public bool IsPotentialFlag { get; set; }
        public bool IsGrouped { get; set; }

        public Square(SquareType type, ThreatType threat, int posX, int posY, int? number)
        {
            Type = type;
            Threat = threat;
            PosX = posX;
            PosY = posY;
            Number = number;
            FlagTotal = 0;
            IsPotentialFlag = false;
        }
    }
}