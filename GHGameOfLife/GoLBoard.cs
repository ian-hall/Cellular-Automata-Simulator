using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

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
        private int[,] _Board;
        private int _RowsUsed;
        private int _ColsUsed;
        private int _Generation;
        public bool _Initialized { get; private set; }
        private const char _LiveCell = '☻';
        private const char _DeadCell = ' ';
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
            for (int r = 0; r < rowMax; r++)
            {
                for (int c = 0; c < colMax; c++)
                    _Board[r, c] = 0;
            }

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
            Random rand = new Random();
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
            MenuText.FileError errType = MenuText.FileError.NONE;
            StreamReader reader = null;

            OpenFileDialog openWindow = new OpenFileDialog();          
            if (openWindow.ShowDialog() == DialogResult.OK)
            {
                string filePath = openWindow.FileName;
                ValidateFile(filePath, out errType);
                reader = new StreamReader(openWindow.FileName);

            }
            else
            {   // No File loaded
                errType = MenuText.FileError.CONTENTS;
                int windowCenter = Console.WindowHeight / 2; //Vert position
                int welcomeLeft = (Console.WindowWidth / 2) -
                                            (MenuText.Welcome.Length / 2);
                int distToBorder = (Console.WindowWidth - 5) - welcomeLeft;

                MenuText.ClearWithinBorder(windowCenter);

                Console.SetCursorPosition(welcomeLeft, windowCenter - 1);
                Console.Write(MenuText.FileError1);
                Console.SetCursorPosition(welcomeLeft, windowCenter);
                Console.Write(MenuText.FileError2);
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
                    string startingPop = reader.ReadToEnd();
                    reader.Close();
                    fillBoard(startingPop);
                    break;
                default:
                    BuildDefaultPop();
                    break;
            }

            _Initialized = true;
            
            
            /*
            int rows = 0;
            while (!reader.EndOfStream)
            {
                reader.ReadLine();
                rows++;
            }
            reader.BaseStream.Position = 0;

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
             *//*
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
            }*/

        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Builds the board from a resource
        /// </summary>
        /// <param name="pop"></param>
        public void BuildFromResource(LoadedPops pop)
        {
            
            var rm = GHGameOfLife.Pops.ResourceManager;
            var startingPop = rm.GetString(pop.ToString());

            fillBoard(startingPop);

            _Initialized = true;
            /*
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
            }*/
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
            /*
            for (int r = 0; r < _RowsUsed; r++)
                for (int c = 0; c < _ColsUsed; c++)
                    _Board[r, c] = nextBoard[r, c];
             */
            _Board = nextBoard;

        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Displays the board in the console. It is centered in the console
        /// with a space of 5 on all sides to compensate for the border
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
                Console.Write(" ".PadRight(Console.WindowWidth));
                String write = "Generation " + _Generation;
                int left = (Console.WindowWidth/2) - (write.Length / 2);
                Console.SetCursorPosition(left, 1);
                Console.Write(write);
            }

            Console.BackgroundColor = MenuText.DefaultBG;         
            int row = space;

            Console.SetCursorPosition(space, row);
            for (int r = 0; r < _RowsUsed; r++)
            {
                for (int c = 0; c < _ColsUsed; c++)
                {
                    int check = _Board[r, c];
                    if (check == 0)
                    {
                        Console.ForegroundColor = MenuText.DeadColor;
                        Console.Write(_DeadCell);
                    }
                    else
                    {
                        Console.ForegroundColor = MenuText.PopColor;
                        Console.Write(_LiveCell);
                    }
                }
                row++;
                Console.SetCursorPosition(space, row);
            }

            Console.BackgroundColor = MenuText.DefaultBG;
            Console.ForegroundColor = MenuText.DefaultFG;    
        }
//------------------------------------------------------------------------------
        public void TestPrint()
        {

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
        private void ValidateFile(String filename, 
                                        out MenuText.FileError errType)
        {
            errType = MenuText.FileError.NONE;

            // File should exist, but its good to make sure.
            FileInfo file = new FileInfo(filename);
            if (!file.Exists)
            {
                errType = MenuText.FileError.CONTENTS;
                return;
            }

            // Checks if the file is empty or too large ( > 256KB )
            if (file.Length == 0 || file.Length > 262144)
            {
                errType = MenuText.FileError.SIZE;
            }

            StreamReader reader = new StreamReader(filename);
            string wholeFile = reader.ReadToEnd();
            reader.Close();
            string[] fileByLine = Regex.Split(wholeFile, "\r\n");


            int rows = fileByLine.Length;
            int cols = fileByLine[0].Length;

            // Error if there are more lines than the board can hold
            if (rows >= _RowsUsed) 
                errType = MenuText.FileError.LENGTH;
            // Error if the first line is too wide,
            // 'cols' also used to check against all other lines
            if (cols >= _ColsUsed) 
                errType = MenuText.FileError.WIDTH;

            foreach (string line in fileByLine)
            {
                //Error if all lines are not the same width
                if (line.Length != cols)
                {
                    errType = MenuText.FileError.WIDTH;
                    break;
                }
                if (!OnesAndZerosOnly(line))
                {
                    errType = MenuText.FileError.CONTENTS;
                    break;
                }
                // No reason to continue after the first error
            }

            /*
            var rows = File.ReadLines(filename).Count();         
            if (rows >= _RowsUsed)
            {
                errType = MenuText.FileError.LENGTH;
                //return false;
            }
            reader.BaseStream.Position = 0;
            int lastLineLen = reader.ReadLine().Length;
            reader.BaseStream.Position = 0;
            while (!reader.EndOfStream)
            {
                String currLine = reader.ReadLine();
                if (currLine.Length >= _ColsUsed)
                {
                    errType = MenuText.FileError.WIDTH;
                    reader.Close();
                    //return false;
                }
                if (!OnesAndZerosOnly(currLine))
                {
                    errType = MenuText.FileError.CONTENTS;
                    reader.Close();
                    //return false;
                }
                lastLineLen = currLine.Length;
            }
            reader.Close();
            errType = MenuText.FileError.NONE;
            //return true;*/
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