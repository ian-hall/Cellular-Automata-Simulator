﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Core_Automata.Rules;

namespace Core_Automata
{
    /// <summary>
    /// This class pretty much does everything. It sets up the console, 
    /// fills in the initial pop from the given BuildType and then 
    /// does all the checking for living/dying of the population.
    /// 
    /// </summary>

    class Automata2D : ConsoleAutomata
    {
        public enum BuildTypes { Random, Resource, User };

        private bool[,] _board;
        private const char _liveCell = '☺';
        private const char _deadCell = ' ';
        private readonly Rules2D.RuleDelegate _rule;
        private string _loadedPopulation = "";

        public override bool[,] BoardCopy
        {
            get
            {
                var temp = new bool[this.Rows, this.Cols];
                for(int r = 0; r < this.Rows; r++)
                {
                    for( int c = 0; c < this.Cols; c++ )
                    {
                        temp[r, c] = this._board[r, c];
                    }
                }
                return temp;
            }
        }
        //Values used only for Build2DBoard_user
        private IEnumerable<int> _validLefts;
        private IEnumerable<int> _validTops;
        private int _cursorLeft, _cursorRight;
        
        /// <summary>
        /// Constructor for the GoLBoard class. Size of the board will be based
        /// on the size of the console window...
        /// </summary>
        /// <param name="rowMax">Number of rows</param>
        /// <param name="colMax">Number of columns</param>
        private Automata2D(int rowMax, int colMax, string rule) : base(rowMax,colMax)
        {
            this._board = new bool[rowMax, colMax];
            this.CalcBuilderBounds();
            var chosenRule = Rules2D.RuleMethods.Where(fn => fn.Name.Contains(rule)).First();
            this._rule = (Rules2D.RuleDelegate)Delegate.CreateDelegate(typeof(Rules2D.RuleDelegate), chosenRule);
        }
        
        public static Automata2D InitializeAutomata(int rowMax, int colMax, BuildTypes bType, string rType, string res = null)
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
                //case BuildTypes.File:
                //    newAutomata2D.Build2DBoard_File();
                //    break;
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
            var infoStr = String.Format("Rule: {0}", rType);
            if (!String.IsNullOrEmpty(newAutomata2D._loadedPopulation))
            {
                infoStr = infoStr + String.Format("\tPop: {0}", newAutomata2D._loadedPopulation);
            }
            MenuHelper.PrintOnLine(2, infoStr.Replace('_', ' '));
            return newAutomata2D;
        } 
        
        /// <summary>
        /// Adds the next board values to a queue to be read from
        /// </summary>
        public override void NextGeneration()
        {
            var lastBoard = this._board;
            var nextBoard = new bool[Rows, Cols];
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    nextBoard[r, c] = this._rule(lastBoard, r, c);
                }
            }
            this._generation++;
            this._board = nextBoard;                  
        }
        
        /// <summary>
        /// Prints the game board to the console window.
        /// </summary>
        public override void PrintBoard()
        {
            Console.SetCursorPosition(0, 1);
            Console.Write(" ".PadRight(Console.WindowWidth));
            string write = "Generation " + this._generation;
            MenuHelper.PrintOnLine(1, write);

            Console.BackgroundColor = MenuHelper.DefaultBG;
            Console.ForegroundColor = MenuHelper.BoardFG;

            Console.SetCursorPosition(0, MenuHelper.Space);
            StringBuilder sb = new StringBuilder();
            for (int r = 0; r < this.Rows; r++)
            {
                sb.Append("    ║");
                for (int c = 0; c < this.Cols; c++)
                {
                    if (!_board[r, c])
                    {
                        sb.Append(Automata2D._deadCell);
                    }
                    else
                    {
                        sb.Append(Automata2D._liveCell);
                    }
                }
                sb.AppendLine("║");
            }
            Console.Write(sb);

            Console.BackgroundColor = MenuHelper.DefaultBG;
            Console.ForegroundColor = MenuHelper.DefaultFG;
        }
        
        
        //private methods used to construct the game board
        
        #region Builders
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
                    this._board[r, c] = (rand.Next() % 2 == 0);
                }
            }
        }

        /// <summary>
        /// Load the initial population from a file of 0s and 1s.
        /// This uses a Windows Forms OpenFileDialog to let the user select
        /// a file. The file is loaded into the center of the console window.
        /// </summary>
        /// TODO: disabled until i find another UI to use, or maybe just roll my own console based thing
        //private void Build2DBoard_File()
        //{
        //    MenuHelper.FileError errType = MenuHelper.FileError.Not_Loaded;
        //    var isValidFile = false;

        //    OpenFileDialog openWindow = new OpenFileDialog();
        //    string startingPop = null;
        //    if (openWindow.ShowDialog() == DialogResult.OK)
        //    {
        //        string filePath = openWindow.FileName;
        //        isValidFile = IsValidFileOrResource(filePath, this, out startingPop, out errType);
        //        if(isValidFile)
        //        {
        //            this.Loaded_Population = openWindow.SafeFileName;
        //        }
        //    }
        //    //no ELSE because it defaults to a file not loaded error

        //    if (isValidFile)
        //    {
        //        this.FillBoard(startingPop);
        //    }
        //    else
        //    {
        //        MenuHelper.PrintFileError(errType);
        //        bool keyPressed = false;
        //        while (!keyPressed)
        //        {
        //            if (!Console.KeyAvailable)
        //                System.Threading.Thread.Sleep(50);
        //            else
        //            {
        //                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
        //                    keyPressed = true;
        //            }
        //        }
        //        this.Build2DBoard_Random();
        //    }
        //}

        /// <summary>
        /// Builds the board from a resource
        /// </summary>
        /// <param name="res"></param>
        private void Build2DBoard_Resource(string res)
        {
            string startingPop;
            MenuHelper.FileError errType = MenuHelper.FileError.NotLoaded;
            var isValidResource = IsValidFileOrResource(res, this, out startingPop, out errType, true);

            if (isValidResource)
            {
                this._loadedPopulation = res;
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
                    error = MenuHelper.FileError.NotLoaded;
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
                var loadedResource = Core_Automata.LargePops.ResourceManager.GetString(filename);
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
        
        /// <summary>
        /// Makes sure there are only '.' and 'O' in a given string, used to 
        /// validate the file loaded in BuildFromFile()
        /// </summary>
        /// <param name="s">current string</param>
        /// <returns>True if the string is all '.' and 'O'</returns>
        /// TODO: Change this again to accept more file formats
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
        
        /// <summary>
        /// Builds the board from user input. This is going to be ugly...
        /// </summary>
        /// For pops: 1: Glider 2: Ship 3: Acorn 4: BlockLayer
        private void Build2DBoard_User()
        {
            Console.SetBufferSize(this._consoleWidth + 50, this._consoleHeight);
            Console.ForegroundColor = ConsoleColor.White;

            bool[,] tempBoard = new bool[_validTops.Count(), _validLefts.Count()];

            for (int i = 0; i < _validTops.Count(); i++)
            {
                for (int j = 0; j < _validLefts.Count(); j++)
                {
                    Console.SetCursorPosition(_validLefts.ElementAt(j), _validTops.ElementAt(i));
                    Console.Write('*');
                    tempBoard[i, j] = false;
                }
            }
            MenuHelper.DrawBorder();
            Console.ForegroundColor = MenuHelper.InfoFG;


            int positionPrintRow = MenuHelper.Space - 3;

            MenuHelper.PrintCreationControls();

            int blinkLeft = this._consoleWidth + 5;
            int charLeft = blinkLeft + 1;
            int extraTop = 2;

            _cursorLeft = _validLefts.ElementAt(_validLefts.Count() / 2);
            _cursorRight = _validTops.ElementAt(_validTops.Count() / 2);
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
                string positionStr = String.Format("Current position: ({0},{1})", _cursorRight - MenuHelper.Space, _cursorLeft - MenuHelper.Space);
                Console.SetCursorPosition(this._consoleWidth / 2 - positionStr.Length / 2, positionPrintRow);
                Console.Write(positionStr);
                Console.SetCursorPosition(0, 0);

                while (!Console.KeyAvailable)
                {
                    if (popLoaderMode)
                    {
                        //If a population is loaded, we blink the loaded population at idle
                        int storeBoardLeft = loadedPopBounds.Left + loadedPopBounds.Width + 1;
                        int storeBoardTop = loadedPopBounds.Top;

                        Console.MoveBufferArea(_cursorLeft, _cursorRight, loadedPopBounds.Width, loadedPopBounds.Height, storeBoardLeft, storeBoardTop);
                        Console.MoveBufferArea(loadedPopBounds.Left, loadedPopBounds.Top, loadedPopBounds.Width, loadedPopBounds.Height, _cursorLeft, _cursorRight);
                        System.Threading.Thread.Sleep(250);
                        Console.MoveBufferArea(_cursorLeft, _cursorRight, loadedPopBounds.Width, loadedPopBounds.Height, loadedPopBounds.Left, loadedPopBounds.Top);
                        Console.MoveBufferArea(storeBoardLeft, storeBoardTop, loadedPopBounds.Width, loadedPopBounds.Height, _cursorLeft, _cursorRight);
                        System.Threading.Thread.Sleep(150);
                    }
                    else
                    {
                        Console.MoveBufferArea(_cursorLeft, _cursorRight, 1, 1, charLeft, extraTop);
                        Console.MoveBufferArea(blinkLeft, extraTop, 1, 1, _cursorLeft, _cursorRight);
                        System.Threading.Thread.Sleep(150);
                        Console.MoveBufferArea(_cursorLeft, _cursorRight, 1, 1, blinkLeft, extraTop);
                        Console.MoveBufferArea(charLeft, extraTop, 1, 1, _cursorLeft, _cursorRight);
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
                        nextLeft = ++_cursorLeft;
                        if (popLoaderMode)
                        {
                            if (nextLeft >= (_validLefts.Last() - loadedPopBounds.Width) + 2)
                            {
                                nextLeft = _validLefts.Min();
                            }
                        }

                        if (!_validLefts.Contains(nextLeft))
                        {
                            nextLeft = _validLefts.Min();
                        }
                        _cursorLeft = nextLeft;
                        break;
                    case ConsoleKey.LeftArrow:
                        nextLeft = --_cursorLeft;
                        if (popLoaderMode)
                        {
                            if (!_validLefts.Contains(nextLeft))
                            {
                                nextLeft = (_validLefts.Last() - loadedPopBounds.Width) + 1;
                            }
                        }

                        if (!_validLefts.Contains(nextLeft))
                        {
                            nextLeft = _validLefts.Max();
                        }
                        _cursorLeft = nextLeft;
                        break;
                    case ConsoleKey.UpArrow:
                        nextTop = --_cursorRight;
                        if (popLoaderMode)
                        {
                            if (!_validTops.Contains(nextTop))
                            {
                                nextTop = (_validTops.Last() - loadedPopBounds.Height) + 1;
                            }
                        }

                        if (!_validTops.Contains(nextTop))
                        {
                            nextTop = _validTops.Max();
                        }
                        _cursorRight = nextTop;
                        break;
                    case ConsoleKey.DownArrow:
                        nextTop = ++_cursorRight;
                        if (popLoaderMode)
                        {
                            if (nextTop >= (_validTops.Last() - loadedPopBounds.Height) + 2)
                            {
                                nextTop = _validTops.Min();
                            }
                        }

                        if (!_validTops.Contains(nextTop))
                        {
                            nextTop = _validTops.Min();
                        }
                        _cursorRight = nextTop;
                        break;
                    case ConsoleKey.Spacebar:
                        if (popLoaderMode)
                        {
                            //If a population is loaded, we slam down the entire thing on spacebar press
                            Console.SetCursorPosition(0, 0);
                            int popRows = (loadedPopBounds.Bottom - loadedPopBounds.Top);
                            int popCols = (loadedPopBounds.Right - loadedPopBounds.Left);

                            for (int r = _cursorRight; r < _cursorRight + popRows; r++)
                            {
                                for (int c = _cursorLeft; c < _cursorLeft + popCols; c++)
                                {
                                    Console.SetCursorPosition(c, r);
                                    if (smallPopVals[r - _cursorRight][c - _cursorLeft])
                                    {
                                        if (tempBoard[r - MenuHelper.Space, c - MenuHelper.Space])
                                        {
                                            Console.ForegroundColor = MenuHelper.DefaultFG;
                                            Console.Write('*');
                                            tempBoard[r - MenuHelper.Space, c - MenuHelper.Space] = false;
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = MenuHelper.BuilderFG;
                                            Console.Write('█');
                                            tempBoard[r - MenuHelper.Space, c - MenuHelper.Space] = true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.SetCursorPosition(_cursorLeft, _cursorRight);
                            bool boardVal = !tempBoard[_cursorRight - MenuHelper.Space, _cursorLeft - MenuHelper.Space];
                            if (boardVal)
                            {
                                Console.ForegroundColor = MenuHelper.BuilderFG;
                                Console.Write('█');
                            }
                            else
                            {
                                Console.ForegroundColor = MenuHelper.DefaultFG;
                                Console.Write('*');
                            }
                            tempBoard[_cursorRight - MenuHelper.Space, _cursorLeft - MenuHelper.Space] = boardVal;
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
                        string smallPop = Core_Automata.BuilderPops.ResourceManager.GetString(MenuHelper.BuilderPops[keyVal - 1]);
                        if (popLoaderMode && (loadedPop == MenuHelper.BuilderPops[keyVal - 1]))
                        {
                            //if the button is pressed that corresponds to the already loaded population we either rotate or mirror
                            if (pressed.Modifiers == ConsoleModifiers.Control)
                            {
                                if (!MirrorBuilderPop(ref smallPopVals, ref loadedPopBounds))
                                {
                                    Console.SetCursorPosition(0, 0);
                                    Console.ForegroundColor = MenuHelper.InfoFG;
                                    Console.Write("Error while trying to mirror");
                                }

                            }
                            else
                            {
                                // Just check if the pop is not rotated, if it is rotated we do nothing
                                if (!RotateBuilderPop(ref smallPopVals, ref loadedPopBounds))
                                {
                                    Console.SetCursorPosition(0, 0);
                                    Console.ForegroundColor = MenuHelper.InfoFG;
                                    Console.Write("Rotating will go out of bounds");
                                }
                            }
                        }
                        else
                        {
                            if (BuilderLoadPop(smallPop, ref smallPopVals, ref loadedPopBounds))
                            {
                                loadedPop = MenuHelper.BuilderPops[keyVal - 1];
                                popLoaderMode = true;
                            }
                            else
                            {
                                Console.SetCursorPosition(0, 0);
                                Console.ForegroundColor = MenuHelper.InfoFG;
                                Console.Write("Cannot load pop outside of bounds");
                            }
                        }
                        break;
                    //case ConsoleKey.S:
                    //    ConsoleRunHelper.SaveBoard(Valid_Tops.Count(), Valid_Lefts.Count(), tempBoard);
                    //    break;
                    case ConsoleKey.C:
                        popLoaderMode = false;
                        loadedPop = null;
                        break;
                    default:
                        break;
                }
            }

            StringBuilder popString = new StringBuilder();
            for (int r = 0; r < _validTops.Count(); r++)
            {
                for (int c = 0; c < _validLefts.Count(); c++)
                {
                    if (tempBoard[r, c])
                        popString.Append('O');
                    else
                        popString.Append('.');
                }
                if (r != _validTops.Count() - 1)
                    popString.AppendLine();
            }

            Console.SetWindowSize(this._consoleWidth, this._consoleHeight);
            Console.SetBufferSize(this._consoleWidth, this._consoleHeight);

            Console.ForegroundColor = MenuHelper.DefaultFG;
            MenuHelper.ClearUnderBoard();
            MenuHelper.DrawBorder();

            MenuHelper.ClearLine(positionPrintRow);
            this.FillBoard(popString.ToString());
        }

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
            if ((_cursorLeft <= (_validLefts.Last() - colsNum) + 1) && (_cursorRight <= (_validTops.Last() - rowsNum) + 1))
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
                        Console.ForegroundColor = MenuHelper.InfoFG;
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

            if ((_cursorLeft <= (_validLefts.Last() - colsNum) + 1) && (_cursorRight <= (_validTops.Last() - rowsNum) + 1))
            {
                for (int r = tempBounds.Top; r < tempBounds.Bottom; r++)
                {
                    int popRow = r - tempBounds.Top;
                    for (int c = tempBounds.Left; c < tempBounds.Right; c++)
                    {
                        int popCol = c - tempBounds.Left;
                        Console.SetCursorPosition(c, r);
                        Console.ForegroundColor = MenuHelper.InfoFG;
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

            if ((_cursorLeft <= (_validLefts.Last() - colsNum) + 1) && (_cursorRight <= (_validTops.Last() - rowsNum) + 1))
            {
                for (int r = tempBounds.Top; r < tempBounds.Bottom; r++)
                {
                    int popRow = r - tempBounds.Top;
                    for (int c = tempBounds.Left; c < tempBounds.Right; c++)
                    {
                        int popCol = c - tempBounds.Left;
                        Console.SetCursorPosition(c, r);
                        Console.ForegroundColor = MenuHelper.InfoFG;
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
                        this._board[r, c] = false;
                    else
                        this._board[r, c] = true;
                }
            }
        }

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

        private void CalcBuilderBounds()
        {
            this._validLefts = Enumerable.Range(MenuHelper.Space, this._consoleWidth - 2 * MenuHelper.Space);
            this._validTops = Enumerable.Range(MenuHelper.Space, this._consoleHeight - 2 * MenuHelper.Space);
        }
        
        #endregion Builders
    } // end class
}