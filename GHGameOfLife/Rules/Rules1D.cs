using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHGameOfLife.Rules
{
    class Rules1D
    {
        private static Dictionary<string, bool> RuleDict;
        public static bool RuleDict_Initialized = false;
        public delegate bool TestDel(bool[] row, int col);
        public static IEnumerable<System.Reflection.MethodInfo> RuleMethods
        {
            get
            {
                return typeof(Rules1D).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).Where(fn => fn.Name.StartsWith("Rule_"));
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
        private static bool Rule_Glider_P168(bool[] currentRow, int col)
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
        private static bool Rule_Inverted_Gliders(bool[] currentRow, int col)
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
        /// <summary>
        /// Gets the neighboring values of the given column.
        /// </summary>
        /// <param name="col">Column the neighbors are centered on</param>
        /// <param name="range">How far to go from the center, default value of 1</param>
        /// <returns>Dictionary indexed by "P", "Q", and "R". "P" (left) and "R" (right) are repeated based on the number of spaces away from the center, "Q".
        ///          For example, with range=2, the keys would be PP, P, Q, R, RR representing col-2 col-1 col col+1 col+2
        /// </returns>
        private Dictionary<string, bool> GetNeighbors(bool[] currentRow, int col, int range = 1)
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
                rule[currRule] = (val == '1') ? true : false;
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
    }
}
