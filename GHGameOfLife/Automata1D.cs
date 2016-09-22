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
        //TODO: Finish adding support for this stuff: http://psoup.math.wisc.edu/mcell/rullex_1dbi.html
        delegate bool Rule1D(int col);
        public enum BuildTypes { Random, Single };
        public enum RuleTypes {Rule_1, Rule_18, Rule_30, Rule_57, Rule_73, Rule_90, Rule_94,
                               Rule_129, Rule_193, Bermuda_Triangle, Fish_Bones, Glider_P168, R3_Gliders };

        private bool[] Current_Row;
        private bool[][] Entire_Board;
        private const char LIVE_CELL = '█';
        private const char DEAD_CELL = ' ';
        private int Print_Row;
        private Rule1D Rule;
        private List<ConsoleColor> Print_Colors;
        private Random Rng;

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
//-----------------------------------------------------------------------------
        private Automata1D(int rowMax, int colMax, RuleTypes rule) : base(rowMax,colMax)
        {
            this.Print_Row = 0;
            this.Current_Row = new bool[this.Cols];
            this.Entire_Board = new bool[this.Rows][];
            this.Rng = new Random();

            var allColors = Enum.GetValues(typeof(ConsoleColor));
            this.Print_Colors = new List<ConsoleColor>();
            foreach(ConsoleColor color in allColors)
            {
                if(color != ConsoleColor.Black)
                {
                    this.Print_Colors.Add(color);
                }
            }

            switch(rule)
            {
                case RuleTypes.Rule_30:
                    this.Rule = Rule30;
                    break;
                case RuleTypes.Rule_90:
                    this.Rule = Rule90;
                    break;
                case RuleTypes.Rule_1:
                    this.Rule = Rule1;
                    break;
                case RuleTypes.Rule_73:
                    this.Rule = Rule73;
                    break;
                case RuleTypes.Rule_129:
                    this.Rule = Rule129;
                    break;
                case RuleTypes.Rule_18:
                    this.Rule = Rule18;
                    break;
                case RuleTypes.Rule_193:
                    this.Rule = Rule193;
                    break;
                case RuleTypes.Rule_94:
                    this.Rule = Rule94;
                    break;
                case RuleTypes.Rule_57:
                    this.Rule = Rule57;
                    break;
                case RuleTypes.Bermuda_Triangle:
                    this.Rule = Bermuda_Triangle;
                    break;
                case RuleTypes.Fish_Bones:
                    this.Rule = Fish_Bones;
                    break;
                case RuleTypes.Glider_P168:
                    this.Rule = Glider_P168;
                    break;
                case RuleTypes.R3_Gliders:
                    this.Rule = R3_Glider;
                    break;
                default:
                    this.Rule = Rule90;
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
            Console.BackgroundColor = MenuHelper.Default_BG;
            Console.ForegroundColor = MenuHelper.Board_FG;

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

            Console.ForegroundColor = this.Print_Colors[this.Rng.Next(this.Print_Colors.Count)];
            Console.Write(printRow);
            this.Print_Row++;

            Console.BackgroundColor = MenuHelper.Default_BG;
            Console.ForegroundColor = MenuHelper.Default_FG;
        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Builds a random initial board
        /// </summary>
        private void Build1DBoard_Random()
        {
            for (int i = 0; i < this.Cols; i++)
            {
                this.Current_Row[i] = (this.Rng.Next() % 2 == 0);
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
        private bool Rule90(int col)
        {
            var neighbors = GetNeighbors(col);
            return neighbors["P"] ^ neighbors["R"];
        }
//-----------------------------------------------------------------------------
        private bool Rule30(int col)
        {
            var neighbors = GetNeighbors(col);
            return neighbors["P"] ^ (neighbors["Q"] | neighbors["R"]);
        }
//-----------------------------------------------------------------------------
        private bool Rule1(int col)
        {
            var neighbors = GetNeighbors(col);
            return !(neighbors["P"] | neighbors["Q"] | neighbors["R"]);
        }
//-----------------------------------------------------------------------------
        private bool Rule73(int col)
        {
            var neighbors = GetNeighbors(col);
            return !(neighbors["P"] & neighbors["R"] | neighbors["P"] ^ neighbors["Q"] ^ neighbors["R"]);
        }
//-----------------------------------------------------------------------------
        private bool Rule129(int col)
        {
            var neighbors = GetNeighbors(col);
            return !(neighbors["P"] ^ neighbors["Q"] | neighbors["P"] ^ neighbors["R"]);
        }
//-----------------------------------------------------------------------------
        private bool Rule18(int col)
        {
            var neighbors = GetNeighbors(col);
            return (neighbors["P"] ^ neighbors["R"] ^ neighbors["Q"]) & !neighbors["Q"];
        }
//-----------------------------------------------------------------------------
        private bool Rule193(int col)
        {
            var neighbors = GetNeighbors(col);
            return neighbors["P"] ^ (neighbors["P"] | neighbors["Q"] | !neighbors["R"]) ^ neighbors["Q"];
        }
//-----------------------------------------------------------------------------
        private bool Rule94(int col)
        {
            var neighbors = GetNeighbors(col);
            return neighbors["P"] & neighbors["R"] ^ (neighbors["P"] | neighbors["Q"] | neighbors["R"]);
        }
//-----------------------------------------------------------------------------
        private bool Rule57(int col)
        {
            var neighbors = GetNeighbors(col);
            return (neighbors["P"] | !neighbors["R"]) ^ neighbors["Q"];
        }
//-----------------------------------------------------------------------------
        private bool Bermuda_Triangle(int col)
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
        private bool Fish_Bones(int col)
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
        private bool Glider_P168(int col)
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
        private bool R3_Glider(int col)
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
