using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using GHGameOfLife.Rules;

namespace GHGameOfLife
{
///////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Class to support drawing 1D automata rules
    /// </summary>
///////////////////////////////////////////////////////////////////////////////
    class Automata1D : ConsoleAutomata
    {
        private Rules1D.RuleDelegate Rule;
        private delegate bool Rule1D(int col);
        public enum BuildTypes { Random, Single };

        private bool[] Current_Row;
        private bool[][] Entire_Board;
        private const char LIVE_CELL = '█';
        private const char DEAD_CELL = ' ';
        private int Print_Row;
        private List<ConsoleColor> Print_Colors;
        private Random RNG;
        private string Rule_Name = "";

        public override bool[,] Board_Copy
        {
            get
            {
                var temp = new bool[this.Rows, this.Cols];
                for( int r = 0; r < this.Rows; r++ )
                {
                    for( int c = 0; c < this.Cols; c++ )
                    {
                        temp[r, c] = this.Entire_Board[r][c];
                    }
                }
                return temp;
            }
        }
//-----------------------------------------------------------------------------
        private Automata1D(int rowMax, int colMax, string rule) : base(rowMax,colMax)
        {
            // TODO: Add a "custom" rule set that allows users to input a rule of the form Ry,Wxxxxxxxx"
            this.Print_Row = 0;
            this.Current_Row = new bool[this.Cols];
            this.Entire_Board = new bool[this.Rows][];
            this.RNG = new Random();

            this.Print_Colors = (Enum.GetValues(typeof(ConsoleColor)) as ConsoleColor[]).Where(color => color != ConsoleColor.Black).ToList();
            var chosenRule = Rules1D.RuleMethods.Where(fn => fn.Name.Contains(rule)).First();
            this.Rule = (Rules1D.RuleDelegate)Delegate.CreateDelegate(typeof(Rules1D.RuleDelegate), chosenRule);
            this.Rule_Name = rule;
            //Rules1D.UserRule = "R1,W16";
            Rules1D.RuleDict_Initialized = false;
        }
//-----------------------------------------------------------------------------
        public static Automata1D InitializeAutomata(int rowMax, int colMax, BuildTypes bType, string rType)
        {
            var newAutomata1D = new Automata1D(rowMax, colMax, rType);
            //This is really gross but it prompts users for input on lines 15, 16 and 17 for the range and hex value of a custom 1d rule
            //This also lets the user type all willy nilly and can mess up the nice boarder but oh well for now
            if(rType == "Custom")
            {
                MenuHelper.PrintOnLine(15, "Select Range");
                var tempRange = MenuHelper.PromptOnLine(16,"[1-4]: ");
                int range;
                while(!MenuHelper.ValidRangeInput(tempRange,out range))
                {
                    MenuHelper.PrintOnLine(17, "Try again");
                    tempRange = MenuHelper.PromptOnLine(16, "[1-4]: ");                                     
                }
                MenuHelper.PrintOnLine(15, "Enter a hex string");
                var tempHex = MenuHelper.PromptOnLine(16, "the hex: ");
                string hex = String.Empty;
                while(!MenuHelper.ValidHexInput(tempHex,out hex))
                {
                    MenuHelper.PrintOnLine(17, "thats a bad hex");
                    tempHex = MenuHelper.PromptOnLine(16, "the hex: ");
                }
                //Clear the inside of the border once we get some valid values
                MenuHelper.ClearAllInBorder();

                Rules1D.UserRule = "R" + range + ",W" + hex;
            }
            switch(bType)
            {
                case BuildTypes.Random:
                    newAutomata1D.Build1DBoard_Random();
                    break;
                case BuildTypes.Single:
                    newAutomata1D.Build1DBoard_Single();
                    break;
            }
            newAutomata1D.Is_Initialized = true;
            MenuHelper.PrintOnLine(2, newAutomata1D.Rule_Name.Replace('_', ' '));
            return newAutomata1D;
        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Function to calculate the next value for all the cells in this.Current_Row
        /// using this.__Rule
        /// </summary>
        public override void NextGeneration()
        {
            var nextRow = new bool[this.Cols];
            for( int i = 0; i < Cols; i++ )
            {
                nextRow[(i + this.Cols) % this.Cols] = this.Rule(this.Current_Row,i);
            }

            //Shift the entire board up if it is already filled, and place this new row
            //at the bottom
            if (this.Print_Row >= this.Rows)
            {
                this.Entire_Board = GenericHelp<bool>.ShiftUp(this.Entire_Board);
                this.Entire_Board[(this.Rows - 1)] = nextRow;
            }
            else
            {
                this.Entire_Board[this.Print_Row] = nextRow;
            }

            this.Current_Row = nextRow;
        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Prings the automata rule within the boarders of the console
        /// </summary>
        public override void PrintBoard()
        {
            if ( this.Print_Row >= this.Rows )
            {
                //If we are at the number of rows, we need to shift everything up
                //by one except the first row and then continue printing and the bottom
                //of the screen
                Console.MoveBufferArea(MenuHelper.Space, MenuHelper.Space+1, this.Cols, this.Rows-1, MenuHelper.Space, MenuHelper.Space);
                this.Print_Row--;
            }
            Console.SetCursorPosition(MenuHelper.Space, MenuHelper.Space + this.Print_Row);
            var printRow = new StringBuilder();
            foreach (bool val in this.Current_Row)
            {
                if (val)
                    printRow.Append(LIVE_CELL);
                else
                    printRow.Append(DEAD_CELL);
            }

            Console.ForegroundColor = this.Print_Colors[this.RNG.Next(this.Print_Colors.Count)];
            Console.Write(printRow);
            this.Print_Row++;

            //Console.BackgroundColor = MenuHelper.Default_BG;
            //Console.ForegroundColor = MenuHelper.Default_FG;
        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Builds a random initial board
        /// </summary>
        private void Build1DBoard_Random()
        {
            for (int i = 0; i < this.Cols; i++)
            {
                this.Current_Row[i] = (this.RNG.Next() % 2 == 0);
            }
            this.Entire_Board[0] = this.Current_Row;
        }
//-----------------------------------------------------------------------------
        private void Build1DBoard_Single()
        {
            this.Current_Row[this.Cols / 2] = true;
            this.Entire_Board[0] = this.Current_Row;
        }
//-----------------------------------------------------------------------------
    }
///////////////////////////////////////////////////////////////////////////////
}