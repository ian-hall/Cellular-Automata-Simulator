using System.Collections.Generic;
using System.Linq;

namespace GHGameOfLife.Rules
{
    class Rules2D
    {
        public delegate bool RuleDelegate(int row, int col, bool[,] board);
        public static IEnumerable<System.Reflection.MethodInfo> RuleMethods
        {
            get
            {
                return typeof(Rules2D).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Where(fn => fn.Name.StartsWith("Rule_"));
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

        //------------------------------------------------------------------------------
        /// <summary>
        /// Game of Life rules.
        /// Live cells stay alive if they have 2 or 3 neighbors.
        /// Dead cells turn live if they have 3 neighbors.
        /// </summary>
        /// <param name="r">row of tile to check</param>
        /// <param name="c">col of tile to check</param>
        /// <returns></returns>
        public static bool Rule_Life(int r, int c, bool[,] board)
        {
            var n = CountNeighbors_Moore(r, c, board);

            if (board[r, c])
            {
                return ((n == 2) || (n == 3));
            }
            else
            {
                return (n == 3);
            }
        }
        //------------------------------------------------------------------------------
        /// <summary>
        /// Life without Death rules.
        /// Live cells always stay alive.
        /// Dead cells turn live if they have 3 neighbors.
        /// </summary>
        /// <param name="r">row of tile to check</param>
        /// <param name="c">col of tile to check</param>
        /// <returns></returns>
        public static bool Rule_Life_Without_Death(int r, int c, bool[,] board)
        {
            if (board[r, c])
            {
                return true;
            }

            var n = CountNeighbors_Moore(r, c, board);

            return n == 3;
        }
        //------------------------------------------------------------------------------
        /// <summary>
        /// Seeds rule
        /// Live cells always die
        /// Dead cells turn live if they have 2 neighbors
        /// </summary>
        /// <param name="r">row of tile to check</param>
        /// <param name="c">col of tile to check</param>
        /// <returns></returns>
        public static bool Rule_Seeds(int r, int c, bool[,] board)
        {
            if (board[r, c])
            {
                return false;
            }

            var n = CountNeighbors_Moore(r, c, board);

            return n == 2;
        }
        //------------------------------------------------------------------------------
        /// <summary>
        /// Replicator rules
        /// Live cells stay alive if they have 1,3,5,or 7 neighbors.
        /// Dead cells turn live if they have 1,3,5, or 7 neighbors.
        /// </summary>
        /// <param name="r">row of tile to check</param>
        /// <param name="c">col of tile to check</param>
        /// <returns></returns>
        public static bool Rule_Replicator(int r, int c, bool[,] board)
        {
            var n = CountNeighbors_Moore(r, c, board);

            return ((n == 1) || (n == 3) || (n == 5) || (n == 7));
        }
        //------------------------------------------------------------------------------
        /// <summary>
        /// DayAndNight rules
        /// Live cells stay alive if they have 3,4,6,7, or 8 neighbors.
        /// Dead cells turn live if they have 3,6,7, or 8 neighbors.
        /// </summary>
        /// <param name="r">row of tile to check</param>
        /// <param name="c">col of tile to check</param>
        /// <returns></returns>
        public static bool Rule_Day_And_Night(int r, int c, bool[,] board)
        {
            var n = CountNeighbors_Moore(r, c, board);

            if (board[r, c])
            {
                return ((n == 3) || (n == 4) || (n == 6) || (n == 7) || (n == 8));
            }
            else
            {
                return ((n == 3) || (n == 6) || (n == 7) || (n == 8));
            }
        }
        //------------------------------------------------------------------------------
        /// <summary>
        /// 34 Life rules.
        /// Live cells stay alive if they have 3 or 4 neighbors.
        /// Dead cells turn live if they have 3 or 4 neighbors.
        /// </summary>
        /// <param name="r">row of tile to check</param>
        /// <param name="c">col of tile to check</param>
        /// <returns></returns>
        public static bool Rule_Life_34(int r, int c, bool[,] board)
        {
            var n = CountNeighbors_Moore(r, c, board);

            return ((n == 3) || (n == 4));
        }
        //------------------------------------------------------------------------------
        /// <summary>
        /// Diamoeba rules
        /// Live cells stay alive if they have 5,6,7, or 8 neighbors.
        /// Dead cells turn live if they have 3,5,6,7, or 8 neighbors.
        /// </summary>
        /// <param name="r">row of tile to check</param>
        /// <param name="c">col of tile to check</param>
        /// <returns></returns>
        public static bool Rule_Diamoeba(int r, int c, bool[,] board)
        {
            var n = CountNeighbors_Moore(r, c, board);

            if (board[r, c])
            {
                return ((n == 5) || (n == 6) || (n == 7) || (n == 8));
            }
            else
            {
                return ((n == 3) || (n == 5) || (n == 6) || (n == 7) || (n == 8));
            }
        }
        //------------------------------------------------------------------------------
        /// <summary>
        /// Morley rules
        /// Live cells stay alive if they have 2,4, or 5 neighbors.
        /// Dead cells turn live if they have 3,6, or 8 neighbors.
        /// </summary>
        /// <param name="r">row of tile to check</param>
        /// <param name="c">col of tile to check</param>
        /// <returns></returns>
        public static bool Rule_Morley(int r, int c, bool[,] board)
        {
            var n = CountNeighbors_Moore(r, c, board);

            if (board[r, c])
            {
                return ((n == 2) || (n == 4) || (n == 5));
            }
            else
            {
                return ((n == 3) || (n == 6) || (n == 8));
            }
        }
        //------------------------------------------------------------------------------
        /// <summary>
        /// Counts number of true values in the Moore neighborhood of a point.
        /// </summary>
        /// <param name="r">Row value</param>
        /// <param name="c">Column value</param>
        /// <param name="range">How large the neighborhood is, default value of 1</param>
        /// <returns>number of neighbors</returns>
        private static int CountNeighbors_Moore(int r, int c, bool[,] board, int range = 1)
        {
            if (range < 1)
            {
                return board[r, c] ? 1 : 0;
            }

            int n = 0;
            for (int i = r - range; i <= r + range; i++)
            {
                for (int j = c - range; j <= c + range; j++)
                {
                    if (i == r && j == c)
                    {
                        continue;
                    }
                    var currRow = (i + board.GetLength(0)) % board.GetLength(0);
                    var currCol = (j + board.GetLength(1)) % board.GetLength(1);
                    if (board[currRow, currCol]) n++;
                }
            }

            return n;
        }
        //------------------------------------------------------------------------------
    }
}
