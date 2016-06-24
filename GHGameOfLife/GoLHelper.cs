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
        }  // end class GoLHelper
//-----------------------------------------------------------------------------
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
    } // end class GoLBoard
///////////////////////////////////////////////////////////////////////////////
}