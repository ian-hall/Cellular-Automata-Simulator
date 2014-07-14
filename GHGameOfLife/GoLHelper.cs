﻿using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace GHGameOfLife
{
    /// <summary>
    /// Static class that keeps getting the next generation of the board.
    /// Will either prompt the user to step through or loop 
    /// Probably should add this to the GolBoard class
    /// or combine it or something
    /// </summary>
///////////////////////////////////////////////////////////////////////////////
    partial class GoL
    {
//-----------------------------------------------------------------------------
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
        private static class GoLHelper
        {
            private static int[] Speeds = { 132, 100, 66, 50, 33 };
            private static int Curr_Speed_Index = 2; //Start at a 66ms wait
            private static IEnumerable<int> validLeft;
            private static IEnumerable<int> validTop;
            private enum SmallPops { None, Glider, Ship };
            private static int CurLeft, CurTop;
//-----------------------------------------------------------------------------
            /// <summary>
            /// Default population is a random spattering of 0s and 1s
            /// Easy enough to get using (random int)%2
            /// </summary>
            public static void BuildBoardRandom()
            {
                Random rand = new Random();

                for (int r = 0; r < Rows; r++)
                {
                    for (int c = 0; c < Cols; c++)
                    {
                        Board[r, c] = (rand.Next() % 2 == 0);
                    }
                }
            }           
//------------------------------------------------------------------------------
            /// <summary>
            /// Load the initial population from a file of 0s and 1s.
            /// This uses a Windows Forms OpenFileDialog to let the user select
            /// a file. The file is loaded into the center of the console window.
            /// </summary>
            public static void BuildBoardFile()
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
                        FillBoard(startingPop);
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
                        GoLHelper.BuildBoardRandom();
                        break;
                }

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
            public static void BuildBoardResource(string pop)
            {
                var startingPop = GHGameOfLife.LargePops.ResourceManager.GetString(pop);

                FillBoard(startingPop);

                IsInitialized = true;
            }
//------------------------------------------------------------------------------
            /// <summary>
            /// Builds the board from user input. This is going to be ugly...
            /// </summary>
            public static void BuildBoardUser()
            {
                //SaveFileDialog saveDia = new SaveFileDialog();
                //saveDia.Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*";


                Console.SetBufferSize(OrigConsWidth * 2, OrigConsHeight);
                Console.ForegroundColor = ConsoleColor.White;

                //IEnumerable<int> validLeft = Enumerable.Range(Space, OrigConsWidth - 2 * Space);
                //IEnumerable<int> validTop = Enumerable.Range(Space, OrigConsHeight - 2 * Space);
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

                Console.ForegroundColor = MenuText.Info_FG;


                int positionPrintRow = Space - 3;

                MenuText.PrintCreationControls();

                int blinkLeft = OrigConsWidth + 5;
                int charLeft = blinkLeft + 1;
                int extraTop = 2;

                CurLeft = validLeft.ElementAt(validLeft.Count() / 2);
                CurTop = validTop.ElementAt(validTop.Count() / 2);
                int nextLeft;
                int nextTop;
                bool exit = false;
                Console.CursorVisible = false;


                Rect loadedPopBounds = new Rect();
                bool popLoaderMode = false;
                //bool smallPopLoaded = false;
                SmallPops loadedPop = SmallPops.None;
                bool[][] smallPopVals = new bool[0][];

                while (!exit)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    MenuText.ClearLine(Space - 3);
                    string positionStr = String.Format("Current position: ({0},{1})", CurTop - Space, CurLeft - Space);
                    Console.SetCursorPosition(OrigConsWidth / 2 - positionStr.Length / 2, positionPrintRow);
                    Console.Write(positionStr);
                    Console.SetCursorPosition(0, 0);

                    if (!popLoaderMode)
                    {
                        while (!Console.KeyAvailable)
                        {
                            Console.MoveBufferArea(CurLeft, CurTop, 1, 1, charLeft, extraTop);
                            Console.MoveBufferArea(blinkLeft, extraTop, 1, 1, CurLeft, CurTop);
                            System.Threading.Thread.Sleep(150);
                            Console.MoveBufferArea(CurLeft, CurTop, 1, 1, blinkLeft, extraTop);
                            Console.MoveBufferArea(charLeft, extraTop, 1, 1, CurLeft, CurTop);
                            System.Threading.Thread.Sleep(150);
                        }

                        MenuText.ClearLine(0);
                        ConsoleKey pressed = Console.ReadKey(true).Key;

                        if (pressed == ConsoleKey.Enter)
                        {
                            exit = true;
                            break;
                        }

                        if (pressed == ConsoleKey.RightArrow)
                        {
                            nextLeft = ++CurLeft;
                            if (!validLeft.Contains(nextLeft))
                                nextLeft = validLeft.Min();

                            CurLeft = nextLeft;
                        }

                        if (pressed == ConsoleKey.LeftArrow)
                        {
                            nextLeft = --CurLeft;
                            if (!validLeft.Contains(nextLeft))
                                nextLeft = validLeft.Max();

                            CurLeft = nextLeft;
                        }

                        if (pressed == ConsoleKey.UpArrow)
                        {
                            nextTop = --CurTop;
                            if (!validTop.Contains(nextTop))
                                nextTop = validTop.Max();

                            CurTop = nextTop;
                        }

                        if (pressed == ConsoleKey.DownArrow)
                        {
                            nextTop = ++CurTop;
                            if (!validTop.Contains(nextTop))
                                nextTop = validTop.Min();

                            CurTop = nextTop;
                        }

                        if (pressed == ConsoleKey.Spacebar)
                        {
                            Console.SetCursorPosition(CurLeft, CurTop);
                            bool boardVal = !tempBoard[CurTop - Space, CurLeft - Space];

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

                            tempBoard[CurTop - Space, CurLeft - Space] = boardVal;
                        }

                        /*
                         * TODO maybe change this to cycle through a few common small pops
                         * or change this to P and then numkeys to go through a few pops
                         * or something
                         * ALSO TODO: Add a check to the size of the loaded population
                         * before actually loading it.
                         */
                        if (pressed == ConsoleKey.D1)
                        {
                            string smallPop = GHGameOfLife.SmallPops.Glider;
                            if( BuilderLoadPop(smallPop, ref smallPopVals, ref loadedPopBounds/*, CurLeft, CurTop*/) )
                            {
                                loadedPop = SmallPops.Glider;
                                popLoaderMode = true;
                            }
                            else
                            {
                                Console.SetCursorPosition(0, 0);
                                Console.Write("Cannot load pop outside of bounds");
                                loadedPop = SmallPops.None;
                            }
                            /*
                            var testPop = GHGameOfLife.SmallPops.Glider;
                            loadedPopBounds = BuilderLoadPop(testPop, ref smallPopVals);
                            int loadedRows = (loadedPopBounds.Bottom - loadedPopBounds.Top);
                            int loadedCols = (loadedPopBounds.Right - loadedPopBounds.Left);

                            if ((curLeft <= (validLeft.Last() - loadedCols) + 1) && (curTop <= (validTop.Last() - loadedRows) + 1))
                            {
                                loaded = SmallPops.Glider;
                            }
                            else
                            {
                                Console.SetCursorPosition(0, 0);
                                Console.Write("Cannot load pop outside of bounds");
                                loaded = SmallPops.None;
                            }

                            if (loaded != SmallPops.None)
                                popLoaderMode = true;
                             */ 
                        }

                        if (pressed == ConsoleKey.D2)
                        {
                            string smallPop = GHGameOfLife.SmallPops.Smallship;
                            if (BuilderLoadPop(smallPop, ref smallPopVals, ref loadedPopBounds/*, CurLeft, CurTop*/))
                            {
                                loadedPop = SmallPops.Ship;
                                popLoaderMode = true;
                            }
                            else
                            {
                                Console.SetCursorPosition(0, 0);
                                Console.Write("Cannot load pop outside of bounds");
                                loadedPop = SmallPops.None;
                            }
                            /*
                            var testPop = GHGameOfLife.SmallPops.Smallship;
                            loadedPopBounds = BuilderLoadPop(testPop, ref smallPopVals);
                            int loadedRows = (loadedPopBounds.Bottom - loadedPopBounds.Top);
                            int loadedCols = (loadedPopBounds.Right - loadedPopBounds.Left);

                            if ((curLeft <= (validLeft.Last() - loadedCols) + 1) && (curTop <= (validTop.Last() - loadedRows) + 1))
                            {
                                loaded = SmallPops.Ship;
                            }
                            else
                            {
                                Console.SetCursorPosition(0, 0);
                                Console.Write("Cannot load pop outside of bounds");
                                loaded = SmallPops.None;
                            }

                            if (loaded != SmallPops.None)
                                popLoaderMode = true;
                             */
                        }

                        if (pressed == ConsoleKey.S)
                        {
                            SaveBoard(validTop.Count(), validLeft.Count(), tempBoard);                            
                        }
                    }
                    else
                    {
                        //int popWidth = loadedPopBounds.Right - loadedPopBounds.Left;
                        //int popHeight = loadedPopBounds.Bottom - loadedPopBounds.Top;

                        int storeBoardLeft = loadedPopBounds.Left + loadedPopBounds.Width + 1;
                        int storeBoardTop = loadedPopBounds.Top;


                        while (!Console.KeyAvailable)
                        {
                            Console.MoveBufferArea(CurLeft, CurTop, loadedPopBounds.Width, loadedPopBounds.Height, storeBoardLeft, storeBoardTop);
                            Console.MoveBufferArea(loadedPopBounds.Left, loadedPopBounds.Top, loadedPopBounds.Width, loadedPopBounds.Height, CurLeft, CurTop);
                            System.Threading.Thread.Sleep(250);
                            Console.MoveBufferArea(CurLeft, CurTop, loadedPopBounds.Width, loadedPopBounds.Height, loadedPopBounds.Left, loadedPopBounds.Top);
                            Console.MoveBufferArea(storeBoardLeft, storeBoardTop, loadedPopBounds.Width, loadedPopBounds.Height, CurLeft, CurTop);
                            System.Threading.Thread.Sleep(100);
                        }

                        MenuText.ClearLine(0);
                        ConsoleKey pressed = Console.ReadKey(true).Key;

                        if (pressed == ConsoleKey.D1)
                        {
                            if (loadedPop != SmallPops.Glider)
                            {
                                string smallPop = GHGameOfLife.SmallPops.Glider;
                                if (BuilderLoadPop(smallPop, ref smallPopVals, ref loadedPopBounds/*, CurLeft, CurTop*/))
                                {
                                    loadedPop = SmallPops.Glider;
                                    popLoaderMode = true;
                                }
                                else
                                {
                                    Console.SetCursorPosition(0, 0);
                                    Console.Write("Cannot load pop outside of bounds");
                                }
                                /*
                                var testPop = GHGameOfLife.SmallPops.Glider;
                                loadedPopBounds = BuilderLoadPop(testPop, ref smallPopVals);
                                int loadedRows = (loadedPopBounds.Bottom - loadedPopBounds.Top);
                                int loadedCols = (loadedPopBounds.Right - loadedPopBounds.Left);

                                if ((curLeft <= (validLeft.Last() - loadedCols) + 1) && (curTop <= (validTop.Last() - loadedRows) + 1))
                                {
                                    loaded = SmallPops.Glider;
                                }
                                else
                                {
                                    Console.SetCursorPosition(0, 0);
                                    Console.Write("Cannot load pop outside of bounds");
                                    //smallPopLoaded = false;
                                }
                                 */ 
                            }
                            else
                            {
                                // Just check if the pop is not rotated, if it is rotated we do nothing
                                if ( !RotateBuilderPop(ref smallPopVals, ref loadedPopBounds))
                                {
                                    Console.SetCursorPosition(0, 0);
                                    Console.Write("Rotating will go out of bounds");
                                }
                            }

                        }

                        if (pressed == ConsoleKey.D2)
                        {
                            if (loadedPop != SmallPops.Ship)
                            {
                                string smallPop = GHGameOfLife.SmallPops.Smallship;
                                if (BuilderLoadPop(smallPop, ref smallPopVals, ref loadedPopBounds/*, CurLeft, CurTop*/))
                                {
                                    loadedPop = SmallPops.Ship;
                                    popLoaderMode = true;
                                }
                                else
                                {
                                    Console.SetCursorPosition(0, 0);
                                    Console.Write("Cannot load pop outside of bounds");
                                    //loaded = SmallPops.None;
                                }
                                /*
                                var testPop = GHGameOfLife.SmallPops.Smallship;
                                loadedPopBounds = BuilderLoadPop(testPop, ref smallPopVals);
                                int loadedRows = (loadedPopBounds.Bottom - loadedPopBounds.Top);
                                int loadedCols = (loadedPopBounds.Right - loadedPopBounds.Left);

                                if ((curLeft <= (validLeft.Last() - loadedCols) + 1) && (curTop <= (validTop.Last() - loadedRows) + 1))
                                {
                                    loaded = SmallPops.Ship;
                                }
                                else
                                {
                                    Console.SetCursorPosition(0, 0);
                                    Console.Write("Cannot load pop outside of bounds");
                                    //smallPopLoaded = false;
                                }
                                 */ 
                            }
                            else
                            {
                                // Just check if the pop is not rotated, if it is rotated we do nothing
                                if (!RotateBuilderPop(ref smallPopVals, ref loadedPopBounds))
                                {
                                    Console.SetCursorPosition(0, 0);
                                    Console.Write("Rotating will go out of bounds");
                                }
                            }

                        }


                        //Go back to adding single values
                        if (pressed == ConsoleKey.C)
                        {
                            popLoaderMode = false;
                        }

                        if (pressed == ConsoleKey.RightArrow)
                        {
                            nextLeft = ++CurLeft;
                            if (nextLeft >= (validLeft.Last() - loadedPopBounds.Width) + 2)
                                nextLeft = validLeft.Min();

                            CurLeft = nextLeft;
                        }

                        if (pressed == ConsoleKey.LeftArrow)
                        {
                            nextLeft = --CurLeft;
                            if (!validLeft.Contains(nextLeft))
                                nextLeft = (validLeft.Last() - loadedPopBounds.Width) + 1;

                            CurLeft = nextLeft;
                        }

                        if (pressed == ConsoleKey.UpArrow)
                        {
                            nextTop = --CurTop;
                            if (!validTop.Contains(nextTop))
                                nextTop = (validTop.Last() - loadedPopBounds.Height) + 1;

                            CurTop = nextTop;
                        }

                        if (pressed == ConsoleKey.DownArrow)
                        {
                            nextTop = ++CurTop;
                            if (nextTop >= (validTop.Last() - loadedPopBounds.Height) + 2)
                                nextTop = validTop.Min();

                            CurTop = nextTop;
                        }


                        if (pressed == ConsoleKey.Spacebar)
                        {
                            Console.SetCursorPosition(0, 0);

                            int popRows = (loadedPopBounds.Bottom - loadedPopBounds.Top);
                            int popCols = (loadedPopBounds.Right - loadedPopBounds.Left);

                            for (int r = CurTop; r < CurTop + popRows; r++)
                            {
                                for (int c = CurLeft; c < CurLeft + popCols; c++)
                                {
                                    Console.SetCursorPosition(c, r);
                                    if (smallPopVals[r - CurTop][c - CurLeft])
                                    {
                                        if (tempBoard[r - Space, c - Space])
                                        {
                                            Console.ForegroundColor = MenuText.Default_FG;
                                            Console.Write('*');
                                            tempBoard[r - Space, c - Space] = false;
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = MenuText.Builder_FG;
                                            Console.Write('█');
                                            tempBoard[r - Space, c - Space] = true;
                                        }
                                    }
                                }
                            }
                        }

                        if (pressed == ConsoleKey.Enter)
                        {
                            exit = true;
                            break;
                        }


                        if (pressed == ConsoleKey.S)
                        {
                            SaveBoard(validTop.Count(), validLeft.Count(), tempBoard);
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
                    if (r != validTop.Count() - 1)
                        popString.AppendLine();
                }

                FillBoard(popString.ToString());
                Console.SetWindowSize(OrigConsWidth, OrigConsHeight);
                Console.SetBufferSize(OrigConsWidth, OrigConsHeight);

                MenuText.ClearUnderBoard();

                Console.ForegroundColor = MenuText.Default_FG;
                MenuText.ClearLine(positionPrintRow);
                IsInitialized = true;
            }
//------------------------------------------------------------------------------
            /// <summary>
            /// Used by files to fill the game board, cente
            /// </summary>
            /// <param name="startingPop"></param>
            /// <returns>Bounds of the pop loaded</returns>
            private static bool BuilderLoadPop(string pop, ref bool[][] popVals, ref Rect bounds/*, int curLeft, int curTop*/)
            {
                string[] popByLine = Regex.Split(pop, "\r\n");

                int midRow = OrigConsHeight / 2;
                int midCol = ((OrigConsWidth / 2)) + (OrigConsWidth);  //Buffer is 2 times window size during building

                int rowsNum = popByLine.Count();
                int colsNum = popByLine[0].Length;

                //int rowTop, rowBottom, colLeft, colRight;

                Rect tempBounds;

                bool loaded = false;

                //This code centers the population based on the given midRow/midCol
                if (rowsNum % 2 == 0)
                {
                    //rowTop = midRow - rowsNum / 2;
                    //rowBottom = midRow + rowsNum / 2;
                    tempBounds.Top = midRow - rowsNum / 2;
                    tempBounds.Bottom = midRow + rowsNum / 2;
                }
                else
                {
                    //rowTop = midRow - rowsNum / 2;
                    //rowBottom = (midRow + rowsNum / 2) + 1;
                    tempBounds.Top = midRow - rowsNum / 2;
                    tempBounds.Bottom = (midRow + rowsNum / 2) + 1;
                }


                if (colsNum % 2 == 0)
                {
                    //colLeft = midCol - colsNum / 2;
                    //colRight = midCol + colsNum / 2;
                    tempBounds.Left = midCol - colsNum / 2;
                    tempBounds.Right = midCol + colsNum / 2;
                }
                else
                {
                    //colLeft = midCol - colsNum / 2;
                    //colRight = (midCol + colsNum / 2) + 1;
                    tempBounds.Left = midCol - colsNum / 2;
                    tempBounds.Right = (midCol + colsNum / 2) + 1;
                }

                // Checks if the loaded pop is going to fit in the window at the current cursor position
                if ((CurLeft <= (validLeft.Last() - colsNum) + 1) && (CurTop <= (validTop.Last() - rowsNum) + 1))
                {                    
                    popVals = new bool[rowsNum][];
                    for (int r = tempBounds.Top; r < tempBounds.Bottom; r++)
                    {
                        int popRow = r - tempBounds.Top;
                        popVals[popRow] = new bool[colsNum];
                        for (int c = tempBounds.Left; c < tempBounds.Right; c++)
                        {
                            //int popRow = r - rowTop;
                            int popCol = c - tempBounds.Left;

                            int currPopVal = (int)Char.GetNumericValue(popByLine[popRow].ElementAt(popCol));

                            Console.SetCursorPosition(c, r);
                            Console.ForegroundColor = MenuText.Info_FG;
                            if (currPopVal == 1)
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
                    GC.Collect();
                    loaded = true;
                }

                //bounds.Left = colLeft;
                //bounds.Right = colRight;
                //bounds.Top = rowTop;
                //bounds.Bottom = rowBottom;

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

                int midRow = OrigConsHeight / 2;
                int midCol = ((OrigConsWidth / 2)) + (OrigConsWidth);  //Buffer is 2 times window size during building

                int rowsNum = rotated.Length;
                int colsNum = rotated[0].Length;

                bool loaded = false;
                Rect tempBounds;

                //int rowsNum = oldVals[0].Length;
                //int colsNum = oldVals.Length;

                //int rowTop, rowBottom, colLeft, colRight;
                

                if (rowsNum % 2 == 0)
                {
                    //rowTop = midRow - rowsNum / 2;
                    //rowBottom = midRow + rowsNum / 2;
                    tempBounds.Top = midRow - rowsNum / 2;
                    tempBounds.Bottom = midRow + rowsNum / 2;
                }
                else
                {
                    //rowTop = midRow - rowsNum / 2;
                    //rowBottom = (midRow + rowsNum / 2) + 1;
                    tempBounds.Top = midRow - rowsNum / 2;
                    tempBounds.Bottom = (midRow + rowsNum / 2) + 1;
                }


                if (colsNum % 2 == 0)
                {
                    //colLeft = midCol - colsNum / 2;
                    //colRight = midCol + colsNum / 2;
                    tempBounds.Left = midCol - colsNum / 2;
                    tempBounds.Right = midCol + colsNum / 2;
                }
                else
                {
                    //colLeft = midCol - colsNum / 2;
                    //colRight = (midCol + colsNum / 2) + 1;
                    tempBounds.Left = midCol - colsNum / 2;
                    tempBounds.Right = (midCol + colsNum / 2) + 1;
                }
                
                if ((CurLeft <= (validLeft.Last() - colsNum) + 1) && (CurTop <= (validTop.Last() - rowsNum) + 1))
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
                    GC.Collect();
                    loaded = true;
                }

                /*
                bounds.Left = colLeft;
                bounds.Right = colRight;
                bounds.Top = rowTop;
                bounds.Bottom = rowBottom;
                 */
              
                return loaded;
            }
//------------------------------------------------------------------------------
            /// <summary>
            /// Runs the game
            /// Starts paused, with stepping enabled
            /// </summary>
            /// <param name="game">The board to start with</param>
            public static void RunIt(GoL game)
            {
                if (!IsInitialized)
                {
                    Console.ForegroundColor = MenuText.Info_FG;
                    Console.Write("ERROR");
                    return;
                }

                MenuText.PrintRunControls();

                bool go = true;
                bool continuous = false;
                bool paused = true;
                bool wrapping = game.Wrap;
                MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                while (go)
                {
                    // If it isnt running, and no keys are pressed
                    while (!Console.KeyAvailable && !continuous)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                    // if it IS running, and no keys are pressed
                    while (!Console.KeyAvailable && continuous)
                    {
                        game.Next();
                        game.Print();
                        System.Threading.Thread.Sleep(Speeds[Curr_Speed_Index]);
                    }
                    //if PAUSE is pressed while it is not running
                    ConsoleKey pressed = Console.ReadKey(true).Key;
                    if (pressed == ConsoleKey.Spacebar && !continuous)
                    {
                        game.Next();
                        game.Print();
                    }

                    /// if paused while running, wait until space
                    /// is pressed again to start going
                    if (pressed == ConsoleKey.Spacebar && continuous)
                    {
                        bool exitPauseLoop = false;
                        paused = true;
                        MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                        while (/*paused*/!exitPauseLoop)
                        {
                            while (!Console.KeyAvailable)
                            {
                                System.Threading.Thread.Sleep(50);
                            }

                            ConsoleKey pauseEntry = Console.ReadKey(true).Key;
                            if (pauseEntry == ConsoleKey.Spacebar) //unpause
                            {
                                exitPauseLoop = true;
                                paused = false;
                                MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                            }
                            else if (pauseEntry == ConsoleKey.Escape) //exit
                            {
                                go = false;
                                paused = false;
                                exitPauseLoop = true;
                            }
                            else if (pauseEntry == ConsoleKey.R) // stop looping
                            {
                                continuous = !continuous;
                                exitPauseLoop = true;
                                //paused = !paused;
                                MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                            }
                            else if (pauseEntry == ConsoleKey.W) // toggle wrapping
                            {
                                wrapping = !wrapping;
                                game.Wrap = wrapping;
                                MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                            }
                            //These two change speed
                            else if (pauseEntry == ConsoleKey.OemMinus || pauseEntry == ConsoleKey.Subtract)
                            {
                                if (Curr_Speed_Index >= 1)
                                    Curr_Speed_Index -= 1;
                                MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                            }
                            else if (pauseEntry == ConsoleKey.OemPlus || pauseEntry == ConsoleKey.Add)
                            {
                                if (Curr_Speed_Index <= 3)
                                    Curr_Speed_Index += 1;
                                MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                            }
                        }
                    }

                    if (pressed == ConsoleKey.OemMinus || pressed == ConsoleKey.Subtract)
                    {
                        if (Curr_Speed_Index >= 1)
                            Curr_Speed_Index -= 1;
                        MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                    }

                    if (pressed == ConsoleKey.OemPlus || pressed == ConsoleKey.Add)
                    {
                        if (Curr_Speed_Index <= 3)
                            Curr_Speed_Index += 1;
                        MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                    }

                    if (pressed == ConsoleKey.R)
                    {
                        continuous = !continuous;
                        paused = !paused;
                        MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                    }

                    if (pressed == ConsoleKey.W)
                    {
                        wrapping = !wrapping;
                        game.Wrap = wrapping;
                        MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                    }

                    if (pressed == ConsoleKey.Escape)
                    {
                        go = false;
                    }
                }

                Console.CursorVisible = false;
            }
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
            /// <summary>
            /// Validates the selected file from the BuildFromFile() method.
            /// A Valid file is all 0s and 1s and does not have more rows or columns
            /// than the console window. The file must also be under 256KB
            /// </summary>
            /// <param name="filename">Path to a file to be checked</param>
            /// <param name="errType">The type of error returned</param>
            private static MenuText.FileError ValidateFile(string filename)
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
                    if (rows >= Rows)
                        return MenuText.FileError.Length;
                    // Error if the first line is too wide,
                    // 'cols' also used to check against all other lines
                    if (cols >= Cols)
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
            private static Boolean OnesAndZerosOnly(string s)
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
            /// <summary>
            /// Used by files to fill the game board, cente
            /// </summary>
            /// <param name="startingPop"></param>
            private static void FillBoard(string startingPop)
            {
                string[] popByLine = Regex.Split(startingPop, "\r\n");

                int midRow = Rows / 2;
                int midCol = Cols / 2;

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
                        //Board[r, c] = (int)Char.GetNumericValue(popByLine[popRow].ElementAt(popCol));
                        if ((int)Char.GetNumericValue(popByLine[popRow].ElementAt(popCol)) == 0)
                            Board[r, c] = false;
                        else
                            Board[r, c] = true;
                    }
                }
            }
//------------------------------------------------------------------------------
            private static void SaveBoard(int numRows, int numCols, bool[,] tempBoard)
            {
                SaveFileDialog saveDia = new SaveFileDialog();
                saveDia.Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*";

                if (saveDia.ShowDialog() == DialogResult.OK)
                {

                    int top = int.MaxValue;
                    int bottom = int.MinValue;
                    int left = int.MaxValue;
                    int right = int.MinValue;

                    // make a box that only includes the minimum needed lines
                    // to save the board
                    for (int r = 0; r < numRows; r++)
                    {
                        for (int c = 0; c < numCols; c++)
                        {
                            if (tempBoard[r, c])
                            {
                                if (r < top)
                                    top = r;
                                if (r > bottom)
                                    bottom = r;
                                if (c < left)
                                    left = c;
                                if (c > right)
                                    right = c;
                            }
                        }
                    }

                    StringBuilder sb = new StringBuilder();
                    for (int r = top; r <= bottom; r++)
                    {
                        for (int c = left; c <= right; c++)
                        {
                            if (tempBoard[r, c])
                                sb.Append('1');
                            else
                                sb.Append('0');
                        }
                        if (r != bottom)
                            sb.AppendLine();
                    }
                    File.WriteAllText(saveDia.FileName, sb.ToString());
                }

            }
//------------------------------------------------------------------------------
            public static void CalcBuilderBounds()
            {
                validLeft = Enumerable.Range(Space, OrigConsWidth - 2 * Space);
                validTop = Enumerable.Range(Space, OrigConsHeight - 2 * Space);
            }
//------------------------------------------------------------------------------
        }  // end class GoLHelper
//-----------------------------------------------------------------------------
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
    } // end class GoLBoard
///////////////////////////////////////////////////////////////////////////////
}