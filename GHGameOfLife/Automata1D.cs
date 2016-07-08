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
        public enum RuleTypes {Rule_1, Rule_18, Rule_30, Rule_73, Rule_90, Rule_129 };

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
        private Automata1D(int rowMax, int colMax, RuleTypes rule) : base(rowMax,colMax)
        {
            this.__Print_Row = 0;
            this.__Current_Row = new bool[this.__Num_Cols];
            this.__Entire_Board = new bool[this.__Num_Rows][];
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

            switch(rule)
            {
                case RuleTypes.Rule_30:
                    this.__Rule = Rule30;
                    break;
                case RuleTypes.Rule_90:
                    this.__Rule = Rule90;
                    break;
                case RuleTypes.Rule_1:
                    this.__Rule = Rule1;
                    break;
                case RuleTypes.Rule_73:
                    this.__Rule = Rule73;
                    break;
                case RuleTypes.Rule_129:
                    this.__Rule = Rule129;
                    break;
                case RuleTypes.Rule_18:
                    this.__Rule = Rule18;
                    break;
                default:
                    this.__Rule = Rule90;
                    break;
            }
        }
//-----------------------------------------------------------------------------
        public static Automata1D InitializeAutomata(int rowMax, int colMax, BuildTypes bType, RuleTypes rType)
        {
            var newAutomata1D = new Automata1D(rowMax, colMax, rType);
            switch(bType)
            {
                case BuildTypes.Random:
                    newAutomata1D.Build1DBoard_Random();
                    break;
                case BuildTypes.Single:
                    newAutomata1D.Build1DBoard_Single();
                    break;
            }
            newAutomata1D.__Is_Initialized = true;
            return newAutomata1D;
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
            Console.BackgroundColor = MenuHelper.Default_BG;
            Console.ForegroundColor = MenuHelper.Board_FG;

            if( this.__Print_Row >= this.__Num_Rows )
            {
                //If we are at the number of rows, we need to shift everything up
                //by one except the first row and then continue printing and the bottom
                //of the screen
                //Magic numbers: 
                //      srcTop -> +1 because we skip the first row of data
                //      srcHeight -> -1 because we skip the first row of data
                Console.MoveBufferArea(MenuHelper.Space, MenuHelper.Space+1, this.__Num_Cols, this.__Num_Rows-1, MenuHelper.Space, MenuHelper.Space);
                --this.__Print_Row;
            }
            Console.SetCursorPosition(MenuHelper.Space, MenuHelper.Space + this.__Print_Row);
            var printRow = new StringBuilder();
            //printRow.Append("    ║");
            foreach (bool val in this.__Current_Row)
            {
                if (val)
                    printRow.Append(LIVE_CELL);
                else
                    printRow.Append(DEAD_CELL);
            }
            //printRow.Append("║");

            Console.ForegroundColor = this.__Print_Colors[this.__Rand.Next(this.__Print_Colors.Count)];
            Console.Write(printRow);
            this.__Print_Row++;

            Console.BackgroundColor = MenuHelper.Default_BG;
            Console.ForegroundColor = MenuHelper.Default_FG;
        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Builds a random initial board
        /// </summary>
        private void Build1DBoard_Random()
        {
            for (int i = 0; i < this.__Num_Cols; i++)
            {
                this.__Current_Row[i] = (this.__Rand.Next() % 2 == 0);
            }
            this.__Entire_Board[0] = this.__Current_Row;
        }
//-----------------------------------------------------------------------------
        private void Build1DBoard_Single()
        {
            this.__Current_Row[this.__Num_Cols / 2] = true;
            this.__Entire_Board[0] = this.__Current_Row;
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

            return left ^ (center | right);
        }
//-----------------------------------------------------------------------------
        private bool Rule1(int col)
        {
            bool left = this.__Current_Row[(col - 1 + this.__Num_Cols) % this.__Num_Cols];
            bool center = this.__Current_Row[(col + this.__Num_Cols) % this.__Num_Cols];
            bool right = this.__Current_Row[(col + 1 + this.__Num_Cols) % this.__Num_Cols];

            return !(left | center | right);
        }
//-----------------------------------------------------------------------------
        private bool Rule73(int col)
        {
            bool left = this.__Current_Row[(col - 1 + this.__Num_Cols) % this.__Num_Cols];
            bool center = this.__Current_Row[(col + this.__Num_Cols) % this.__Num_Cols];
            bool right = this.__Current_Row[(col + 1 + this.__Num_Cols) % this.__Num_Cols];

            return !(left & right | left ^ center ^ right);
        }
//-----------------------------------------------------------------------------
        private bool Rule129(int col)
        {
            bool left = this.__Current_Row[(col - 1 + this.__Num_Cols) % this.__Num_Cols];
            bool center = this.__Current_Row[(col + this.__Num_Cols) % this.__Num_Cols];
            bool right = this.__Current_Row[(col + 1 + this.__Num_Cols) % this.__Num_Cols];

            return !(left ^ center | left ^ right);
        }
//-----------------------------------------------------------------------------
        private bool Rule18(int col)
        {
            bool left = this.__Current_Row[(col - 1 + this.__Num_Cols) % this.__Num_Cols];
            bool center = this.__Current_Row[(col + this.__Num_Cols) % this.__Num_Cols];
            bool right = this.__Current_Row[(col + 1 + this.__Num_Cols) % this.__Num_Cols];

            return (left ^ right ^ center) & !center;
        }
//-----------------------------------------------------------------------------
    }
///////////////////////////////////////////////////////////////////////////////
}
