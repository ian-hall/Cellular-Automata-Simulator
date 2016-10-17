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
        //TODO: Maybe convert all rules to the new style
        //      Also maybe abstract some stuff out so im not repeating all the code for checking if the RuleDict is set
        private delegate bool Rule1D(int col);
        public enum BuildTypes { Random, Single };

        private bool[] Current_Row;
        private bool[][] Entire_Board;
        private const char LIVE_CELL = '█';
        private const char DEAD_CELL = ' ';
        private int Print_Row;
        private Rule1D Rule;
        private List<ConsoleColor> Print_Colors;
        private Random RNG;

        private Dictionary<string, bool> RuleDict;
        private bool RuleDict_Initialized = false;

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
        private static IEnumerable<System.Reflection.MethodInfo> RuleMethods
        {
            get
            {
                return typeof(Automata1D).GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Where(fn => fn.Name.StartsWith("Rule_"));
            }
        }
        public static string[] RuleNames
        {
            get
            {
                //5 because that is the length of "Rule_" prefix on rule stuffs
                return RuleMethods.Select(fn => fn.Name.Substring(5)).ToArray(); //The names of the methods
            }
        }
//-----------------------------------------------------------------------------
        private Automata1D(int rowMax, int colMax, string rule) : base(rowMax,colMax)
        {
            this.Print_Row = 0;
            this.Current_Row = new bool[this.Cols];
            this.Entire_Board = new bool[this.Rows][];
            this.RNG = new Random();

            this.Print_Colors = (Enum.GetValues(typeof(ConsoleColor)) as ConsoleColor[]).Where(color => color != ConsoleColor.Black).ToList();
            var chosenRule = RuleMethods.Where(fn => fn.Name.Contains(rule)).First();
            this.Rule = (Rule1D)Delegate.CreateDelegate(typeof(Rule1D), this, chosenRule);
        }
//-----------------------------------------------------------------------------
        public static Automata1D InitializeAutomata(int rowMax, int colMax, BuildTypes bType, string rType)
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
            newAutomata1D.Is_Initialized = true;
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
                nextRow[(i + this.Cols) % this.Cols] = this.Rule(i);
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
            //Console.BackgroundColor = MenuHelper.Default_BG;
            //Console.ForegroundColor = MenuHelper.Board_FG;

            if( this.Print_Row >= this.Rows )
            {
                //If we are at the number of rows, we need to shift everything up
                //by one except the first row and then continue printing and the bottom
                //of the screen
                Console.MoveBufferArea(MenuHelper.Space, MenuHelper.Space+1, this.Cols, this.Rows-1, MenuHelper.Space, MenuHelper.Space);
                --this.Print_Row;
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
//  Automata Rules (http://atlas.wolfram.com/TOC/TOC_200.html)
//-----------------------------------------------------------------------------
        private bool Rule_Rule_90(int col)
        {
            var neighbors = GetNeighbors(col);
            return neighbors["P"] ^ neighbors["R"];
        }
//-----------------------------------------------------------------------------
        private bool Rule_Rule_30(int col)
        {
            var neighbors = GetNeighbors(col);
            return neighbors["P"] ^ (neighbors["Q"] | neighbors["R"]);
        }
//-----------------------------------------------------------------------------
        private bool Rule_Rule_1(int col)
        {
            var neighbors = GetNeighbors(col);
            return !(neighbors["P"] | neighbors["Q"] | neighbors["R"]);
        }
//-----------------------------------------------------------------------------
        private bool Rule_Rule_73(int col)
        {
            var neighbors = GetNeighbors(col);
            return !(neighbors["P"] & neighbors["R"] | neighbors["P"] ^ neighbors["Q"] ^ neighbors["R"]);
        }
//-----------------------------------------------------------------------------
        private bool Rule_Rule_129(int col)
        {
            var neighbors = GetNeighbors(col);
            return !(neighbors["P"] ^ neighbors["Q"] | neighbors["P"] ^ neighbors["R"]);
        }
//-----------------------------------------------------------------------------
        private bool Rule_Rule_18(int col)
        {
            var neighbors = GetNeighbors(col);
            return (neighbors["P"] ^ neighbors["R"] ^ neighbors["Q"]) & !neighbors["Q"];
        }
//-----------------------------------------------------------------------------
        private bool Rule_Rule_193(int col)
        {
            var neighbors = GetNeighbors(col);
            return neighbors["P"] ^ (neighbors["P"] | neighbors["Q"] | !neighbors["R"]) ^ neighbors["Q"];
        }
//-----------------------------------------------------------------------------
        private bool Rule_Rule_94(int col)
        {
            var neighbors = GetNeighbors(col);
            return neighbors["P"] & neighbors["R"] ^ (neighbors["P"] | neighbors["Q"] | neighbors["R"]);
        }
//-----------------------------------------------------------------------------
        private bool Rule_Rule_57(int col)
        {
            var neighbors = GetNeighbors(col);
            return (neighbors["P"] | !neighbors["R"]) ^ neighbors["Q"];
        }
//-----------------------------------------------------------------------------
        private bool Rule_Bermuda_Triangle(int col)
        {
            var ruleStr = "R2,WBC82271C";
            var range = int.Parse(ruleStr.Split(',')[0].Substring(1));
            var hex = ruleStr.Split(',')[1].Substring(1);
            if (!RuleDict_Initialized)
            {
                this.RuleDict = BuildRulesDict(hex, range);
                this.RuleDict_Initialized = true;
            }
            var neighborhood = GetNeighborsBinary(col, range);
            return this.RuleDict[neighborhood];
        }
//-----------------------------------------------------------------------------
        private bool Rule_Fish_Bones(int col)
        {
            var ruleStr = "R2,W5F0C9AD8";
            var range = int.Parse(ruleStr.Split(',')[0].Substring(1));
            var hex = ruleStr.Split(',')[1].Substring(1);
            if (!RuleDict_Initialized)
            {
                this.RuleDict = BuildRulesDict(hex, range);
                this.RuleDict_Initialized = true;
            }
            var neighborhood = GetNeighborsBinary(col, range);
            return this.RuleDict[neighborhood];
        }
//-----------------------------------------------------------------------------
        private bool Rule_Glider_P168(int col)
        {
            var ruleStr = "R2,W6C1E53A8";
            var range = int.Parse(ruleStr.Split(',')[0].Substring(1));
            var hex = ruleStr.Split(',')[1].Substring(1);
            if (!RuleDict_Initialized)
            {
                this.RuleDict = BuildRulesDict(hex, range);
                this.RuleDict_Initialized = true;
            }
            var neighborhood = GetNeighborsBinary(col, range);
            return this.RuleDict[neighborhood];
        }
//-----------------------------------------------------------------------------
        private bool Rule_R3_Glider(int col)
        {
            var ruleStr = "R3,W3B469C0EE4F7FA96F93B4D32B09ED0E0";
            var range = int.Parse(ruleStr.Split(',')[0].Substring(1));
            var hex = ruleStr.Split(',')[1].Substring(1);
            if (!RuleDict_Initialized)
            {
                this.RuleDict = BuildRulesDict(hex, range);
                this.RuleDict_Initialized = true;
            }
            var neighborhood = GetNeighborsBinary(col, range);
            return this.RuleDict[neighborhood];
        }
//-----------------------------------------------------------------------------
        private bool Rule_Inverted_Gliders(int col)
        {
            var ruleStr = "R2,W360A96F9";
            var range = int.Parse(ruleStr.Split(',')[0].Substring(1));
            var hex = ruleStr.Split(',')[1].Substring(1);
            if (!RuleDict_Initialized)
            {
                this.RuleDict = BuildRulesDict(hex, range);
                this.RuleDict_Initialized = true;
            }
            var neighborhood = GetNeighborsBinary(col, range);
            return this.RuleDict[neighborhood];
        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Gets the neighboring values of the given column.
        /// </summary>
        /// <param name="col">Column the neighbors are centered on</param>
        /// <param name="range">How far to go from the center, default value of 1</param>
        /// <returns>Dictionary indexed by "P", "Q", and "R". "P" (left) and "R" (right) are repeated based on the number of spaces away from the center, "Q".
        ///          For example, with range=2, the keys would be PP, P, Q, R, RR representing col-2 col-1 col col+1 col+2
        /// </returns>
        private Dictionary<string,bool> GetNeighbors(int col, int range = 1)
        {
            range = Math.Abs(range);
            var neighbors = new Dictionary<string, bool>();
            for( int n = range * -1; n <= range; n++ )
            {
                var keyBuilder = new StringBuilder();
                if(n < 0 )
                {
                    foreach (int i in Enumerable.Range(0, Math.Abs(n)))
                    {
                        keyBuilder.Append("P");
                    }
                }
                else if (n == 0)
                {
                    keyBuilder.Append("Q");
                }
                else
                {
                    foreach (int i in Enumerable.Range(0, Math.Abs(n)))
                    {
                        keyBuilder.Append("R");
                    }
                }
                neighbors[keyBuilder.ToString()] = this.Current_Row[((col + n) + this.Cols) % this.Cols];
            }
            return neighbors;
        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Function that returns a binary string representing the values of a neighborhood with the given range.
        /// </summary>
        /// <param name="col">center of the neighborhood</param>
        /// <param name="range">how far from the center to go</param>
        /// <returns>Binary string representing the neighborhood.</returns>
        private string GetNeighborsBinary(int col, int range)
        {
            range = Math.Abs(range);
            var sb = new StringBuilder();
            for (int n = range * -1; n <= range; n++)
            {
                if (this.Current_Row[((col + n) + this.Cols) % this.Cols])
                {
                    sb.Append('1');
                }
                else
                {
                    sb.Append('0');
                }
            }
            return sb.ToString();
        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Returns a dictionary with keys being binary strings and values being a boolean representing the next state of the center bit in the bin string.
        /// </summary>
        /// <param name="hex">A hex string representing the wolfram code of an automata rule.</param>
        /// <param name="range">The neighborhood size that the rule is based on.</param>
        /// <returns></returns>
        private Dictionary<string,bool> BuildRulesDict(string hex, int range)
        {
            var numNeighbors = 1 + 2 * range;
            var numRuleValues = (int)Math.Pow(2, numNeighbors);
            var binList = new List<string>();
            for (int i = 0; i < numRuleValues; i++)
            {
                binList.Add(Convert.ToString(i, 2).PadLeft(numNeighbors, '0'));
            }
            var ruleVals = HexToBin(hex).PadLeft(numRuleValues, '0'); ;
            var rule = new Dictionary<string, bool>();
            for (int i = 0; i < numRuleValues; i++)
            {
                var currRule = binList[numRuleValues - 1 - i];
                var val = ruleVals[i];
                rule[currRule] = (val == '1') ? true : false;
            }
            return rule;
        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Converts a string of hex to a string of binary.
        /// </summary>
        /// <param name="hex">a string of hex</param>
        /// <returns>a string of binary</returns>
        private string HexToBin(string hex)
        {
            //kill me
            var lol = new Dictionary<char, string>()
            {
                {'0',"0000" },
                {'1',"0001" },
                {'2',"0010" },
                {'3',"0011" },
                {'4',"0100" },
                {'5',"0101" },
                {'6',"0110" },
                {'7',"0111" },
                {'8',"1000" },
                {'9',"1001" },
                {'A',"1010" },
                {'B',"1011" },
                {'C',"1100" },
                {'D',"1101" },
                {'E',"1110" },
                {'F',"1111" }
            };
            var sb = new StringBuilder();
            foreach(var c in hex)
            {
                sb.Append(lol[c]);
            }
            return sb.ToString();
        }
//-----------------------------------------------------------------------------
    }
///////////////////////////////////////////////////////////////////////////////
}