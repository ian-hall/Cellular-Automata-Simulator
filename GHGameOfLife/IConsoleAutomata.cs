using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHGameOfLife
{
    /// <summary>
    /// Interface for handling ConsoleAutomata whatevers.
    /// Rows,Cols -> number of valid rows/cols for printing/calculating next generations
    /// ConsoleHeight,ConsoleWidth -> size of the console window
    /// Board -> bool[,] representation of rows and cols currently displaying
    /// </summary>
///////////////////////////////////////////////////////////////////////////////
    interface IConsoleAutomata
    {
        int Rows { get; }
        int Cols { get; }
        int Console_Height { get; }
        int Console_Width { get; }
        bool Is_Initialized { get; }
        bool Is_Wrapping { get; set; }
        bool[,] Board { get; }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Generates the next generation for the board
        /// </summary>
        void NextGeneration();
//-----------------------------------------------------------------------------
        /// <summary>
        /// Prints the board to the console window
        /// </summary>
        void PrintBoard();
//-----------------------------------------------------------------------------
    }
///////////////////////////////////////////////////////////////////////////////
}
