using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace Core_Automata
{
    
    class MenuHelper
    {
        public enum FileError { None, Length, Width, Contents, Size, NotLoaded };
        public const ConsoleColor InfoFG = ConsoleColor.Red;
        public const ConsoleColor DefaultBG = ConsoleColor.Black;
        public const ConsoleColor DefaultFG = ConsoleColor.White;
        public const ConsoleColor BoardFG = ConsoleColor.White;
        public const ConsoleColor BuilderFG = ConsoleColor.Cyan;

        public const string WelcomeMessage = "Welcome to Ian's Automata Simulator";
        public const string ChooseMessage = "Please choose an option!";
        public const string SizeChangeMessage = "[Ctrl + [+/-]] Change board size";
        public const string Prompt = "Your choice: ";
        public const string EntryErrorMessage = "**Invalid entry**";
        public const string PressEnterMessage = "Press ENTER to confirm";
        public const string LoadingRandomMessage = "Loading random pop...";

        public static string[] RunControls;
        public static string[] CreationControls;

        public static int WindowCenter; // Center Row
        public static int LeftAlign;    // Align text with the Welcome message

        public static List<string> LargePops;
        public static List<string> BuilderPops;

        private const int InfoRow = 3;
        private const int WelcomeRow = 6;
        private static int MenuStarRow;

        public const int Space = 5;
        public const int ChoicesPerPage = 7;
        
        static MenuHelper()
        {
            WindowCenter = Console.WindowHeight / 2;
            LeftAlign = (Console.WindowWidth / 2) - (WelcomeMessage.Length / 2);

            // Start the menus at 1/3 of the window
            MenuStarRow = Console.WindowHeight / 3 + 1;
            LargePops = new List<string>();
            BuilderPops = new List<string>();

            ResourceManager rm = Core_Automata.LargePops.ResourceManager;
            rm.IgnoreCase = true;
            ResourceSet all = rm.GetResourceSet(CultureInfo.CurrentCulture, true, true);


            foreach (DictionaryEntry res in all)
            {
                LargePops.Add(res.Key.ToString());
            }

            LargePops.Sort();

            rm = Core_Automata.BuilderPops.ResourceManager;
            rm.IgnoreCase = true;
            all = rm.GetResourceSet(CultureInfo.CurrentCulture, true, true);

            foreach (DictionaryEntry res in all)
            {
                BuilderPops.Add(res.Key.ToString());
            }

            RunControls = new string[] {  "[SPACE] Step/Pause",
                                        "[R] Toggle running",
                                        "[ESC] Exit",
                                        "[+/-] Adjust speed" };

            CreationControls = new string[] {   "[↑|↓|←|→] Move cursor",
                                            "[SPACE] Add/Remove cells",
                                            "[ENTER] Start Game",
                                            "[[#]] Load/Rotate pop",
                                            "[Ctrl + [#]] Mirror pop",
                                            "[C] Cancel pop mode"};

            ClearConsoleWindow();
        }
        
        public static void ReInitialize()
        {
            WindowCenter = Console.WindowHeight / 2;
            LeftAlign = (Console.WindowWidth / 2) - (WelcomeMessage.Length / 2);

            // Start the menus at 1/3 of the window
            MenuStarRow = Console.WindowHeight / 3 + 1;
        }
        
        public static void ClearLine(int row)
        {
            Console.SetCursorPosition(0, row);
            Console.Write("".PadRight(Console.WindowWidth - 1));
        }
        
        public static void ClearWithinBorder(int row)
        {
            Console.SetCursorPosition(5, row);
            Console.Write("".PadRight(Console.WindowWidth - 10));
        }
        
        public static void ClearConsoleWindow()
        {
            for( int i = 0; i < Console.WindowHeight; i++ )
            {
                ClearLine(i);
            }
        }
        
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
        
        /// <summary>
        /// Prints the controls for controlling the game while running
        /// </summary>
        public static void PrintRunControls()
        {
            Console.ForegroundColor = MenuHelper.InfoFG;
            int printRow = (Console.WindowHeight) - 4;

            Console.SetCursorPosition(5, printRow);
            Console.Write("{0,-25}{1,-25}", RunControls[0], RunControls[3]);
            Console.SetCursorPosition(5, ++printRow);
            Console.Write("{0,-25}", RunControls[1]);
            Console.SetCursorPosition(5, ++printRow);
            Console.Write("{0,-25}", RunControls[2]);
            Console.ForegroundColor = MenuHelper.DefaultFG;
        }
        
        /// <summary>
        /// Prints the game status while running
        /// </summary>
        /// <param name="running">if the game is looping or stepping</param>
        /// <param name="paused">if the game is paused</param>
        /// <param name="wrapping">if the board has wrapping on</param>
        /// <param name="speed">the speed the board is running at</param>
        public static void PrintStatus(bool running, bool paused,
                                        bool wrapping, int speed)
        {
            Console.ForegroundColor = MenuHelper.InfoFG;
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
            int speedCol = Console.WindowWidth - colOne - colTwo - colThree - 10;
            //Hardcode 10 because of the border around the game board
            //when it is displayed, so its like space*2
            string formatStr = "{0,-" + colOne + "}{1,-" + colTwo +
                                                        "}{2,-" + colThree + "}{3," + speedCol + "}";
            sb.AppendFormat(formatStr, runStr, pauseStr, wrapStr, speedStr);
            ClearLine(InfoRow);
            Console.SetCursorPosition(5, InfoRow);
            Console.Write(sb);
            Console.ForegroundColor = MenuHelper.DefaultFG;
        }
        
        /// <summary>
        /// Prints menu when user is building a population
        /// </summary>
        public static void PrintCreationControls()
        {
            Console.ForegroundColor = MenuHelper.InfoFG;
            int printStart = Console.WindowHeight - 4;
            int printRow = printStart;
            var textWidth = 25;

            Console.SetCursorPosition(5, printRow);
            Console.Write("{0,-25}{1,-25}{2,-25}", CreationControls[0], CreationControls[3], CreationControls[5]);
            Console.SetCursorPosition(5, ++printRow);
            Console.Write("{0,-25}{1,-25}", CreationControls[1], CreationControls[4]);
            Console.SetCursorPosition(5, ++printRow);
            Console.Write("{0,-25}", CreationControls[2]);

            /* Start count at 1 because of the above. Need to limit this to 3 entries per column because
             * of the scroll bar that shows up. printCol is calculated based on the above since we start
             * printing the pops directly beneath the last entry.
             */
            int count = 1;
            printRow = printStart;
            foreach (string popName in BuilderPops)
            {
                printRow = printStart + (count % 3);
                var printCol = 5 + (textWidth * (2 + count / 3));
                Console.SetCursorPosition(printCol, printRow);
                Console.Write("[{0}]{1,-25}", BuilderPops.IndexOf(popName) + 1, popName);
                ++count;
                ++printRow;
            }

            Console.ForegroundColor = MenuHelper.DefaultFG;
        }
        
        /// <summary>
        /// Prints messages to prompt for another round
        /// </summary>
        public static void PromptForAnother()
        {
            ClearAllInBorder();
            ClearUnderBoard();
            ClearAboveBoard();
            ClearLine(InfoRow);

            int printRow = MenuStarRow + 1;
            Console.SetCursorPosition(LeftAlign, printRow);
            Console.Write("Do you really want to exit?");
            Console.SetCursorPosition(LeftAlign, ++printRow);
            Console.ForegroundColor = InfoFG;
            Console.Write("[ENTER] No, keep playing.");
            Console.SetCursorPosition(LeftAlign, ++printRow);
            Console.Write("[ESC] Yes, let me out.");
            Console.ForegroundColor = DefaultFG;
        }
        
        /// <summary>
        /// Clear all lines below the border
        /// </summary>
        public static void ClearUnderBoard()
        {
            for (int i = Console.WindowHeight - 4; i < Console.WindowHeight; i++)
            {
                ClearLine(i);
            }
        }
        
        /// <summary>
        /// Clear everything inside the board area
        /// </summary>
        public static void ClearAllInBorder()
        {
            for (int i = 5; i < Console.WindowHeight - 5; i++)
                ClearWithinBorder(i);

        }
        
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
        
        /// <summary>
        /// Prints a user friendly error message
        /// </summary>
        /// <param name="err">the error to get friendly with</param>
        public static void PrintFileError(MenuHelper.FileError err)
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
                case FileError.NotLoaded:
                    errorStr = "No file loaded";
                    break;
                case FileError.Size:
                    errorStr = "File either empty or larger than 20KB";
                    break;
                case FileError.Width:
                    errorStr = "Too many characters per line";
                    break;
                default:
                    errorStr = "Generic file error...";
                    break;
            }

            int windowCenter = Console.WindowHeight / 2; //Vert position
            int welcomeLeft = (Console.WindowWidth / 2) -
                (MenuHelper.WelcomeMessage.Length / 2);
            int distToBorder = (Console.WindowWidth - 5) - welcomeLeft;

            MenuHelper.ClearWithinBorder(windowCenter);
            Console.SetCursorPosition(welcomeLeft, windowCenter - 1);
            Console.Write(errorStr);
            Console.SetCursorPosition(welcomeLeft, windowCenter);
            Console.Write(MenuHelper.LoadingRandomMessage);
            Console.SetCursorPosition(welcomeLeft, windowCenter + 1);
            Console.Write(MenuHelper.PressEnterMessage);
        }
        
        /// <summary>
        /// Prints a menu from the given choices
        /// </summary>
        /// <param name="choices">IEnumerable<string> of choices to display</param>
        public static void PrintMenuFromList(IEnumerable<string> choices)
        {
            ClearAllInBorder();

            Console.ForegroundColor = MenuHelper.InfoFG;
            Console.SetCursorPosition(5, (Console.WindowHeight) - 4);
            Console.WriteLine(SizeChangeMessage);

            Console.ForegroundColor = MenuHelper.DefaultFG;
            Console.SetCursorPosition(LeftAlign, WelcomeRow);
            Console.Write(WelcomeMessage);

            int curRow = MenuStarRow;

            Console.SetCursorPosition(LeftAlign, curRow);
            Console.Write(ChooseMessage);
            Console.SetCursorPosition(LeftAlign, ++curRow);
            //Console.Write(Press_Enter);
            foreach (string choice in choices)
            {
                Console.SetCursorPosition(LeftAlign + 4, ++curRow);
                Console.Write(choice);
            }
        }
        
        /// <summary>
        /// Changes enums to a list of strings prefixed by numbers 1-7
        /// </summary>
        /// <param name="enumVals"></param>
        /// <returns></returns>
        public static List<string> EnumToChoiceStrings(Array enumVals)
        {
            var choiceStrings = new List<string>();
            for (int i = 0; i < enumVals.Length; i++)
            {
                var enumStr = enumVals.GetValue(i).ToString();
                enumStr = enumStr.Replace('_', ' ');
                var choiceStr = String.Format("{0}) {1}", (i % 7) + 1, enumStr);
                choiceStrings.Add(choiceStr);
            }
            return choiceStrings;
        }
        
        /// <summary>
        /// Changes enums to a list of strings, and also adds a back option.
        /// NOTE: This only works on enums with 7 or less choices, otherwise the back
        /// option will not be in the correct position. 
        /// </summary>
        /// <param name="enumVals"></param>
        /// <returns></returns>
        public static List<string> EnumToChoiceStringsWithBack(Array enumVals)
        {
            var choiceStrings = EnumToChoiceStrings(enumVals);
            var backString = String.Format("{0}) Back", enumVals.Length + 1);
            choiceStrings.Add(backString);
            return choiceStrings;
        }
        
        /// <summary>
        /// Displays options in a paged fashion.
        /// </summary>
        /// <returns>the index of the chosen choice</returns>
        ///       1-7 are used to select an option
        ///       8 goes back to prev page
        ///       9 goes to next page
        ///       0 cancels
        public static int PrintPagedMenu(List<string> choices, int pageNum, out bool onLastPage)
        {
            onLastPage = false;
            MenuHelper.ClearLine((Console.WindowHeight - 4));
            var totalNumChoices = choices.Count;
            var totalPages = totalNumChoices / MenuHelper.ChoicesPerPage;
            var lo = pageNum * MenuHelper.ChoicesPerPage;
            var hi = -1;
            if ((lo + MenuHelper.ChoicesPerPage) < totalNumChoices)
            {
                hi = (lo + MenuHelper.ChoicesPerPage);
            }
            else
            {
                hi = totalNumChoices;
                onLastPage = true;
            }

            string[] defaultPrompts = new string[] {    "8) Prev Page",
                                                        "9) Next Page",
                                                        "0) Back"};

            var currentPage = new List<string>();
            for (int i = lo; i < hi; i++)
            {
                //the strings in choices might or might not have keys (ex 1), 2), etc) associated with them
                //we need to add these prompts if they are not there. I think that only means when loading a resource
                var hasPrompt = Regex.IsMatch(choices[i], "^[0-9][)] ");
                if (hasPrompt)
                {
                    currentPage.Add(choices[i]);
                }
                else
                {
                    currentPage.Add(String.Format("{0}) {1}", (i - lo) + 1, choices[i]));
                }
            }
            if (pageNum != 0)
            {
                currentPage.Add(defaultPrompts[0]);
            }
            if (!onLastPage)
            {
                currentPage.Add(defaultPrompts[1]);
            }
            currentPage.Add(defaultPrompts[2]);

            MenuHelper.PrintMenuFromList(currentPage);
            return hi - 1;
        }
        
        /// <summary>
        /// Prints the given string centered on the given line.
        /// </summary>
        /// <param name="line">line to print on</param>
        /// <param name="toPrint">string to print</param>
        public static void PrintOnLine(int line, string toPrint)
        {
            MenuHelper.ClearWithinBorder(line);
            var left = (Console.WindowWidth/2) - (toPrint.Length/2);
            Console.SetCursorPosition(left, line);
            Console.WriteLine(toPrint);
        }
        
        /// <summary>
        /// Writes a prompt at the line and returns user input
        /// </summary>
        /// <param name="line"></param>
        /// <param name="toPrint"></param>
        /// <returns></returns>
        public static string PromptOnLine(int line, string prompt = MenuHelper.Prompt)
        {
            MenuHelper.ClearWithinBorder(line);
            var left = (Console.WindowWidth / 2) - (prompt.Length / 2);
            Console.SetCursorPosition(left, line);
            Console.Write(prompt);
            Console.CursorVisible = true;
            var input = Console.ReadLine();
            Console.CursorVisible = false;
            return input;
        }
        
        /// <summary>
        /// Checks if the input string is a valid range for 1D automata.
        /// </summary>
        /// <param name="input">user's input string</param>
        /// <param name="range">int representing the range</param>
        /// <returns></returns>
        ///  Gonna limit range to some magic number like 4
        public static bool ValidRangeInput(string input, out int range)
        {
            range = 0;
            try
            {
                var temp = Int32.Parse(input);
                if(temp < 1 || temp > 4)
                {
                    return false;
                }
                range = temp;
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public static bool ValidHexInput(string input, out string hex)
        {
            hex = String.Empty;
            var temp = input.ToUpper();
            var allValid = temp.All(c => "0123456789ABCDEF".Contains(c));
            if (!allValid)
            {
                return false;
            }
            hex = temp;
            return true;
        }
        
        
    } // end class
    
}