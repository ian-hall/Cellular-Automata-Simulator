using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHGameOfLife
{
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
}
