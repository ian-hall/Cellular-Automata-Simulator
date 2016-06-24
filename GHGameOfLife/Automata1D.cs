using System;
using System.Text;

namespace GHGameOfLife
{
///////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Class to support drawing 2D automata rules
    /// </summary>
///////////////////////////////////////////////////////////////////////////////
    class Automata1D : IConsoleAutomata
    {
        delegate bool Rule1D(bool p, bool q, bool r);

        private bool[] __Current_Row;
        private bool[][] __Entire_Board;
        private bool __Is_Initialized;
        private int __Num_Cols;
        private int __Num_Rows;
        private int __Print_Row;
        private int __Orig_Console_Height;
        private int __Orig_Console_Width;
        private const char LIVE_CELL = '█';
        private int __Generation;
        private Rule1D __Rule;

        public bool Is_Initialized { get { return this.__Is_Initialized; } }
        public int Rows { get { return this.__Num_Rows; } }
        public int Cols { get { return this.__Num_Cols; } }
        public int Console_Height { get { return this.__Orig_Console_Height; } }
        public int Console_Width { get { return this.__Orig_Console_Width; } }
        public bool Is_Wrapping { get; set; }
        public bool[,] Board
        {
            get
            {
                var temp = new bool[this.__Num_Rows, this.__Num_Cols];
                for( int r = 0; r < this.__Num_Rows; r++ )
                {
                    for( int c = 0; c < this.__Num_Cols; c++ )
                    {
                        temp[r, c] = this.__Entire_Board[r][c];
                    }
                }
                return temp;
            }
        }

        public enum RuleTypes { rule90 };
//-----------------------------------------------------------------------------
        public Automata1D(int rows, int cols, RuleTypes rule)
        {
            this.Is_Wrapping = true;
            this.__Num_Cols = cols;
            this.__Num_Rows = rows;
            this.__Print_Row = 0;
            __Orig_Console_Height = Console.WindowHeight;
            __Orig_Console_Width = Console.WindowWidth;
            this.__Current_Row = new bool[this.__Num_Cols];
            this.__Entire_Board = new bool[this.__Num_Rows][];
            this.__Rule = new Rule1D(Rule90);

            //This will eventually move to a separate function for building the board
            var rand = new Random();
            for (int i = 0; i < this.__Num_Cols; i++)
            {
                this.__Current_Row[i] = (rand.Next() % 2 == 0) ? false : false;
            }
            this.__Current_Row[this.__Num_Cols / 2] = true;
            this.__Entire_Board[0] = this.__Current_Row;
            this.__Is_Initialized = true;
        }
//-----------------------------------------------------------------------------
         /// <summary>
         /// Function to calculate the next value for all the cells in this.Current_Row
         /// using this.__Rule
         /// </summary>
        public void NextGeneration()
        {
            var nextRow = new bool[this.__Num_Cols];
            for( int i = 0; i < __Num_Cols; i++ )
            {
                bool p = this.__Current_Row[(i + this.__Num_Cols) % this.__Num_Cols];
                bool q = this.__Current_Row[(i + 1 + this.__Num_Cols) % this.__Num_Cols];
                bool r = this.__Current_Row[(i + 2 + this.__Num_Cols) % this.__Num_Cols];

                nextRow[(i + 1 + this.__Num_Cols) % this.__Num_Cols] = this.__Rule(p, q, r);
            }
            if (this.__Print_Row >= this.__Num_Rows)
            {
                this.__Entire_Board = GenericHelp<bool>.ShiftUp(this.__Entire_Board);
                this.__Entire_Board[(this.__Num_Rows - 1)] = nextRow;
            }
            else
            {
                this.__Entire_Board[this.__Print_Row] = nextRow;
            }
            this.__Current_Row = nextRow;
            this.__Generation++;
        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Prings the automata rule within the boarders of the console
        /// </summary>
        public void PrintBoard()
        {
            Console.BackgroundColor = MenuText.Default_BG;
            Console.ForegroundColor = MenuText.Board_FG;

            if( this.__Print_Row >= this.__Num_Rows )
            {
                //If we are at the number of rows, we need to shift everything up
                //by one except the first row and then continue printing and the bottom
                //of the screen
                //Magic numbers: 
                //          1 -> copy from the second line to the bottom
                //          5 -> heck if I know, something to do with menutext.space 
                Console.MoveBufferArea(0, MenuText.Space + 1, this.__Num_Cols+MenuText.Space, this.__Num_Rows-1, 0, MenuText.Space);
                --this.__Print_Row;
            }
            Console.SetCursorPosition(0, MenuText.Space + this.__Print_Row);
            var printRow = new StringBuilder();
            printRow.Append("    ║");
            foreach (bool val in this.__Current_Row)
            {
                if (val)
                    printRow.Append(LIVE_CELL);
                else
                    printRow.Append(" ");
            }
            printRow.Append("║");
            Console.Write(printRow);
            this.__Print_Row++;

            Console.BackgroundColor = MenuText.Default_BG;
            Console.ForegroundColor = MenuText.Default_FG;
        }
//-----------------------------------------------------------------------------
//  Automata Rules (http://atlas.wolfram.com/TOC/TOC_200.html)
//-----------------------------------------------------------------------------
        private bool Rule90(bool p, bool q, bool r)
        {
            return p ^ r;
        }
//-----------------------------------------------------------------------------
        private bool Rule30(bool p, bool q, bool r)
        {
            return p ^ (q || r);
        }
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
    }
///////////////////////////////////////////////////////////////////////////////
}
