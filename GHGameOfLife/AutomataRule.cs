using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHGameOfLife
{
///////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Class to support drawing 2D automata rules
    /// </summary>
///////////////////////////////////////////////////////////////////////////////
    class AutomataRule
    {
        private bool[] Current_Row;
        private int Num_Cols;
        private int Num_Rows;
        private int Print_Row;
        private int OrigConsHeight;
        private int OrigConsWidth;
        private char Live_Cell = '█';
        private RuleTypes Rule;
        private int Generation;

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
            this.Rule = rule;
        }
//-----------------------------------------------------------------------------
        public void Run()
        {
            for( int i = 0; i < 5000; i++ )
            {
                this.PrintRule();
                this.Next();
            }
        }
//-----------------------------------------------------------------------------
        private void Next()
        {
            var nextRow = new bool[this.Num_Cols];
            for( int i = 0; i < Num_Cols; i++ )
            {
                bool p = this.Current_Row[(i + this.Num_Cols) % this.Num_Cols];
                bool q = this.Current_Row[(i + 1 + this.Num_Cols) % this.Num_Cols];
                bool r = this.Current_Row[(i + 2 + this.Num_Cols) % this.Num_Cols];

                bool rule90 = p ^ r;

                nextRow[(i + 1 + this.Num_Cols) % this.Num_Cols] = rule90;
            }
            this.Current_Row = nextRow;
            this.Generation++;
        }
//-----------------------------------------------------------------------------
        private void PrintRule()
        {
            Console.BackgroundColor = MenuText.Default_BG;
            Console.ForegroundColor = MenuText.Board_FG;

            if( this.Print_Row >= this.Num_Rows )
            {
                //If we are at the number of rows, we need to shift everything up
                //by one except the first row and then continue printing and the bottom
                //of the screen
                Console.MoveBufferArea(0, MenuText.Space + 1, this.Num_Cols+5, this.Num_Rows-1, 0, MenuText.Space);
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
    }
///////////////////////////////////////////////////////////////////////////////
}
