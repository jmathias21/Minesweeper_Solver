using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;

namespace MinesweeperSolver
{
    public class MouseOperations
    {
        public static User32.Rect WindowRect { get; set; }

        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010
        }

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out MousePoint lpMousePoint);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        public static void SetCursorPosition(int X, int Y)
        {
            SetCursorPos(X, Y);
        }

        public static void SetCursorPosition(MousePoint point)
        {
            SetCursorPos(point.X, point.Y);
        }

        public static MousePoint GetCursorPosition()
        {
            MousePoint currentMousePoint;
            var gotPoint = GetCursorPos(out currentMousePoint);
            if (!gotPoint) { currentMousePoint = new MousePoint(0, 0); }
            return currentMousePoint;
        }

        public static void MouseEvent(MouseEventFlags value)
        {
            MousePoint position = GetCursorPosition();
            mouse_event((int)value, position.X, position.Y, 0, 0);
        }

        public static void PerformLeftClick(int posX, int posY)
        {
            FocusWindow();
            MousePoint mousePoint = GetCursorPosition();
            SetCursorPosition(posX, posY);
            mouse_event((int)MouseEventFlags.LeftDown, 0, 0, 0, 0);
            mouse_event((int)MouseEventFlags.LeftDown, 0, 0, 0, 0);
            mouse_event((int)MouseEventFlags.LeftUp, 0, 0, 0, 0);
            SetCursorPosition(mousePoint);
        }

        public static void PerformFlagClick(int posX, int posY)
        {
            FocusWindow();
            MousePoint mousePoint = GetCursorPosition();
            SetCursorPosition(posX, posY);
            mouse_event((int)MouseEventFlags.RightDown, 0, 0, 0, 0);
            mouse_event((int)MouseEventFlags.RightUp, 0, 0, 0, 0);
            SetCursorPosition(mousePoint);
        }

        public static void PerformQuestionClick(int posX, int posY)
        {
            FocusWindow();
            MousePoint mousePoint = GetCursorPosition();
            SetCursorPosition(posX, posY);
            mouse_event((int)MouseEventFlags.RightDown, 0, 0, 0, 0);
            mouse_event((int)MouseEventFlags.RightUp, 0, 0, 0, 0);
            mouse_event((int)MouseEventFlags.RightDown, 0, 0, 0, 0);
            mouse_event((int)MouseEventFlags.RightUp, 0, 0, 0, 0);
            SetCursorPosition(mousePoint);
        }

        private static void FocusWindow()
        {
            MousePoint mousePoint = GetCursorPosition();
            SetCursorPosition(WindowRect.left + 1, WindowRect.bottom - 1);
            mouse_event((int)MouseEventFlags.LeftDown, 0, 0, 0, 0);
            mouse_event((int)MouseEventFlags.LeftUp, 0, 0, 0, 0);
            SetCursorPosition(mousePoint);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MousePoint
        {
            public int X;
            public int Y;

            public MousePoint(int x, int y)
            {
                X = x;
                Y = y;
            }

        }
    }
}
