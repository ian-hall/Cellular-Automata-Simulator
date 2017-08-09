using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GHGameOfLife.Rules
{
    /// <summary>
    /// 1D Automata Rules (http://atlas.wolfram.com/TOC/TOC_200.html)
    /// </summary>
    class Rules1D
    {
        private static Dictionary<string, bool> RuleDict;
        private static RuleDelegate RandomRule;
        private static Random RNG = new Random();
        private static string CustomRule = String.Empty;
        public static bool RuleDict_Initialized = false;
        public delegate bool RuleDelegate(bool[] row, int col);
        public static string UserRule
        {
            get
            {
                return Rules1D.CustomRule;
            }
            set
            {
                Rules1D.CustomRule = value;
            }
        }
        public static IEnumerable<System.Reflection.MethodInfo> RuleMethods
        {
            get
            {
                return typeof(Rules1D).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Where(fn => fn.Name.StartsWith("Rule_"));
            }
        }
        public static string[] RuleNames
        {
            get
            {
                //5 because that is the length of "Rule_" prefix on rule stuffs
                return RuleMethods.Select(fn => fn.Name.Substring(5)).OrderBy(s => s).ToArray(); //The names of the methods
            }
        }
        //-----------------------------------------------------------------------------
        public static bool Rule_Rule_90(bool[] currentRow, int col)
        {
            var neighbors = GetNeighbors(currentRow,col);
            return neighbors["P"] ^ neighbors["R"];
        }
        //-----------------------------------------------------------------------------
        public static bool Rule_Rule_30(bool[] currentRow, int col)
        {
            var neighbors = GetNeighbors(currentRow, col);
            return neighbors["P"] ^ (neighbors["Q"] | neighbors["R"]);
        }
        //-----------------------------------------------------------------------------
        public static bool Rule_Rule_1(bool[] currentRow, int col)
        {
            var neighbors = GetNeighbors(currentRow, col);
            return !(neighbors["P"] | neighbors["Q"] | neighbors["R"]);
        }
        //-----------------------------------------------------------------------------
        public static bool Rule_Rule_73(bool[] currentRow, int col)
        {
            var neighbors = GetNeighbors(currentRow, col);
            return !(neighbors["P"] & neighbors["R"] | neighbors["P"] ^ neighbors["Q"] ^ neighbors["R"]);
        }
        //-----------------------------------------------------------------------------
        public static bool Rule_Rule_129(bool[] currentRow, int col)
        {
            var neighbors = GetNeighbors(currentRow, col);
            return !(neighbors["P"] ^ neighbors["Q"] | neighbors["P"] ^ neighbors["R"]);
        }
        //-----------------------------------------------------------------------------
        public static bool Rule_Rule_18(bool[] currentRow, int col)
        {
            var neighbors = GetNeighbors(currentRow, col);
            return (neighbors["P"] ^ neighbors["R"] ^ neighbors["Q"]) & !neighbors["Q"];
        }
        //-----------------------------------------------------------------------------
        public static bool Rule_Rule_193(bool[] currentRow, int col)
        {
            var neighbors = GetNeighbors(currentRow, col);
            return neighbors["P"] ^ (neighbors["P"] | neighbors["Q"] | !neighbors["R"]) ^ neighbors["Q"];
        }
        //-----------------------------------------------------------------------------
        public static bool Rule_Rule_94(bool[] currentRow, int col)
        {
            var neighbors = GetNeighbors(currentRow, col);
            return neighbors["P"] & neighbors["R"] ^ (neighbors["P"] | neighbors["Q"] | neighbors["R"]);
        }
        //-----------------------------------------------------------------------------
        public static bool Rule_Rule_57(bool[] currentRow, int col)
        {
            var neighbors = GetNeighbors(currentRow, col);
            return (neighbors["P"] | !neighbors["R"]) ^ neighbors["Q"];
        }
        //-----------------------------------------------------------------------------
        public static bool Rule_Bermuda_Triangle(bool[] currentRow, int col)
        {
            var ruleStr = "R2,WBC82271C";
            var range = int.Parse(ruleStr.Split(',')[0].Substring(1));
            if (!RuleDict_Initialized)
            {
                RuleDict = BuildRulesDict(ruleStr);
            }
            var neighborhood = GetNeighborsBinary(currentRow, col, range);
            return RuleDict[neighborhood];
        }
        //-----------------------------------------------------------------------------
        public static bool Rule_Glider_P168(bool[] currentRow, int col)
        {
            var ruleStr = "R2,W6C1E53A8";
            var range = int.Parse(ruleStr.Split(',')[0].Substring(1));
            if (!RuleDict_Initialized)
            {
                RuleDict = BuildRulesDict(ruleStr);
            }
            var neighborhood = GetNeighborsBinary(currentRow, col, range);
            return RuleDict[neighborhood];
        }
        //-----------------------------------------------------------------------------
        public static bool Rule_Inverted_Gliders(bool[] currentRow, int col)
        {
            var ruleStr = "R2,W360A96F9";
            var range = int.Parse(ruleStr.Split(',')[0].Substring(1));
            if (!RuleDict_Initialized)
            {
                RuleDict = BuildRulesDict(ruleStr);
            }
            var neighborhood = GetNeighborsBinary(currentRow, col, range);
            return RuleDict[neighborhood];
        }
        //-----------------------------------------------------------------------------
        public static bool Rule_Fish_Bones(bool[] currentRow, int col)
        {
            var ruleStr = "R2,W5F0C9AD8";
            var range = int.Parse(ruleStr.Split(',')[0].Substring(1));
            if (!RuleDict_Initialized)
            {
                RuleDict = BuildRulesDict(ruleStr);
            }
            var neighborhood = GetNeighborsBinary(currentRow, col, range);
            return RuleDict[neighborhood];
        }
        //-----------------------------------------------------------------------------
        public static bool Rule_R3_Glider(bool[] currentRow, int col)
        {
            var ruleStr = "R3,W3B469C0EE4F7FA96F93B4D32B09ED0E0";
            var range = int.Parse(ruleStr.Split(',')[0].Substring(1));
            if (!RuleDict_Initialized)
            {
                RuleDict = BuildRulesDict(ruleStr);
            }
            var neighborhood = GetNeighborsBinary(currentRow, col, range);
            return RuleDict[neighborhood];
        }
        //-----------------------------------------------------------------------------
        public static bool Rule_Custom(bool[] currentRow, int col)
        {
            //Should maybe sanity check that the CustomRule string is set and, if it isn't, put some default 
            //value in so we dont just error out. Maybe have it change to a new rule and reset the rulesdict
            var range = int.Parse(Rules1D.CustomRule.Split(',')[0].Substring(1));
            if (!RuleDict_Initialized)
            {
                RuleDict = BuildRulesDict(Rules1D.CustomRule);
            }
            var neighborhood = GetNeighborsBinary(currentRow, col, range);
            return RuleDict[neighborhood];
        }
        //-----------------------------------------------------------------------------
        public static bool Rule_Random(bool[] currentRow, int col)
        {
            // Need to keep this last so the randomRule thing below doesn't include it.
            //TODO: temp 33 just to see when new rule is chosen
            //      can't use col == 0 unless i want a whole lot of nothing
            if(col%33 == 0)
            {
                RuleDict_Initialized = false;
                // magic number 1 because Rule_Random is last and we don't want some kind of horrible recursion happening
                var chosen = RuleMethods.Take(RuleMethods.Count() - 1).ElementAt(RNG.Next(RuleMethods.Count() - 1));
                RandomRule = (RuleDelegate)Delegate.CreateDelegate(typeof(RuleDelegate), chosen);
            }
            return RandomRule(currentRow, col);
        }
        //-----------------------------------------------------------------------------
        #region Helpers
        /// <summary>
        /// Gets the neighboring values of the given column.
        /// </summary>
        /// <param name="col">Column the neighbors are centered on</param>
        /// <param name="range">How far to go from the center, default value of 1</param>
        /// <returns>Dictionary indexed by "P", "Q", and "R". "P" (left) and "R" (right) are repeated based on the number of spaces away from the center, "Q".
        ///          For example, with range=2, the keys would be PP, P, Q, R, RR representing col-2 col-1 col col+1 col+2
        /// </returns>
        private static Dictionary<string, bool> GetNeighbors(bool[] currentRow, int col, int range = 1)
        {
            range = Math.Abs(range);
            var maxCols = currentRow.Length;
            var neighbors = new Dictionary<string, bool>();
            for (int n = range * -1; n <= range; n++)
            {
                var keyBuilder = new StringBuilder();
                if (n < 0)
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
                neighbors[keyBuilder.ToString()] = currentRow[((col + n) + maxCols) % maxCols];
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
        private static string GetNeighborsBinary(bool[] currentRow, int col, int range = 1)
        {
            range = Math.Abs(range);
            var maxCols = currentRow.Length;
            var sb = new StringBuilder();
            for (int n = range * -1; n <= range; n++)
            {
                if(currentRow[((col + n) + maxCols) % maxCols])
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
        /// <param name="ruleStr">the rulestr of form "R1,FFF" or whatever</param>
        /// <returns>Returns a dictionary containing the binary rules</returns>
        private static Dictionary<string, bool> BuildRulesDict(string ruleStr)
        {
            var range = int.Parse(ruleStr.Split(',')[0].Substring(1));
            var hex = ruleStr.Split(',')[1].Substring(1);

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
                rule[currRule] = (val == '1') /*? true : false*/;
            }
            RuleDict_Initialized = true;
            return rule;
        }
        //-----------------------------------------------------------------------------
        /// <summary>
        /// Converts a string of hex to a string of binary.
        /// </summary>
        /// <param name="hex">a string of hex</param>
        /// <returns>a string of binary</returns>
        private static string HexToBin(string hex)
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
            foreach (var c in hex)
            {
                sb.Append(lol[c]);
            }
            return sb.ToString();
        }
        //-----------------------------------------------------------------------------
        /// <summary>
        /// Checks if a given string is hex
        /// </summary>
        /// <param name="s">a string</param>
        /// <returns>if the string is a hex</returns>
        private static bool IsValidHex(string s)
        {
            return s.All(c => "0123456789ABCDEF".Contains(c));
        }
        //-----------------------------------------------------------------------------
        #endregion Helpers
    }
}
