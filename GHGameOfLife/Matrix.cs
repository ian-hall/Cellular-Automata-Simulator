using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHGameOfLife
{
    class Matrix<T>
    {
        public int Rows { get; private set; }
        public int Cols { get; private set; }
        private T[][] Vals;

//-----------------------------------------------------------------------------
        public Matrix(int nRows, int nCols)
        {
            this.Rows = nRows;
            this.Cols = nCols;

            this.Vals = new T[Rows][];
            for (int r = 0; r < Rows; r++)
                Vals[r] = new T[Cols];
        }
//-----------------------------------------------------------------------------
        public Matrix(int nRows, int nCols, T[,] vals)
        {
            this.Rows = nRows;
            this.Cols = nCols;

            this.Vals = new T[Rows][];
            for (int r = 0; r < Rows; r++)
            {
                this.Vals[r] = new T[Cols];
                for (int c = 0; c < Cols; c++)
                {
                    this.Vals[r][c] = vals[r, c];
                }
            }
        }
//-----------------------------------------------------------------------------
        public Matrix(T[][] vals)
        {
            this.Rows = vals.Length;
            this.Cols = vals[0].Length;

            this.Vals = new T[Rows][];
            for (int r = 0; r < Rows; r++)
            {
                this.Vals[r] = new T[Cols];
                vals[r].CopyTo(this.Vals[r], 0);
            }

        }
//-----------------------------------------------------------------------------
        public void set(int r, int c, T val)
        {
            this.Vals[r][c] = val;
        }
//-----------------------------------------------------------------------------
        public T get(int r, int c)
        {
            return this.Vals[r][c];
        }
//-----------------------------------------------------------------------------
        //Equivilent to a mirror + 90deg clockwise turn
        public Matrix<T> Transpose()
        {
            T[][] transVals = new T[Cols][];
            //for (int i = 0; i < Cols; i++)
            //    transVals[i] = new T[Rows];

            for (int c = 0; c < Cols; c++)
            {
                transVals[c] = new T[Rows];
                for (int r = 0; r < Rows; r++)
                {
                    transVals[c][r] = this.Vals[r][c];
                }
            }

            return new Matrix<T>(transVals);
        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Doing 90 degree rotations of the matrix..
        /// 0: The original matrix (this)
        /// 1: 90 degrees counterclockwise
        /// 2: 180 degrees
        /// 3: 270 degrees
        /// </summary>
        /// <returns>Array of the rotations, including the original</returns>
        public Matrix<T>[] Rotations()
        {
            Matrix<T>[] rots = new Matrix<T>[4];

            rots[0] = this;


            T[][] rotated = Rotate90(this.Vals);
            //Rotate90(this.Vals, ref rotated90);
            rots[1] = new Matrix<T>(rotated);

            rotated = Rotate90(rotated);
            //Rotate90(rotated90, ref rotated180);
            rots[2] = new Matrix<T>(rotated);

            rotated = Rotate90(rotated);
            //Rotate90(rotated180, ref rotated270);
            rots[3] = new Matrix<T>(rotated);


            return rots;
        }
//-----------------------------------------------------------------------------
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    sb.Append(String.Format("{0,3}", Vals[r][c]));
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
//-----------------------------------------------------------------------------
        private T[][] Rotate90(T[][] oldVals/*, ref T[][] rotatedVals*/)
        {
            int rotatedCols = oldVals.Length;
            int rotatedRows = oldVals[0].Length;

            T[][] rotatedVals = new T[rotatedCols][];
            for (int c = rotatedCols - 1; c >= 0; c--)
            {
                rotatedVals[c] = new T[Rows];
                for (int r = 0; r < Rows; r++)
                {
                    rotatedVals[c][r] = oldVals[r][rotatedCols - 1 - c];
                }
            }

            return rotatedVals;
        }
//-----------------------------------------------------------------------------
        /*
        private void Rotate180(T[][] oldVals, ref T[][] rotatedVals)
        {
            int rotateRows = oldVals.Length;
            int rotateCols = oldVals[0].Length;

            rotatedVals = new T[rotateRows][];
            for (int r = rotateRows - 1; r >= 0; r--)
            {
                rotatedVals[r] = new T[rotateCols];
                for (int c = rotateCols - 1; c >= 0; c--)
                {
                    rotatedVals[r][c] = oldVals[(rotateRows - 1 - r)][(rotateCols - 1 - c)];
                }
            }
        }*/
    } //End Class
}
