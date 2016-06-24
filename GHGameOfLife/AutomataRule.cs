using System;
using System.Text;

namespace GHGameOfLife
{
///////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Class to support drawing 2D automata rules
    /// </summary>
///////////////////////////////////////////////////////////////////////////////
    class AutomataRule
    {
        delegate bool RuleFN(bool p, bool q, bool r);

        private bool[] Current_Row;
        private int Num_Cols;
        private int Num_Rows;
        private int Print_Row;
        private int OrigConsHeight;
        private int OrigConsWidth;
        private char Live_Cell = '█';
        private int Generation;
        private RuleFN Rule;

        public enum RuleTypes { rule90 };
//-----------------------------------------------------------------------------
        public AutomataRule(int rows, int cols, RuleTypes rule)
        {
            this.Num_Cols = cols;
            this.Num_Rows = rows;
            this.Print_Row = 0;
            OrigConsHeight = Console.WindowHeight;
            OrigConsWidth = Console.WindowWidth;
            this.Current_Row = new bool[this.Num_Cols];
            var rand = new Random();
            for (int i = 0; i < this.Num_Cols; i++)
            {
                this.Current_Row[i] = (rand.Next() % 2 == 0) ? false : false;
            }
            this.Current_Row[this.Num_Cols / 2] = true;
            this.Rule = new RuleFN(Rule30);
        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Function for handling the running of the AutomataRule
        /// </summary>
        public void Run()
        {
            for( int i = 0; i < 500; i++ )
            {
                this.PrintRule();
                this.Next(this.Rule);
            }
        }
//-----------------------------------------------------------------------------
         /// <summary>
         /// Function to calculate the next value for all the cells in this.Current_Row
         /// </summary>
         /// <param name="rule">A function of form (bool,bool,bool) -> bool that takes the 
         /// current cell and it's neighbors and returns the next value of the cell.</param>
        private void Next(RuleFN rule)
        {
            var nextRow = new bool[this.Num_Cols];
            for( int i = 0; i < Num_Cols; i++ )
            {
                bool p = this.Current_Row[(i + this.Num_Cols) % this.Num_Cols];
                bool q = this.Current_Row[(i + 1 + this.Num_Cols) % this.Num_Cols];
                bool r = this.Current_Row[(i + 2 + this.Num_Cols) % this.Num_Cols];

                nextRow[(i + 1 + this.Num_Cols) % this.Num_Cols] = rule(p, q, r);
            }
            this.Current_Row = nextRow;
            this.Generation++;
        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Prings the automata rule within the boarders of the console
        /// </summary>
        private void PrintRule()
        {
            Console.BackgroundColor = MenuText.Default_BG;
            Console.ForegroundColor = MenuText.Board_FG;

            if( this.Print_Row >= this.Num_Rows )
            {
                //If we are at the number of rows, we need to shift everything up
                //by one except the first row and then continue printing and the bottom
                //of the screen
                //Magic numbers: 
                //          1 -> copy from the second line to the bottom
                //          5 -> heck if I know, something to do with menutext.space 
                Console.MoveBufferArea(0, MenuText.Space + 1, this.Num_Cols+MenuText.Space, this.Num_Rows-1, 0, MenuText.Space);
                --this.Print_Row;
            }
            Console.SetCursorPosition(0, MenuText.Space + this.Print_Row);
            var printRow = new StringBuilder();
            printRow.Append("    ║");
            foreach (bool val in this.Current_Row)
            {
                if (val)
                    printRow.Append(Live_Cell);
                else
                    printRow.Append(" ");
            }
            printRow.Append("║");
            Console.Write(printRow);
            this.Print_Row++;

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
