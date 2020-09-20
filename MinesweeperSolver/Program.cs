using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MinesweeperSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get process information for minesweeper game
            var proc = Process.GetProcessesByName("Winmine__XP")[0];
            var rect = new User32.Rect();
            User32.GetWindowRect(proc.MainWindowHandle, ref rect);
            MouseOperations.WindowRect = rect;
            var os = System.Environment.OSVersion;

            // Run minesweeper solver logic
            Solver solver = new Solver(os);
            Task.Run(() => solver.BeginSolving(rect));

            Console.ReadLine();
        }
    }
}