using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace GHGameOfLife
{
///////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Class to support drawing 1D automata rules
    /// </summary>
///////////////////////////////////////////////////////////////////////////////
    class Automata1D : ConsoleAutomata
    {
        delegate bool Rule1D(int col);
        public enum BuildTypes { Random, Single };
        public enum RuleTypes { Rule30, Rule90 };

        private bool[] __Current_Row;
        private bool[][] __Entire_Board;
        private const char LIVE_CELL = '█';
        private const char DEAD_CELL = ' ';
        private int __Print_Row;
        private Rule1D __Rule;
        private List<ConsoleColor> __Print_Colors;
        private Random __Rand;

        public override bool[,] Board
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
//-----------------------------------------------------------------------------
        public Automata1D(int rowMax, int colMax, RuleTypes rule) : base(rowMax,colMax)
        {
            this.__Print_Row = 0;
            this.__Current_Row = new bool[this.__Num_Cols];
            this.__Entire_Board = new bool[this.__Num_Rows][];
            this.__Rule = new Rule1D(Rule90);
            this.__Rand = new Random();

            var allColors = Enum.GetValues(typeof(ConsoleColor));
            this.__Print_Colors = new List<ConsoleColor>();
            foreach(ConsoleColor color in allColors)
            {
                if(color != ConsoleColor.Black)
                {
                    this.__Print_Colors.Add(color);
                }
            }

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
        public override void NextGeneration()
        {
            var nextRow = new bool[this.__Num_Cols];
            for( int i = 0; i < __Num_Cols; i++ )
            {
                nextRow[(i + this.__Num_Cols) % this.__Num_Cols] = this.__Rule(i);
            }

            //Shift the entire board up if it is already filled, and place this new row
            //at the bottom
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
        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Prings the automata rule within the boarders of the console
        /// </summary>
        public override void PrintBoard()
        {
            Console.BackgroundColor = MenuText.Default_BG;
            Console.ForegroundColor = MenuText.Board_FG;

            if( this.__Print_Row >= this.__Num_Rows )
            {
                //If we are at the number of rows, we need to shift everything up
                //by one except the first row and then continue printing and the bottom
                //of the screen
                //Magic numbers: 
                //      srcLeft, destLeft -> -1 so we also scroll the colored border
                //      srcTop -> +1 because we skip the first row of data
                //      srcWidth -> cols+2 so we scroll the colored border on the right side
                //      srcHeight -> -1 because we skip the first row of data
                Console.MoveBufferArea(MenuText.Space-1, MenuText.Space+1, this.__Num_Cols+2, this.__Num_Rows-1, MenuText.Space-1, MenuText.Space);
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
                    printRow.Append(DEAD_CELL);
            }
            printRow.Append("║");

            Console.ForegroundColor = this.__Print_Colors[this.__Rand.Next(this.__Print_Colors.Count)];
            Console.Write(printRow);
            this.__Print_Row++;

            Console.BackgroundColor = MenuText.Default_BG;
            Console.ForegroundColor = MenuText.Default_FG;
        }
//-----------------------------------------------------------------------------
//  Automata Rules (http://atlas.wolfram.com/TOC/TOC_200.html)
//-----------------------------------------------------------------------------
        private bool Rule90(int col)
        {
            bool left = this.__Current_Row[(col - 1 + this.__Num_Cols) % this.__Num_Cols];
            bool center = this.__Current_Row[(col + this.__Num_Cols) % this.__Num_Cols];
            bool right = this.__Current_Row[(col + 1 + this.__Num_Cols) % this.__Num_Cols];

            return left ^ right;
        }
//-----------------------------------------------------------------------------
        private bool Rule30(int col)
        {
            bool left = this.__Current_Row[(col - 1 + this.__Num_Cols) % this.__Num_Cols];
            bool center = this.__Current_Row[(col + this.__Num_Cols) % this.__Num_Cols];
            bool right = this.__Current_Row[(col + 1 + this.__Num_Cols) % this.__Num_Cols];

            return left ^ (center || right);
        }
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
    }
///////////////////////////////////////////////////////////////////////////////
}
