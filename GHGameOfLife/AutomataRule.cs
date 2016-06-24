using System;
using System.Text;

namespace GHGameOfLife
{
///////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Class to support drawing 2D automata rules
    /// </summary>
///////////////////////////////////////////////////////////////////////////////
    class AutomataRule : IConsoleAutomata
    {
        delegate bool Rule1D(bool p, bool q, bool r);

        private bool[] __Current_Row;
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
        //TODO: Change this to actually be a copy of the current console display
        public bool[,] Board
        {
            get
            {
                return new bool[0, 0];
            }
        }

        public enum RuleTypes { rule90 };
//-----------------------------------------------------------------------------
        public AutomataRule(int rows, int cols, RuleTypes rule)
        {
            this.Is_Wrapping = true;
            this.__Num_Cols = cols;
            this.__Num_Rows = rows;
            this.__Print_Row = 0;
            __Orig_Console_Height = Console.WindowHeight;
            __Orig_Console_Width = Console.WindowWidth;
            this.__Current_Row = new bool[this.__Num_Cols];
            var rand = new Random();
            for (int i = 0; i < this.__Num_Cols; i++)
            {
                this.__Current_Row[i] = (rand.Next() % 2 == 0) ? false : false;
            }
            this.__Current_Row[this.__Num_Cols / 2] = true;
            this.__Rule = new Rule1D(Rule90);
            this.__Is_Initialized = true;
        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Function for handling the running of the AutomataRule
        /// </summary>
        public void Run()
        {
            for( int i = 0; i < 500; i++ )
            {
                this.PrintBoard();
                this.NextGeneration();
            }
        }
//-----------------------------------------------------------------------------
         /// <summary>
         /// Function to calculate the next value for all the cells in this.Current_Row
         /// </summary>
         /// <param name="rule">A function of form (bool,bool,bool) -> bool that takes the 
         /// current cell and it's neighbors and returns the next value of the cell.</param>
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
