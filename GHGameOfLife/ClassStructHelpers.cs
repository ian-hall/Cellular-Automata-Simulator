using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GHGameOfLife
{
///////////////////////////////////////////////////////////////////////////////
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
///////////////////////////////////////////////////////////////////////////////
    [StructLayout(LayoutKind.Sequential)]
    public struct ScreenRes
    {
        public int Height, Width;

        public ScreenRes(int w, int h)
        {
            this.Height = h;
            this.Width = w;
        }
    } //end struct
///////////////////////////////////////////////////////////////////////////////
    class ConsSize
    {
        private int _rows;
        private int _cols;
        public double Ratio { get; private set; }

        public ConsSize(int c, int r)
        {
            _cols = c;
            _rows = r;
            calcRatio();
        }

        public int Cols
        {
            get
            {
                return this._cols;
            }
            set
            {
                _cols = value;
                calcRatio();
            }
        }

        public int Rows
        {
            get
            {
                return this._rows;
            }
            set
            {
                _rows = value;
                calcRatio();
            }
        }

        public override string ToString()
        {
            return string.Format("W: {0,-10} H: {1,-10} R: {2,-10}", _cols, _rows, Ratio);
        }

        private void calcRatio()
        {
            if (_cols < 1 || _rows < 1)
                Ratio = 1.0;
            else
                Ratio = 1.0 * _cols / _rows;
        }

    } // End CLass
///////////////////////////////////////////////////////////////////////////////
}
