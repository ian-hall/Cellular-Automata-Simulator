using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;

namespace GHGameOfLife
{
    class ConsoleRunHelper
    {
        private static int[] __Speeds = { 125, 100, 75, 50, 25 };
        private static int __Curr_Speed_Index = 2;
        private static IEnumerable<int> __Valid_Lefts;
        private static IEnumerable<int> __Valid_Tops;
        private static int __Cursor_Left, __Cursor_Top;
        //-----------------------------------------------------------------------------
        /// <summary>
        /// Default population is a random spattering of 0s and 1s
        /// Easy enough to get using (random int)%2
        /// </summary>
        public static bool[,] Build2DBoard_Random(Automata2D currentGame)
        {
            var rand = new Random();
            var newBoard = new bool[currentGame.Rows, currentGame.Cols];
            for (int r = 0; r < currentGame.Rows; r++)
            {
                for (int c = 0; c < currentGame.Cols; c++)
                {
                    newBoard[r, c] = (rand.Next() % 2 == 0);
                }
            }
            return newBoard;
        }
        //------------------------------------------------------------------------------
        /// <summary>
        /// Load the initial population from a file of 0s and 1s.
        /// This uses a Windows Forms OpenFileDialog to let the user select
        /// a file. The file is loaded into the center of the console window.
        /// </summary>
        public static bool[,] Build2DBoard_File(Automata2D currentGame)
        {
            MenuText.FileError errType = MenuText.FileError.Not_Loaded;
            var isValidFile = false;

            OpenFileDialog openWindow = new OpenFileDialog();
            string startingPop = null;
            if (openWindow.ShowDialog() == DialogResult.OK)
            {
                string filePath = openWindow.FileName;
                isValidFile = IsValidFileOrResource(filePath, currentGame, out startingPop, out errType);
            }
            //no ELSE because it defaults to a file not loaded error

            if (isValidFile)
            {
                return ConsoleRunHelper.FillBoard(startingPop,currentGame.Rows,currentGame.Cols);
            }
            else
            {
                MenuText.PrintFileError(errType);
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
                return ConsoleRunHelper.Build2DBoard_Random(currentGame);
            }
        }
        //------------------------------------------------------------------------------
        /// <summary>
        /// Builds the board from a resource
        /// TODO: Add the name of the resource to the screen
        /// </summary>
        /// <param name="res"></param>
        public static bool[,] Build2DBoard_Resource(string res, Automata2D currentGame)
        {
            string startingPop;
            MenuText.FileError errType = MenuText.FileError.Not_Loaded;
            var isValidResource = IsValidFileOrResource(res, currentGame, out startingPop, out errType, true);

            if (isValidResource)
            {
                return FillBoard(startingPop,currentGame.Rows,currentGame.Cols);
            }
            else
            {
                MenuText.PrintFileError(errType);
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
                return ConsoleRunHelper.Build2DBoard_Random(currentGame);
            }
        }
        //------------------------------------------------------------------------------
        /// <summary>
        /// Validates the selected file from the BuildFromFile() method.
        /// A Valid file is all 0s and 1s and does not have more rows or columns
        /// than the console window. The file must also be pretty small.
        /// This is also used to validate files from the LargePops resource.
        /// </summary>
        /// <param name="filename">Path to a file to be checked, or resource to be loaded</param>
        /// <param name="popToLoad">Out set if the filename or resource are valid</param>
        /// <param name="fromRes">Set True if loading from a resource file</param>
        private static bool IsValidFileOrResource(string filename, Automata2D currentGame, out string popToLoad, out MenuText.FileError error, bool fromRes = false)
        {
            popToLoad = "";
            error = MenuText.FileError.None;
            var wholeFile = new List<string>();
            if (!fromRes)
            {
                // File should exist, but its good to make sure.
                FileInfo file = new FileInfo(filename);
                if (!file.Exists)
                {
                    error = MenuText.FileError.Not_Loaded;
                    return false;
                }

                // Checks if the file is empty or too large ( > 20KB )
                if (file.Length == 0 || file.Length > 20480)
                {
                    error = MenuText.FileError.Size;
                    return false;
                }

                using (StreamReader reader = new StreamReader(filename))
                {
                    while (!reader.EndOfStream)
                    {
                        string temp = reader.ReadLine().Trim();
                        wholeFile.Add(temp);
                    }
                }
            }
            else
            {
                wholeFile = Regex.Split(GHGameOfLife.LargePops.ResourceManager.GetString(filename), Environment.NewLine).ToList();
            }
            var fileByLine = new List<string>();
            foreach (var line in wholeFile)
            {
                string temp = line.Trim();
                if (temp == String.Empty)
                {
                    fileByLine.Add(temp);
                    continue;
                }
                switch (temp[0])
                {
                    case '!':
                    case '#':
                    case '/':
                        // Ignore these lines
                        break;
                    default:
                        fileByLine.Add(temp);
                        break;
                }
            }

            var longestLine = fileByLine.Select(line => line.Length).Max(len => len);
            var fileRows = fileByLine.Count;

            if (fileRows > currentGame.Rows)
            {
                error = MenuText.FileError.Length;
                return false;
            }
            if (longestLine > currentGame.Cols)
            {
                error = MenuText.FileError.Width;
                return false;
            }

            var sb = new StringBuilder();
            foreach (var line in fileByLine)
            {
                //Pad all lines to the same length as the longest for loading into the game board.
                var newLine = line.PadRight(longestLine, '.');
                if (!ValidLine(newLine))
                {
                    error = MenuText.FileError.Contents;
                    return false;
                }
                sb.AppendLine(newLine);
            }
            popToLoad = sb.ToString();
            error = MenuText.FileError.None;
            return true;           
        }
        //------------------------------------------------------------------------------
        /// <summary>
        /// TODO: Change this again to accept more file formats
        /// Makes sure there are only '.' and 'O' in a given string, used to 
        /// validate the file loaded in BuildFromFile()
        /// </summary>
        /// <param name="s">current string</param>
        /// <returns>True if the string is all '.' and 'O'</returns>
        private static bool ValidLine(string s)
        {
            try
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == '.' || s[i] == 'O')
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
        /// <summary>
        /// Builds the board from user input. This is going to be ugly...
        /// For pops: 1: Glider 2: Ship 3: Acorn 4: BlockLayer
        /// </summary>
        public static bool[,] Build2DBoard_User(Automata2D currentGame)
        {
            Console.SetBufferSize(currentGame.Console_Width + 50, currentGame.Console_Height);
            Console.ForegroundColor = ConsoleColor.White;

            bool[,] tempBoard = new bool[__Valid_Tops.Count(), __Valid_Lefts.Count()];

            for (int i = 0; i < __Valid_Tops.Count(); i++)
            {
                for (int j = 0; j < __Valid_Lefts.Count(); j++)
                {
                    Console.SetCursorPosition(__Valid_Lefts.ElementAt(j), __Valid_Tops.ElementAt(i));
                    Console.Write('*');
                    tempBoard[i, j] = false;
                }
            }
            MenuText.DrawBorder();
            Console.ForegroundColor = MenuText.Info_FG;


            int positionPrintRow = MenuText.Space - 3;

            MenuText.PrintCreationControls();

            int blinkLeft = currentGame.Console_Width + 5;
            int charLeft = blinkLeft + 1;
            int extraTop = 2;

            __Cursor_Left = __Valid_Lefts.ElementAt(__Valid_Lefts.Count() / 2);
            __Cursor_Top = __Valid_Tops.ElementAt(__Valid_Tops.Count() / 2);
            int nextLeft;
            int nextTop;
            bool exit = false;
            Console.CursorVisible = false;


            Rect loadedPopBounds = new Rect();
            bool popLoaderMode = false;
            string loadedPop = null;
            bool[][] smallPopVals = new bool[0][];

            while (!exit)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                MenuText.ClearLine(MenuText.Space - 3);
                string positionStr = String.Format("Current position: ({0},{1})", __Cursor_Top - MenuText.Space, __Cursor_Left - MenuText.Space);
                Console.SetCursorPosition(currentGame.Console_Width / 2 - positionStr.Length / 2, positionPrintRow);
                Console.Write(positionStr);
                Console.SetCursorPosition(0, 0);

                if (!popLoaderMode)
                {
                    while (!Console.KeyAvailable)
                    {
                        Console.MoveBufferArea(__Cursor_Left, __Cursor_Top, 1, 1, charLeft, extraTop);
                        Console.MoveBufferArea(blinkLeft, extraTop, 1, 1, __Cursor_Left, __Cursor_Top);
                        System.Threading.Thread.Sleep(150);
                        Console.MoveBufferArea(__Cursor_Left, __Cursor_Top, 1, 1, blinkLeft, extraTop);
                        Console.MoveBufferArea(charLeft, extraTop, 1, 1, __Cursor_Left, __Cursor_Top);
                        System.Threading.Thread.Sleep(150);
                    }

                    MenuText.ClearLine(0);
                    ConsoleKeyInfo pressed = Console.ReadKey(true);

                    switch (pressed.Key)
                    {
                        case ConsoleKey.Enter:
                            exit = true;
                            continue;
                        case ConsoleKey.RightArrow:
                            nextLeft = ++__Cursor_Left;
                            if (!__Valid_Lefts.Contains(nextLeft))
                            {
                                nextLeft = __Valid_Lefts.Min();
                            }
                            __Cursor_Left = nextLeft;
                            break;
                        case ConsoleKey.LeftArrow:
                            nextLeft = --__Cursor_Left;
                            if (!__Valid_Lefts.Contains(nextLeft))
                            {
                                nextLeft = __Valid_Lefts.Max();
                            }
                            __Cursor_Left = nextLeft;
                            break;
                        case ConsoleKey.UpArrow:
                            nextTop = --__Cursor_Top;
                            if (!__Valid_Tops.Contains(nextTop))
                            {
                                nextTop = __Valid_Tops.Max();
                            }
                            __Cursor_Top = nextTop;
                            break;
                        case ConsoleKey.DownArrow:
                            nextTop = ++__Cursor_Top;
                            if (!__Valid_Tops.Contains(nextTop))
                            {
                                nextTop = __Valid_Tops.Min();
                            }
                            __Cursor_Top = nextTop;
                            break;
                        case ConsoleKey.Spacebar:
                            Console.SetCursorPosition(__Cursor_Left, __Cursor_Top);
                            bool boardVal = !tempBoard[__Cursor_Top - MenuText.Space, __Cursor_Left - MenuText.Space];

                            if (boardVal)
                            {
                                Console.ForegroundColor = MenuText.Builder_FG;
                                Console.Write('█');
                            }
                            else
                            {
                                Console.ForegroundColor = MenuText.Default_FG;
                                Console.Write('*');

                            }

                            tempBoard[__Cursor_Top - MenuText.Space, __Cursor_Left - MenuText.Space] = boardVal;
                            break;
                        case ConsoleKey.D1:
                        case ConsoleKey.D2:
                        case ConsoleKey.D3:
                        case ConsoleKey.D4:
                            var keyNum = pressed.Key.ToString()[1];
                            var keyVal = Int32.Parse("" + keyNum);
                            string smallPop = GHGameOfLife.BuilderPops.ResourceManager.GetString(MenuText.Builder_Pops[keyVal - 1]);
                            if (BuilderLoadPop(smallPop, ref smallPopVals, ref loadedPopBounds))
                            {
                                loadedPop = MenuText.Builder_Pops[keyVal - 1];
                                popLoaderMode = true;
                            }
                            else
                            {
                                Console.SetCursorPosition(0, 0);
                                Console.ForegroundColor = MenuText.Info_FG;
                                Console.Write("Cannot load pop outside of bounds");
                                loadedPop = null;
                            }
                            break;
                        case ConsoleKey.S:
                            SaveBoard(__Valid_Tops.Count(), __Valid_Lefts.Count(), tempBoard);
                            break;
                        default:
                            break;
                    }
                }
                else //This means a population is loaded into the builder
                {
                    int storeBoardLeft = loadedPopBounds.Left + loadedPopBounds.Width + 1;
                    int storeBoardTop = loadedPopBounds.Top;


                    while (!Console.KeyAvailable)
                    {
                        Console.MoveBufferArea(__Cursor_Left, __Cursor_Top, loadedPopBounds.Width, loadedPopBounds.Height, storeBoardLeft, storeBoardTop);
                        Console.MoveBufferArea(loadedPopBounds.Left, loadedPopBounds.Top, loadedPopBounds.Width, loadedPopBounds.Height, __Cursor_Left, __Cursor_Top);
                        System.Threading.Thread.Sleep(250);
                        Console.MoveBufferArea(__Cursor_Left, __Cursor_Top, loadedPopBounds.Width, loadedPopBounds.Height, loadedPopBounds.Left, loadedPopBounds.Top);
                        Console.MoveBufferArea(storeBoardLeft, storeBoardTop, loadedPopBounds.Width, loadedPopBounds.Height, __Cursor_Left, __Cursor_Top);
                        System.Threading.Thread.Sleep(150);
                    }

                    MenuText.ClearLine(0);
                    ConsoleKeyInfo pressed = Console.ReadKey(true);

                    switch (pressed.Key)
                    {
                        case ConsoleKey.Enter:
                            exit = true;
                            continue;
                        case ConsoleKey.RightArrow:
                            nextLeft = ++__Cursor_Left;
                            if (nextLeft >= (__Valid_Lefts.Last() - loadedPopBounds.Width) + 2)
                            {
                                nextLeft = __Valid_Lefts.Min();
                            }
                            __Cursor_Left = nextLeft;
                            break;
                        case ConsoleKey.LeftArrow:
                            nextLeft = --__Cursor_Left;
                            if (!__Valid_Lefts.Contains(nextLeft))
                            {
                                nextLeft = (__Valid_Lefts.Last() - loadedPopBounds.Width) + 1;
                            }
                            __Cursor_Left = nextLeft;
                            break;

                        case ConsoleKey.UpArrow:
                            nextTop = --__Cursor_Top;
                            if (!__Valid_Tops.Contains(nextTop))
                            {
                                nextTop = (__Valid_Tops.Last() - loadedPopBounds.Height) + 1;
                            }
                            __Cursor_Top = nextTop;
                            break;

                        case ConsoleKey.DownArrow:
                            nextTop = ++__Cursor_Top;
                            if (nextTop >= (__Valid_Tops.Last() - loadedPopBounds.Height) + 2)
                            {
                                nextTop = __Valid_Tops.Min();
                            }
                            __Cursor_Top = nextTop;
                            break;
                        case ConsoleKey.Spacebar:
                            Console.SetCursorPosition(0, 0);
                            int popRows = (loadedPopBounds.Bottom - loadedPopBounds.Top);
                            int popCols = (loadedPopBounds.Right - loadedPopBounds.Left);

                            for (int r = __Cursor_Top; r < __Cursor_Top + popRows; r++)
                            {
                                for (int c = __Cursor_Left; c < __Cursor_Left + popCols; c++)
                                {
                                    Console.SetCursorPosition(c, r);
                                    if (smallPopVals[r - __Cursor_Top][c - __Cursor_Left])
                                    {
                                        if (tempBoard[r - MenuText.Space, c - MenuText.Space])
                                        {
                                            Console.ForegroundColor = MenuText.Default_FG;
                                            Console.Write('*');
                                            tempBoard[r - MenuText.Space, c - MenuText.Space] = false;
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = MenuText.Builder_FG;
                                            Console.Write('█');
                                            tempBoard[r - MenuText.Space, c - MenuText.Space] = true;
                                        }
                                    }
                                }
                            }
                            break;
                        case ConsoleKey.D1:
                        case ConsoleKey.D2:
                        case ConsoleKey.D3:
                        case ConsoleKey.D4:
                            var keyNum = pressed.Key.ToString()[1];
                            var keyVal = Int32.Parse("" + keyNum);
                            if (loadedPop != MenuText.Builder_Pops[keyVal - 1])
                            {
                                string smallPop = GHGameOfLife.BuilderPops.ResourceManager.GetString(MenuText.Builder_Pops[keyVal - 1]);
                                if (BuilderLoadPop(smallPop, ref smallPopVals, ref loadedPopBounds))
                                {
                                    loadedPop = MenuText.Builder_Pops[keyVal - 1];
                                    popLoaderMode = true;
                                }
                                else
                                {
                                    Console.SetCursorPosition(0, 0);
                                    Console.ForegroundColor = MenuText.Info_FG;
                                    Console.Write("Cannot load pop outside of bounds");
                                }
                            }
                            else // Population is already loaded, either rotate or mirror
                            {
                                if (pressed.Modifiers == ConsoleModifiers.Control)
                                {
                                    if (!MirrorBuilderPop(ref smallPopVals, ref loadedPopBounds))
                                    {
                                        Console.SetCursorPosition(0, 0);
                                        Console.ForegroundColor = MenuText.Info_FG;
                                        Console.Write("Error while trying to mirror");
                                    }

                                }
                                else
                                {
                                    // Just check if the pop is not rotated, if it is rotated we do nothing
                                    if (!RotateBuilderPop(ref smallPopVals, ref loadedPopBounds))
                                    {
                                        Console.SetCursorPosition(0, 0);
                                        Console.ForegroundColor = MenuText.Info_FG;
                                        Console.Write("Rotating will go out of bounds");
                                    }
                                }
                            }
                            break;
                        case ConsoleKey.S:
                            SaveBoard(__Valid_Tops.Count(), __Valid_Lefts.Count(), tempBoard);
                            break;
                        case ConsoleKey.C:
                            popLoaderMode = false;
                            break;
                        default:
                            break;
                    }
                }
            }

            StringBuilder popString = new StringBuilder();
            for (int r = 0; r < __Valid_Tops.Count(); r++)
            {
                for (int c = 0; c < __Valid_Lefts.Count(); c++)
                {
                    if (tempBoard[r, c])
                        popString.Append('O');
                    else
                        popString.Append('.');
                }
                if (r != __Valid_Tops.Count() - 1)
                    popString.AppendLine();
            }

            Console.SetWindowSize(currentGame.Console_Width, currentGame.Console_Height);
            Console.SetBufferSize(currentGame.Console_Width, currentGame.Console_Height);

            Console.ForegroundColor = MenuText.Default_FG;
            MenuText.ClearUnderBoard();
            MenuText.DrawBorder();

            MenuText.ClearLine(positionPrintRow);
            return ConsoleRunHelper.FillBoard(popString.ToString(), currentGame.Rows, currentGame.Cols);
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Loads the selected builder pop into the board
        /// </summary>
        /// <param name="startingPop"></param>
        /// <returns>Bounds of the pop loaded</returns>
        private static bool BuilderLoadPop(string pop, ref bool[][] popVals, ref Rect bounds)
        {
            string[] popByLine = Regex.Split(pop, Environment.NewLine);

            int midRow = Console.BufferHeight / 2;
            int midCol = Console.BufferWidth - 25;

            int rowsNum = popByLine.Count();
            int colsNum = popByLine[0].Length;

            Rect tempBounds = Center(rowsNum, colsNum, midRow, midCol);

            bool loaded = false;

            // Checks if the loaded pop is going to fit in the window at the current cursor position
            if ((__Cursor_Left <= (__Valid_Lefts.Last() - colsNum) + 1) && (__Cursor_Top <= (__Valid_Tops.Last() - rowsNum) + 1))
            {
                popVals = new bool[rowsNum][];
                for (int r = tempBounds.Top; r < tempBounds.Bottom; r++)
                {
                    int popRow = r - tempBounds.Top;
                    popVals[popRow] = new bool[colsNum];
                    for (int c = tempBounds.Left; c < tempBounds.Right; c++)
                    {
                        int popCol = c - tempBounds.Left;

                        Console.SetCursorPosition(c, r);
                        Console.ForegroundColor = MenuText.Info_FG;
                        if (popByLine[popRow][popCol] == 'O')
                        {
                            Console.Write('█');
                            popVals[popRow][popCol] = true;
                        }
                        else
                        {
                            Console.Write(' ');
                            popVals[popRow][popCol] = false;
                        }
                    }
                }
                bounds = tempBounds;
                loaded = true;
            }
            return loaded;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Rotates the loaded builder pop 90 degrees clockwise
        /// </summary>
        /// <param name="oldVals"></param>
        /// <returns></returns>
        private static bool RotateBuilderPop(ref bool[][] popVals, ref Rect bounds)
        {
            bool[][] rotated = GenericHelp<bool>.Rotate90(popVals);

            int midRow = Console.BufferHeight / 2;
            int midCol = Console.BufferWidth - 25;

            int rowsNum = rotated.Length;
            int colsNum = rotated[0].Length;

            bool loaded = false;
            Rect tempBounds = Center(rowsNum, colsNum, midRow, midCol);

            if ((__Cursor_Left <= (__Valid_Lefts.Last() - colsNum) + 1) && (__Cursor_Top <= (__Valid_Tops.Last() - rowsNum) + 1))
            {
                for (int r = tempBounds.Top; r < tempBounds.Bottom; r++)
                {
                    int popRow = r - tempBounds.Top;
                    for (int c = tempBounds.Left; c < tempBounds.Right; c++)
                    {
                        int popCol = c - tempBounds.Left;
                        Console.SetCursorPosition(c, r);
                        Console.ForegroundColor = MenuText.Info_FG;
                        if (rotated[popRow][popCol])
                        {
                            Console.Write('█');
                        }
                        else
                        {
                            Console.Write(' ');
                        }
                    }
                }
                popVals = rotated;
                bounds = tempBounds;
                loaded = true;
            }

            return loaded;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Mirrors the loaded builder pop
        /// </summary>
        /// <param name="oldVals"></param>
        /// <returns></returns>
        private static bool MirrorBuilderPop(ref bool[][] popVals, ref Rect bounds)
        {
            bool[][] rotated = GenericHelp<bool>.Mirror(popVals);

            int midRow = Console.BufferHeight / 2;
            int midCol = Console.BufferWidth - 25;

            int rowsNum = rotated.Length;
            int colsNum = rotated[0].Length;

            bool loaded = false;

            Rect tempBounds = Center(rowsNum, colsNum, midRow, midCol);

            if ((__Cursor_Left <= (__Valid_Lefts.Last() - colsNum) + 1) && (__Cursor_Top <= (__Valid_Tops.Last() - rowsNum) + 1))
            {
                for (int r = tempBounds.Top; r < tempBounds.Bottom; r++)
                {
                    int popRow = r - tempBounds.Top;
                    for (int c = tempBounds.Left; c < tempBounds.Right; c++)
                    {
                        int popCol = c - tempBounds.Left;
                        Console.SetCursorPosition(c, r);
                        Console.ForegroundColor = MenuText.Info_FG;
                        if (rotated[popRow][popCol])
                        {
                            Console.Write('█');
                        }
                        else
                        {
                            Console.Write(' ');
                        }
                    }
                }
                popVals = rotated;
                bounds = tempBounds;
                loaded = true;
            }
            return loaded;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Used by files to fill the game board, centered
        /// </summary>
        /// <param name="startingPop"></param>
        private static bool[,] FillBoard(string startingPop,int rows, int cols)
        {
            string[] popByLine = Regex.Split(startingPop, Environment.NewLine);
            var newBoard = new bool[rows, cols];

            int midRow = rows / 2;
            int midCol = cols / 2;

            int rowsNum = popByLine.Count();
            int colsNum = popByLine[0].Length;

            /* toss the last line if its empty */
            if (popByLine.Last() == String.Empty)
                rowsNum -= 1;

            Rect bounds = Center(rowsNum, colsNum, midRow, midCol);

            for (int r = bounds.Top; r < bounds.Bottom; r++)
            {
                for (int c = bounds.Left; c < bounds.Right; c++)
                {
                    int popRow = r - bounds.Top;
                    int popCol = c - bounds.Left;

                    if (popByLine[popRow][popCol] == '.')
                        newBoard[r, c] = false;
                    else
                        newBoard[r, c] = true;
                }
            }
            return newBoard;
        }
        //------------------------------------------------------------------------------
        /// <summary>
        /// Saves the current board to a file. 
        /// </summary>
        /// <param name="numRows">Total number of rows on the board</param>
        /// <param name="numCols">Total number of cols on the board</param>
        /// <param name="tempBoard">2d bool array representing the board</param>
        private static void SaveBoard(int numRows, int numCols, bool[,] tempBoard)
        {
            SaveFileDialog saveDia = new SaveFileDialog();
            saveDia.Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*";

            // We only save if the dialog box comes back true, otherwise
            // we just do nothing
            if (saveDia.ShowDialog() == DialogResult.OK)
            {
                Rect saveBox = new Rect();
                saveBox.Top = int.MaxValue;
                saveBox.Bottom = int.MinValue;
                saveBox.Left = int.MaxValue;
                saveBox.Right = int.MinValue;

                // make a box that only includes the minimum needed lines
                // to save the board
                // We only need to check live cells
                for (int r = 0; r < numRows; r++)
                {
                    for (int c = 0; c < numCols; c++)
                    {
                        if (tempBoard[r, c])
                        {
                            if (r < saveBox.Top)
                                saveBox.Top = r;
                            if (r > saveBox.Bottom)
                                saveBox.Bottom = r;
                            if (c < saveBox.Left)
                                saveBox.Left = c;
                            if (c > saveBox.Right)
                                saveBox.Right = c;
                        }
                    }
                }

                StringBuilder sb = new StringBuilder();
                for (int r = saveBox.Top; r <= saveBox.Bottom; r++)
                {
                    for (int c = saveBox.Left; c <= saveBox.Right; c++)
                    {
                        if (tempBoard[r, c])
                            sb.Append('O');
                        else
                            sb.Append('.');
                    }
                    if (r != saveBox.Bottom)
                        sb.AppendLine();
                }
                File.WriteAllText(saveDia.FileName, sb.ToString());
            }

        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Runs the game using my half-assed threading
        /// Wrapping is always on in this case.
        /// </summary>
        /// <param name="game">The board to start with</param>
        public static void ConsoleAutomataRunner(ConsoleAutomata game)
        {
            if (!game.Is_Initialized)
            {
                Console.ForegroundColor = MenuText.Info_FG;
                Console.Write("ERROR");
                return;
            }

            MenuText.PrintRunControls();

            var statusValues = new Dictionary<string, bool>();
            statusValues["Go"] = true;
            statusValues["Continuous"] = false;
            statusValues["Paused"] = true;
            statusValues["Wrapping"] = game.Is_Wrapping;
            statusValues["ExitPause"] = false;

            MenuText.PrintStatus(statusValues["Continuous"], statusValues["Paused"], statusValues["Wrapping"], __Curr_Speed_Index);

            game.PrintBoard();
            while (statusValues["Go"])
            {
                // If it isnt running, and no keys are pressed
                while (!Console.KeyAvailable && !statusValues["Continuous"])
                {
                    Thread.Sleep(10);
                }
                // if it IS running, and no keys are pressed
                while (!Console.KeyAvailable && statusValues["Continuous"])
                {
                    game.NextGeneration();
                    game.PrintBoard();
                    Thread.Sleep(__Speeds[__Curr_Speed_Index]);
                }

                //Catch the key press here
                ConsoleKeyInfo pressed = Console.ReadKey(true);
                if (pressed.Key == ConsoleKey.Spacebar)
                {
                    //If space is pressed and the game is not running continuously
                    if (!statusValues["Continuous"])
                    {
                        game.NextGeneration();
                        game.PrintBoard();
                    }
                    else //if space is pressed, pausing the game
                    {
                        statusValues["ExitPause"] = false;
                        statusValues["Paused"] = true;
                        MenuText.PrintStatus(statusValues["Continuous"], statusValues["Paused"], statusValues["Wrapping"], __Curr_Speed_Index);
                        while (!statusValues["ExitPause"])
                        {
                            while (!Console.KeyAvailable)
                            {
                                System.Threading.Thread.Sleep(10);
                            }
                            //If any key is pressed while the game is paused.
                            ConsoleKeyInfo pauseEntry = Console.ReadKey(true);
                            ConsoleRunHelper.HandleRunningInput(pauseEntry.Key, game, ref statusValues);
                        }
                    }
                }
                else
                {
                    //handle any other key pressed while the game is running.
                    ConsoleRunHelper.HandleRunningInput(pressed.Key, game, ref statusValues);
                }
            }

            Console.CursorVisible = false;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Handles all input while the game is running.
        /// </summary>
        /// <param name="pressed"></param>
        /// <param name="pauseLoop"></param>
        /// <returns></returns>
        private static void HandleRunningInput(ConsoleKey pressed, ConsoleAutomata currentGame, ref Dictionary<string, bool> currentStatus)
        {
            switch (pressed)
            {
                case ConsoleKey.R:
                    currentStatus["Continuous"] = !currentStatus["Continuous"];
                    if (currentStatus["Paused"])
                    {
                        currentStatus["ExitPause"] = true;
                        currentStatus["Paused"] = false;
                    }
                    break;
                case ConsoleKey.S:
                    if (!currentStatus["Continuous"] || currentStatus["Paused"])
                    {
                        SaveBoard(currentGame.Rows, currentGame.Cols, currentGame.Board);
                    }
                    break;
                case ConsoleKey.OemMinus:
                case ConsoleKey.Subtract:
                    if (__Curr_Speed_Index >= 1)
                    {
                        __Curr_Speed_Index -= 1;
                    }
                    break;
                case ConsoleKey.OemPlus:
                case ConsoleKey.Add:
                    if (__Curr_Speed_Index <= 3)
                    {
                        __Curr_Speed_Index += 1;
                    }
                    break;
                case ConsoleKey.Spacebar: 
                    //Unpause, will only hit if game is already paused.
                    currentStatus["ExitPause"] = true;
                    currentStatus["Paused"] = false;
                    break;
                case ConsoleKey.Escape:
                    currentStatus["Go"] = false;
                    currentStatus["ExitPause"] = true;
                    currentStatus["Paused"] = false;
                    break;
                default:
                    break;
            }
            MenuText.PrintStatus(currentStatus["Continuous"], currentStatus["Paused"], currentStatus["Wrapping"], ConsoleRunHelper.__Curr_Speed_Index);
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Gives the bounds of a rectangle of width popCols and height popRows
        /// centered on the given boardRow and boardCol.
        /// </summary>
        /// <returns></returns>
        private static Rect Center(int popRows, int popCols,
                                            int centerRow, int centerCol)
        {
            Rect bounds = new Rect();

            if (popRows % 2 == 0)
            {
                bounds.Top = centerRow - popRows / 2;
                bounds.Bottom = centerRow + popRows / 2;
            }
            else
            {
                bounds.Top = centerRow - popRows / 2;
                bounds.Bottom = (centerRow + popRows / 2) + 1;
            }


            if (popCols % 2 == 0)
            {
                bounds.Left = centerCol - popCols / 2;
                bounds.Right = centerCol + popCols / 2;
            }
            else
            {
                bounds.Left = centerCol - popCols / 2;
                bounds.Right = (centerCol + popCols / 2) + 1;
            }

            return bounds;
        }
//------------------------------------------------------------------------------
        public static void CalcBuilderBounds(Automata2D currentBoard)
        {
            __Valid_Lefts = Enumerable.Range(MenuText.Space, currentBoard.Console_Width - 2 * MenuText.Space);
            __Valid_Tops = Enumerable.Range(MenuText.Space, currentBoard.Console_Height - 2 * MenuText.Space);
        }
//------------------------------------------------------------------------------
    }
    //////////////////////////////////////////////////////////////////////////////////
}
