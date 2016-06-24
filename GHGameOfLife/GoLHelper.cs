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
//------------------------------------------------------------------------------
            /// <summary>
            /// Builds the board from user input. This is going to be ugly...
            /// For pops: 1: Glider 2: Ship 3: Acorn 4: BlockLayer
            /// </summary>
            public static void BuildBoardUser(GoL currentGame)
            {
                Console.SetBufferSize(currentGame.OrigConsWidth + 50, currentGame.OrigConsHeight);
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
                MenuText.DrawBorder();
                Console.ForegroundColor = MenuText.Info_FG;


                int positionPrintRow = MenuText.Space - 3;

                MenuText.PrintCreationControls();

                int blinkLeft = currentGame.OrigConsWidth + 5;
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
                    Console.SetCursorPosition(currentGame.OrigConsWidth / 2 - positionStr.Length / 2, positionPrintRow);
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
                            case ConsoleKey.D2:
                            case ConsoleKey.D3:
                            case ConsoleKey.D4:
                                var keyNum = pressed.Key.ToString()[1];
                                var keyVal = Int32.Parse("" + keyNum);
                                string smallPop = GHGameOfLife.BuilderPops.ResourceManager.GetString(MenuText.Builder_Pops[keyVal-1]);
                                if (BuilderLoadPop(smallPop, ref smallPopVals, ref loadedPopBounds))
                                {
                                    loadedPop = MenuText.Builder_Pops[keyVal-1];
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
                            case ConsoleKey.D2:
                            case ConsoleKey.D3:
                            case ConsoleKey.D4:
                                var keyNum = pressed.Key.ToString()[1];
                                var keyVal = Int32.Parse("" + keyNum);
                                if (loadedPop != MenuText.Builder_Pops[keyVal-1])
                                {
                                    string smallPop = GHGameOfLife.BuilderPops.ResourceManager.GetString(MenuText.Builder_Pops[keyVal-1]);
                                    if (BuilderLoadPop(smallPop, ref smallPopVals, ref loadedPopBounds))
                                    {
                                        loadedPop = MenuText.Builder_Pops[keyVal-1];
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

                ConsoleRunHelper.FillBoard(popString.ToString(),currentGame.Rows,currentGame.Cols);
                Console.SetWindowSize(currentGame.OrigConsWidth, currentGame.OrigConsHeight);
                Console.SetBufferSize(currentGame.OrigConsWidth, currentGame.OrigConsHeight);

                Console.ForegroundColor = MenuText.Default_FG;
                MenuText.ClearUnderBoard();
                MenuText.DrawBorder();

                MenuText.ClearLine(positionPrintRow);
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
        /// Handles all 
        /// </summary>
        /// <param name="pressed"></param>
        /// <param name="pauseLoop"></param>
        /// <returns></returns>
            private static void HandleRunningInput(ConsoleKey pressed, GoL currentGame, ref Dictionary<string,bool> currentStatus, bool threadedHandler = false)
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
                if (!game.IsInitialized)
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
                        game.ThreadedNext();
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
                            game.ThreadedNext();
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
                                    System.Threading.Thread.Sleep(10);
                                }
                                //If any key is pressed while the game is paused.
                                ConsoleKeyInfo pauseEntry = Console.ReadKey(true);
                                GoLHelper.HandleRunningInput(pauseEntry.Key,game,ref statusValues,true);
                            }
                        }
                    }
                    else
                    {
                        //handle any other key pressed while the game is running.
                        GoLHelper.HandleRunningInput(pressed.Key,game, ref statusValues,true);
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
            /// Used by files to fill the game board, centered
            /// </summary>
            /// <param name="startingPop"></param>
            //private static bool[,] FillBoard(string startingPop, GoL current)
            //{
            //    string[] popByLine = Regex.Split(startingPop, Environment.NewLine);

            //    int midRow = current.Rows / 2;
            //    int midCol = current.Cols / 2;

            //    int rowsNum = popByLine.Count();
            //    int colsNum = popByLine[0].Length;

            //    /* I somehow introduced a bug here where I'm getting a newline
            //     * at the end of the string when I am loading a file from the
            //     * user. This simply throws that line away. 
            //     */ 
            //    if (popByLine.Last() == String.Empty)
            //        rowsNum -= 1;
                
            //    Rect bounds = Center(rowsNum, colsNum, midRow, midCol);

            //    for (int r = bounds.Top; r < bounds.Bottom; r++)
            //    {
            //        for (int c = bounds.Left; c < bounds.Right; c++)
            //        {
            //            int popRow = r - bounds.Top;
            //            int popCol = c - bounds.Left;

            //            if (popByLine[popRow][popCol] == '.')
            //                current.Board[r, c] = false;
            //            else
            //                current.Board[r, c] = true;
            //        }
            //    }
            //    return current.Board;
            //}
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
            public static void CalcBuilderBounds(GoL current)
            {
                Valid_Left = Enumerable.Range(MenuText.Space, current.OrigConsWidth - 2 * MenuText.Space);
                Valid_Top = Enumerable.Range(MenuText.Space, current.OrigConsHeight - 2 * MenuText.Space);
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