using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;

namespace GHGameOfLife
{
    /// <summary>
    /// Static class that keeps getting the next generation of the board.
    /// Will either prompt the user to step through or loop 
    /// Probably should add this to the GolBoard class
    /// or combine it or something
    /// </summary>
///////////////////////////////////////////////////////////////////////////////
    partial class GoL
    {
//-----------------------------------------------------------------------------
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
        private class GoLHelper
        {
            //private static int[] Speeds = { 132, 100, 66, 50, 0 };
            //private static int Curr_Speed_Index = 2;
            ///// <summary>
            ///// Saves the current board to a file. 
            ///// </summary>
            ///// <param name="numRows">Total number of rows on the board</param>
            ///// <param name="numCols">Total number of cols on the board</param>
            ///// <param name="tempBoard">2d bool array representing the board</param>
            //private static void SaveBoard(int numRows, int numCols, bool[,] tempBoard)
            //{
            //    SaveFileDialog saveDia = new SaveFileDialog();
            //    saveDia.Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*";

            //    // We only save if the dialog box comes back true, otherwise
            //    // we just do nothing
            //    if (saveDia.ShowDialog() == DialogResult.OK)
            //    {
            //        Rect saveBox = new Rect();
            //        saveBox.Top = int.MaxValue;
            //        saveBox.Bottom = int.MinValue;
            //        saveBox.Left = int.MaxValue;
            //        saveBox.Right = int.MinValue;

            //        // make a box that only includes the minimum needed lines
            //        // to save the board
            //        // We only need to check live cells
            //        for (int r = 0; r < numRows; r++)
            //        {
            //            for (int c = 0; c < numCols; c++)
            //            {
            //                if (tempBoard[r, c])
            //                {
            //                    if (r < saveBox.Top)
            //                        saveBox.Top = r;
            //                    if (r > saveBox.Bottom)
            //                        saveBox.Bottom = r;
            //                    if (c < saveBox.Left)
            //                        saveBox.Left = c;
            //                    if (c > saveBox.Right)
            //                        saveBox.Right = c;
            //                }
            //            }
            //        }

            //        StringBuilder sb = new StringBuilder();
            //        for (int r = saveBox.Top; r <= saveBox.Bottom; r++)
            //        {
            //            for (int c = saveBox.Left; c <= saveBox.Right; c++)
            //            {
            //                if (tempBoard[r, c])
            //                    sb.Append('O');
            //                else
            //                    sb.Append('.');
            //            }
            //            if (r != saveBox.Bottom)
            //                sb.AppendLine();
            //        }
            //        File.WriteAllText(saveDia.FileName, sb.ToString());
            //    }

            //}
//-----------------------------------------------------------------------------
        }  // end class GoLHelper
//-----------------------------------------------------------------------------
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
    } // end class GoLBoard
///////////////////////////////////////////////////////////////////////////////
}