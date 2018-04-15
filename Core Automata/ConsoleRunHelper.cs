using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Avalonia.Controls;

namespace Core_Automata
{
    class ConsoleRunHelper
    {
        private static int[] __Speeds = { 100, 75, 50, 25, 10 };
        private static int __Curr_Speed_Index = 2;
//-----------------------------------------------------------------------------
        /// <summary>
        /// Takes a ConsoleAutomata and runs it
        /// </summary>
        /// <param name="game">The board to start with</param>
        public static void ConsoleAutomataRunner(ConsoleAutomata game)
        {
            if (!game.Is_Initialized)
            {
                Console.ForegroundColor = MenuHelper.Info_FG;
                Console.Write("ERROR");
                return;
            }

            MenuHelper.PrintRunControls();

            var statusValues = new Dictionary<string, bool>();
            statusValues["Go"] = true;
            statusValues["Continuous"] = false;
            statusValues["Paused"] = true;
            statusValues["Wrapping"] = game.Is_Wrapping;
            statusValues["ExitPause"] = false;

            MenuHelper.PrintStatus(statusValues["Continuous"], statusValues["Paused"], statusValues["Wrapping"], __Curr_Speed_Index);

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
                        MenuHelper.PrintStatus(statusValues["Continuous"], statusValues["Paused"], statusValues["Wrapping"], __Curr_Speed_Index);
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
                        SaveBoard(currentGame.Rows, currentGame.Cols, currentGame.Board_Copy);
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
            MenuHelper.PrintStatus(currentStatus["Continuous"], currentStatus["Paused"], currentStatus["Wrapping"], ConsoleRunHelper.__Curr_Speed_Index);
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Saves the current board to a file. 
        /// </summary>
        /// <param name="numRows">Total number of rows on the board</param>
        /// <param name="numCols">Total number of cols on the board</param>
        /// <param name="tempBoard">2d bool array representing the board</param>
        public static async void SaveBoard(int numRows, int numCols, bool[,] tempBoard)
        {
            SaveFileDialog saveDia = new SaveFileDialog();
            var filters = new List<FileDialogFilter>();
            var tempFilter = new FileDialogFilter();
            tempFilter.Name = "Text";
            tempFilter.Extensions = new List<string>() { "txt" };
            saveDia.Filters = new List<FileDialogFilter>() { tempFilter };

            var t1 = new Window();
            var test = await saveDia.ShowAsync(t1);

            var idunno = "";

            // We only save if the dialog box comes back true, otherwise
            // we just do nothing
            //if (saveDia.ShowDialog() == DialogResult.OK)
            //{
            //    Rect saveBox = new Rect();
            //    saveBox.Top = int.MaxValue;
            //    saveBox.Bottom = int.MinValue;
            //    saveBox.Left = int.MaxValue;
            //    saveBox.Right = int.MinValue;

            //    // make a box that only includes the minimum needed lines
            //    // to save the board
            //    // We only need to check live cells
            //    for (int r = 0; r < numRows; r++)
            //    {
            //        for (int c = 0; c < numCols; c++)
            //        {
            //            if (tempBoard[r, c])
            //            {
            //                if (r < saveBox.Top)
            //                    saveBox.Top = r;
            //                if (r > saveBox.Bottom)
            //                    saveBox.Bottom = r;
            //                if (c < saveBox.Left)
            //                    saveBox.Left = c;
            //                if (c > saveBox.Right)
            //                    saveBox.Right = c;
            //            }
            //        }
            //    }

            //    StringBuilder sb = new StringBuilder();
            //    for (int r = saveBox.Top; r <= saveBox.Bottom; r++)
            //    {
            //        for (int c = saveBox.Left; c <= saveBox.Right; c++)
            //        {
            //            if (tempBoard[r, c])
            //                sb.Append('O');
            //            else
            //                sb.Append('.');
            //        }
            //        if (r != saveBox.Bottom)
            //            sb.AppendLine();
            //    }
            //    File.WriteAllText(saveDia.FileName, sb.ToString());
            //}

        }
//------------------------------------------------------------------------------
    }
//////////////////////////////////////////////////////////////////////////////////
}
