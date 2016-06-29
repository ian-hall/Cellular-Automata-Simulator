namespace GHGameOfLife
{
///////////////////////////////////////////////////////////////////////////////
    static class GenericHelp<T>
    {
//-----------------------------------------------------------------------------
        /// <summary>
        /// Rotates the values in a 2d jagged array 90 degrees clockwise
        /// </summary>
        /// <param name="oldVals">The 2d jagged array to be rotated</param>
        /// <returns>A new jagged array with the rotated values</returns>
        public static T[][] Rotate90(T[][] oldVals)
        {
            int rotatedCols = oldVals.Length;
            int rotatedRows = oldVals[0].Length;

            T[][] rotatedVals = new T[rotatedRows][];
            for (int r = 0; r < rotatedRows; r++)
            {
                rotatedVals[r] = new T[rotatedCols];
                for (int c = 0; c < rotatedCols; c++)
                {
                    rotatedVals[r][c] = oldVals[rotatedCols - 1 - c][r];
                }
            }
            return rotatedVals;
        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Mirrors the values in a 2d jagged array
        /// </summary>
        /// <param name="oldVals">The 2d jagged array to be mirrored</param>
        /// <returns>A new jagged array with the mirrored values</returns>
        public static T[][] Mirror(T[][] oldVals)
        {
            T[][] rotatedVals = new T[oldVals.Length][];
            for (int r = 0; r < oldVals.Length; r++)
            {
                rotatedVals[r] = new T[oldVals[r].Length];
                for (int c = oldVals[r].Length - 1; c >= 0; c--)
                {
                    rotatedVals[r][oldVals[r].Length - 1 - c] = oldVals[r][c];
                }
            }
            return rotatedVals;
        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Shifts all values of a 2d jagged array up one row
        /// </summary>
        /// <param name="oldVals">The 2d jagged array to be shifted</param>
        /// <returns>A new jagged array with the shifted values</returns>
        public static T[][] ShiftUp(T[][] oldVals)
        {
            var shiftedVals = new T[oldVals.Length][];
            for (int r = 1; r < oldVals.Length; r++)
            {
                shiftedVals[r - 1] = oldVals[r];
            }
            return shiftedVals;
        }
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
    } //End Class
///////////////////////////////////////////////////////////////////////////////
}
