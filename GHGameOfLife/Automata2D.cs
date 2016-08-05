using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GHGameOfLife
{
    /// <summary>
    /// This class pretty much does everything. It sets up the console, 
    /// fills in the initial pop from a file or randomly, and then 
    /// does all the checking for living/dying of the population.
    /// </summary>
///////////////////////////////////////////////////////////////////////////////
    class Automata2D : ConsoleAutomata
    {
        delegate bool Rule2D(int row, int col);
        public enum BuildTypes { Random, File, Resource, User };
        public enum RuleTypes { Life, Life_Without_Death, Life_34, Seeds, Replicator, Day_And_Night,
                                Diamoeba };

        private bool[,] Board;
        private const char LIVE_CELL = '☺';
        private const char DEAD_CELL = ' ';
        private Rule2D Rule;

        public override bool[,] Board_Copy
        {
            get
            {
                var temp = new bool[this.Rows, this.Cols];
                for(int r = 0; r < this.Rows; r++)
                {
                    for( int c = 0; c < this.Cols; c++ )
                    {
                        temp[r, c] = this.Board[r, c];
                    }
                }
                return temp;
            }
        }

        //Values used only for Build2DBoard_user
        private IEnumerable<int> Valid_Lefts;
        private IEnumerable<int> Valid_Tops;
        private int Cursor_Left, Cursor_Top;
        //------------------------------------------------------------------------------
        /// <summary>
        /// Constructor for the GoLBoard class. Size of the board will be based
        /// off the size of the console window...
        /// </summary>
        /// <param name="rowMax">Number of rows</param>
        /// <param name="colMax">Number of columns</param>
        private Automata2D(int rowMax, int colMax, RuleTypes rule) : base(rowMax,colMax)
        {
            this.Board = new bool[rowMax, colMax];
            this.CalcBuilderBounds();
            switch(rule)
            {
                case RuleTypes.Life:
                    this.Rule = Life;
                    break;
                case RuleTypes.Life_Without_Death:
                    this.Rule = LifeWithoutDeath;
                    break;
                case RuleTypes.Seeds:
                    this.Rule = Seeds;
                    break;
                case RuleTypes.Replicator:
                    this.Rule = Replicator;
                    break;
                case RuleTypes.Day_And_Night:
                    this.Rule = DayAndNight;
                    break;
                case RuleTypes.Life_34:
                    this.Rule = Life34;
                    break;
                case RuleTypes.Diamoeba:
                    this.Rule = Diamoeba;
                    break;
                default:
                    this.Rule = Life;
                    break;
            }
        }
//------------------------------------------------------------------------------
        public static Automata2D InitializeAutomata(int rowMax, int colMax, BuildTypes bType, RuleTypes rType, string res = null)
        {
            var newAutomata2D = new Automata2D(rowMax, colMax, rType);
            switch (bType)
            {
                //Build a random population
                case BuildTypes.Random:
                    newAutomata2D.Build2DBoard_Random();
                    break;
                //Build a population from a CELLS-style file
                //defaults to random in case of an error
                case BuildTypes.File:
                    newAutomata2D.Build2DBoard_File();
                    break;
                //Build a population using one of the CELLS files that is stored as a resource
                //defaults to random in case of an error
                case BuildTypes.Resource:
                    newAutomata2D.Build2DBoard_Resource(res);
                    break;
                //Build a population based on user input
                case BuildTypes.User:
                    newAutomata2D.Build2DBoard_User();
                    break;
            }
            newAutomata2D.Is_Initialized = true;
            return newAutomata2D;
        } 
//------------------------------------------------------------------------------
        /// <summary>
        /// Adds the next board values to a queue to be read from
        /// </summary>
        public override void NextGeneration()
        {
            var lastBoard = this.Board;
            var nextBoard = new bool[Rows, Cols];
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    nextBoard[r, c] = this.Rule(r, c);
                }
            }
            this.Generation++;
            this.Board = nextBoard;                  
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Prints the game based on the threaded set up.
        /// Waits until there are at least 2 boards in the board queue and then 
        /// prints the next board in the queue. 
        /// </summary>
        public override void PrintBoard()
        {
            Console.SetCursorPosition(0, 1);
            Console.Write(" ".PadRight(Console.WindowWidth));
            string write = "Generation " + this.Generation;
            int left = (Console.WindowWidth / 2) - (write.Length / 2);
            Console.SetCursorPosition(left, 1);
            Console.Write(write);

            Console.BackgroundColor = MenuHelper.Default_BG;
            Console.ForegroundColor = MenuHelper.Board_FG;

            Console.SetCursorPosition(0, MenuHelper.Space);
            StringBuilder sb = new StringBuilder();
            for (int r = 0; r < this.Rows; r++)
            {
                sb.Append("    ║");
                for (int c = 0; c < this.Cols; c++)
                {
                    if (!Board[r, c])
                    {
                        sb.Append(Automata2D.DEAD_CELL);
                    }
                    else
                    {
                        sb.Append(Automata2D.LIVE_CELL);
                    }
                }
                sb.AppendLine("║");
            }
            Console.Write(sb);

            Console.BackgroundColor = MenuHelper.Default_BG;
            Console.ForegroundColor = MenuHelper.Default_FG;
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
        private bool Life(int r, int c)
        {
            var n = CountNeighbors_Moore(r, c);

            if(this.Board[r,c])
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
        private bool LifeWithoutDeath(int r, int c)
        {
            if (this.Board[r, c])
            {
                return true;
            }

            var n = CountNeighbors_Moore(r, c);

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
        private bool Seeds(int r, int c)
        {
            if (this.Board[r, c])
            {
                return false;
            }

            var n = CountNeighbors_Moore(r, c);

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
        private bool Replicator(int r, int c)
        {
            var n = CountNeighbors_Moore(r, c);

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
        private bool DayAndNight(int r, int c)
        {
            var n = CountNeighbors_Moore(r, c);

            if (this.Board[r, c])
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
        private bool Life34(int r, int c)
        {
            var n = CountNeighbors_Moore(r, c);

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
        private bool Diamoeba(int r, int c)
        {
            var n = CountNeighbors_Moore(r, c);

            if (this.Board[r, c])
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
        /// Counts number of true values in the Moore neighborhood of a point.
        /// </summary>
        /// <param name="r">Row value</param>
        /// <param name="c">Column value</param>
        /// <param name="range">How large the neighborhood is, default value of 1</param>
        /// <returns>number of neighbors</returns>
        private int CountNeighbors_Moore(int r, int c, int range=1)
        {
            if( range < 1 )
            {
                return this.Board[r, c] ? 1 : 0;
            }

            int n = 0;
            for( int i = r-range; i <= r+range; i++ )
            {
                for (int j = c - range; j <= c + range; j++)
                {
                    if( i==r && j==c )
                    {
                        continue;
                    }
                    var currRow = (i + this.Rows) % this.Rows;
                    var currCol = (j + this.Cols) % this.Cols;
                    if (this.Board[currRow, currCol]) n++;                  
                }
            }

            return n;
        }
//------------------------------------------------------------------------------
//private methods used to construct the game board
//------------------------------------------------------------------------------
        /// <summary>
        /// Default population is a random spattering of 0s and 1s
        /// Easy enough to get using (random int)%2
        /// </summary>
        private void Build2DBoard_Random()
        {
            var rand = new Random();
            var newBoard = new bool[this.Rows, this.Cols];
            for (int r = 0; r < this.Rows; r++)
            {
                for (int c = 0; c < this.Cols; c++)
                {
                    this.Board[r, c] = (rand.Next() % 2 == 0);
                }
            }
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Load the initial population from a file of 0s and 1s.
        /// This uses a Windows Forms OpenFileDialog to let the user select
        /// a file. The file is loaded into the center of the console window.
        /// </summary>
        private void Build2DBoard_File()
        {
            MenuHelper.FileError errType = MenuHelper.FileError.Not_Loaded;
            var isValidFile = false;

            OpenFileDialog openWindow = new OpenFileDialog();
            string startingPop = null;
            if (openWindow.ShowDialog() == DialogResult.OK)
            {
                string filePath = openWindow.FileName;
                isValidFile = IsValidFileOrResource(filePath, this, out startingPop, out errType);
            }
            //no ELSE because it defaults to a file not loaded error

            if (isValidFile)
            {
                this.FillBoard(startingPop);
            }
            else
            {
                MenuHelper.PrintFileError(errType);
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
                this.Build2DBoard_Random();
            }
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Builds the board from a resource
        /// TODO: Add the name of the resource to the screen
        /// </summary>
        /// <param name="res"></param>
        private void Build2DBoard_Resource(string res)
        {
            string startingPop;
            MenuHelper.FileError errType = MenuHelper.FileError.Not_Loaded;
            var isValidResource = IsValidFileOrResource(res, this, out startingPop, out errType, true);

            if (isValidResource)
            {
                this.FillBoard(startingPop);
            }
            else
            {
                MenuHelper.PrintFileError(errType);
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
                this.Build2DBoard_Random();
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
        private bool IsValidFileOrResource(string filename, Automata2D currentGame, out string popToLoad, out MenuHelper.FileError error, bool fromRes = false)
        {
            popToLoad = "";
            error = MenuHelper.FileError.None;
            var wholeFile = new List<string>();
            if (!fromRes)
            {
                // File should exist, but its good to make sure.
                FileInfo file = new FileInfo(filename);
                if (!file.Exists)
                {
                    error = MenuHelper.FileError.Not_Loaded;
                    return false;
                }

                // Checks if the file is empty or too large ( > 20KB )
                if (file.Length == 0 || file.Length > 20480)
                {
                    error = MenuHelper.FileError.Size;
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
                var loadedResource = GHGameOfLife.LargePops.ResourceManager.GetString(filename);
                wholeFile = Regex.Split(loadedResource, Environment.NewLine).ToList();
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
                error = MenuHelper.FileError.Length;
                return false;
            }
            if (longestLine > currentGame.Cols)
            {
                error = MenuHelper.FileError.Width;
                return false;
            }

            var sb = new StringBuilder();
            foreach (var line in fileByLine)
            {
                //Pad all lines to the same length as the longest for loading into the game board.
                var newLine = line.PadRight(longestLine, '.');
                if (!ValidLine(newLine))
                {
                    error = MenuHelper.FileError.Contents;
                    return false;
                }
                sb.AppendLine(newLine);
            }
            popToLoad = sb.ToString();
            error = MenuHelper.FileError.None;
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
        private bool ValidLine(string s)
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
        private void Build2DBoard_User()
        {
            Console.SetBufferSize(this.Console_Width + 50, this.Console_Height);
            Console.ForegroundColor = ConsoleColor.White;

            bool[,] tempBoard = new bool[Valid_Tops.Count(), Valid_Lefts.Count()];

            for (int i = 0; i < Valid_Tops.Count(); i++)
            {
                for (int j = 0; j < Valid_Lefts.Count(); j++)
                {
                    Console.SetCursorPosition(Valid_Lefts.ElementAt(j), Valid_Tops.ElementAt(i));
                    Console.Write('*');
                    tempBoard[i, j] = false;
                }
            }
            MenuHelper.DrawBorder();
            Console.ForegroundColor = MenuHelper.Info_FG;


            int positionPrintRow = MenuHelper.Space - 3;

            MenuHelper.PrintCreationControls();

            int blinkLeft = this.Console_Width + 5;
            int charLeft = blinkLeft + 1;
            int extraTop = 2;

            Cursor_Left = Valid_Lefts.ElementAt(Valid_Lefts.Count() / 2);
            Cursor_Top = Valid_Tops.ElementAt(Valid_Tops.Count() / 2);
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
                MenuHelper.ClearLine(MenuHelper.Space - 3);
                string positionStr = String.Format("Current position: ({0},{1})", Cursor_Top - MenuHelper.Space, Cursor_Left - MenuHelper.Space);
                Console.SetCursorPosition(this.Console_Width / 2 - positionStr.Length / 2, positionPrintRow);
                Console.Write(positionStr);
                Console.SetCursorPosition(0, 0);

                while (!Console.KeyAvailable)
                {
                    if (popLoaderMode)
                    {
                        //If a population is loaded, we blink the loaded population at idle
                        int storeBoardLeft = loadedPopBounds.Left + loadedPopBounds.Width + 1;
                        int storeBoardTop = loadedPopBounds.Top;

                        Console.MoveBufferArea(Cursor_Left, Cursor_Top, loadedPopBounds.Width, loadedPopBounds.Height, storeBoardLeft, storeBoardTop);
                        Console.MoveBufferArea(loadedPopBounds.Left, loadedPopBounds.Top, loadedPopBounds.Width, loadedPopBounds.Height, Cursor_Left, Cursor_Top);
                        System.Threading.Thread.Sleep(250);
                        Console.MoveBufferArea(Cursor_Left, Cursor_Top, loadedPopBounds.Width, loadedPopBounds.Height, loadedPopBounds.Left, loadedPopBounds.Top);
                        Console.MoveBufferArea(storeBoardLeft, storeBoardTop, loadedPopBounds.Width, loadedPopBounds.Height, Cursor_Left, Cursor_Top);
                        System.Threading.Thread.Sleep(150);
                    }
                    else
                    {
                        Console.MoveBufferArea(Cursor_Left, Cursor_Top, 1, 1, charLeft, extraTop);
                        Console.MoveBufferArea(blinkLeft, extraTop, 1, 1, Cursor_Left, Cursor_Top);
                        System.Threading.Thread.Sleep(150);
                        Console.MoveBufferArea(Cursor_Left, Cursor_Top, 1, 1, blinkLeft, extraTop);
                        Console.MoveBufferArea(charLeft, extraTop, 1, 1, Cursor_Left, Cursor_Top);
                        System.Threading.Thread.Sleep(150);
                    }

                }

                MenuHelper.ClearLine(0);
                ConsoleKeyInfo pressed = Console.ReadKey(true);

                switch (pressed.Key)
                {
                    case ConsoleKey.Enter:
                        exit = true;
                        continue;
                    case ConsoleKey.RightArrow:
                        nextLeft = ++Cursor_Left;
                        if (popLoaderMode)
                        {
                            if (nextLeft >= (Valid_Lefts.Last() - loadedPopBounds.Width) + 2)
                            {
                                nextLeft = Valid_Lefts.Min();
                            }
                        }

                        if (!Valid_Lefts.Contains(nextLeft))
                        {
                            nextLeft = Valid_Lefts.Min();
                        }
                        Cursor_Left = nextLeft;
                        break;
                    case ConsoleKey.LeftArrow:
                        nextLeft = --Cursor_Left;
                        if (popLoaderMode)
                        {
                            if (!Valid_Lefts.Contains(nextLeft))
                            {
                                nextLeft = (Valid_Lefts.Last() - loadedPopBounds.Width) + 1;
                            }
                        }

                        if (!Valid_Lefts.Contains(nextLeft))
                        {
                            nextLeft = Valid_Lefts.Max();
                        }
                        Cursor_Left = nextLeft;
                        break;
                    case ConsoleKey.UpArrow:
                        nextTop = --Cursor_Top;
                        if (popLoaderMode)
                        {
                            if (!Valid_Tops.Contains(nextTop))
                            {
                                nextTop = (Valid_Tops.Last() - loadedPopBounds.Height) + 1;
                            }
                        }

                        if (!Valid_Tops.Contains(nextTop))
                        {
                            nextTop = Valid_Tops.Max();
                        }
                        Cursor_Top = nextTop;
                        break;
                    case ConsoleKey.DownArrow:
                        nextTop = ++Cursor_Top;
                        if (popLoaderMode)
                        {
                            if (nextTop >= (Valid_Tops.Last() - loadedPopBounds.Height) + 2)
                            {
                                nextTop = Valid_Tops.Min();
                            }
                        }

                        if (!Valid_Tops.Contains(nextTop))
                        {
                            nextTop = Valid_Tops.Min();
                        }
                        Cursor_Top = nextTop;
                        break;
                    case ConsoleKey.Spacebar:
                        if (popLoaderMode)
                        {
                            //If a population is loaded, we slam down the entire thing on spacebar press
                            Console.SetCursorPosition(0, 0);
                            int popRows = (loadedPopBounds.Bottom - loadedPopBounds.Top);
                            int popCols = (loadedPopBounds.Right - loadedPopBounds.Left);

                            for (int r = Cursor_Top; r < Cursor_Top + popRows; r++)
                            {
                                for (int c = Cursor_Left; c < Cursor_Left + popCols; c++)
                                {
                                    Console.SetCursorPosition(c, r);
                                    if (smallPopVals[r - Cursor_Top][c - Cursor_Left])
                                    {
                                        if (tempBoard[r - MenuHelper.Space, c - MenuHelper.Space])
                                        {
                                            Console.ForegroundColor = MenuHelper.Default_FG;
                                            Console.Write('*');
                                            tempBoard[r - MenuHelper.Space, c - MenuHelper.Space] = false;
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = MenuHelper.Builder_FG;
                                            Console.Write('█');
                                            tempBoard[r - MenuHelper.Space, c - MenuHelper.Space] = true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.SetCursorPosition(Cursor_Left, Cursor_Top);
                            bool boardVal = !tempBoard[Cursor_Top - MenuHelper.Space, Cursor_Left - MenuHelper.Space];
                            if (boardVal)
                            {
                                Console.ForegroundColor = MenuHelper.Builder_FG;
                                Console.Write('█');
                            }
                            else
                            {
                                Console.ForegroundColor = MenuHelper.Default_FG;
                                Console.Write('*');
                            }
                            tempBoard[Cursor_Top - MenuHelper.Space, Cursor_Left - MenuHelper.Space] = boardVal;
                        }
                        break;
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        var keyVal = Int32.Parse("" + pressed.Key.ToString().Last());
                        string smallPop = GHGameOfLife.BuilderPops.ResourceManager.GetString(MenuHelper.Builder_Pops[keyVal - 1]);
                        if (popLoaderMode && (loadedPop == MenuHelper.Builder_Pops[keyVal - 1]))
                        {
                            //if the button is pressed that corresponds to the already loaded population we either rotate or mirror
                            if (pressed.Modifiers == ConsoleModifiers.Control)
                            {
                                if (!MirrorBuilderPop(ref smallPopVals, ref loadedPopBounds))
                                {
                                    Console.SetCursorPosition(0, 0);
                                    Console.ForegroundColor = MenuHelper.Info_FG;
                                    Console.Write("Error while trying to mirror");
                                }

                            }
                            else
                            {
                                // Just check if the pop is not rotated, if it is rotated we do nothing
                                if (!RotateBuilderPop(ref smallPopVals, ref loadedPopBounds))
                                {
                                    Console.SetCursorPosition(0, 0);
                                    Console.ForegroundColor = MenuHelper.Info_FG;
                                    Console.Write("Rotating will go out of bounds");
                                }
                            }
                        }
                        else
                        {
                            if (BuilderLoadPop(smallPop, ref smallPopVals, ref loadedPopBounds))
                            {
                                loadedPop = MenuHelper.Builder_Pops[keyVal - 1];
                                popLoaderMode = true;
                            }
                            else
                            {
                                Console.SetCursorPosition(0, 0);
                                Console.ForegroundColor = MenuHelper.Info_FG;
                                Console.Write("Cannot load pop outside of bounds");
                            }
                        }
                        break;
                    case ConsoleKey.S:
                        ConsoleRunHelper.SaveBoard(Valid_Tops.Count(), Valid_Lefts.Count(), tempBoard);
                        break;
                    case ConsoleKey.C:
                        popLoaderMode = false;
                        loadedPop = null;
                        break;
                    default:
                        break;
                }
            }

            StringBuilder popString = new StringBuilder();
            for (int r = 0; r < Valid_Tops.Count(); r++)
            {
                for (int c = 0; c < Valid_Lefts.Count(); c++)
                {
                    if (tempBoard[r, c])
                        popString.Append('O');
                    else
                        popString.Append('.');
                }
                if (r != Valid_Tops.Count() - 1)
                    popString.AppendLine();
            }

            Console.SetWindowSize(this.Console_Width, this.Console_Height);
            Console.SetBufferSize(this.Console_Width, this.Console_Height);

            Console.ForegroundColor = MenuHelper.Default_FG;
            MenuHelper.ClearUnderBoard();
            MenuHelper.DrawBorder();

            MenuHelper.ClearLine(positionPrintRow);
            this.FillBoard(popString.ToString());
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Loads the selected builder pop into the board
        /// </summary>
        /// <param name="startingPop"></param>
        /// <returns>Bounds of the pop loaded</returns>
        private bool BuilderLoadPop(string pop, ref bool[][] popVals, ref Rect bounds)
        {
            string[] popByLine = Regex.Split(pop, Environment.NewLine);

            int midRow = Console.BufferHeight / 2;
            int midCol = Console.BufferWidth - 25;

            int rowsNum = popByLine.Count();
            int colsNum = popByLine[0].Length;

            Rect tempBounds = Center(rowsNum, colsNum, midRow, midCol);

            bool loaded = false;

            // Checks if the loaded pop is going to fit in the window at the current cursor position
            if ((Cursor_Left <= (Valid_Lefts.Last() - colsNum) + 1) && (Cursor_Top <= (Valid_Tops.Last() - rowsNum) + 1))
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
                        Console.ForegroundColor = MenuHelper.Info_FG;
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
        private bool RotateBuilderPop(ref bool[][] popVals, ref Rect bounds)
        {
            bool[][] rotated = GenericHelp<bool>.Rotate90(popVals);

            int midRow = Console.BufferHeight / 2;
            int midCol = Console.BufferWidth - 25;

            int rowsNum = rotated.Length;
            int colsNum = rotated[0].Length;

            bool loaded = false;
            Rect tempBounds = Center(rowsNum, colsNum, midRow, midCol);

            if ((Cursor_Left <= (Valid_Lefts.Last() - colsNum) + 1) && (Cursor_Top <= (Valid_Tops.Last() - rowsNum) + 1))
            {
                for (int r = tempBounds.Top; r < tempBounds.Bottom; r++)
                {
                    int popRow = r - tempBounds.Top;
                    for (int c = tempBounds.Left; c < tempBounds.Right; c++)
                    {
                        int popCol = c - tempBounds.Left;
                        Console.SetCursorPosition(c, r);
                        Console.ForegroundColor = MenuHelper.Info_FG;
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
        private bool MirrorBuilderPop(ref bool[][] popVals, ref Rect bounds)
        {
            bool[][] rotated = GenericHelp<bool>.Mirror(popVals);

            int midRow = Console.BufferHeight / 2;
            int midCol = Console.BufferWidth - 25;

            int rowsNum = rotated.Length;
            int colsNum = rotated[0].Length;

            bool loaded = false;

            Rect tempBounds = Center(rowsNum, colsNum, midRow, midCol);

            if ((Cursor_Left <= (Valid_Lefts.Last() - colsNum) + 1) && (Cursor_Top <= (Valid_Tops.Last() - rowsNum) + 1))
            {
                for (int r = tempBounds.Top; r < tempBounds.Bottom; r++)
                {
                    int popRow = r - tempBounds.Top;
                    for (int c = tempBounds.Left; c < tempBounds.Right; c++)
                    {
                        int popCol = c - tempBounds.Left;
                        Console.SetCursorPosition(c, r);
                        Console.ForegroundColor = MenuHelper.Info_FG;
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
        private void FillBoard(string startingPop)
        {
            string[] popByLine = Regex.Split(startingPop, Environment.NewLine);
            //var newBoard = new bool[rows, cols];

            int midRow = this.Rows / 2;
            int midCol = this.Cols / 2;

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
                        this.Board[r, c] = false;
                    else
                        this.Board[r, c] = true;
                }
            }
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Gives the bounds of a rectangle of width popCols and height popRows
        /// centered on the given boardRow and boardCol.
        /// </summary>
        /// <returns></returns>
        private Rect Center(int popRows, int popCols,
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
        private void CalcBuilderBounds()
        {
            this.Valid_Lefts = Enumerable.Range(MenuHelper.Space, this.Console_Width - 2 * MenuHelper.Space);
            this.Valid_Tops = Enumerable.Range(MenuHelper.Space, this.Console_Height - 2 * MenuHelper.Space);
        }
//------------------------------------------------------------------------------
    } // end class
///////////////////////////////////////////////////////////////////////////////
}