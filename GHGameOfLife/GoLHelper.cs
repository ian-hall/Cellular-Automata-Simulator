using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;

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
        private class GoLHelper
        {
            private static int[] Speeds = { 132, 100, 66, 50, 0 };
            private static int Curr_Speed_Index = 2;
            private static IEnumerable<int> Valid_Left;
            private static IEnumerable<int> Valid_Top;
            private static int CurLeft, CurTop;
//-----------------------------------------------------------------------------
            /// <summary>
            /// Default population is a random spattering of 0s and 1s
            /// Easy enough to get using (random int)%2
            /// </summary>
            public static void BuildBoardRandom()
            {
                Random rand = new Random();

                for (int r = 0; r < GoL.Rows; r++)
                {
                    for (int c = 0; c < GoL.Cols; c++)
                    {
                        GoL.Board[r, c] = (rand.Next() % 2 == 0);
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
                string startingPop = null;
                if (openWindow.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openWindow.FileName;
                    errType = ValidateFileNew(filePath, out startingPop);
                }
                //no ELSE because it defaults to a file not loaded error

                switch (errType)
                {
                    // startingPop will not be null if this case is called
                    case MenuText.FileError.None:                        
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
                        Console.Write(MenuText.Press_Enter);

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

                GoL.IsInitialized = true;
            
            }
//------------------------------------------------------------------------------
            /// <summary>
            /// Builds the board from a resource
            /// TODO: Add the name of the resource to the screen
            /// </summary>
            /// <param name="pop"></param>
            public static void BuildBoardResource(string pop)
            {
                string startingPop;
                var errType = ValidateFileNew(pop, out startingPop, true);

                switch (errType)
                {
                    case MenuText.FileError.None:
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
                        Console.Write(MenuText.Press_Enter);

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

                GoL.IsInitialized = true;
            }
//------------------------------------------------------------------------------
            /// <summary>
            /// Builds the board from user input. This is going to be ugly...
            /// For pops: 1: Glider 2: Ship 3: Acorn 4: BlockLayer
            /// </summary>
            public static void BuildBoardUser()
            {
                //Console.SetBufferSize(GoL.OrigConsWidth * 2, GoL.OrigConsHeight);
                Console.SetBufferSize(GoL.OrigConsWidth + 50, GoL.OrigConsHeight);
                Console.ForegroundColor = ConsoleColor.White;

                //char horiz = '═';       // '\u2550'
                //char botLeft = '╚';     // '\u255A'
                //char botRight = '╝';    // '\u255D'

                bool[,] tempBoard = new bool[Valid_Top.Count(), Valid_Left.Count()];

                for (int i = 0; i < Valid_Top.Count(); i++)
                {
                    for (int j = 0; j < Valid_Left.Count(); j++)
                    {
                        Console.SetCursorPosition(Valid_Left.ElementAt(j), Valid_Top.ElementAt(i));
                        Console.Write('*');
                        tempBoard[i, j] = false;
                    }
                }

                //Console.SetCursorPosition(Valid_Left.First() - 1,Valid_Top.Last()+1);
                //Console.Write(botLeft);
                //for (int i = 0; i < Valid_Left.Count(); i++)
                //    Console.Write(horiz);
                //Console.Write(botRight);
                MenuText.DrawBorder();
                Console.ForegroundColor = MenuText.Info_FG;


                int positionPrintRow = MenuText.Space - 3;

                MenuText.PrintCreationControls();

                int blinkLeft = GoL.OrigConsWidth + 5;
                int charLeft = blinkLeft + 1;
                int extraTop = 2;

                CurLeft = Valid_Left.ElementAt(Valid_Left.Count() / 2);
                CurTop = Valid_Top.ElementAt(Valid_Top.Count() / 2);
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
                    string positionStr = String.Format("Current position: ({0},{1})", CurTop - MenuText.Space, CurLeft - MenuText.Space);
                    Console.SetCursorPosition(GoL.OrigConsWidth / 2 - positionStr.Length / 2, positionPrintRow);
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
                        ConsoleKeyInfo pressed = Console.ReadKey(true);

                        switch (pressed.Key)
                        {
                            case ConsoleKey.Enter:
                                exit = true;
                                continue;
                            case ConsoleKey.RightArrow:
                                nextLeft = ++CurLeft;
                                if (!Valid_Left.Contains(nextLeft))
                                {
                                    nextLeft = Valid_Left.Min();
                                }
                                CurLeft = nextLeft;
                                break;
                            case ConsoleKey.LeftArrow:
                                nextLeft = --CurLeft;
                                if (!Valid_Left.Contains(nextLeft))
                                {
                                    nextLeft = Valid_Left.Max();
                                }
                                CurLeft = nextLeft;
                                break;
                            case ConsoleKey.UpArrow:
                                nextTop = --CurTop;
                                if (!Valid_Top.Contains(nextTop))
                                { 
                                    nextTop = Valid_Top.Max(); 
                                }
                                CurTop = nextTop;
                                break;
                            case ConsoleKey.DownArrow:
                                nextTop = ++CurTop;
                                if (!Valid_Top.Contains(nextTop))
                                { 
                                    nextTop = Valid_Top.Min(); 
                                }
                                CurTop = nextTop;
                                break;
                            case ConsoleKey.Spacebar:
                                Console.SetCursorPosition(CurLeft, CurTop);
                                bool boardVal = !tempBoard[CurTop - MenuText.Space, CurLeft - MenuText.Space];

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

                                tempBoard[CurTop - MenuText.Space, CurLeft - MenuText.Space] = boardVal;
                                break;
                            case ConsoleKey.D1:
                                string smallPop = GHGameOfLife.BuilderPops.ResourceManager.GetString(MenuText.Builder_Pops[0]);
                                if( BuilderLoadPop(smallPop, ref smallPopVals, ref loadedPopBounds) )
                                {
                                    loadedPop = MenuText.Builder_Pops[0];
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
                            case ConsoleKey.D2:
                                smallPop = GHGameOfLife.BuilderPops.ResourceManager.GetString(MenuText.Builder_Pops[1]);
                                if (BuilderLoadPop(smallPop, ref smallPopVals, ref loadedPopBounds))
                                {
                                    loadedPop = MenuText.Builder_Pops[1];
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
                            case ConsoleKey.D3:
                                smallPop = GHGameOfLife.BuilderPops.ResourceManager.GetString(MenuText.Builder_Pops[2]);
                                if (BuilderLoadPop(smallPop, ref smallPopVals, ref loadedPopBounds))
                                {
                                    loadedPop = MenuText.Builder_Pops[2];
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
                            case ConsoleKey.D4:
                                smallPop = GHGameOfLife.BuilderPops.ResourceManager.GetString(MenuText.Builder_Pops[3]);
                                if (BuilderLoadPop(smallPop, ref smallPopVals, ref loadedPopBounds))
                                {
                                    loadedPop = MenuText.Builder_Pops[3];
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
                                SaveBoard(Valid_Top.Count(), Valid_Left.Count(), tempBoard);    
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
                            Console.MoveBufferArea(CurLeft, CurTop, loadedPopBounds.Width, loadedPopBounds.Height, storeBoardLeft, storeBoardTop);
                            Console.MoveBufferArea(loadedPopBounds.Left, loadedPopBounds.Top, loadedPopBounds.Width, loadedPopBounds.Height, CurLeft, CurTop);
                            System.Threading.Thread.Sleep(250);
                            Console.MoveBufferArea(CurLeft, CurTop, loadedPopBounds.Width, loadedPopBounds.Height, loadedPopBounds.Left, loadedPopBounds.Top);
                            Console.MoveBufferArea(storeBoardLeft, storeBoardTop, loadedPopBounds.Width, loadedPopBounds.Height, CurLeft, CurTop);
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
                                nextLeft = ++CurLeft;
                                if (nextLeft >= (Valid_Left.Last() - loadedPopBounds.Width) + 2)
                                {
                                    nextLeft = Valid_Left.Min();
                                }
                                CurLeft = nextLeft;
                                break;
                            case ConsoleKey.LeftArrow:
                                nextLeft = --CurLeft;
                                if (!Valid_Left.Contains(nextLeft))
                                {
                                    nextLeft = (Valid_Left.Last() - loadedPopBounds.Width) + 1;
                                }
                                CurLeft = nextLeft;
                                break;

                            case ConsoleKey.UpArrow:
                                nextTop = --CurTop;
                                if (!Valid_Top.Contains(nextTop))
                                {
                                    nextTop = (Valid_Top.Last() - loadedPopBounds.Height) + 1;
                                }
                                CurTop = nextTop;
                                break;

                            case ConsoleKey.DownArrow:
                                nextTop = ++CurTop;
                                if (nextTop >= (Valid_Top.Last() - loadedPopBounds.Height) + 2)
                                {
                                    nextTop = Valid_Top.Min();
                                }
                                CurTop = nextTop;
                                break;
                            case ConsoleKey.Spacebar:
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
                                if (loadedPop != MenuText.Builder_Pops[0])
                                {
                                    string smallPop = GHGameOfLife.BuilderPops.ResourceManager.GetString(MenuText.Builder_Pops[0]);
                                    if (BuilderLoadPop(smallPop, ref smallPopVals, ref loadedPopBounds))
                                    {
                                        loadedPop = MenuText.Builder_Pops[0];
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
                            case ConsoleKey.D2:
                                if (loadedPop != MenuText.Builder_Pops[1])
                                {
                                    string smallPop = GHGameOfLife.BuilderPops.ResourceManager.GetString(MenuText.Builder_Pops[1]);
                                    if (BuilderLoadPop(smallPop, ref smallPopVals, ref loadedPopBounds))
                                    {
                                        loadedPop = MenuText.Builder_Pops[1];
                                        popLoaderMode = true;
                                    }
                                    else
                                    {
                                        Console.SetCursorPosition(0, 0);
                                        Console.ForegroundColor = MenuText.Info_FG;
                                        Console.Write("Cannot load pop outside of bounds");
                                    }
                                }
                                else
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
                            case ConsoleKey.D3:
                                if (loadedPop != MenuText.Builder_Pops[2])
                                {
                                    string smallPop = GHGameOfLife.BuilderPops.ResourceManager.GetString(MenuText.Builder_Pops[2]);
                                    if (BuilderLoadPop(smallPop, ref smallPopVals, ref loadedPopBounds))
                                    {
                                        loadedPop = MenuText.Builder_Pops[2];
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
                            case ConsoleKey.D4:
                                if (loadedPop != MenuText.Builder_Pops[3])
                                {
                                    string smallPop = GHGameOfLife.BuilderPops.ResourceManager.GetString(MenuText.Builder_Pops[3]);
                                    if (BuilderLoadPop(smallPop, ref smallPopVals, ref loadedPopBounds))
                                    {
                                        loadedPop = MenuText.Builder_Pops[3];
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
                                SaveBoard(Valid_Top.Count(), Valid_Left.Count(), tempBoard);
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
                for (int r = 0; r < Valid_Top.Count(); r++)
                {
                    for (int c = 0; c < Valid_Left.Count(); c++)
                    {
                        if (tempBoard[r, c])
                            popString.Append('O');
                        else
                            popString.Append('.');
                    }
                    if (r != Valid_Top.Count() - 1)
                        popString.AppendLine();
                }

                FillBoard(popString.ToString());
                Console.SetWindowSize(GoL.OrigConsWidth, GoL.OrigConsHeight);
                Console.SetBufferSize(GoL.OrigConsWidth, GoL.OrigConsHeight);

                Console.ForegroundColor = MenuText.Default_FG;
                MenuText.ClearUnderBoard();
                MenuText.DrawBorder();

                MenuText.ClearLine(positionPrintRow);
                GoL.IsInitialized = true;
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

                int midRow = Console.BufferHeight/2;
                int midCol = Console.BufferWidth - 25;

                int rowsNum = popByLine.Count();
                int colsNum = popByLine[0].Length;

                Rect tempBounds = Center(rowsNum, colsNum, midRow, midCol);

                bool loaded = false;

                // Checks if the loaded pop is going to fit in the window at the current cursor position
                if ((CurLeft <= (Valid_Left.Last() - colsNum) + 1) && (CurTop <= (Valid_Top.Last() - rowsNum) + 1))
                {                    
                    popVals = new bool[rowsNum][];
                    for (int r = tempBounds.Top; r < tempBounds.Bottom; r++)
                    {
                        int popRow = r - tempBounds.Top;
                        popVals[popRow] = new bool[colsNum];
                        for (int c = tempBounds.Left; c < tempBounds.Right; c++)
                        {
                            int popCol = c - tempBounds.Left;

                            //int currPopVal = (int)Char.GetNumericValue(popByLine[popRow].ElementAt(popCol));

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

                int midRow = Console.BufferHeight/2;
                int midCol = Console.BufferWidth - 25;

                int rowsNum = rotated.Length;
                int colsNum = rotated[0].Length;

                bool loaded = false;
                Rect tempBounds = Center(rowsNum, colsNum, midRow, midCol);
                
                if ((CurLeft <= (Valid_Left.Last() - colsNum) + 1) && (CurTop <= (Valid_Top.Last() - rowsNum) + 1))
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

                if ((CurLeft <= (Valid_Left.Last() - colsNum) + 1) && (CurTop <= (Valid_Top.Last() - rowsNum) + 1))
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
            /// Runs the game
            /// </summary>
            /// <param name="game">The board to start with</param>
            /*public static void RunIt(GoL game)
            {
                if (!IsInitialized)
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
                statusValues["Wrapping"] = game.Wrap;
                statusValues["ExitPause"] = false;

                MenuText.PrintStatus(statusValues["Continuous"], statusValues["Paused"], statusValues["Wrapping"], Curr_Speed_Index);
                while (statusValues["Go"])
                {
                    // If it isnt running, and no keys are pressed
                    while (!Console.KeyAvailable && !statusValues["Continuous"])
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                    // if it IS running, and no keys are pressed
                    while (!Console.KeyAvailable && statusValues["Continuous"])
                    {
                        game.Next();
                        game.Print();
                        System.Threading.Thread.Sleep(Speeds[Curr_Speed_Index]);
                    }
                    
                    //Catch the key press here
                    ConsoleKeyInfo pressed = Console.ReadKey(true);
                    if (pressed.Key == ConsoleKey.Spacebar)
                    {
                        //If space is pressed and the game is not running continuously
                        if (!statusValues["Continuous"])
                        {
                            game.Next();
                            game.Print();
                        }
                        else //if space is pressed, pausing the game
                        {
                            statusValues["ExitPause"] = false;
                            statusValues["Paused"] = true;
                            MenuText.PrintStatus(statusValues["Continuous"], statusValues["Paused"], statusValues["Wrapping"], Curr_Speed_Index);
                            while(!statusValues["ExitPause"])
                            {
                                while (!Console.KeyAvailable)
                                {
                                    System.Threading.Thread.Sleep(50);
                                }
                                //If any key is pressed while the game is paused.
                                ConsoleKeyInfo pauseEntry = Console.ReadKey(true);
                                GoLHelper.HandleRunningInput(pauseEntry.Key, ref statusValues);
                                if(pauseEntry.Key == ConsoleKey.W)
                                {
                                    game.Wrap = statusValues["Wrapping"];
                                }
                            }
                        }
                    }
                    else
                    {
                        //handle any other key pressed while the game is running.
                        GoLHelper.HandleRunningInput(pressed.Key, ref statusValues);
                        if(pressed.Key == ConsoleKey.W )
                        {
                            game.Wrap = statusValues["Wrapping"];
                        }
                    }
                }

                Console.CursorVisible = false;
            }*/
//------------------------------------------------------------------------------
        /// <summary>
        /// Handles all 
        /// </summary>
        /// <param name="pressed"></param>
        /// <param name="pauseLoop"></param>
        /// <returns></returns>
            private static void HandleRunningInput(ConsoleKey pressed, ref Dictionary<string,bool> currentStatus, bool threadedHandler = false)
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
                    case ConsoleKey.W:
                        if(threadedHandler)
                        {
                            break;
                        }
                        if (!currentStatus["Continuous"] || currentStatus["Paused"])
                        {
                            currentStatus["Wrapping"] = !currentStatus["Wrapping"];
                        }
                        break;
                    case ConsoleKey.S:
                        if (!currentStatus["Continuous"] || currentStatus["Paused"])
                        {
                            SaveBoard(GoL.Rows, GoL.Cols, GoL.Board);
                        }
                        break;
                    case ConsoleKey.OemMinus:
                    case ConsoleKey.Subtract:
                        if (Curr_Speed_Index >= 1)
                        {
                            Curr_Speed_Index -= 1;
                        }
                        break;
                    case ConsoleKey.OemPlus:
                    case ConsoleKey.Add:
                        if (Curr_Speed_Index <= 3)
                        {
                            Curr_Speed_Index += 1;
                        }
                        break;
                    case ConsoleKey.Spacebar: //Unpause, will only hit if game is already paused.
                        currentStatus["ExitPause"] = true;
                        currentStatus["Paused"] = false;
                        break;
                    case ConsoleKey.Escape:
                        currentStatus["Go"] = false;
                        currentStatus["ExitPause"] = true;
                        currentStatus["Paused"] = false;

                        GoL.StopLock.WaitOne();
                        GoL.StopNext = true;
                        GoL.StopLock.ReleaseMutex();
                        break;
                    default:
                        break;
                }
                MenuText.PrintStatus(currentStatus["Continuous"], currentStatus["Paused"], currentStatus["Wrapping"], Curr_Speed_Index);
            }
//------------------------------------------------------------------------------
            /// <summary>
            /// Runs the game using my half-assed threading
            /// Wrapping is always on in this case.
            /// </summary>
            /// <param name="game">The board to start with</param>
            public static void ThreadedRunner(GoL game)
            {
                if (!IsInitialized)
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
                statusValues["Wrapping"] = true;
                statusValues["ExitPause"] = false;

                MenuText.PrintStatus(statusValues["Continuous"], statusValues["Paused"], statusValues["Wrapping"], Curr_Speed_Index);

                var boardRunner = new Thread(GoL.ThreadedNext);
                boardRunner.Start();
                game.ThreadedPrint();
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
                        game.ThreadedPrint();
                        Thread.Sleep(Speeds[Curr_Speed_Index]);
                    }

                    //Catch the key press here
                    ConsoleKeyInfo pressed = Console.ReadKey(true);
                    if (pressed.Key == ConsoleKey.Spacebar)
                    {
                        //If space is pressed and the game is not running continuously
                        if (!statusValues["Continuous"])
                        {
                            game.ThreadedPrint();
                        }
                        else //if space is pressed, pausing the game
                        {
                            statusValues["ExitPause"] = false;
                            statusValues["Paused"] = true;
                            MenuText.PrintStatus(statusValues["Continuous"], statusValues["Paused"], statusValues["Wrapping"], Curr_Speed_Index);
                            while (!statusValues["ExitPause"])
                            {
                                while (!Console.KeyAvailable)
                                {
                                    System.Threading.Thread.Sleep(50);
                                }
                                //If any key is pressed while the game is paused.
                                ConsoleKeyInfo pauseEntry = Console.ReadKey(true);
                                GoLHelper.HandleRunningInput(pauseEntry.Key, ref statusValues,true);
                                if (pauseEntry.Key == ConsoleKey.W)
                                {
                                    game.Wrap = statusValues["Wrapping"];
                                }
                            }
                        }
                    }
                    else
                    {
                        //handle any other key pressed while the game is running.
                        GoLHelper.HandleRunningInput(pressed.Key, ref statusValues,true);
                        if (pressed.Key == ConsoleKey.W)
                        {
                            game.Wrap = statusValues["Wrapping"];
                        }
                    }
                }

                Console.CursorVisible = false;
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
            private static MenuText.FileError ValidateFileNew(string filename, out string popToLoad,bool fromRes = false)
            {
                popToLoad = "";
                if (fromRes)
                {
                    var resourceByLine = Regex.Split(GHGameOfLife.LargePops.ResourceManager.GetString(filename),Environment.NewLine);
                    var fileByLine = new List<string>();
                    foreach(var line in resourceByLine)
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
                    var rows = fileByLine.Count;

                    if (rows > GoL.Rows)
                    {
                        return MenuText.FileError.Length;
                    }
                    if (longestLine > GoL.Cols)
                    {
                        return MenuText.FileError.Width;
                    }

                    var sb = new StringBuilder();
                    foreach (var line in fileByLine)
                    {
                        //Pad all lines to the same length as the longest for loading into the game board.
                        var newLine = line.PadRight(longestLine, '.');
                        if (!ValidLine(newLine))
                        {
                            return MenuText.FileError.Contents;
                        }
                        sb.AppendLine(newLine);
                    }
                    popToLoad = sb.ToString();
                    return MenuText.FileError.None;
                }
                else
                {
                    // File should exist, but its good to make sure.
                    FileInfo file = new FileInfo(filename);
                    if (!file.Exists)
                    {
                        return MenuText.FileError.Not_Loaded;
                    }

                    // Checks if the file is empty or too large ( > 20KB )
                    if (file.Length == 0 || file.Length > 20480)
                    {
                        return MenuText.FileError.Size;
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
                    var rows = fileByLine.Count;

                    if (rows > GoL.Rows)
                    {
                        return MenuText.FileError.Length;
                    }
                    if (longestLine > GoL.Cols)
                    {
                        return MenuText.FileError.Width;
                    }

                    var sb = new StringBuilder();
                    foreach (var line in fileByLine)
                    {
                        //Pad all lines to the same length as the longest for loading into the game board.
                        var newLine = line.PadRight(longestLine, '.');
                        if (!ValidLine(newLine))
                        {
                            return MenuText.FileError.Contents;
                        }
                        sb.AppendLine(newLine);
                    }
                    popToLoad = sb.ToString();
                    return MenuText.FileError.None;
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
            private static void FillBoard(string startingPop)
            {
                string[] popByLine = Regex.Split(startingPop, Environment.NewLine);

                int midRow = GoL.Rows / 2;
                int midCol = GoL.Cols / 2;

                int rowsNum = popByLine.Count();
                int colsNum = popByLine[0].Length;

                /* I somehow introduced a bug here where I'm getting a newline
                 * at the end of the string when I am loading a file from the
                 * user. This simply throws that line away. 
                 */ 
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
                            GoL.Board[r, c] = false;
                        else
                            GoL.Board[r, c] = true;
                    }
                }
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
            public static void CalcBuilderBounds()
            {
                Valid_Left = Enumerable.Range(MenuText.Space, GoL.OrigConsWidth - 2 * MenuText.Space);
                Valid_Top = Enumerable.Range(MenuText.Space, GoL.OrigConsHeight - 2 * MenuText.Space);
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
//-----------------------------------------------------------------------------
        }  // end class GoLHelper
//-----------------------------------------------------------------------------
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
    } // end class GoLBoard
///////////////////////////////////////////////////////////////////////////////
}