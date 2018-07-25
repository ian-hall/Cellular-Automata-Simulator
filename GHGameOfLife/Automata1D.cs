using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Core_Automata.Rules;

namespace Core_Automata
{

    /// <summary>
    /// Class to support drawing 1D automata rules
    /// </summary>

    class Automata1D : ConsoleAutomata
    {
        private readonly Rules1D.RuleDelegate _rule;
        //private delegate bool Rule1D(int col);
        public enum BuildTypes { Random, Single };

        private bool[] _currentRow;
        private bool[][] _entireBoard;
        private const char _liveCell = '█';
        private const char _deadCell = ' ';
        private int _printRow;
        private readonly List<ConsoleColor> _printColors;
        private readonly Random _rng;
        private string _ruleName = "";

        public override bool[,] BoardCopy
        {
            get
            {
                var temp = new bool[this.Rows, this.Cols];
                for (int r = 0; r < this.Rows; r++)
                {
                    //TODO: Error if trying to copy a partially filled board. Make sure this stops it?? (or just force to False?)
                    if (this._entireBoard[r] == null)
                    {
                        break;
                    }
                    for (int c = 0; c < this.Cols; c++)
                    {
                        temp[r, c] = this._entireBoard[r][c];
                    }
                }
                return temp;
            }
        }

        private Automata1D(int rowMax, int colMax, string rule) : base(rowMax, colMax)
        {
            this._printRow = 0;
            this._currentRow = new bool[this.Cols];
            this._entireBoard = new bool[this.Rows][];
            this._rng = new Random();

            this._printColors = (Enum.GetValues(typeof(ConsoleColor)) as ConsoleColor[]).Where(color => color != ConsoleColor.Black).ToList();
            var chosenRule = Rules1D.RuleMethods.Where(fn => fn.Name.Contains(rule)).First();
            this._rule = (Rules1D.RuleDelegate)Delegate.CreateDelegate(typeof(Rules1D.RuleDelegate), chosenRule);
            this._ruleName = rule;
            Rules1D.IsRuleDictInitialized = false;
        }

        public static Automata1D InitializeAutomata(int rowMax, int colMax, BuildTypes bType, string rType)
        {
            var newAutomata1D = new Automata1D(rowMax, colMax, rType);
            //This is really gross but it prompts users for input on lines 15, 16 and 17 for the range and hex value of a custom 1d rule
            //This also lets the user type all willy nilly and can mess up the nice border but oh well for now
            if (rType == "Custom")
            {
                MenuHelper.PrintOnLine(15, "Select Range");
                var tempRange = MenuHelper.PromptOnLine(16, "[1-4]: ");
                int range;
                while (!MenuHelper.ValidRangeInput(tempRange, out range))
                {
                    MenuHelper.PrintOnLine(17, "Try again");
                    tempRange = MenuHelper.PromptOnLine(16, "[1-4]: ");
                }
                MenuHelper.ClearWithinBorder(17);
                MenuHelper.PrintOnLine(15, "Enter a hex string");
                var tempHex = MenuHelper.PromptOnLine(16, "the hex: ");
                string hex = String.Empty;
                while (!MenuHelper.ValidHexInput(tempHex, out hex))
                {
                    MenuHelper.PrintOnLine(17, "thats a bad hex");
                    tempHex = MenuHelper.PromptOnLine(16, "the hex: ");
                }
                //Clear the inside of the border once we get some valid values
                MenuHelper.ClearAllInBorder();
                var ruleStr = "R" + range + ",W" + hex;
                Rules1D.UserRule = ruleStr;
                newAutomata1D._ruleName = ruleStr;
            }
            switch (bType)
            {
                case BuildTypes.Random:
                    newAutomata1D.Build1DBoard_Random();
                    break;
                case BuildTypes.Single:
                    newAutomata1D.Build1DBoard_Single();
                    break;
            }
            newAutomata1D.Is_Initialized = true;
            MenuHelper.PrintOnLine(2, newAutomata1D._ruleName.Replace('_', ' '));
            return newAutomata1D;
        }

        /// <summary>
        /// Function to calculate the next value for all the cells in this.Current_Row
        /// using this.__Rule
        /// </summary>
        public override void NextGeneration()
        {
            var nextRow = new bool[this.Cols];
            for (int i = 0; i < Cols; i++)
            {
                nextRow[(i + this.Cols) % this.Cols] = this._rule(this._currentRow, i);
            }

            //Shift the entire board up if it is already filled, and place this new row
            //at the bottom
            if (this._printRow >= this.Rows)
            {
                this._entireBoard = GenericHelp<bool>.ShiftUp(this._entireBoard);
                this._entireBoard[(this.Rows - 1)] = nextRow;
            }
            else
            {
                this._entireBoard[this._printRow] = nextRow;
            }

            this._currentRow = nextRow;
        }

        /// <summary>
        /// Prints the automata rule within the boarders of the console
        /// </summary>
        public override void PrintBoard()
        {
            if (this._printRow >= this.Rows)
            {
                //If we are at the number of rows, we need to shift everything up
                //by one except the first row and then continue printing and the bottom
                //of the screen
                Console.MoveBufferArea(MenuHelper.Space, MenuHelper.Space + 1, this.Cols, this.Rows - 1, MenuHelper.Space, MenuHelper.Space);
                this._printRow--;
            }
            Console.SetCursorPosition(MenuHelper.Space, MenuHelper.Space + this._printRow);
            var printRow = new StringBuilder();
            foreach (bool val in this._currentRow)
            {
                if (val)
                    printRow.Append(_liveCell);
                else
                    printRow.Append(_deadCell);
            }

            Console.ForegroundColor = this._printColors[this._rng.Next(this._printColors.Count)];
            Console.Write(printRow);
            this._printRow++;
        }

        /// <summary>
        /// Builds a random initial board
        /// </summary>
        private void Build1DBoard_Random()
        {
            for (int i = 0; i < this.Cols; i++)
            {
                this._currentRow[i] = (this._rng.Next() % 2 == 0);
            }
            this._entireBoard[0] = this._currentRow;
        }

        private void Build1DBoard_Single()
        {
            this._currentRow[this.Cols / 2] = true;
            this._entireBoard[0] = this._currentRow;
        }
    }
}