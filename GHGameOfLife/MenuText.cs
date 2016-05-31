using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Globalization;
using System.Text;

namespace GHGameOfLife
{
///////////////////////////////////////////////////////////////////////////////
    class MenuText
    {
        // TODO: Change this to be less ugly maybe?
        public enum FileError { None, Length, Width, Uneven, Contents, Size, Not_Loaded };
        public const ConsoleColor Info_FG    = ConsoleColor.Red;
        public const ConsoleColor Default_BG = ConsoleColor.Black;
        public const ConsoleColor Default_FG = ConsoleColor.White;
        public const ConsoleColor Board_FG   = ConsoleColor.White;
        public const ConsoleColor Builder_FG = ConsoleColor.Cyan;
        
        public const string Welcome      = "Welcome to the GAME OF LIFE!!!!";
        public const string Choose_Msg   = "Please choose an option!";
        public const string Change_Size   = "[Ctrl + [+/-]] Change board size";
        public const string Prompt = "Your choice: ";
        public const string Entry_Error = "**Invalid entry**";
        public const string Press_Enter = "Press ENTER to confirm";
        public const string Load_Rand = "Loading random pop.";

        public static string[] Run_Ctrls;
        public static string[] Menu_Choices; 
        public static string[] Create_Ctrls;
        
        public static int Window_Center; // Center Row
        public static int Left_Align;    // Align text with the Welcome message
       
        public static List<string> Large_Pops;
        public static List<string> Builder_Pops;
        public static ArrayList Large_Pops_Pages;

        private const int Info_Row = 3;
        private const int Welcome_Row = 6;
        private static int Menu_Start_Row;

        public static int Space = 5;         
//------------------------------------------------------------------------------
        public static void Initialize()
        {           
            Window_Center = Console.WindowHeight / 2;
            Left_Align = (Console.WindowWidth/2) - (Welcome.Length/2);
            
            // Start the menus at 1/3 of the window
            Menu_Start_Row = Console.WindowHeight/3 + 1;
            Large_Pops = new List<string>();
            Builder_Pops = new List<string>();

            ResourceManager rm = GHGameOfLife.LargePops.ResourceManager;
            rm.IgnoreCase = true;
            ResourceSet all = rm.GetResourceSet(CultureInfo.CurrentCulture, true, true);
            

            foreach (DictionaryEntry res in all)
            {
                Large_Pops.Add(res.Key.ToString());
            }

            Large_Pops.Sort();
            Large_Pops_Pages = new ArrayList();

            List<string> temp = new List<string>();
            int count = 0;
            int elementNum = 0;
            bool addPage = false;
            while (elementNum != Large_Pops.Count)
            {
                temp.Add(Large_Pops[elementNum]);
                count++;
                elementNum++;
                if (count == 7 || elementNum == Large_Pops.Count)
                    addPage = true;

                if (addPage)
                {
                    Large_Pops_Pages.Add(temp);
                    temp = new List<string>();
                    addPage = false;
                    count = 0;
                }
            }
            //End testing for pop list

            rm = GHGameOfLife.BuilderPops.ResourceManager;
            rm.IgnoreCase = true;
            all = rm.GetResourceSet(CultureInfo.CurrentCulture, true, true);

            foreach (DictionaryEntry res in all)
            {
                Builder_Pops.Add(res.Key.ToString());
            }


            Menu_Choices = new string[] {   "1) Random population",
                                            "2) Load population from a file",
                                            "3) Load a premade population",
                                            "4) Create your own population",
                                            "5) Exit"};
            
            
            Run_Ctrls = new string[] {  "[SPACE] Step/Pause",
                                        "[R] Toggle running",
                                        "[ESC] Exit",
                                        "[+/-] Speed adjust",
                                        "[W] Toggle wrapping",
                                        "[S] Save board"};

            Create_Ctrls = new string[] {   "[↑|↓|←|→] Move cursor", 
                                            "[SPACE] Add/Remove cells",
                                            "[ENTER] Start Game",
                                            "[S] Save board",
                                            "[C] Cancel pop mode",
                                            "[Ctrl + [#]] Mirror pop",
                                            "[[#]] Load/Rotate pop"};

            


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
            Console.Write("".PadRight(Console.WindowWidth-1));
        }
//------------------------------------------------------------------------------
        public static void ClearWithinBorder(int row)
        {
            Console.SetCursorPosition(5, row);
            Console.Write("".PadRight(Console.WindowWidth-10));
        }
//------------------------------------------------------------------------------
        public static void DrawBorder()
        {
            var rows = Console.WindowHeight;
            var cols = Console.WindowWidth;

            char vert = '║'; // '\u2551'
            char horiz = '═'; // '\u2550'
            char topLeft = '╔'; // '\u2554'
            char topRight = '╗'; // '\u2557'
            char botLeft = '╚'; // '\u255A'
            char botRight = '╝'; // '\u255D'

            int borderTop = 4;
            int borderBottom = rows - 5;
            int borderLeft = 4;
            int borderRight = cols - 5;


            // This draws the nice little border on the screen...
            Console.SetCursorPosition(borderLeft, borderTop);
            Console.Write(topLeft);
            for (int i = borderLeft; i < borderRight; i++)
                Console.Write(horiz);
            Console.SetCursorPosition(borderRight, borderTop);
            Console.Write(topRight);
            for (int i = borderTop + 1; i < borderBottom; i++)
            {
                Console.SetCursorPosition(borderLeft, i);
                Console.Write(vert);
                Console.SetCursorPosition(borderRight, i);
                Console.Write(vert);
            }
            Console.SetCursorPosition(borderLeft, borderBottom);
            Console.Write(botLeft);
            for (int i = 5; i < borderRight; i++)
                Console.Write(horiz);
            Console.Write(botRight);
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Prints the main menu
        /// </summary>
        /// <returns>Returns the line to print the choice prompt on</returns>
        public static int PrintMainMenu()
        {
            ClearAllInBorder();

            Console.ForegroundColor = MenuText.Info_FG;
            Console.SetCursorPosition(5,(Console.WindowHeight) - 4);
            Console.WriteLine(Change_Size);

            Console.ForegroundColor = MenuText.Default_FG;
            Console.SetCursorPosition(Left_Align, Welcome_Row);
            Console.Write(Welcome);

            int curRow = Menu_Start_Row;

            Console.SetCursorPosition(Left_Align, curRow);
            Console.Write(Choose_Msg);
            Console.SetCursorPosition(Left_Align, ++curRow);
            Console.Write(Press_Enter);
            foreach( string choice in Menu_Choices )
            {
                Console.SetCursorPosition(Left_Align + 4, ++curRow);
                Console.Write(choice);
            }
            return (++curRow);
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Prints the resource menu
        /// </summary>
        /// <param name="resCount">Outputs the number of resources printed</param>
        /// <returns>Returns the line to print the choice prompt on</returns>
        public static int PrintResourceMenu(List<string> list, bool lastPage, bool firstPage)
        {
            int curRow = Menu_Start_Row;

            Console.SetCursorPosition(Left_Align, curRow);
            Console.Write(Choose_Msg);
            Console.SetCursorPosition(Left_Align, ++curRow);
            Console.Write(Press_Enter);

            int count = 1;
            string[] defaultPrompts = new string[] {    "8) Prev Page",
                                                        "9) Next Page",
                                                        "0) Cancel"};
            foreach (string s in list)
            {
                Console.SetCursorPosition(Left_Align + 4, ++curRow);
                Console.Write("{0}) {1}", count, s.Replace("_"," "));
                count++;
            }

            if (!firstPage)
            {
                Console.SetCursorPosition(Left_Align + 4, ++curRow);
                Console.WriteLine(defaultPrompts[0]);
            }
            if (!lastPage)
            {
                Console.SetCursorPosition(Left_Align + 4, ++curRow);
                Console.WriteLine(defaultPrompts[1]);
            }
            Console.SetCursorPosition(Left_Align + 4, ++curRow);
            Console.WriteLine(defaultPrompts[2]);

            return ++curRow;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Prints the controls for controlling the game while running
        /// </summary>
        public static void PrintRunControls()
        {
            Console.ForegroundColor = MenuText.Info_FG;
            int printRow = (Console.WindowHeight) - 4;

            Console.SetCursorPosition(5, printRow);
            Console.Write("{0,-25}{1,-25}",Run_Ctrls[0],Run_Ctrls[3]);
            Console.SetCursorPosition(5, ++printRow);
            Console.Write("{0,-25}{1,-25}",Run_Ctrls[1],Run_Ctrls[4]);
            Console.SetCursorPosition(5, ++printRow);
            Console.Write("{0,-25}{1,-25}",Run_Ctrls[2],Run_Ctrls[5]);
            Console.ForegroundColor = MenuText.Default_FG;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Prints the game status while running
        /// </summary>
        /// <param name="running"></param>
        /// <param name="paused"></param>
        /// <param name="wrapping"></param>
        /// <param name="speed"></param>
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
        /// <summary>
        /// Prints menu when user is building a population
        /// </summary>
        public static void PrintCreationControls()
        {
            Console.ForegroundColor = MenuText.Info_FG;
            int printStart = (Console.WindowHeight) - 4;
            int printRow = printStart;

            Console.SetCursorPosition(5, printRow);            
            Console.Write("{0,-25}{1,-25}",Create_Ctrls[0],Create_Ctrls[4]);
            Console.SetCursorPosition(5, ++printRow);
            Console.Write("{0,-25}{1,-25}",Create_Ctrls[1], Create_Ctrls[5]);
            Console.SetCursorPosition(5, ++printRow);
            Console.Write("{0,-25}{1,-25}",Create_Ctrls[2], Create_Ctrls[6]);
            Console.SetCursorPosition(5, ++printRow);
            Console.Write("{0,-25}{1,-25}", Create_Ctrls[3],"");
            
            //Each pop gets a space of 20 cols to write to
            //We start at 55 because the above messages have a space of 50, plus the 5 for the border
            //We allow 4 pops to display per column, with a max of 2 columns because of the min window
            //size of 100
            int count = 0;
            printRow = printStart;
            foreach(string popName in Builder_Pops)
            {
                printRow = printStart + (count % 4);
                Console.SetCursorPosition(55 + (20*(count/4)), printRow);
                Console.Write("[{0}]{1,-17}",Builder_Pops.IndexOf(popName)+1, popName);
                ++count;
                ++printRow;
            }
             
            Console.ForegroundColor = MenuText.Default_FG;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Prints messages to prompt for another round
        /// </summary>
        public static void PromptForAnother()
        {
            ClearAllInBorder();
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
            for (int i = Console.WindowHeight - 4; i < Console.WindowHeight; i++)
            {
                ClearLine(i);
            }
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Clear everything inside the board area
        /// </summary>
        public static void ClearAllInBorder()
        {
            for (int i = 5; i < Console.WindowHeight-5; i++)
              ClearWithinBorder(i);

        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Clear above the border
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
                    errorStr = "File not all .'s and O's";
                    break;
                case FileError.Length:
                    errorStr = "Too many lines for current window size";
                    break;
                case FileError.Not_Loaded:
                    errorStr = "No file loaded";
                    break;
                case FileError.Size:
                    errorStr = "File either empty or larger than 20KB";
                    break;
                case FileError.Width:
                    errorStr = "Too many characters per line";
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
///////////////////////////////////////////////////////////////////////////////
}
