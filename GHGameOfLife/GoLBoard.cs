using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Drawing;

namespace GHGameOfLife
{
    /// <summary>
    /// This class pretty much does everything. It sets up the console, 
    /// fills in the initial pop from a file or randomly, and then 
    /// does all the checking for living/dying of the population.
    /// TODO: Change around constructors
    /// </summary>
    class GoLBoard
    {
        public bool _Initialized { get; private set; }

        private int[,] _Board;
        private int _RowsUsed;
        private int _ColsUsed;
        private int _Generation;
        private const char _LiveCell = '☺';
        //private const char _DeadCell = ' ';
        private Random rand = new Random();
//------------------------------------------------------------------------------
        /// <summary>
        /// Constructor for the GoLBoard class. Size of the board will be based
        /// off the size of the console window...
        /// </summary>
        /// <param name="rowMax">Number of rows</param>
        /// <param name="colMax">Number of columns</param>
        public GoLBoard(int rowMax, int colMax)
        {
            _Board = new int[rowMax, colMax];
            
            /*for (int r = 0; r < rowMax; r++)
            {
                for (int c = 0; c < colMax; c++)
                    _Board[r, c] = rand.Next() % 2;
            }*/
            
            _RowsUsed = rowMax;
            _ColsUsed = colMax;
            _Initialized = false;
        }
//------------------------------------------------------------------------------
        
        /// <summary>
        /// Default population is a random spattering of 0s and 1s
        /// Easy enough to get using (random int)%2
        /// </summary>
        public void BuildDefaultPop() 
        {
            for (int r = 0; r < _RowsUsed; r++)
            {
                for (int c = 0; c < _ColsUsed; c++)
                {
                    _Board[r, c] = rand.Next()%2;
                }
            }
            _Initialized = true;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Load the initial population from a file of 0s and 1s.
        /// This uses a Windows Forms OpenFileDialog to let the user select
        /// a file. The file is loaded into the center of the console window.
        /// </summary>
        public void BuildFromFile()
        {
            MenuText.FileError errType = MenuText.FileError.NOT_LOADED;

            OpenFileDialog openWindow = new OpenFileDialog();          
            if (openWindow.ShowDialog() == DialogResult.OK)
            {
                string filePath = openWindow.FileName;
                errType = ValidateFile(filePath);
            }
            else
            {   // No File loaded
                int windowCenter = Console.WindowHeight / 2; //Vert position
                int welcomeLeft = (Console.WindowWidth / 2) -
                                            (MenuText.Welcome.Length / 2);
                int distToBorder = (Console.WindowWidth - 5) - welcomeLeft;

                MenuText.ClearWithinBorder(windowCenter);

                Console.SetCursorPosition(welcomeLeft, windowCenter - 1);
                Console.Write(MenuText.FileErr1);
                Console.SetCursorPosition(welcomeLeft, windowCenter);
                Console.Write(MenuText.FileErr2);
                Console.SetCursorPosition(welcomeLeft, windowCenter + 1);
                Console.Write(MenuText.Enter);

                bool keyPressed = false;
                while (!keyPressed)
                {
                    if (!Console.KeyAvailable)
                        System.Threading.Thread.Sleep(50);
                    else
                    {
                        if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                            keyPressed = true;
                    }
                }
            }

            switch(errType)
            {
                case MenuText.FileError.NONE:
                    string startingPop;
                    using (StreamReader reader = new StreamReader(openWindow.FileName))
                        startingPop = reader.ReadToEnd();
                    fillBoard(startingPop);
                    break;
                default:
                    BuildDefaultPop();
                    break;
            }

            _Initialized = true;
            
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Builds the board from a resource
        /// TODO: Don't really need to validate built in stuff, but probably 
        /// need to add the ability to resize the window if for some reason
        /// it is set smaller than a preloaded population can display in.
        /// </summary>
        /// <param name="pop"></param>
        public void BuildFromResource(string pop)
        {                  
            var startingPop = GHGameOfLife.Pops.ResourceManager.GetString(pop);

            fillBoard(startingPop);

            _Initialized = true;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Used by files to fill the game board, cente
        /// </summary>
        /// <param name="startingPop"></param>
        private void fillBoard(string startingPop)
        {
            string[] popByLine = Regex.Split(startingPop, "\r\n");

            int midRow = _RowsUsed / 2;
            int midCol = _ColsUsed / 2;

            int rowsNum = popByLine.Count();
            int colNum = popByLine[0].Length;
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
                    _Board[r, c] = (int)Char.GetNumericValue(popByLine[popRow].ElementAt(popCol));
                }
            }
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Updates the board for the next generation of peoples
        /// </summary>
        /// Need to enable wrapping here
        public void Next()
        {
            int[,] nextBoard = new int[_RowsUsed, _ColsUsed];

            for (int r = 0; r < _RowsUsed; r++)
            {
                for (int c = 0; c < _ColsUsed; c++)
                {
                    if (_Board[r, c] == 0)
                    {
                        if (WillBeBorn(r, c))
                        {
                            nextBoard[r, c] = 1;
                        }
                        else
                        {
                            nextBoard[r, c] = 0;
                        } 
                    }

                    if (_Board[r, c] == 1)
                    {
                        if (WillDie(r, c))
                        {
                            nextBoard[r, c] = 0;
                        }
                        else
                        {
                            nextBoard[r, c] = 1;
                        }
                    }
                }
            }
            _Generation++;          
            _Board = nextBoard;

        }
//------------------------------------------------------------------------------
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
                Console.Write(" ".PadRight(Console.WindowWidth));
                String write = "Generation " + _Generation;
                int left = (Console.WindowWidth/2) - (write.Length / 2);
                Console.SetCursorPosition(left, 1);
                Console.Write(write);
            }

            Console.BackgroundColor = MenuText.DefaultBG;
            Console.ForegroundColor = MenuText.PopColor;
            int row = space;

            Console.SetCursorPosition(space, row);
            for (int r = 0; r < _RowsUsed; r++)
            {
                StringBuilder sb = new StringBuilder();
                for (int c = 0; c < _ColsUsed; c++)
                {
                    if (_Board[r, c] == 0)
                    {
                        sb.Append(' ');
                    }
                    else
                    {
                        sb.Append(_LiveCell);
                    }
                }
                Console.Write(sb);
                row++;
                Console.SetCursorPosition(space, row);
            }

            Console.BackgroundColor = MenuText.DefaultBG;
            Console.ForegroundColor = MenuText.DefaultFG;    
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
        private Boolean WillDie(int r, int c)
        {
            int n = 0;

            if (_Board[(r - 1 + _RowsUsed) % _RowsUsed, (c - 1 + _ColsUsed) % _ColsUsed] == 1) n++;
            if (_Board[(r - 1 + _RowsUsed) % _RowsUsed, (c + 1 + _ColsUsed) % _ColsUsed] == 1) n++;
            if (_Board[(r - 1 + _RowsUsed) % _RowsUsed, c] == 1) n++;
            if (_Board[(r + 1 + _RowsUsed) % _RowsUsed, (c - 1 + _ColsUsed) % _ColsUsed] == 1) n++;
            if (_Board[r, (c - 1 + _ColsUsed) % _ColsUsed] == 1) n++;
            if (_Board[(r + 1 + _RowsUsed) % _RowsUsed, c] == 1) n++;
            if (_Board[r, (c + 1 + _ColsUsed) % _ColsUsed] == 1) n++;
            if (_Board[(r + 1 + _RowsUsed) % _RowsUsed, (c + 1 + _ColsUsed) % _ColsUsed] == 1) n++;

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
        private Boolean WillBeBorn(int r, int c)
        {
            int n = 0;

            if (_Board[(r - 1 + _RowsUsed) % _RowsUsed, (c - 1 + _ColsUsed) % _ColsUsed] == 1) n++;
            if (_Board[(r - 1 + _RowsUsed) % _RowsUsed, (c + 1 + _ColsUsed) % _ColsUsed] == 1) n++;
            if (_Board[(r - 1 + _RowsUsed) % _RowsUsed, c] == 1) n++;
            if (_Board[(r + 1 + _RowsUsed) % _RowsUsed, (c - 1 + _ColsUsed) % _ColsUsed] == 1) n++;
            if (_Board[r, (c - 1 + _ColsUsed) % _ColsUsed] == 1) n++;
            if (_Board[(r + 1 + _RowsUsed) % _RowsUsed, c] == 1) n++;
            if (_Board[r, (c + 1 + _ColsUsed) % _ColsUsed] == 1) n++;
            if (_Board[(r + 1 + _RowsUsed) % _RowsUsed, (c + 1 + _ColsUsed) % _ColsUsed] == 1) n++;

            if (n == 3) return true;
            else return false;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Validates the selected file from the BuildFromFile() method.
        /// A Valid file is all 0s and 1s and does not have more rows or columns
        /// than the console window. The file must also be under 256KB
        /// </summary>
        /// <param name="filename">Path to a file to be checked</param>
        /// <param name="errType">The type of error returned</param>
        private MenuText.FileError ValidateFile(String filename)
        {
            // File should exist, but its good to make sure.
            FileInfo file = new FileInfo(filename);
            if (!file.Exists)
            {
                return MenuText.FileError.CONTENTS;
            }

            // Checks if the file is empty or too large ( > 256KB )
            if (file.Length == 0 || file.Length > 262144)
            {
                return MenuText.FileError.SIZE;
            }

            using (StreamReader reader = new StreamReader(filename))
            {
                string wholeFile = reader.ReadToEnd();
                string[] fileByLine = Regex.Split(wholeFile, "\r\n");


                int rows = fileByLine.Length;
                int cols = fileByLine[0].Length;

                // Error if there are more lines than the board can hold
                if (rows >= _RowsUsed)
                    return MenuText.FileError.LENGTH;
                // Error if the first line is too wide,
                // 'cols' also used to check against all other lines
                if (cols >= _ColsUsed)
                    return MenuText.FileError.WIDTH;

                foreach (string line in fileByLine)
                {
                    //Error if all lines are not the same width
                    if (line.Length != cols)
                    {
                        return MenuText.FileError.WIDTH;
                    }
                    //Error of the line is not all 0 and 1
                    if (!OnesAndZerosOnly(line))
                    {
                        return MenuText.FileError.CONTENTS;
                    }
                    // Update cols to compare to the next line
                    cols = line.Length;
                }
            }
            
            return MenuText.FileError.NONE;
       
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Makes sure there are only 1s and 0s in a given string, used to 
        /// validate the file loaded in BuildFromFile()
        /// </summary>
        /// <param name="s">current string</param>
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
    } // end class
}