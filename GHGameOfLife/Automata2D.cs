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
    class Automata2D : ConsoleAutomata
    {
        delegate bool Rule2D(int row, int col);
        public enum BuildTypes { Random, File, Resource, User };
        public enum RuleTypes { Life };

        private bool[,] __Board;
        private const char LIVE_CELL = '☺';
        private const char DEAD_CELL = ' ';

        public override bool[,] Board
        {
            get
            {
                var temp = new bool[this.__Num_Rows, this.__Num_Cols];
                for(int r = 0; r < this.__Num_Rows; r++)
                {
                    for( int c = 0; c < this.__Num_Cols; c++ )
                    {
                        temp[r, c] = this.__Board[r, c];
                    }
                }
                return temp;
            }
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Constructor for the GoLBoard class. Size of the board will be based
        /// off the size of the console window...
        /// </summary>
        /// <param name="rowMax">Number of rows</param>
        /// <param name="colMax">Number of columns</param>
        public Automata2D(int rowMax, int colMax, BuildTypes bType, string res = null) : base(rowMax,colMax)
        {
            this.__Board = new bool[rowMax, colMax];
            ConsoleRunHelper.CalcBuilderBounds(this);
            this.InitializeBoard(bType,res);
        }
//------------------------------------------------------------------------------
        private void InitializeBoard(BuildTypes bType, string res)
        {
            switch (bType)
            {
                //Build a random population
                case BuildTypes.Random:
                    this.__Board = ConsoleRunHelper.Build2DBoard_Random(this);
                    break;
                //Build a population from a CELLS-style file
                //defaults to random in case of an error
                case BuildTypes.File:
                    this.__Board = ConsoleRunHelper.Build2DBoard_File(this);
                    break;
                //Build a population using one of the CELLS files that is stored as a resource
                //defaults to random in case of an error
                case BuildTypes.Resource:
                    this.__Board = ConsoleRunHelper.Build2DBoard_Resource(res, this);
                    break;
                //Build a population based on user input
                case BuildTypes.User:
                    this.__Board = ConsoleRunHelper.Build2DBoard_User(this);
                    break;
            }
            this.__Is_Initialized = true;
        } 
//------------------------------------------------------------------------------
        /// <summary>
        /// Adds the next board values to a queue to be read from
        /// </summary>
        public override void NextGeneration()
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
            this.__Generation++;
            this.__Board = nextBoard;                  
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Prints the game based on the threaded set up.
        /// Waits until there are at least 2 boards in the board queue and then 
        /// prints the next board in the queue. 
        /// </summary>
        public override void PrintBoard()
        {
            Console.SetCursorPosition(0, 1);
            Console.Write(" ".PadRight(Console.WindowWidth));
            string write = "Generation " + __Generation;
            int left = (Console.WindowWidth / 2) - (write.Length / 2);
            Console.SetCursorPosition(left, 1);
            Console.Write(write);

            Console.BackgroundColor = MenuText.Default_BG;
            Console.ForegroundColor = MenuText.Board_FG;

            Console.SetCursorPosition(0, MenuText.Space);
            StringBuilder sb = new StringBuilder();
            for (int r = 0; r < this.__Num_Rows; r++)
            {
                sb.Append("    ║");
                for (int c = 0; c < this.__Num_Cols; c++)
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

            if (this.__Board[(r - 1 + this.__Num_Rows) % this.__Num_Rows, (c - 1 + this.__Num_Cols) % this.__Num_Cols]) n++;
            if (this.__Board[(r - 1 + this.__Num_Rows) % this.__Num_Rows, (c + 1 + this.__Num_Cols) % this.__Num_Cols]) n++;
            if (this.__Board[(r - 1 + this.__Num_Rows) % this.__Num_Rows, c]) n++;
            if (this.__Board[(r + 1 + this.__Num_Rows) % this.__Num_Rows, (c - 1 + this.__Num_Cols) % this.__Num_Cols]) n++;
            if (this.__Board[r, (c - 1 + this.__Num_Cols) % this.__Num_Cols]) n++;
            if (this.__Board[(r + 1 + this.__Num_Rows) % this.__Num_Rows, c]) n++;
            if (this.__Board[r, (c + 1 + this.__Num_Cols) % this.__Num_Cols]) n++;
            if (this.__Board[(r + 1 + this.__Num_Rows) % this.__Num_Rows, (c + 1 + this.__Num_Cols) % this.__Num_Cols]) n++;

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