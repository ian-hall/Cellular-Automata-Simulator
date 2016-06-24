using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Threading;
using System.Collections.Concurrent;

namespace GHGameOfLife
{
    /// <summary>
    /// This class pretty much does everything. It sets up the console, 
    /// fills in the initial pop from a file or randomly, and then 
    /// does all the checking for living/dying of the population.
    /// </summary>
///////////////////////////////////////////////////////////////////////////////
    partial class GoL
    {

        private bool[,] Board;
        private int Generation;
        private const char LIVE_CELL = '☺';
        //private const char DEAD_CELL = ' ';
        private bool Wrap { get; set; }

        private bool __IsInitialized;
        private int __Rows;
        private int __Cols;
        private int __OrigConsHeight;
        private int __OrigConsWidth;
        public bool IsInitialized { get { return this.__IsInitialized; } }
        public int Rows { get { return this.__Rows; } }
        public int Cols { get { return this.__Cols; } }
        public int OrigConsHeight { get { return this.__OrigConsHeight; } }
        public int OrigConsWidth { get { return this.__OrigConsWidth; } }

        public enum BuildType { Random, File, Resource, User };
//------------------------------------------------------------------------------
        /// <summary>
        /// Constructor for the GoLBoard class. Size of the board will be based
        /// off the size of the console window...
        /// </summary>
        /// <param name="rowMax">Number of rows</param>
        /// <param name="colMax">Number of columns</param>
        public GoL(int rowMax, int colMax, BuildType bType, string res = null)
        {
            Board = new bool[rowMax, colMax];
                        
            this.__Rows = rowMax;
            this.__Cols = colMax;
            this.__OrigConsHeight = Console.WindowHeight;
            this.__OrigConsWidth = Console.WindowWidth;
            this.__IsInitialized = false;
            this.Generation = 1;
            Wrap = true;

            GoLHelper.CalcBuilderBounds(this);

            switch (bType)
            {
                case BuildType.Random:
                    BuildDefaultPop();
                    break;
                case BuildType.File:
                    BuildFromFile();
                    break;
                case BuildType.Resource:
                    BuildFromResource(res);
                    break;
                case BuildType.User:
                    BuildFromUser();
                    break;
            }
        }
//------------------------------------------------------------------------------        
        /// <summary>
        /// Default population is a random spattering of 0s and 1s
        /// Easy enough to get using (random int)%2
        /// </summary>
        private void BuildDefaultPop() 
        {
            Board = ConsoleRunHelper.BuildGOLBoardRandom(this);
            this.__IsInitialized = true;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Load the initial population from a file of 0s and 1s.
        /// This uses a Windows Forms OpenFileDialog to let the user select
        /// a file. The file is loaded into the center of the console window.
        /// </summary>
        private void BuildFromFile()
        {          
            Board = ConsoleRunHelper.BuildGOLBoardFile(this);          
            this.__IsInitialized = true;            
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Builds the board from a resource
        /// TODO: Don't really need to validate built in stuff, but probably 
        /// need to add the ability to resize the window if for some reason
        /// it is set smaller than a preloaded population can display in.
        /// </summary>
        /// <param name="res"></param>
        private void BuildFromResource(string res)
        {
            Board = ConsoleRunHelper.BuildGOLBoardResource(res, this);
            this.__IsInitialized = true;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Builds the board from user input. This is going to be ugly...
        /// </summary>
        private void BuildFromUser()
        {
            GoLHelper.BuildBoardUser(this);
            this.__IsInitialized = true;
        }
//------------------------------------------------------------------------------
        public void RunGame()
        {
            GoLHelper.ThreadedRunner(this);
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Adds the next board values to a queue to be read from
        /// </summary>
        private void ThreadedNext()
        {
            var lastBoard = this.Board;
            var nextBoard = new bool[Rows, Cols];
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    nextBoard[r, c] = ThreadedNextCellState(r, c, ref lastBoard);
                }
            }
            this.Generation++;
            this.Board = nextBoard;                  
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Prints the game based on the threaded set up.
        /// Waits until there are at least 2 boards in the board queue and then 
        /// prints the next board in the queue. 
        /// </summary>
        private void ThreadedPrint()
        {
            Console.SetCursorPosition(0, 1);
            Console.Write(" ".PadRight(Console.WindowWidth));
            string write = "Generation " + Generation;
            int left = (Console.WindowWidth / 2) - (write.Length / 2);
            Console.SetCursorPosition(left, 1);
            Console.Write(write);

            Console.BackgroundColor = MenuText.Default_BG;
            Console.ForegroundColor = MenuText.Board_FG;

            Console.SetCursorPosition(0, MenuText.Space);
            StringBuilder sb = new StringBuilder();
            for (int r = 0; r < this.__Rows; r++)
            {
                sb.Append("    ║");
                for (int c = 0; c < this.__Cols; c++)
                {
                    if (!Board[r, c])
                    {
                        sb.Append(" ");
                    }
                    else
                    {
                        sb.Append(LIVE_CELL);
                    }
                }
                sb.AppendLine("║");
            }
            Console.Write(sb);

            Console.BackgroundColor = MenuText.Default_BG;
            Console.ForegroundColor = MenuText.Default_FG;
        }
//------------------------------------------------------------------------------
        private bool ThreadedNextCellState(int r, int c, ref bool[,] board)
        {
            int n = 0;

            if (board[(r - 1 + this.__Rows) % this.__Rows, (c - 1 + this.__Cols) % this.__Cols]) n++;
            if (board[(r - 1 + this.__Rows) % this.__Rows, (c + 1 + this.__Cols) % this.__Cols]) n++;
            if (board[(r - 1 + this.__Rows) % this.__Rows, c]) n++;
            if (board[(r + 1 + this.__Rows) % this.__Rows, (c - 1 + this.__Cols) % this.__Cols]) n++;
            if (board[r, (c - 1 + this.__Cols) % this.__Cols]) n++;
            if (board[(r + 1 + this.__Rows) % this.__Rows, c]) n++;
            if (board[r, (c + 1 + this.__Cols) % this.__Cols]) n++;
            if (board[(r + 1 + this.__Rows) % this.__Rows, (c + 1 + this.__Cols) % this.__Cols]) n++;

            if(board[r,c])
            {
                return ((n == 2) || (n == 3));
            }
            else
            {
                return (n == 3);
            }
        }
//------------------------------------------------------------------------------
    } // end class
///////////////////////////////////////////////////////////////////////////////
}