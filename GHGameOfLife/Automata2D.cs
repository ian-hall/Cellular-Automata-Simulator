using System;
using System.Text;

namespace GHGameOfLife
{
    /// <summary>
    /// This class pretty much does everything. It sets up the console, 
    /// fills in the initial pop from a file or randomly, and then 
    /// does all the checking for living/dying of the population.
    /// </summary>
///////////////////////////////////////////////////////////////////////////////
    class Automata2D : IConsoleAutomata
    {
        delegate bool Rule2D(int row, int col);

        private bool[,] __Board;
        private int Generation;
        private const char LIVE_CELL = '☺';
        private const char DEAD_CELL = ' ';
        private bool __Is_Initialized;
        private int __Rows;
        private int __Cols;
        private int __Orig_Console_Height;
        private int __Orig_Console_Width;

        public bool Is_Initialized { get { return this.__Is_Initialized; } }
        public int Rows { get { return this.__Rows; } }
        public int Cols { get { return this.__Cols; } }
        public int Console_Height { get { return this.__Orig_Console_Height; } }
        public int Console_Width { get { return this.__Orig_Console_Width; } }
        public bool Is_Wrapping { get; set; }
        public bool[,] Board {
            get
            {
                var temp = new bool[this.__Rows, this.__Cols];
                for(int r = 0; r < this.__Rows; r++)
                {
                    for( int c = 0; c < this.__Cols; c++ )
                    {
                        temp[r, c] = this.__Board[r, c];
                    }
                }
                return temp;
            }
        }

        public enum BuildType { Random, File, Resource, User };
//------------------------------------------------------------------------------
        /// <summary>
        /// Constructor for the GoLBoard class. Size of the board will be based
        /// off the size of the console window...
        /// </summary>
        /// <param name="rowMax">Number of rows</param>
        /// <param name="colMax">Number of columns</param>
        public Automata2D(int rowMax, int colMax, BuildType bType, string res = null)
        {
            this.__Board = new bool[rowMax, colMax];
                        
            this.__Rows = rowMax;
            this.__Cols = colMax;
            this.__Orig_Console_Height = Console.WindowHeight;
            this.__Orig_Console_Width = Console.WindowWidth;
            this.__Is_Initialized = false;
            this.Generation = 1;
            this.Is_Wrapping = true;

            ConsoleRunHelper.CalcBuilderBounds(this);
            this.InitializeBoard(bType,res);
        }
//------------------------------------------------------------------------------
        private void InitializeBoard(BuildType bType, string res)
        {
            switch (bType)
            {
                //Build a random population
                case BuildType.Random:
                    this.__Board = ConsoleRunHelper.Build2DBoard_Random(this);
                    break;
                //Build a population from a CELLS-style file
                //defaults to random in case of an error
                case BuildType.File:
                    this.__Board = ConsoleRunHelper.Build2DBoard_File(this);
                    break;
                //Build a population using one of the CELLS files that is stored as a resource
                //defaults to random in case of an error
                case BuildType.Resource:
                    this.__Board = ConsoleRunHelper.Build2DBoard_Resource(res, this);
                    break;
                //Build a population based on user input
                case BuildType.User:
                    this.__Board = ConsoleRunHelper.Build2DBoard_User(this);
                    break;
            }
            this.__Is_Initialized = true;
        } 
//------------------------------------------------------------------------------
        /// <summary>
        /// Adds the next board values to a queue to be read from
        /// </summary>
        public void NextGeneration()
        {
            var lastBoard = this.__Board;
            var nextBoard = new bool[Rows, Cols];
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    nextBoard[r, c] = NextCellState(r, c);
                }
            }
            this.Generation++;
            this.__Board = nextBoard;                  
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Prints the game based on the threaded set up.
        /// Waits until there are at least 2 boards in the board queue and then 
        /// prints the next board in the queue. 
        /// </summary>
        public void PrintBoard()
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
                    if (!__Board[r, c])
                    {
                        sb.Append(Automata2D.DEAD_CELL);
                    }
                    else
                    {
                        sb.Append(Automata2D.LIVE_CELL);
                    }
                }
                sb.AppendLine("║");
            }
            Console.Write(sb);

            Console.BackgroundColor = MenuText.Default_BG;
            Console.ForegroundColor = MenuText.Default_FG;
        }
//------------------------------------------------------------------------------
        private bool NextCellState(int r, int c)
        {
            int n = 0;

            if (this.__Board[(r - 1 + this.__Rows) % this.__Rows, (c - 1 + this.__Cols) % this.__Cols]) n++;
            if (this.__Board[(r - 1 + this.__Rows) % this.__Rows, (c + 1 + this.__Cols) % this.__Cols]) n++;
            if (this.__Board[(r - 1 + this.__Rows) % this.__Rows, c]) n++;
            if (this.__Board[(r + 1 + this.__Rows) % this.__Rows, (c - 1 + this.__Cols) % this.__Cols]) n++;
            if (this.__Board[r, (c - 1 + this.__Cols) % this.__Cols]) n++;
            if (this.__Board[(r + 1 + this.__Rows) % this.__Rows, c]) n++;
            if (this.__Board[r, (c + 1 + this.__Cols) % this.__Cols]) n++;
            if (this.__Board[(r + 1 + this.__Rows) % this.__Rows, (c + 1 + this.__Cols) % this.__Cols]) n++;

            if(this.__Board[r,c])
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