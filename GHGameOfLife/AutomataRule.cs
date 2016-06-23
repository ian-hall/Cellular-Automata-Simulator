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
        private bool[] Row;
        private int Cols;
        private int OrigConsHeight;
        private int OrigConsWidth;
        private char Live_Cell = '█';
        private RuleTypes Rule;

        public enum RuleTypes { rule90 };
//-----------------------------------------------------------------------------
        public AutomataRule(int cols, RuleTypes rule)
        {
            this.Cols = cols;
            OrigConsHeight = Console.WindowHeight;
            OrigConsWidth = Console.WindowWidth;
            this.Row = new bool[this.Cols];
            var rand = new Random();
            for (int i = 0; i < this.Cols; i++)
            {
                this.Row[i] = (rand.Next() % 2 == 0) ? false : false;
            }
            this.Row[this.Cols / 2] = true;
            this.Rule = rule;
        }
//-----------------------------------------------------------------------------
        public void Run()
        {
            for( int i = 0; i < 100; i++ )
            {
                this.Next();
                this.PrintRule();
            }
        }
//-----------------------------------------------------------------------------
        private void Next()
        {
            var nextRow = new bool[this.Cols];
            for( int i = 0; i < Cols; i++ )
            {
                bool p = this.Row[(i + this.Cols) % this.Cols];
                bool q = this.Row[(i + 1 + this.Cols) % this.Cols];
                bool r = this.Row[(i + 2 + this.Cols) % this.Cols];

                bool rule90 = p ^ r;

                nextRow[(i + 1 + this.Cols) % this.Cols] = rule90;
            }
            this.Row = nextRow;
        }
//-----------------------------------------------------------------------------
        private void PrintRule()
        {
            StringBuilder printRow = new StringBuilder();
            printRow.Append('▌');
            foreach (bool val in this.Row)
            {
                if (val)
                    printRow.Append("█");
                else
                    printRow.Append(" ");
            }
            printRow.Append('▐');
            Console.WriteLine(printRow.ToString());
        }
//-----------------------------------------------------------------------------
    }
///////////////////////////////////////////////////////////////////////////////
}
