using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;


namespace GHGameOfLife
{
///////////////////////////////////////////////////////////////////////////////
    class Program
    {
        // Don't go below these values or the text will be screwy
        static int Min_Cols = 100;
        static int Min_Rows = 30;
        // Don't go below these values or the text will be screwy

        static int Current_Cols, Current_Rows;
        static int Max_Cols = 175;
        static int Max_Rows = 52;  
  
        static int Num_Sizes = 3;  // The amount of different sizes allowed
        static BoardSize[] Valid_Sizes = new BoardSize[Num_Sizes];
        static int Curr_Size_Index = 1; // Which size to default to, 1 is med
//------------------------------------------------------------------------------
        [STAThread]
        static void Main(string[] args)
        {
            
            int initBuffWidth = Console.BufferWidth;
            int initBuffHeight = Console.BufferHeight;
            int initConsWidth = Console.WindowWidth;
            int initConsHeight = Console.WindowHeight;
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            int[] initialValues = new int[] { initBuffWidth, initBuffHeight, 
                                              initConsWidth, initConsHeight };     

            InitializeConsole();
            bool exit = false;
            do
            {
                NewMenu();

                MenuText.PromptForAnother();
                bool validKey = false;             
                while (!validKey)
                {
                    while (!Console.KeyAvailable)
                        System.Threading.Thread.Sleep(50);

                    ConsoleKey pressed = Console.ReadKey(true).Key;
                    if (pressed == ConsoleKey.Escape)
                    {
                        exit = true;
                        validKey = true;
                    }
                    if (pressed == ConsoleKey.Enter)
                    {
                        validKey = true;
                    }

                }
                

            } while (!exit);
            
            ResetConsole(initialValues);
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Initializes the console for display. 
        /// </summary>
        private static void InitializeConsole()
        {
            Console.OutputEncoding = Encoding.Unicode;
            MenuText mt = new MenuText();
            Console.BackgroundColor = MenuText.Default_BG;
            Console.ForegroundColor = MenuText.Default_FG;
            Console.Title = "Ian's Conway's Game of Life";        
            
            
            Max_Cols = (Console.LargestWindowWidth < Max_Cols)? Console.LargestWindowWidth : Max_Cols;
            Max_Rows = (Console.LargestWindowHeight < Max_Rows)? Console.LargestWindowHeight : Max_Rows;

            int difWid = (Max_Cols - Min_Cols) / (Num_Sizes - 1);
            int difHeight = Math.Max(1, (Max_Rows - Min_Rows) / (Num_Sizes - 1));

            // Initialize with the smallest window size and build from there
            // keeping the window ratio near that of the max window size ratio
            // I am just moving the width because we have more play there than height
            // unless you have some weird portrait set up I guess then enjoy your
            // small windows??

            BoardSize max = new BoardSize(Max_Cols, Max_Rows);
            double consRatio = max.Ratio;

            // Don't actually allow use of the full area to account for something probably
            Valid_Sizes[Num_Sizes - 1] = new BoardSize(Max_Cols, Max_Rows - 4);
            for (int i = Num_Sizes - 2; i >= 0; i--)
            {
                int tempCols = Math.Max(Min_Cols, Valid_Sizes[i + 1].Cols - difWid);
                int tempRows = Math.Max(Min_Rows, Valid_Sizes[i + 1].Rows - difHeight);
                Valid_Sizes[i] = new BoardSize(tempCols, tempRows);
            }


            // Check the ratios and adjust as needed
            foreach (BoardSize cs in Valid_Sizes)
            {
                while (cs.Ratio > consRatio)
                {
                    if ((cs.Cols - 1) <= Min_Cols)
                    {
                        cs.Rows = Math.Min(Max_Rows, cs.Rows + 1);
                    }
                    else
                    {
                        cs.Cols = Math.Max(Min_Cols, cs.Cols - 1);
                    }
                }

                while (cs.Ratio < consRatio)
                {
                    if ((cs.Cols + 1) >= Max_Cols)
                    {
                        cs.Rows = Math.Max(Min_Rows, cs.Rows = cs.Rows - 1);
                    }
                    else
                    {
                        cs.Cols = Math.Min(Max_Cols, (cs.Cols + 1));
                    }

                }
            }


            AdjustWindowSize(Valid_Sizes[Curr_Size_Index]);
            Current_Rows = Console.WindowHeight;
            Current_Cols = Console.WindowWidth;
            MenuText.Initialize();
            MenuText.DrawBorder();
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Displays the main menu for starting the game running.
        /// 
        ///         Prompt for 1d or 2d
        ///             if 1d   -> prompt for which rule
        ///                     -> prompt for type of starting population
        ///             if 2d   -> prompt for which rule (once implemented)
        ///                     -> prompt for type of starting population
        ///                         -> prompt for resource if needed
        /// TODO: Change rule selection to be paged
        /// </summary>
        private static void NewMenu()
        {
            var typePrompts = new List<string>() {"1) 1D Automata",
                                                  "2) 2D Automata",
                                                  "3) Exit"};

            var ruleTypes1D = Enum.GetValues(typeof(Automata1D.RuleTypes));
            var ruleTypes1DStrings = MenuText.EnumToChoiceStrings(ruleTypes1D);

            var initTypes1D = Enum.GetValues(typeof(Automata1D.BuildTypes));
            var initTypes1DStrings = MenuText.EnumToChoiceStrings(initTypes1D);

            var ruleTypes2D = Enum.GetValues(typeof(Automata2D.RuleTypes));
            var ruleTypes2DStrings = MenuText.EnumToChoiceStrings(ruleTypes2D);

            var initTypes2D = Enum.GetValues(typeof(Automata2D.BuildTypes));
            var initTypes2DStrings = MenuText.EnumToChoiceStrings(initTypes2D);

            var isTypeChosen = false;
            var isRuleChosen = false;
            var isInitChosen = false;

            var typeChoice = -1;
            var ruleChoice = -1;
            var initChoice = -1;

            var currentPrompts = typePrompts;
            var promptRow = -1;
            var numChoices = -1;

            var inputFinished = false;
            var consoleResized = false;
            var exitGame = false;
            string res2D = null;
            Console.CursorVisible = false;
            while (!inputFinished)
            {
                if(!isTypeChosen)
                {
                    currentPrompts = typePrompts;
                }
                else if(!isRuleChosen)
                {
                    if(typeChoice == 1)
                    {
                        currentPrompts = ruleTypes1DStrings;
                    }
                    else
                    {
                        currentPrompts = ruleTypes2DStrings;
                    }
                }
                else
                {
                    if( typeChoice == 1)
                    {
                        currentPrompts = initTypes1DStrings;
                    }
                    else
                    {
                        currentPrompts = initTypes2DStrings;
                    }
                }
                promptRow = MenuText.PrintMenuFromList(currentPrompts);
                numChoices = currentPrompts.Count;
                consoleResized = false;
                while (!Console.KeyAvailable)
                    System.Threading.Thread.Sleep(50);

                var keyInfo = Console.ReadKey(true);
                var charIn = keyInfo.KeyChar;
                //First, handle resizing the console
                if (keyInfo.Modifiers == ConsoleModifiers.Control)
                {
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.OemPlus:
                        case ConsoleKey.Add:
                            if (Curr_Size_Index < Valid_Sizes.Count() - 1)
                            {
                                Curr_Size_Index++;
                            }
                            ReinitializeConsole_NoPrinting();
                            consoleResized = true;
                            break;
                        case ConsoleKey.OemMinus:
                        case ConsoleKey.Subtract:
                            if (Curr_Size_Index > 0)
                            {
                                Curr_Size_Index--;
                            }
                            ReinitializeConsole_NoPrinting();
                            consoleResized = true;
                            break;
                    }
                    if (consoleResized)
                    {
                        continue;
                    }
                }
                //After checking for the resize we should process the input
                //start with chosing type, then rules, then board init type
                if(!isTypeChosen)
                {
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.D1:
                            isTypeChosen = true;
                            typeChoice = 1;
                            break;
                        case ConsoleKey.D2:
                            isTypeChosen = true;
                            typeChoice = 2;
                            break;
                        case ConsoleKey.D3:
                            inputFinished = true;
                            exitGame = true;
                            break;
                        default:
                            break;
                    }
                }
                else if(!isRuleChosen)
                {
                    //These can have different sizes and maybe even be paged...
                    //TODO: support paging like when choosing a resource 
                    if(char.IsDigit(keyInfo.KeyChar))
                    {
                        var keyVal = Int32.Parse(keyInfo.Key.ToString()[1]+"");
                        if (keyVal >= 1 && keyVal <= numChoices)
                        {
                            if(keyVal == numChoices)
                            {
                                //We hit the last option which will be Cancel or Back or something
                                isTypeChosen = false;
                                continue;
                            }
                            isRuleChosen = true;
                            ruleChoice = keyVal;
                        }
                    }
                }
                else if(!isInitChosen)
                {
                    //Rule and Type are set, now we prompt for the initial board conditions
                    if (char.IsDigit(keyInfo.KeyChar))
                    {
                        var keyVal = Int32.Parse(keyInfo.Key.ToString()[1] + "");
                        if (keyVal >= 1 && keyVal <= numChoices)
                        {
                            if (keyVal == numChoices)
                            {
                                //We hit the last option which will be Cancel or Back or something
                                //go up one menu level
                                isRuleChosen = false;
                                continue;
                            }

                            //Going to check if user wants to choose a resource
                            if(typeChoice == 2)
                            {
                                if( (Automata2D.BuildTypes)(keyVal-1) == Automata2D.BuildTypes.Resource)
                                {
                                    res2D = PromptForRes();
                                    if( string.IsNullOrEmpty(res2D) )
                                    {
                                        isInitChosen = false;
                                        continue;
                                    }
                                }
                            }
                            isInitChosen = true;
                            initChoice = keyVal;
                        }
                    }
                }
                if (isTypeChosen && isRuleChosen && isInitChosen)
                {
                    inputFinished = true;
                }            
            }
            if (!exitGame)
            {
                //run this sucker
                switch(typeChoice)
                {
                    case 1:
                        //1D automata
                        var ruleVal1D = (Automata1D.RuleTypes)(ruleChoice - 1);
                        var initVal1D = (Automata1D.BuildTypes)(initChoice - 1);
                        MenuText.ClearAllInBorder();
                        var autoBoard1D = Automata1D.InitializeAutomata(Current_Rows - 10, Current_Cols - 10, initVal1D ,ruleVal1D);
                        ConsoleRunHelper.ConsoleAutomataRunner(autoBoard1D);
                        break;
                    case 2:
                        //2D automata
                        var ruleVal2D = (Automata2D.RuleTypes)(ruleChoice - 1);
                        var initVal2D = (Automata2D.BuildTypes)(initChoice - 1);
                        MenuText.ClearAllInBorder();
                        var autoBoard2D = Automata2D.InitializeAutomata(Current_Rows - 10, Current_Cols - 10, initVal2D, ruleVal2D, res2D);
                        ConsoleRunHelper.ConsoleAutomataRunner(autoBoard2D);
                        break;
                    default:
                        //Start over if stuffs broke
                        NewMenu();
                        break;
                }
            }
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Display a list of all resources built in to the program
        /// TODO: Change this to use key presses like the NewMenu
        ///             Have this take a list of strings to display
        ///             return instead an index for the chosen string
        /// </summary>
        /// <returns>The string key value of the resource to load</returns>
        private static string PromptForRes()
        {
            //Clear the resize options, can no longer resize.
            MenuText.ClearLine((Console.WindowHeight - 4));
            string retVal = null;
            int numRes = MenuText.Large_Pops.Count;

            int promptRow = 0;
            int pageIndex = 0;
            bool reprintPage = true;
            bool onLastPage = (MenuText.Large_Pops_Pages.Count == 1);
            bool onFirstPage = true;
            List<string> currPage = null;
            
            
            bool go = true;
            while (go)
            {
                if( reprintPage )
                {
                    MenuText.ClearAllInBorder();
                    currPage = (List<string>)MenuText.Large_Pops_Pages[pageIndex];
                    promptRow = MenuText.PrintResourceMenu(currPage,onLastPage,onFirstPage);                   
                }
                reprintPage = false;

                Console.SetCursorPosition(MenuText.Left_Align, promptRow);
                Console.Write(MenuText.Prompt);              
                Console.CursorVisible = true;

                string input = "";
                int maxLen = 1;
                while (true)
                {
                    char c = Console.ReadKey(true).KeyChar;
                    if (c == '\r')
                        break;
                    if (c == '\b')
                    {
                        if (input != "")
                        {
                            input = input.Substring(0, input.Length - 1);
                            Console.Write("\b \b");
                        }
                    }
                    else if (input.Length < maxLen)
                    {
                        Console.Write(c);
                        input += c;
                    }
                }

                switch (input)
                {
                    case "1":
                    case "2":
                    case "3":
                    case "4":
                    case "5":
                    case "6":
                    case "7":
                        MenuText.ClearWithinBorder(promptRow + 1);
                        int keyVal = Int32.Parse(input);
                        if (keyVal <= currPage.Count)
                            retVal = MenuText.Large_Pops[(pageIndex * 7) + keyVal - 1];
                        go = false;
                        break;
                    case "8":
                        MenuText.ClearWithinBorder(promptRow + 1);
                        if (!onFirstPage)
                        {
                            --pageIndex;
                            reprintPage = true;
                            onLastPage = false;
                            if (pageIndex == 0)
                                onFirstPage = true;
                        }
                        break;
                    case "9":
                        MenuText.ClearWithinBorder(promptRow + 1);
                        if (!onLastPage)
                        {
                            ++pageIndex;
                            reprintPage = true;
                            onFirstPage = false;
                            if (pageIndex == MenuText.Large_Pops_Pages.Count - 1)
                                onLastPage = true;
                        }
                        break;
                    case "0":
                        MenuText.ClearWithinBorder(promptRow + 1);
                        go = false;
                        break;
                    default:
                        Console.SetCursorPosition(MenuText.Left_Align, promptRow+1);
                        Console.Write(MenuText.Entry_Error);
                        break;
                }
            }

            Console.CursorVisible = false;
            return retVal;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Reinitialize the console after resizing
        /// </summary>
        /// <returns>Returns the new row to print the response text on</returns>
        /// TODO: This does not seem to get run after having the user create their own population....?
        //private static int ReInitializeConsole()
        //{
        //    Console.Clear();
        //    AdjustWindowSize(Valid_Sizes[Curr_Size_Index]);

        //    Current_Rows = Console.WindowHeight;
        //    Current_Cols = Console.WindowWidth;
        //    MenuText.ReInitialize();
        //    MenuText.DrawBorder();
        //    return MenuText.PrintMainMenu();            
        //}
//------------------------------------------------------------------------------
        /// <summary>
        /// Makes sure the string can be converted to a valid int.
        /// Also makes sure it is in range of the number of choices presented
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static bool IsValidNumber(string s, int numPrinted)
        {
            try
            {
                int val = Int32.Parse(s);
                if (val > 0 && val <= numPrinted)
                {
                    return true;
                }
                else return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
//------------------------------------------------------------------------------
        private static void ResetConsole(int[] initValues)
        {
            int initBuffWidth = initValues[0];
            int initBuffHeight = initValues[1];
            int initConsoleWidth = initValues[2];
            int initConsHeight = initValues[3];

            MenuText.ClearLine(Current_Rows - 2);
            Console.SetCursorPosition(0, Current_Rows - 2);
                        
            Console.SetWindowSize(1, 1);
            Console.SetWindowPosition(0, 0);
            Console.SetWindowSize(initConsoleWidth, initConsHeight);
            Console.SetBufferSize(initBuffWidth, initBuffHeight);      
            Console.ResetColor();
            Console.CursorVisible = true;
        }
//------------------------------------------------------------------------------
        private static void AdjustWindowSize(BoardSize size)
        {              
            //Resize the console window
            Console.SetWindowSize(1, 1);
            Console.SetWindowPosition(0, 0);
            Console.SetBufferSize(size.Cols, size.Rows);
            Console.SetWindowSize(size.Cols, size.Rows);
            Console.SetCursorPosition(0, 0);
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Reinitialize the console after resizing
        /// </summary>
        /// <returns>Returns the new row to print the response text on</returns>
        /// TODO: This does not seem to get run after having the user create their own population....?
        private static void ReinitializeConsole_NoPrinting()
        {
            Console.Clear();
            AdjustWindowSize(Valid_Sizes[Curr_Size_Index]);

            Current_Rows = Console.WindowHeight;
            Current_Cols = Console.WindowWidth;
            MenuText.ReInitialize();
            MenuText.DrawBorder();
            //return MenuText.PrintMenuFromList(prompts);
        }
//------------------------------------------------------------------------------
    } // end class
//------------------------------------------------------------------------------
////////////////////////////////////////////////////////////////////////////////
} 