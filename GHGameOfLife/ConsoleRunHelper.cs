using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace GHGameOfLife
{
    class ConsoleRunHelper
    {
        private static int[] __Speeds = { 132, 100, 66, 50, 0 };
        private static int __Curr_Speed_Index = 2;
        private static IEnumerable<int> __Valid_Lefts;
        private static IEnumerable<int> __Valid_Tops;
        private static int __Cursor_Left, __Cursor_Top;
        //-----------------------------------------------------------------------------
        /// <summary>
        /// Default population is a random spattering of 0s and 1s
        /// Easy enough to get using (random int)%2
        /// </summary>
        public static bool[,] BuildGOLBoardRandom(GoL currentGame)
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
        public static bool[,] BuildGOLBoardFile(GoL currentGame)
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
                return ConsoleRunHelper.BuildGOLBoardRandom(currentGame);
            }
        }
        //------------------------------------------------------------------------------
        /// <summary>
        /// Builds the board from a resource
        /// TODO: Add the name of the resource to the screen
        /// </summary>
        /// <param name="res"></param>
        public static bool[,] BuildGOLBoardResource(string res, GoL currentGame)
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
                return ConsoleRunHelper.BuildGOLBoardRandom(currentGame);
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
        private static bool IsValidFileOrResource(string filename, GoL currentGame, out string popToLoad, out MenuText.FileError error, bool fromRes = false)
        {
            popToLoad = "";
            error = MenuText.FileError.None;
            if (fromRes)
            {
                var resourceByLine = Regex.Split(GHGameOfLife.LargePops.ResourceManager.GetString(filename), Environment.NewLine);
                var fileByLine = new List<string>();
                foreach (var line in resourceByLine)
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
                if (longestLine > currentGame.Rows)
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
            else
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

                List<string> fileByLine = new List<string>();
                using (StreamReader reader = new StreamReader(filename))
                {
                    // New way to read all the lines for checking...
                    // Skips newlines, also skips lines that are 
                    // probably comments
                    while (!reader.EndOfStream)
                    {
                        string temp = reader.ReadLine().Trim();
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
                }

                //Find the longest line in the file
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
        /// Used by files to fill the game board, centered
        /// </summary>
        /// <param name="startingPop"></param>
        public static bool[,] FillBoard(string startingPop,int rows, int cols)
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
        //------------------------------------------------------------------------------
        public static void CalcBuilderBounds(int origHeight, int origWidth)
        {
            __Valid_Lefts = Enumerable.Range(MenuText.Space, origWidth - 2 * MenuText.Space);
            __Valid_Tops = Enumerable.Range(MenuText.Space, origHeight - 2 * MenuText.Space);
        }
        //------------------------------------------------------------------------------
        /// <summary>
        /// Gives the bounds of a rectangle of width popCols and height popRows
        /// centered on the given boardRow and boardCol.
        /// </summary>
        /// <returns></returns>
        public static Rect Center(int popRows, int popCols,
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
    }
    //////////////////////////////////////////////////////////////////////////////////
}
