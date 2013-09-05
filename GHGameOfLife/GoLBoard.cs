using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace GameOfLife
{
    /// <summary>
    /// This class pretty much does everything. It sets up the console, 
    /// fills in the initial pop from a file or randomly, and then 
    /// does all the checking for living/dying of the population.
    /// </summary>
    class GoLBoard
    {
        private int[,] _Board;
        private int _RowsUsed;
        private int _ColsUsed;
        private int _Generation;
        private const char _LiveCell = '☻';
//------------------------------------------------------------------------------
        /// <summary>
        /// Constructor for the GoLBoard class. Size of the board will be based
        /// off the size of the console window...
        /// </summary>
        /// <param name="rowMax"></param>
        /// <param name="colMax"></param>
        public GoLBoard(int rowMax, int colMax)
        {
            if (rowMax < 20 || colMax < 20)
            {
                Console.WriteLine("Must be 20x20 or larger");
                Environment.Exit(1);
            }

            int[,] tempBoard = new int[rowMax, colMax];
            _Board = new int[rowMax, colMax];
            for (int r = 0; r < rowMax; r++)
            {
                for (int c = 0; c < colMax; c++)
                    tempBoard[r, c] = 0;
            }

            for (int r = 0; r < _RowsUsed; r++)
                for (int c = 0; c < _ColsUsed; c++)
                    _Board[r, c] = tempBoard[r, c];

            _RowsUsed = rowMax;
            _ColsUsed = colMax;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Default population is a random spattering of 0s and 1s
        /// Easy enough to get using (random int) % 2
        /// </summary>
        public void BuildDefaultPop() 
        {
            Random gen = new Random();
            for (int r = 0; r < _RowsUsed; r++)
            {
                for (int c = 0; c < _ColsUsed; c++)
                {
                    _Board[r, c] = gen.Next()%2;
                }
            }
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Load the initial population from a file of 0s and 1s.
        /// This uses a Windows Forms OpenFileDialog to let the user select
        /// a file. The file is loaded into the center of the console window.
        /// </summary>
        public void BuildFromFile()
        {
            OpenFileDialog openWindow = new OpenFileDialog();
            if (openWindow.ShowDialog() == DialogResult.OK && ValidFile(openWindow.FileName))
            {
                StreamReader reader = new StreamReader(openWindow.FileName);
                var rows = File.ReadLines(openWindow.FileName).Count();
                int[][] startingPop = new int[rows][];

                int currRow = 0;
                while (!reader.EndOfStream)
                {
                    String currLine = reader.ReadLine();
                    int[] temp = new int[currLine.Length];
                    for (int i = 0; i < temp.Length; i++)
                    {
                        temp[i] = (int)char.GetNumericValue(currLine[i]);
                    }
                    startingPop[currRow] = temp;
                    currRow++;
                }

                /* Because the loaded population is centered we need
                 * to do some math to keep it centered relative to the
                 * total size of the board.
                 */ 
                int midRow = _RowsUsed / 2;
                int midCol = _ColsUsed / 2;

                int rowsNum = startingPop.Length;
                int colNum = startingPop[0].Length;
                int rowLow, rowHigh, colLow, colHigh;

                if (rowsNum % 2 == 0)
                {
                    rowLow = midRow - rowsNum / 2;
                    rowHigh = midRow + rowsNum / 2;
                }
                else
                {
                    rowLow = midRow - rowsNum / 2;
                    rowHigh = (midRow + rowsNum / 2) + 1;
                }


                if (colNum % 2 == 0)
                {
                    colLow = midCol - colNum / 2;
                    colHigh = midCol + colNum / 2;
                }
                else
                {
                    colLow = midCol - colNum / 2;
                    colHigh = (midCol + colNum / 2) + 1;
                }
                

                for (int r = rowLow; r < rowHigh; r++)
                {
                    for (int c = colLow; c < colHigh; c++)
                    {
                        int popRow = r - rowLow;
                        int popCol = c - colLow;
                        _Board[r, c] = startingPop[popRow][popCol];
                    }
                }
                reader.Close();
            }
            else
            {
                int windowCenter = Console.WindowHeight / 2;
                int welcomeLeft = (Console.WindowWidth / 2) - (MenuEntries.Welcome.Length / 2);
                int distToBorder = (Console.WindowWidth - 5) - welcomeLeft;

                //Clear the selection...
                Console.SetCursorPosition(welcomeLeft, windowCenter);
                Console.Write("".PadRight(distToBorder));

                Console.SetCursorPosition(welcomeLeft, windowCenter-1);
                Console.Write(MenuEntries.FileError1);
                Console.SetCursorPosition(welcomeLeft, windowCenter);
                Console.Write(MenuEntries.FileError2);
                Console.SetCursorPosition(welcomeLeft, windowCenter+1);
                Console.Write(MenuEntries.Enter);


                while (true)
                {
                    char c = Console.ReadKey().KeyChar;
                    Console.SetCursorPosition(welcomeLeft + MenuEntries.Enter.Length, windowCenter + 1);
                    Console.Write(" ");
                    if (c == '\r')
                    {
                        break;
                    }
                    else
                    {
                        //Keeps the cursor in place until ENTER is pressed
                        Console.SetCursorPosition(welcomeLeft + MenuEntries.Enter.Length, windowCenter + 1);
                    }
                }
 
                BuildDefaultPop();
            }

        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Updates the board for the next generation of peoples
        /// </summary>
        public void Next()
        {
            int[,] nextBoard = new int[_RowsUsed, _ColsUsed];

            for (int r = 0; r < _RowsUsed; r++)
            {
                for (int c = 0; c < _ColsUsed; c++)
                {
                    if (_Board[r, c] == 0)
                    {
                        if (WillBeBorn(r, c)) nextBoard[r, c] = 1;
                        else nextBoard[r, c] = 0;
                    }

                    if (_Board[r, c] == 1)
                    {
                        if (WillDie(r, c)) nextBoard[r, c] = 0;
                        else nextBoard[r, c] = 1;
                    }
                }
            }
            _Generation++;
            for (int r = 0; r < _RowsUsed; r++)
                for (int c = 0; c < _ColsUsed; c++)
                    _Board[r, c] = nextBoard[r, c];

        }
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
        /// <summary>
        /// Displays the board in the console. It is centered in the console
        /// with a space of 5 on all sides. A border displays around the board.
        /// </summary>
        public void Print()
        {
            int space = 5;
            if (_Generation == 0)
            {
                String write = "Starting population...";
                int left = (Console.WindowWidth/2) - (write.Length/2);
                Console.SetCursorPosition(left, 1);
                Console.Write(write);
            }
            else
            {
                Console.SetCursorPosition(0, 1);
                String clear = " ";
                Console.Write(clear.PadRight(Console.WindowWidth));
                String write = "Generation " + _Generation;
                int left = (Console.WindowWidth/2) - (write.Length / 2);
                Console.SetCursorPosition(left, 1);
                Console.Write(write);
            }

            Console.BackgroundColor = MenuEntries.DefaultBG;         
            int rowStart = space;

            Console.SetCursorPosition(space, rowStart);
            for (int r = 0; r < _RowsUsed; r++)
            {
                for (int c = 0; c < _ColsUsed; c++)
                {
                    int check = _Board[r, c];
                    if (check == 0)
                    {                        
                        Console.Write(' ');
                    }
                    else
                    {
                        Console.ForegroundColor = MenuEntries.PopColor;
                        Console.Write(_LiveCell);
                    }
                }
                rowStart++;
                Console.SetCursorPosition(space, rowStart);
            }

            Console.BackgroundColor = MenuEntries.DefaultBG;
            Console.ForegroundColor = MenuEntries.DefaultFG;    
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Calculates if the current dude at _Board[r,c] will die or not.
        /// If a dude has less than 2, or more than 3 neighbors that dude
        /// is dead next generation.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <returns>True if the current dude dies.</returns>
        public Boolean WillDie(int r, int c)
        {
            int n = 0;

            if (r != 0 && c != 0)
            {
                if (_Board[r - 1, c - 1] == 1) n++;
            }
            if (r != 0 && c != _ColsUsed - 1)
            {
                if (_Board[r - 1, c + 1] == 1) n++;
            }
            if (r != 0)
            {
                if (_Board[r - 1, c] == 1) n++;
            }
            if (r != _RowsUsed - 1 && c != 0)
            {
                if (_Board[r + 1, c - 1] == 1) n++;
            }
            if (c != 0)
            {
                if (_Board[r, c - 1] == 1) n++;
            }
            if (r != _RowsUsed - 1)
            {
                if (_Board[r + 1, c] == 1) n++;
            }
            if (c != _ColsUsed - 1)
            {
                if (_Board[r, c + 1] == 1) n++;
            }
            if (r != _RowsUsed - 1 && c != _ColsUsed - 1)
            {
                if (_Board[r + 1, c + 1] == 1) n++;
            }

            if (n < 2) return true;
            if (n > 3) return true;
            else return false;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Calculates if the current space at _Board[r,c] will become alive
        /// or not. If nothingness has exactly 3 neighbors it will become
        /// living next generation.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <returns>True if the miracle of life occurs.</returns>
        public Boolean WillBeBorn(int r, int c)
        {
            int n = 0;

            if (r != 0 && c != 0)
            {
                if (_Board[r - 1, c - 1] == 1) n++;
            }
            if (r != 0 && c != _ColsUsed - 1)
            {
                if (_Board[r - 1, c + 1] == 1) n++;
            }
            if (r != 0)
            {
                if (_Board[r - 1, c] == 1) n++;
            }
            if (r != _RowsUsed - 1 && c != 0)
            {
                if (_Board[r + 1, c - 1] == 1) n++;
            }
            if (c != 0)
            {
                if (_Board[r, c - 1] == 1) n++;
            }
            if (r != _RowsUsed - 1)
            {
                if (_Board[r + 1, c] == 1) n++;
            }
            if (c != _ColsUsed - 1)
            {
                if (_Board[r, c + 1] == 1) n++;
            }
            if (r != _RowsUsed - 1 && c != _ColsUsed - 1)
            {
                if (_Board[r + 1, c + 1] == 1) n++;
            }

            if (n == 3) return true;
            else return false;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Validates the selected file from the BuildFromFile() method.
        /// A Valid file is all 0s and 1s and does not have more rows or columns
        /// than the console window.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>True if it is a valid file</returns>
        private Boolean ValidFile(String filename)
        {
            StreamReader reader = new StreamReader(filename);
            var rows = File.ReadLines(filename).Count();
            if (rows >= _RowsUsed)
            {
                return false;
            }
            while (!reader.EndOfStream)
            {
                String currLine = reader.ReadLine();
                if (currLine.Length >= _ColsUsed)
                {
                    return false;
                }
                if (!OnesAndZerosOnly(currLine))
                {
                    return false;
                }

            }
            reader.Close();
            return true;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Makes sure there are only 1s and 0s in a given string, used to 
        /// validate the file loaded in BuildFromFile()
        /// </summary>
        /// <param name="s"></param>
        /// <returns>True if the string is 1s and 0s</returns>
        private Boolean OnesAndZerosOnly(String s)
        {
            try
            {
                for (int i = 0; i < s.Length; i++)
                {
                    int check = (int)Char.GetNumericValue(s[i]);
                    if (check == 1 || check == 0)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
//------------------------------------------------------------------------------
    }
}
