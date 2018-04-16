using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Core_Automata
{

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect : IComparable<Rect>
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public override string ToString()
        {
            return String.Format("T:{0,-5} B:{1,-5} L:{2,-5} R:{3,-5}", Top, Bottom, Left, Right);
        }

        public int Width
        {
            get
            {
                return this.Right - this.Left;
            }
            private set { }
        }

        public int Height
        {
            get
            {
                return this.Bottom - this.Top;
            }
            private set { }

        }

        public int CompareTo(Rect rhs)
        {
            if ((this.Width < rhs.Width) && (this.Height <= rhs.Height) ||
                (this.Width <= rhs.Width) && (this.Height < rhs.Height))
            {
                return -1;
            }
            else if ((this.Width == rhs.Width) && (this.Height == rhs.Height))
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        public bool IsZero()
        {
            return (Left == Right) && (Top == Bottom) && (Left == 0);
        }
    } // end struct

    class BoardSize
    {
        public int Rows;
        public int Cols;

        public BoardSize(int c, int r)
        {
            Rows = r;
            Cols = c;
        }

        public double Ratio
        {
            get
            {
                return 1.0 * Cols / Rows;
            }
        }

        public override string ToString()
        {
            return string.Format("W: {0,-10} H: {1,-10} R: {2,-10}", Cols, Rows, Ratio);
        }
    } // End Class

}
