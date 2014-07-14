﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Drawing;

namespace GHGameOfLife
{
    /// <summary>
    /// This class pretty much does everything. It sets up the console, 
    /// fills in the initial pop from a file or randomly, and then 
    /// does all the checking for living/dying of the population.
    /// TODO: Change around constructors
    /// </summary>
///////////////////////////////////////////////////////////////////////////////
    partial class GoL
    {
        /*
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }*/

        private static bool[,] Board;
        private int Generation;
        private const char Live_Cell = '☺';
        //private const char _DeadCell = ' ';
        private Random Rand = new Random();
        private bool Wrap { get; set; }
        

        /*
        private int Used_Rows;
        private int Used_Cols;
        private int OrigConsHeight;
        private int OrigConsWidth;
         */

        private static bool IsInitialized;
        private static int Rows;
        private static int Cols;
        private static int OrigConsHeight;
        private static int OrigConsWidth;

        private static int Space = MenuText.Space;
//------------------------------------------------------------------------------
        /// <summary>
        /// Constructor for the GoLBoard class. Size of the board will be based
        /// off the size of the console window...
        /// </summary>
        /// <param name="rowMax">Number of rows</param>
        /// <param name="colMax">Number of columns</param>
        public GoL(int rowMax, int colMax)
        {
            Board = new bool[rowMax, colMax];
                        
            Rows = rowMax;
            Cols = colMax;
            OrigConsHeight = Console.WindowHeight;
            OrigConsWidth = Console.WindowWidth;
            IsInitialized = false;
            this.Generation = 1;
            Wrap = true;

            GoLHelper.CalcBuilderBounds();
        }
//------------------------------------------------------------------------------        
        /// <summary>
        /// Default population is a random spattering of 0s and 1s
        /// Easy enough to get using (random int)%2
        /// </summary>
        public void BuildDefaultPop() 
        {
            GoLHelper.BuildBoardRandom();
            IsInitialized = true;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Load the initial population from a file of 0s and 1s.
        /// This uses a Windows Forms OpenFileDialog to let the user select
        /// a file. The file is loaded into the center of the console window.
        /// </summary>
        public void BuildFromFile()
        {          
            GoLHelper.BuildBoardFile();
            IsInitialized = true;
            
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Builds the board from user input. This is going to be ugly...
        /// </summary>
        public void BuildFromUser()
        {
            GoLHelper.BuildBoardUser();
            IsInitialized = true;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Builds the board from a resource
        /// TODO: Don't really need to validate built in stuff, but probably 
        /// need to add the ability to resize the window if for some reason
        /// it is set smaller than a preloaded population can display in.
        /// </summary>
        /// <param name="pop"></param>
        public void BuildFromResource(string pop)
        {
            GoLHelper.BuildBoardResource(pop);
            IsInitialized = true;
        }
//------------------------------------------------------------------------------
        public void RunGame()
        {
            Print();
            GoLHelper.RunIt(this);
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Updates the board for the next generation of peoples
        /// </summary>
        /// Need to enable wrapping here
        private void Next()
        {
            bool[,] nextBoard = new bool[Rows, Cols];

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (!Board[r, c])
                    {
                        if (Wrap)
                        {
                            nextBoard[r, c] = WillBeBornWrap(r, c);
                        }
                        else
                        {
                            nextBoard[r, c] = WillBeBornNoWrap(r, c);
                        }
                        
                    }

                    if (Board[r, c])
                    {
                        if (Wrap)
                        {
                            nextBoard[r, c] = !WillDieWrap(r, c);
                        }
                        else
                        {
                            nextBoard[r, c] = !WillDieNoWrap(r, c);
                        }
                        
                    }
                }
            }
            Generation++;          
            Board = nextBoard;

        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Slams down the board and the sides of the border using a
        /// StringBuilder
        /// </summary>
        private void Print()
        {
            Console.SetCursorPosition(0, 1);
            Console.Write(" ".PadRight(Console.WindowWidth));
            string write = "Generation " + Generation;
            int left = (Console.WindowWidth / 2) - (write.Length / 2);
            Console.SetCursorPosition(left, 1);
            Console.Write(write);

            Console.BackgroundColor = MenuText.Default_BG;
            Console.ForegroundColor = MenuText.Board_FG;

            Console.SetCursorPosition(0, Space);
            StringBuilder sb = new StringBuilder();
            for (int r = 0; r < Rows; r++)
            {
                sb.Append("    ║");
                for (int c = 0; c < Cols; c++)
                {
                    if (!Board[r, c])
                    {
                        sb.Append(" ");
                    }
                    else
                    {
                        sb.Append(Live_Cell);
                    }
                }
                sb.AppendLine("║");
            }
            Console.Write(sb);

            Console.BackgroundColor = MenuText.Default_BG;
            Console.ForegroundColor = MenuText.Default_FG;    
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Calculates if the current dude at Board[r,c] will die or not.
        /// If a dude has less than 2, or more than 3 neighbors that dude
        /// is dead next generation.
        /// % because this allows wrapping around the board
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <returns>True if the current dude dies.</returns>
        private Boolean WillDieWrap(int r, int c)
        {
            int n = 0;

            if (Board[(r - 1 + Rows) % Rows, (c - 1 + Cols) % Cols]) n++;
            if (Board[(r - 1 + Rows) % Rows, (c + 1 + Cols) % Cols]) n++;
            if (Board[(r - 1 + Rows) % Rows, c]) n++;
            if (Board[(r + 1 + Rows) % Rows, (c - 1 + Cols) % Cols]) n++;
            if (Board[r, (c - 1 + Cols) % Cols]) n++;
            if (Board[(r + 1 + Rows) % Rows, c]) n++;
            if (Board[r, (c + 1 + Cols) % Cols]) n++;
            if (Board[(r + 1 + Rows) % Rows, (c + 1 + Cols) % Cols]) n++;

            if (n < 2) return true;
            if (n > 3) return true;
            else return false;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Calculates if the current space at Board[r,c] will become alive
        /// or not. If nothingness has exactly 3 neighbors it will become
        /// living next generation.
        /// % because this allows wrapping around the board
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <returns>True if the miracle of life occurs.</returns>
        private Boolean WillBeBornWrap(int r, int c)
        {
            int n = 0;

            if (Board[(r - 1 + Rows) % Rows, (c - 1 + Cols) % Cols]) n++;
            if (Board[(r - 1 + Rows) % Rows, (c + 1 + Cols) % Cols]) n++;
            if (Board[(r - 1 + Rows) % Rows, c]) n++;
            if (Board[(r + 1 + Rows) % Rows, (c - 1 + Cols) % Cols]) n++;
            if (Board[r, (c - 1 + Cols) % Cols]) n++;
            if (Board[(r + 1 + Rows) % Rows, c]) n++;
            if (Board[r, (c + 1 + Cols) % Cols]) n++;
            if (Board[(r + 1 + Rows) % Rows, (c + 1 + Cols) % Cols]) n++;

            if (n == 3) return true;
            else return false;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Calculates if the current dude at _Board[r,c] will die or not.
        /// If a dude has less than 2, or more than 3 neighbors that dude
        /// is dead next generation.
        /// Ugly because I dont fluff the board with a border of nothing
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <returns>True if the current dude dies.</returns>
        private Boolean WillDieNoWrap(int r, int c)
        {
            int n = 0;

            if (r != 0 && c != 0)
            {
                if (Board[r - 1, c - 1]) n++;
            }
            if (r != 0 && c != Cols - 1)
            {
                if (Board[r - 1, c + 1]) n++;
            }
            if (r != 0)
            {
                if (Board[r - 1, c]) n++;
            }
            if (r != Rows - 1 && c != 0)
            {
                if (Board[r + 1, c - 1]) n++;
            }
            if (c != 0)
            {
                if (Board[r, c - 1]) n++;
            }
            if (r != Rows - 1)
            {
                if (Board[r + 1, c]) n++;
            }
            if (c != Cols - 1)
            {
                if (Board[r, c + 1]) n++;
            }
            if (r != Rows - 1 && c != Cols - 1)
            {
                if (Board[r + 1, c + 1]) n++;
            }

            if (n < 2) return true;
            if (n > 3) return true;
            else return false;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Calculates if the current space at _Board[r,c] will become alive
        /// or not. If nothingness has exactly 3 neighbors it will become
        /// living next generation.
        /// Ugly because I dont fluff the board with a border of nothing
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <returns>True if the miracle of life occurs.</returns>
        private Boolean WillBeBornNoWrap(int r, int c)
        {
            int n = 0;

            if (r != 0 && c != 0)
            {
                if (Board[r - 1, c - 1]) n++;
            }
            if (r != 0 && c != Cols - 1)
            {
                if (Board[r - 1, c + 1]) n++;
            }
            if (r != 0)
            {
                if (Board[r - 1, c]) n++;
            }
            if (r != Rows - 1 && c != 0)
            {
                if (Board[r + 1, c - 1]) n++;
            }
            if (c != 0)
            {
                if (Board[r, c - 1]) n++;
            }
            if (r != Rows - 1)
            {
                if (Board[r + 1, c]) n++;
            }
            if (c != Cols - 1)
            {
                if (Board[r, c + 1]) n++;
            }
            if (r != Rows - 1 && c != Cols - 1)
            {
                if (Board[r + 1, c + 1]) n++;
            }

            if (n == 3) return true;
            else return false;
        }
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
    } // end class
///////////////////////////////////////////////////////////////////////////////
}