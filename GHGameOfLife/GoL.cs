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

        private static bool[,] Board;
        private int Generation;
        private const char Live_Cell = '☺';
        //private const char _DeadCell = ' ';
        private bool Wrap { get; set; }
        
        private static bool IsInitialized;
        private static int Rows;
        private static int Cols;
        private static int OrigConsHeight;
        private static int OrigConsWidth;

        private static bool StopNext;
        private static AutoResetEvent NextStopped;
        private static Mutex StopLock;
        private const int Max_Boards = 100;
        private static ConcurrentQueue<bool[,]> Next_Boards;
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


            StopNext = false;
            StopLock = new Mutex(false);
            NextStopped = new AutoResetEvent(false);
            Next_Boards = new ConcurrentQueue<bool[,]>();

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
            Next_Boards.Enqueue(Board);
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
            Next_Boards.Enqueue(Board);
            IsInitialized = true;            
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Builds the board from user input. This is going to be ugly...
        /// </summary>
        public void BuildFromUser()
        {
            GoLHelper.BuildBoardUser();
            Next_Boards.Enqueue(Board);
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
            Next_Boards.Enqueue(Board);
            IsInitialized = true;
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
        private static void ThreadedNext()
        {
            var go = true;
            while(go)
            {
                while(Next_Boards.Count >= Max_Boards)
                {
                    StopLock.WaitOne();
                    if(StopNext)
                    {
                        go = false;
                        break;
                    }
                    StopLock.ReleaseMutex();
                    Thread.Sleep(10);
                }

                if (Next_Boards.Count < Max_Boards && IsInitialized && go)
                {
                    var lastBoard = Next_Boards.Last();
                    var nextBoard = new bool[Rows, Cols];

                    for (int r = 0; r < Rows; r++)
                    {
                        for (int c = 0; c < Cols; c++)
                        {
                            nextBoard[r, c] = ThreadedNextCellState(r, c, ref lastBoard);
                        }
                    }
                    Next_Boards.Enqueue(nextBoard);
                }
                else
                {
                    Thread.Sleep(10);
                }

                StopLock.WaitOne();
                if (StopNext)
                {
                    go = false;
                }
                StopLock.ReleaseMutex();
            }
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Prints the game based on the threaded set up.
        /// Waits until there are at least 2 boards in the board queue and then 
        /// prints the next board in the queue. 
        /// </summary>
        private void ThreadedPrint()
        {
            bool printed = false;
            do
            {
                if (Next_Boards.Count >= 2)
                {
                    if (Next_Boards.TryDequeue(out Board))
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
                        printed = true;
                    }
                }
            } while (!printed);
            Generation++;
        }
//------------------------------------------------------------------------------
        private static bool ThreadedNextCellState(int r, int c, ref bool[,] board)
        {
            int n = 0;

            if (board[(r - 1 + Rows) % Rows, (c - 1 + Cols) % Cols]) n++;
            if (board[(r - 1 + Rows) % Rows, (c + 1 + Cols) % Cols]) n++;
            if (board[(r - 1 + Rows) % Rows, c]) n++;
            if (board[(r + 1 + Rows) % Rows, (c - 1 + Cols) % Cols]) n++;
            if (board[r, (c - 1 + Cols) % Cols]) n++;
            if (board[(r + 1 + Rows) % Rows, c]) n++;
            if (board[r, (c + 1 + Cols) % Cols]) n++;
            if (board[(r + 1 + Rows) % Rows, (c + 1 + Cols) % Cols]) n++;

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