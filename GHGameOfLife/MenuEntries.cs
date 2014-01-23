using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Globalization;
using System.Text;

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

        public const String LoadRandom  = "Loading random pop.";
        public const String Enter       = "Press ENTER to confirm";

        public const String Controls1 = "[SPACE] Get next/Pause";
        public const String Controls2 = "[R] Toggle running";
        public const String Controls3 = "[ESC] Exit";
        public const String Controls4 = "[+/-] Speed adjust";

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
            
            foreach (String res in MenuText.ResNames)
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
            Console.ForegroundColor = MenuText.InfoColor;
            int printRow = (Console.WindowHeight) - 4;

            Console.SetCursorPosition(5, printRow++);
            Console.Write(String.Format("{0,-25}{1,-20}",Controls1,Controls4));
            Console.SetCursorPosition(5, printRow++);
            Console.Write(Controls2);
            Console.SetCursorPosition(5, printRow);
            Console.Write(Controls3);
            Console.ForegroundColor = MenuText.DefaultFG;

            return printRow;
        }
//------------------------------------------------------------------------------
        public static void printStatus(bool running, bool paused, int speed)
        {
            //TODO: Maybe only show the PAUSED text while it is set to AUTO
            Console.ForegroundColor = MenuText.InfoColor;
            StringBuilder sb = new StringBuilder();
            string runStr = (running) ? "LOOPING" : "STEPPING";
            //string pauseStr = (paused) ? "PAUSED" : " ";
            string pauseStr = (running && paused) ? "PAUSED" : " ";

            // █
            string speedStr = "SPEED - ";
            for (int i = 0; i < 5; i++)
            {
                if (i <= speed)
                    speedStr += "█ ";
                else
                    speedStr += "  ";
            }
            speedStr += "+";
            

            int colOne = 10;
            int colTwo = 10;
            int speedCol = Console.WindowWidth - colOne - colTwo - 10;
            //Hardcode 10 because of the border around the game board
            //when it is displayed, so its like space*2
            string testFormat = "{0,-" + colOne + "}{1,-" + colTwo + 
                                                        "}{2," + speedCol + "}";
            sb.AppendFormat(testFormat, runStr, pauseStr, speedStr);
            ClearLine(InfoLine);
            Console.SetCursorPosition(5, InfoLine);
            Console.Write(sb);
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
        public static string GetReadableError(MenuText.FileError err)
        {
            string errorStr;
            switch (err)
            {
                case FileError.CONTENTS:
                    errorStr = "File not all 0s and 1s";
                    break;
                case FileError.LENGTH:
                    errorStr = "File too long for current window";
                    break;
                case FileError.NOT_LOADED:
                    errorStr = "No file loaded";
                    break;
                case FileError.SIZE:
                    errorStr = "File larger than 10KB";
                    break;
                case FileError.WIDTH:
                    errorStr = "File too wide for current window";
                    break;
                default:
                    errorStr = "Generic error...";
                    break;
            }
            return errorStr;
        }
//------------------------------------------------------------------------------
    } // end class
}
