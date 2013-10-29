﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Globalization;

namespace GHGameOfLife
{
    public static class MenuText
    {
        public enum FileError { NONE, LENGTH, WIDTH, CONTENTS, SIZE, NOT_LOADED };
        public const ConsoleColor InfoColor = ConsoleColor.Red;
        public const ConsoleColor DefaultBG = ConsoleColor.Black;
        public const ConsoleColor DefaultFG = ConsoleColor.White;
        public const ConsoleColor PopColor  = ConsoleColor.Cyan;
        public const ConsoleColor DeadColor = ConsoleColor.Gray;
        
        public const String Welcome     = "Welcome to the GAME OF LIFE!!!!";
        public const String PlsChoose   = "Please choose an option!";
        public const String MenuChoice1 = "1) Random population";
        public const String MenuChoice2 = "2) Load population from a file";
        public const String MenuChoice3 = "3) Load a premade population";
        public const String Choice      = "Your choice: ";
        public const String Err         = "**Invalid entry**";

        public const String PopChoice1  = "1)  Canada Goose";
        public const String PopChoice2  = "2) Grow-By-One";
        public const String PopChoice3  = "3) Ship in a bottle";
        public const String PopChoice4  = "4) Sparky";
        public const String PopChoice5  = "5) Twin Bees";
        public const String PopChoice6  = "6) Wickstretcher";

        public const String FileErr1    = "Error loading file...";
        public const String FileErr2    = "Loading random pop.";
        public const String Enter       = "Press ENTER to confirm";

        //public const String NextPrompt  = "Get next generation (y/n)? ";
        //public const String Pause       = "Press SPACE to pause";
        //public const String Unpause     = "Press SPACE to continue " + 
        //                                                    "or ESC to exit";

        public const String RunOptions1 = "Press SPACE to step through, " +
                                                        "or pause if running.";
        public const String RunOptions2 = "Press R to toggle auto stepping.";
        public const String RunOptions3 = "Press ESC to exit";

        public static int WindowCenter; // Vertical center of the console
        public static int LeftAlign;    // Align text with the Welcome message
        public static List<String> ResNames;

        private const int InfoLine = 3;
        private const int WelcomeRow = 6;
        private static int MenuStart;
//------------------------------------------------------------------------------
        public static void Initialize()
        {
            WindowCenter = Console.WindowHeight / 2;
            LeftAlign = (Console.WindowWidth/2) - (Welcome.Length/2);
            
            // Start the menus at 1/3 of the window
            MenuStart = Console.WindowHeight/3 + 1;
            ResNames = new List<String>();

            ResourceManager rm = GHGameOfLife.Pops.ResourceManager;
            rm.IgnoreCase = true;
            ResourceSet all = rm.GetResourceSet(CultureInfo.CurrentCulture, true, true);

            foreach (DictionaryEntry res in all)
            {
                ResNames.Add(res.Key.ToString());
            }

            Console.SetCursorPosition(LeftAlign, WelcomeRow);
            Console.Write(Welcome);

        }
//------------------------------------------------------------------------------
        public static void ClearLine(int row)
        {
            Console.SetCursorPosition(0, row);
            Console.Write("".PadRight(Console.WindowWidth));
        }
//------------------------------------------------------------------------------
        public static void ClearWithinBorder(int row)
        {
            Console.SetCursorPosition(5, row);
            Console.Write("".PadRight(Console.WindowWidth-10));
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Prints the main menu
        /// </summary>
        /// <returns>Returns the line to print the choice prompt on</returns>
        public static int PrintMainMenu(out int numChoices)
        {
            ClearMenuOptions();

            int curRow = MenuStart;

            Console.SetCursorPosition(LeftAlign, curRow);
            Console.Write(PlsChoose);
            Console.SetCursorPosition(LeftAlign, ++curRow);
            Console.Write(Enter);
            Console.SetCursorPosition(LeftAlign + 4, ++curRow);
            Console.Write(MenuChoice1);
            Console.SetCursorPosition(LeftAlign + 4, ++curRow);
            Console.Write(MenuChoice2);
            Console.SetCursorPosition(LeftAlign + 4, ++curRow);
            Console.Write(MenuChoice3);
            numChoices = 3;
            return (++curRow);
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Prints the resource menu
        /// </summary>
        /// <param name="resCount">Outputs the number of resources printed</param>
        /// <returns>Returns the line to put the choice prompt on</returns>
        public static int PrintResourceMenu(out int resCount)
        {
            ClearMenuOptions();

            int curRow = MenuStart;

            Console.SetCursorPosition(LeftAlign, curRow);
            Console.Write(PlsChoose);
            Console.SetCursorPosition(LeftAlign, ++curRow);
            Console.Write(Enter);

            int count = 1;
            
            foreach (String res in ResNames)
            {
                Console.SetCursorPosition(LeftAlign + 4, ++curRow);
                string option = String.Format("{0,3}) {1}", count, res).Replace("_"," ");
                Console.Write(option);
                count += 1;
            }

            resCount = count;

            Console.SetCursorPosition(LeftAlign + 4, ++curRow);
            Console.ForegroundColor = InfoColor;
            string cancel = String.Format("{0,3}) {1}", count, "Cancel");
            Console.Write(cancel);
            Console.ForegroundColor = DefaultFG;

            /* USED FOR FINDING SPACING AT MIN WINDOW HEIGHT
            for (int i = 0; i < 10; i++)
            {
                Console.SetCursorPosition(LeftAlign + 4, WindowCenter + currLine);
                string option = String.Format("{0,3}) {1}", count, i).Replace("_", " ");
                Console.Write(option);
                count += 1;
                currLine += 1;
            }*/

            return (++curRow);
        }
//------------------------------------------------------------------------------
        public static int PrintControls()
        {
            int printRow = (Console.WindowHeight) - 4;
            int opt1Left = (Console.WindowWidth / 2) -
                                        (RunOptions1.Length / 2);
            int opt2Left = (Console.WindowWidth / 2) -
                                        (RunOptions2.Length / 2);
            int opt3Left = (Console.WindowWidth / 2) -
                                        (RunOptions3.Length / 2);

            Console.SetCursorPosition(opt1Left, printRow++);
            Console.Write(RunOptions1);
            Console.SetCursorPosition(opt2Left, printRow++);
            Console.Write(RunOptions2);
            Console.SetCursorPosition(opt3Left, printRow);
            Console.Write(RunOptions3);

            return printRow;
        }
//------------------------------------------------------------------------------
        public static void printStatus(bool running, bool paused)
        {
            Console.ForegroundColor = MenuText.InfoColor;
            ClearLine(InfoLine);
            if (running)
            {
                Console.SetCursorPosition(5, InfoLine);
                Console.Write("AUTO");
            }
            if (paused)
            {
                Console.SetCursorPosition(10, InfoLine);
                Console.Write("PAUSED");
            }
            Console.ForegroundColor = MenuText.DefaultFG;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Only allow lines within 5 of the middle to be used for printing the
        /// menus.
        /// </summary>
        public static void ClearMenuOptions()
        {
            for (int i = WelcomeRow+1; i < Console.WindowHeight-5; i++)
                ClearWithinBorder(i);
        }
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
    } // end class
}
