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
        public bool IsInitialized { get; private set; }

        private int[,] Board;
        private int Used_Rows;
        private int Used_Cols;
        private int Generation;
        private const char Live_Cell = '☺';
        //private const char _DeadCell = ' ';
        private Random Rand = new Random();
        private int Space = 5;
        public bool Wrap { get; set; }
//------------------------------------------------------------------------------
        /// <summary>
        /// Constructor for the GoLBoard class. Size of the board will be based
        /// off the size of the console window...
        /// </summary>
        /// <param name="rowMax">Number of rows</param>
        /// <param name="colMax">Number of columns</param>
        public GoLBoard(int rowMax, int colMax)
        {
            Board = new int[rowMax, colMax];
            
            /*for (int r = 0; r < rowMax; r++)
            {
                for (int c = 0; c < colMax; c++)
                    _Board[r, c] = rand.Next() % 2;
            }*/
            
            Used_Rows = rowMax;
            Used_Cols = colMax;
            IsInitialized = false;
            Wrap = true;
        }
//------------------------------------------------------------------------------
        
        /// <summary>
        /// Default population is a random spattering of 0s and 1s
        /// Easy enough to get using (random int)%2
        /// </summary>
        public void BuildDefaultPop() 
        {
            for (int r = 0; r < Used_Rows; r++)
            {
                for (int c = 0; c < Used_Cols; c++)
                {
                    Board[r, c] = Rand.Next()%2;
                }
            }
            IsInitialized = true;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Load the initial population from a file of 0s and 1s.
        /// This uses a Windows Forms OpenFileDialog to let the user select
        /// a file. The file is loaded into the center of the console window.
        /// </summary>
        public void BuildFromFile()
        {
            MenuText.FileError errType = MenuText.FileError.Not_Loaded;

            OpenFileDialog openWindow = new OpenFileDialog();          
            if (openWindow.ShowDialog() == DialogResult.OK)
            {
                string filePath = openWindow.FileName;
                errType = ValidateFile(filePath);
            }
            //no ELSE because it defaults to a file not loaded error

            switch (errType)
            {
                case MenuText.FileError.None:
                    string startingPop;
                    using (StreamReader reader = new StreamReader(openWindow.FileName))
                        startingPop = reader.ReadToEnd();
                    fillBoard(startingPop);
                    break;
                default:
                    int windowCenter = Console.WindowHeight / 2; //Vert position
                    int welcomeLeft = (Console.WindowWidth / 2) -
                        (MenuText.Welcome.Length / 2);
                    int distToBorder = (Console.WindowWidth - 5) - welcomeLeft;

                    MenuText.ClearWithinBorder(windowCenter);
                    Console.SetCursorPosition(welcomeLeft, windowCenter - 1);
                    Console.Write(MenuText.GetReadableError(errType));
                    Console.SetCursorPosition(welcomeLeft, windowCenter);
                    Console.Write(MenuText.Load_Rand);
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
                    BuildDefaultPop();
                    break;
            }

            IsInitialized = true;
            
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Builds the board from user input. This is going to be ugly...
        /// </summary>
        public void BuildFromUser()
        {
            SaveFileDialog saveDia = new SaveFileDialog();
            saveDia.Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*";

            int origWidth = Console.WindowWidth;
            int origHeight = Console.WindowHeight;

            Console.SetBufferSize(origWidth * 2, origHeight);
            Console.ForegroundColor = ConsoleColor.White;

            IEnumerable<int> validLeft = Enumerable.Range(Space, origWidth - 2 * Space );
            IEnumerable<int> validTop = Enumerable.Range(Space, origHeight - 2 * Space );
            bool[,] tempBoard = new bool[validTop.Count(), validLeft.Count()];

            for (int i = 0; i < validTop.Count(); i++)
            {
                for (int j = 0; j < validLeft.Count(); j++)
                {
                    Console.SetCursorPosition(validLeft.ElementAt(j), validTop.ElementAt(i));
                    Console.Write('*');
                    tempBoard[i, j] = false;
                }
            }

            Console.ForegroundColor = MenuText.Builder_Color;
            foreach (int top in validTop)
            {
                foreach (int left in validLeft)
                {
                    Console.SetCursorPosition(left + origWidth, top);
                    Console.Write('█');
                }
            }

            Console.ForegroundColor = MenuText.Info_Color;
            string colFirst = (validLeft.First() - Space).ToString();
            string colLast = (validLeft.Last() - Space).ToString();
            string rowFirst = (validLeft.First() - Space).ToString();
            string rowLast = (validTop.Last() - Space).ToString();

            int positionPrintRow = Space - 3;

            MenuText.PrintCreationControls();

            int blinkLeft = origWidth + 5;
            int charLeft = blinkLeft + 1;
            int extraTop = 2;

            int curLeft = Space;
            int curTop = Space;
            int nextLeft;
            int nextTop;
            bool exit = false;
            Console.CursorVisible = false;


            while (!exit)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                MenuText.ClearLine(Space - 3);
                string positionStr = String.Format("Current position: ({0},{1})", curTop - Space, curLeft - Space);
                Console.SetCursorPosition(origWidth / 2 - positionStr.Length/2, positionPrintRow);
                Console.Write(positionStr);
                Console.SetCursorPosition(0, 0);
                while (!Console.KeyAvailable)
                {
                    Console.MoveBufferArea(curLeft, curTop, 1, 1, charLeft, extraTop);
                    Console.MoveBufferArea(blinkLeft, extraTop, 1, 1, curLeft, curTop);
                    System.Threading.Thread.Sleep(100);
                    Console.MoveBufferArea(curLeft, curTop, 1, 1, blinkLeft, extraTop);
                    Console.MoveBufferArea(charLeft, extraTop, 1, 1, curLeft, curTop);
                    System.Threading.Thread.Sleep(100);
                }

                ConsoleKey pressed = Console.ReadKey(true).Key;

                if (pressed == ConsoleKey.Enter)
                {
                    exit = true;
                    break;
                }

                if (pressed == ConsoleKey.RightArrow)
                {
                    nextLeft = ++curLeft;
                    if (!validLeft.Contains(nextLeft))
                        nextLeft = validLeft.Min();

                    curLeft = nextLeft;
                }

                if (pressed == ConsoleKey.LeftArrow)
                {
                    nextLeft = --curLeft;
                    if (!validLeft.Contains(nextLeft))
                        nextLeft = validLeft.Max();

                    curLeft = nextLeft;
                }

                if (pressed == ConsoleKey.UpArrow)
                {
                    nextTop = --curTop;
                    if (!validTop.Contains(nextTop))
                        nextTop = validTop.Max();

                    curTop = nextTop;
                }

                if (pressed == ConsoleKey.DownArrow)
                {
                    nextTop = ++curTop;
                    if (!validTop.Contains(nextTop))
                        nextTop = validTop.Min();

                    curTop = nextTop;
                }

                if (pressed == ConsoleKey.Spacebar)
                {
                    Console.SetCursorPosition(0, 0);
                    bool boardVal = !tempBoard[curTop - Space, curLeft - Space];

                    if (boardVal)
                        Console.MoveBufferArea(curLeft + origWidth, curTop, 1, 1, curLeft, curTop, '█', MenuText.Builder_Color, ConsoleColor.Black);
                    else
                        Console.MoveBufferArea(curLeft, curTop, 1, 1, curLeft + origWidth, curTop, '*', ConsoleColor.White, ConsoleColor.Black);

                    tempBoard[curTop - Space, curLeft - Space] = boardVal;
                }

                if (pressed == ConsoleKey.S)
                {
                    if (saveDia.ShowDialog() == DialogResult.OK)
                    {
                        
                        int top = int.MaxValue;
                        int bottom = int.MinValue;
                        int left = int.MaxValue;
                        int right = int.MinValue;

                        // make a box that only includes the minimum needed lines
                        // to save the board
                        for (int i = 0; i < validTop.Count(); i++)
                        {
                            for (int j = 0; j < validLeft.Count(); j++)
                            {
                                if (tempBoard[i, j])
                                {
                                    if (i < top)
                                        top = i;
                                    if (i > bottom)
                                        bottom = i;
                                    if (j < left)
                                        left = j;
                                    if (j > right)
                                        right = j;
                                }
                            }
                        }

                        StringBuilder sb = new StringBuilder();
                        for (int i = top; i <= bottom; i++)
                        {
                            for (int j = left; j <= right; j++)
                            {
                                if (tempBoard[i, j])
                                    sb.Append('1');
                                else
                                    sb.Append('0');
                            }
                            if (i != bottom)
                                sb.AppendLine();
                        }
                        File.WriteAllText(saveDia.FileName, sb.ToString());
                    }

                }
            }

            StringBuilder popString = new StringBuilder();
            for (int r = 0; r < validTop.Count(); r++)
            {
                for (int c = 0; c < validLeft.Count(); c++)
                {
                    if (tempBoard[r, c])
                        popString.Append(1);
                    else
                        popString.Append(0);
                }
                if( r != validTop.Count() -1 )
                    popString.AppendLine();
            }

            fillBoard(popString.ToString());
            Console.SetWindowSize(origWidth, origHeight);
            Console.SetBufferSize(origWidth, origHeight);

            /*
             * I think this was used for something I dont do anymore??
            Console.SetCursorPosition(Space, Space - 2);
            Console.Write(" ");          
            Console.SetCursorPosition(origWidth - Space - colLast.Length, Space - 2);            
            StringBuilder clear = new StringBuilder();
            for (int i = 0; i < colLast.Length; i++)
                clear.Append(" ");
            Console.Write(clear);

            Console.SetCursorPosition(Space - 2, Space);
            Console.Write(" ");
            Console.SetCursorPosition(Space - rowLast.Length - 1, origHeight - Space - 1);
            clear = new StringBuilder();
            for (int i = 0; i < rowLast.Length; i++)
                clear.Append(" ");
            Console.Write(clear);
             */ 

            MenuText.ClearUnderBoard();

            Console.ForegroundColor = MenuText.Default_FG;
            MenuText.ClearLine(positionPrintRow);
            IsInitialized = true;
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

            IsInitialized = true;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Used by files to fill the game board, cente
        /// </summary>
        /// <param name="startingPop"></param>
        private void fillBoard(string startingPop)
        {
            string[] popByLine = Regex.Split(startingPop, "\r\n");

            int midRow = Used_Rows / 2;
            int midCol = Used_Cols / 2;

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
                    Board[r, c] = (int)Char.GetNumericValue(popByLine[popRow].ElementAt(popCol));
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
            int[,] nextBoard = new int[Used_Rows, Used_Cols];

            for (int r = 0; r < Used_Rows; r++)
            {
                for (int c = 0; c < Used_Cols; c++)
                {
                    if (Board[r, c] == 0)
                    {
                        if (Wrap)
                        {
                            if (WillBeBornWrap(r, c))
                            {
                                nextBoard[r, c] = 1;
                            }
                            else
                            {
                                nextBoard[r, c] = 0;
                            }
                        }
                        else
                        {
                            if (WillBeBornNoWrap(r, c))
                            {
                                nextBoard[r, c] = 1;
                            }
                            else
                            {
                                nextBoard[r, c] = 0;
                            }
                        }
                        
                    }

                    if (Board[r, c] == 1)
                    {
                        if (Wrap)
                        {
                            if (WillDieWrap(r, c))
                            {
                                nextBoard[r, c] = 0;
                            }
                            else
                            {
                                nextBoard[r, c] = 1;
                            }
                        }
                        else
                        {
                            if (WillDieNoWrap(r, c))
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
            }
            Generation++;          
            Board = nextBoard;

        }
//------------------------------------------------------------------------------
        public void Print()
        {
            if (Generation == 0)
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
                String write = "Generation " + Generation;
                int left = (Console.WindowWidth/2) - (write.Length / 2);
                Console.SetCursorPosition(left, 1);
                Console.Write(write);
            }

            Console.BackgroundColor = MenuText.Default_BG;
            Console.ForegroundColor = MenuText.Board_Color;

            Console.SetCursorPosition(0, Space);
            StringBuilder sb = new StringBuilder();
            for (int r = 0; r < Used_Rows; r++)
            {
                sb.Append("    ║");
                for (int c = 0; c < Used_Cols; c++)
                {
                    if (Board[r, c] == 0)
                    {
                        sb.Append(" ");
                    }
                    else
                    {
                        sb.Append(Live_Cell);
                    }
                }
                sb.AppendLine("║");
            }
            Console.Write(sb);

            Console.BackgroundColor = MenuText.Default_BG;
            Console.ForegroundColor = MenuText.Default_FG;    
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
        private Boolean WillDieWrap(int r, int c)
        {
            int n = 0;

            if (Board[(r - 1 + Used_Rows) % Used_Rows, (c - 1 + Used_Cols) % Used_Cols] == 1) n++;
            if (Board[(r - 1 + Used_Rows) % Used_Rows, (c + 1 + Used_Cols) % Used_Cols] == 1) n++;
            if (Board[(r - 1 + Used_Rows) % Used_Rows, c] == 1) n++;
            if (Board[(r + 1 + Used_Rows) % Used_Rows, (c - 1 + Used_Cols) % Used_Cols] == 1) n++;
            if (Board[r, (c - 1 + Used_Cols) % Used_Cols] == 1) n++;
            if (Board[(r + 1 + Used_Rows) % Used_Rows, c] == 1) n++;
            if (Board[r, (c + 1 + Used_Cols) % Used_Cols] == 1) n++;
            if (Board[(r + 1 + Used_Rows) % Used_Rows, (c + 1 + Used_Cols) % Used_Cols] == 1) n++;

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
        private Boolean WillBeBornWrap(int r, int c)
        {
            int n = 0;

            if (Board[(r - 1 + Used_Rows) % Used_Rows, (c - 1 + Used_Cols) % Used_Cols] == 1) n++;
            if (Board[(r - 1 + Used_Rows) % Used_Rows, (c + 1 + Used_Cols) % Used_Cols] == 1) n++;
            if (Board[(r - 1 + Used_Rows) % Used_Rows, c] == 1) n++;
            if (Board[(r + 1 + Used_Rows) % Used_Rows, (c - 1 + Used_Cols) % Used_Cols] == 1) n++;
            if (Board[r, (c - 1 + Used_Cols) % Used_Cols] == 1) n++;
            if (Board[(r + 1 + Used_Rows) % Used_Rows, c] == 1) n++;
            if (Board[r, (c + 1 + Used_Cols) % Used_Cols] == 1) n++;
            if (Board[(r + 1 + Used_Rows) % Used_Rows, (c + 1 + Used_Cols) % Used_Cols] == 1) n++;

            if (n == 3) return true;
            else return false;
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
        public Boolean WillDieNoWrap(int r, int c)
        {
            int n = 0;

            if (r != 0 && c != 0)
            {
                if (Board[r - 1, c - 1] == 1) n++;
            }
            if (r != 0 && c != Used_Cols - 1)
            {
                if (Board[r - 1, c + 1] == 1) n++;
            }
            if (r != 0)
            {
                if (Board[r - 1, c] == 1) n++;
            }
            if (r != Used_Rows - 1 && c != 0)
            {
                if (Board[r + 1, c - 1] == 1) n++;
            }
            if (c != 0)
            {
                if (Board[r, c - 1] == 1) n++;
            }
            if (r != Used_Rows - 1)
            {
                if (Board[r + 1, c] == 1) n++;
            }
            if (c != Used_Cols - 1)
            {
                if (Board[r, c + 1] == 1) n++;
            }
            if (r != Used_Rows - 1 && c != Used_Cols - 1)
            {
                if (Board[r + 1, c + 1] == 1) n++;
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
        public Boolean WillBeBornNoWrap(int r, int c)
        {
            int n = 0;

            if (r != 0 && c != 0)
            {
                if (Board[r - 1, c - 1] == 1) n++;
            }
            if (r != 0 && c != Used_Cols - 1)
            {
                if (Board[r - 1, c + 1] == 1) n++;
            }
            if (r != 0)
            {
                if (Board[r - 1, c] == 1) n++;
            }
            if (r != Used_Rows - 1 && c != 0)
            {
                if (Board[r + 1, c - 1] == 1) n++;
            }
            if (c != 0)
            {
                if (Board[r, c - 1] == 1) n++;
            }
            if (r != Used_Rows - 1)
            {
                if (Board[r + 1, c] == 1) n++;
            }
            if (c != Used_Cols - 1)
            {
                if (Board[r, c + 1] == 1) n++;
            }
            if (r != Used_Rows - 1 && c != Used_Cols - 1)
            {
                if (Board[r + 1, c + 1] == 1) n++;
            }

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
                return MenuText.FileError.Contents;
            }

            // Checks if the file is empty or too large ( > 10KB )
            if (file.Length == 0 || file.Length > 10240)
            {
                return MenuText.FileError.Size;
            }

            using (StreamReader reader = new StreamReader(filename))
            {
                string wholeFile = reader.ReadToEnd();
                string[] fileByLine = Regex.Split(wholeFile, "\r\n");


                int rows = fileByLine.Length;
                int cols = fileByLine[0].Length;

                // Error if there are more lines than the board can hold
                if (rows >= Used_Rows)
                    return MenuText.FileError.Length;
                // Error if the first line is too wide,
                // 'cols' also used to check against all other lines
                if (cols >= Used_Cols)
                    return MenuText.FileError.Width;

                foreach (string line in fileByLine)
                {
                    //Error if all lines are not the same width
                    if (line.Length != cols)
                    {
                        return MenuText.FileError.Uneven;
                    }
                    //Error of the line is not all 0 and 1
                    if (!OnesAndZerosOnly(line))
                    {
                        return MenuText.FileError.Contents;
                    }
                    // Update cols to compare to the next line
                    cols = line.Length;
                }
            }
            
            return MenuText.FileError.None;
       
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