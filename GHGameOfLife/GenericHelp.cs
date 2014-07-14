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
    } //End Class
///////////////////////////////////////////////////////////////////////////////
}
