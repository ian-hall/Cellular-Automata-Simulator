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
        /* MESSAGES TO ADD:
         * Resize screen from main menu
         * Load small population from user build screen
         *      Rotate loaded pop
        */ 
        public enum FileError { None, Length, Width, Uneven, Contents, Size, Not_Loaded };
        public const ConsoleColor Info_FG    = ConsoleColor.Red;
        public const ConsoleColor Default_BG    = ConsoleColor.Black;
        public const ConsoleColor Default_FG    = ConsoleColor.White;
        public const ConsoleColor Board_FG      = ConsoleColor.White;
        public const ConsoleColor Builder_FG = ConsoleColor.Cyan;
        
        public const String Welcome      = "Welcome to the GAME OF LIFE!!!!";
        public const String Choose_Msg   = "Please choose an option!";
        
        public const String Menu_Choice1 = "1) Random population";
        public const String Menu_Choice2 = "2) Load population from a file";
        public const String Menu_Choice3 = "3) Load a premade population";
        public const String Menu_Choice4 = "4) Create your own population";
        public const String Menu_Choice5 = "5) Exit";
        public const int    NMenu_Choice = 5;
        
        public const String Prompt       = "Your choice: ";
        public const String Err          = "**Invalid entry**";

        public const String Load_Rand   = "Loading random pop.";
        public const String Enter       = "Press ENTER to confirm";

        public const String Run_Ctrl1 = "[SPACE] Get next/Pause";
        public const String Run_Ctrl2 = "[R] Toggle running";
        public const String Run_Ctrl3 = "[ESC] Exit";
        public const String Run_Ctrl4 = "[+/-] Speed adjust";
        public const String Run_Ctrl5 = "[W] Toggle wrapping";
        public const int    NRun_Ctrl = 5;

        public const String Create_Ctrl1 = "Arrow keys to move cursor";
        public const String Create_Ctrl2 = "[SPACE] Add/Remove cell";
        public const String Create_Ctrl3 = "[ENTER] Start Game";
        public const String Create_Ctrl4 = "[S] Save board";
        public const int    NCreate_Ctrl = 4;
        

        public static int Window_Center; // Vertical center of the console
        public static int Left_Align;    // Align text with the Welcome message
        public static List<String> Res_Names;

        private const int Info_Row = 3;
        private const int Welcome_Row = 6;
        private static int Menu_Start_Row;

        private static int Space = GoLBoard.Space;

        //public static bool Initialized {get; private set;}
//------------------------------------------------------------------------------
        public static void Initialize()
        {
            Window_Center = Console.WindowHeight / 2;
            Left_Align = (Console.WindowWidth/2) - (Welcome.Length/2);
            
            // Start the menus at 1/3 of the window
            Menu_Start_Row = Console.WindowHeight/3 + 1;
            Res_Names = new List<String>();

            ResourceManager rm = GHGameOfLife.LargePops.ResourceManager;
            rm.IgnoreCase = true;
            ResourceSet all = rm.GetResourceSet(CultureInfo.CurrentCulture, true, true);

            foreach (DictionaryEntry res in all)
            {
                Res_Names.Add(res.Key.ToString());
            }

            /*
            Console.SetCursorPosition(Left_Align, Welcome_Row);
            Console.Write(Welcome);*/

            //Initialized = true;
        }
//------------------------------------------------------------------------------
        public static void ReInitialize()
        {
            Window_Center = Console.WindowHeight / 2;
            Left_Align = (Console.WindowWidth / 2) - (Welcome.Length / 2);

            // Start the menus at 1/3 of the window
            Menu_Start_Row = Console.WindowHeight / 3 + 1;
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
        public static int PrintMainMenu(/*out int numChoices*/)
        {
            ClearAllInBoarder();

            Console.SetCursorPosition(Left_Align, Welcome_Row);
            Console.Write(Welcome);

            int curRow = Menu_Start_Row;

            Console.SetCursorPosition(Left_Align, curRow);
            Console.Write(Choose_Msg);
            Console.SetCursorPosition(Left_Align, ++curRow);
            Console.Write(Enter);
            Console.SetCursorPosition(Left_Align + 4, ++curRow);
            Console.Write(Menu_Choice1);
            Console.SetCursorPosition(Left_Align + 4, ++curRow);
            Console.Write(Menu_Choice2);
            Console.SetCursorPosition(Left_Align + 4, ++curRow);
            Console.Write(Menu_Choice3);
            Console.SetCursorPosition(Left_Align + 4, ++curRow);
            Console.Write(Menu_Choice4);
            Console.SetCursorPosition(Left_Align + 4, ++curRow);
            Console.Write(Menu_Choice5);
            //numChoices = 5;
            return (++curRow);
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Prints the resource menu
        /// </summary>
        /// <param name="resCount">Outputs the number of resources printed</param>
        /// <returns>Returns the line to print the choice prompt on</returns>
        public static int PrintResourceMenu(out int resCount)
        {
            ClearAllInBoarder();

            int curRow = Menu_Start_Row;

            Console.SetCursorPosition(Left_Align, curRow);
            Console.Write(Choose_Msg);
            Console.SetCursorPosition(Left_Align, ++curRow);
            Console.Write(Enter);

            int count = 1;
            
            foreach (String res in MenuText.Res_Names)
            {
                Console.SetCursorPosition(Left_Align + 4, ++curRow);
                string option = String.Format("{0,3}) {1}", count, res).Replace("_"," ");
                Console.Write(option);
                count += 1;
            }

            resCount = count;

            Console.SetCursorPosition(Left_Align + 4, ++curRow);
            Console.ForegroundColor = Info_FG;
            string cancel = String.Format("{0,3}) {1}", count, "Cancel");
            Console.Write(cancel);
            Console.ForegroundColor = Default_FG;

            return (++curRow);
        }
//------------------------------------------------------------------------------
        public static void PrintRunControls()
        {
            Console.ForegroundColor = MenuText.Info_FG;
            int printRow = (Console.WindowHeight) - 4;

            Console.SetCursorPosition(5, printRow);
            Console.Write(String.Format("{0,-25}{1,-20}",Run_Ctrl1,Run_Ctrl4));
            Console.SetCursorPosition(5, ++printRow);
            Console.Write(String.Format("{0,-25}{1,-20}",Run_Ctrl2,Run_Ctrl5));
            Console.SetCursorPosition(5, ++printRow);
            Console.Write(Run_Ctrl3);
            Console.ForegroundColor = MenuText.Default_FG;
        }
//------------------------------------------------------------------------------
        public static void PrintStatus(bool running, bool paused,
                                        bool wrapping, int speed)
        {
            Console.ForegroundColor = MenuText.Info_FG;
            StringBuilder sb = new StringBuilder();
            string runStr = (running) ? "LOOPING" : "STEPPING";
            string pauseStr = (running && paused) ? "PAUSED" : " ";
            string wrapStr = (wrapping) ? "WRAPPING" : " ";

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
            int colThree = 10;
            int speedCol = Console.WindowWidth - colOne - colTwo - colThree- 10;
            //Hardcode 10 because of the border around the game board
            //when it is displayed, so its like space*2
            string formatStr = "{0,-" + colOne + "}{1,-" + colTwo + 
                                                        "}{2,-" + colThree + "}{3," + speedCol + "}";
            sb.AppendFormat(formatStr, runStr, pauseStr, wrapStr, speedStr);
            ClearLine(Info_Row);
            Console.SetCursorPosition(5, Info_Row);
            Console.Write(sb);
            Console.ForegroundColor = MenuText.Default_FG;
        }
//------------------------------------------------------------------------------
        public static void PrintCreationControls()
        {
            Console.ForegroundColor = MenuText.Info_FG;
            int printRow = (Console.WindowHeight) - 4;

            Console.SetCursorPosition(5, printRow);
            Console.Write(Create_Ctrl1);
            Console.SetCursorPosition(5, ++printRow);
            Console.Write(String.Format("{0,-25}{1,-20}", Create_Ctrl2, Create_Ctrl4));
            Console.SetCursorPosition(5, ++printRow);
            Console.Write(Create_Ctrl3);
            Console.ForegroundColor = MenuText.Default_FG;
        }
//------------------------------------------------------------------------------
        public static void PromptForAnother()
        {
            ClearAllInBoarder();
            ClearUnderBoard();
            ClearAboveBoard();
            ClearLine(Info_Row);

            int printRow = Menu_Start_Row + 1;
            Console.SetCursorPosition(Left_Align, printRow);
            Console.Write("Do you really want to exit?");
            Console.SetCursorPosition(Left_Align, ++printRow);
            Console.ForegroundColor = Info_FG;
            Console.Write("[ENTER] No, keep playing.");
            Console.SetCursorPosition(Left_Align, ++printRow);
            Console.Write("[ESC] Yes, let me out.");
            Console.ForegroundColor = Default_FG;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Only the space under the bottom border is used for the menu
        /// </summary>
        public static void ClearUnderBoard()
        {
            for (int i = Console.WindowHeight - 4; i < Console.WindowHeight-1; i++)
            {
                ClearLine(i);
            }
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Clear everything inside the board area
        /// </summary>
        public static void ClearAllInBoarder()
        {
            for (int i = 5; i < Console.WindowHeight-5; i++)
              ClearWithinBorder(i);

        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Clear above the boarder
        /// </summary>
        public static void ClearAboveBoard()
        {
            for (int i = 0; i < Space - 1; i++)
            {
                ClearLine(i);
            }
        }
//------------------------------------------------------------------------------
        public static string GetReadableError(MenuText.FileError err)
        {
            string errorStr;
            switch (err)
            {
                case FileError.Contents:
                    errorStr = "File not all 0s and 1s";
                    break;
                case FileError.Length:
                    errorStr = "File has too many lines for current window";
                    break;
                case FileError.Not_Loaded:
                    errorStr = "No file loaded";
                    break;
                case FileError.Size:
                    errorStr = "File larger than 10KB";
                    break;
                case FileError.Width:
                    errorStr = "Lines are too long";
                    break;
                case FileError.Uneven:
                    errorStr = "Lines are not of even length";
                    break;
                default:
                    errorStr = "Generic file error...";
                    break;
            }
            return errorStr;
        }
//------------------------------------------------------------------------------
    } // end class
}
