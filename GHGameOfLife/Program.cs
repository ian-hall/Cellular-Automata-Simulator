﻿using System;
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

            InitializeConsole();
            bool exit = false;
            do
            {
                NewMenu();

                MenuHelper.PromptForAnother();
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
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Initializes the console for display. 
        /// </summary>
        private static void InitializeConsole()
        {
            Console.OutputEncoding = Encoding.Unicode;
            MenuHelper mt = new MenuHelper();
            Console.BackgroundColor = MenuHelper.Default_BG;
            Console.ForegroundColor = MenuHelper.Default_FG;
            Console.Title = "Ian's Automata Whatevers";        
            
            
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
            MenuHelper.Initialize();
            MenuHelper.DrawBorder();
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
            var ruleTypes1DStrings = MenuHelper.EnumToChoiceStrings(ruleTypes1D);

            var initTypes1D = Enum.GetValues(typeof(Automata1D.BuildTypes));
            var initTypes1DStrings = MenuHelper.EnumToChoiceStrings(initTypes1D);

            var ruleTypes2D = Enum.GetValues(typeof(Automata2D.RuleTypes));
            var ruleTypes2DStrings = MenuHelper.EnumToChoiceStrings(ruleTypes2D);

            var initTypes2D = Enum.GetValues(typeof(Automata2D.BuildTypes));
            var initTypes2DStrings = MenuHelper.EnumToChoiceStrings(initTypes2D);

            var isTypeChosen = false;
            var isRuleChosen = false;
            var isInitChosen = false;

            var typeChoice = -1;
            var ruleChoice = -1;
            var initChoice = -1;

            var currentPrompts = typePrompts;
            var numChoices = -1;
            var promptPage = 0;
            var onLastPage = false;

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
                    MenuHelper.PrintMenuFromList(currentPrompts);
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
                    numChoices = PagedPrompt(currentPrompts,promptPage, out onLastPage);
                }
                else if(!isInitChosen)
                {
                    if( typeChoice == 1)
                    {
                        currentPrompts = initTypes1DStrings;
                    }
                    else
                    {
                        currentPrompts = initTypes2DStrings;
                    }
                    MenuHelper.PrintMenuFromList(currentPrompts);
                }
                //MenuHelper.PrintMenuFromList(currentPrompts);
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
                        case ConsoleKey.NumPad1:
                            isTypeChosen = true;
                            typeChoice = 1;
                            break;
                        case ConsoleKey.D2:
                        case ConsoleKey.NumPad2:
                            isTypeChosen = true;
                            typeChoice = 2;
                            break;
                        case ConsoleKey.D3:
                        case ConsoleKey.NumPad3:
                            inputFinished = true;
                            exitGame = true;
                            break;
                        default:
                            break;
                    }
                }
                else if(!isRuleChosen)
                {
                    if (char.IsDigit(keyInfo.KeyChar))
                    {
                        var keyVal = Int32.Parse(keyInfo.KeyChar + "");
                        switch (keyVal)
                        {
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                                //a choice has been made, do (page*numPerPage)+choice - 1 to get the index and return
                                //first check if that is a valid value i.e. cant choose option 5 if only 3 are displayed
                                var intendedIndex = ((promptPage * 7) + keyVal) - 1;
                                if (intendedIndex < currentPrompts.Count)
                                {
                                    //This means we have a valid choice, return this value
                                    ruleChoice = intendedIndex;
                                    isRuleChosen = true;
                                }
                                break;
                            case 8:
                                //Go to the previous page if one is available
                                if (promptPage > 0)
                                {
                                    --promptPage;
                                }
                                break;
                            case 9:
                                //Go to the next page if one is available
                                if (!onLastPage)
                                {
                                    ++promptPage;
                                }
                                break;
                            case 0:
                                //exit without returning a value
                                isTypeChosen = false;
                                continue;
                        }
                    }
                    //if(char.IsDigit(keyInfo.KeyChar))
                    //{
                    //    var keyVal = Int32.Parse("" + keyInfo.KeyChar);
                    //    if (keyVal >= 1 && keyVal <= numChoices)
                    //    {
                    //        if(keyVal == numChoices)
                    //        {
                    //            //We hit the last option which will be Cancel or Back or something
                    //            isTypeChosen = false;
                    //            continue;
                    //        }
                    //        isRuleChosen = true;
                    //        ruleChoice = keyVal;
                    //    }
                    //}
                }
                else if(!isInitChosen)
                {
                    //Rule and Type are set, now we prompt for the initial board conditions
                    if (char.IsDigit(keyInfo.KeyChar))
                    {
                        var keyVal = Int32.Parse("" + keyInfo.KeyChar);
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
                        MenuHelper.ClearAllInBorder();
                        var autoBoard1D = Automata1D.InitializeAutomata(Current_Rows - 10, Current_Cols - 10, initVal1D ,ruleVal1D);
                        ConsoleRunHelper.ConsoleAutomataRunner(autoBoard1D);
                        break;
                    case 2:
                        //2D automata
                        var ruleVal2D = (Automata2D.RuleTypes)(ruleChoice - 1);
                        var initVal2D = (Automata2D.BuildTypes)(initChoice - 1);
                        MenuHelper.ClearAllInBorder();
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
        /// Displays options in a paged fashion.
        /// TODO: 1-7 are used to select an option
        ///       8 goes back to prev page
        ///       9 goes to next page
        ///       0 cancels
        /// </summary>
        /// <returns>the index of the chosen choice</returns>
        private static int PagedPrompt(List<string> choices, int pageNum, out bool onLastPage)
        {
            //Clear the resize options, can no longer resize.
            onLastPage = false;
            MenuHelper.ClearLine((Console.WindowHeight - 4));
            var choicesPerPage = 7;
            var totalNumChoices = choices.Count;
            var totalPages = totalNumChoices / choicesPerPage;
            //var numPrintedChoices = -1;
            //var isValueChosen = false;
            //var chosenChoice = 0;
            //var pageNum = 0;
            var lo = pageNum * choicesPerPage;
            var hi = -1;
            if ((lo + choicesPerPage) < totalNumChoices)
            {
                hi = (lo + choicesPerPage);
            }
            else
            {
                hi = totalNumChoices;
                onLastPage = true;
            }
            //var startingHi = (totalNumChoices < choicesPerPage) ? totalNumChoices : choicesPerPage;

            string[] defaultPrompts = new string[] {    "8) Prev Page",
                                                        "9) Next Page",
                                                        "0) Cancel"};

            var currentPage = new List<string>();
            for( int i = lo; i < hi; i++ )
            {
                currentPage.Add(choices.ElementAt(i));
            }
            foreach (var prompt in defaultPrompts)
            {
                currentPage.Add(prompt);
            }

            MenuHelper.PrintMenuFromList(currentPage);

            //while (!isValueChosen)
            //{
            //    MenuHelper.PrintMenuFromList(currentPage);
            //    //numPrintedChoices = currentPage.Count;
                
            //    while (!Console.KeyAvailable)
            //        System.Threading.Thread.Sleep(50);

            //    var keyInfo = Console.ReadKey(true);
            //    var charIn = keyInfo.KeyChar;

            //    if (char.IsDigit(keyInfo.KeyChar))
            //    {
            //        var keyVal = Int32.Parse(keyInfo.KeyChar + "");
            //        switch(keyVal)
            //        {
            //            case 1:
            //            case 2:
            //            case 3:
            //            case 4:
            //            case 5:
            //            case 6:
            //            case 7:
            //                //a choice has been made, do (page*numPerPage)+choice - 1 to get the index and return
            //                //first check if that is a valid value i.e. cant choose option 5 if only 3 are displayed
            //                var intendedIndex = ((pageNum * choicesPerPage) + keyVal) - 1;
            //                if(intendedIndex < totalNumChoices )
            //                {
            //                    //This means we have a valid choice, return this value
            //                    chosenChoice = intendedIndex;
            //                    isValueChosen = true;
            //                }
            //                break;
            //            case 8:
            //                //Go to the previous page if one is available
            //                if(pageNum > 0 )
            //                {
            //                    --pageNum;
            //                    var lo = (pageNum * choicesPerPage);
            //                    var hi = (lo + choicesPerPage);
            //                    currentPage = new List<string>();
            //                    for ( int i = lo; i < hi; i++ )
            //                    {
            //                        currentPage.Add(choices.ElementAt(i));
            //                    }
            //                    foreach(var prompt in defaultPrompts)
            //                    {
            //                        currentPage.Add(prompt);
            //                    }
            //                }
            //                break;
            //            case 9:
            //                //Go to the next page if one is available
            //                if(pageNum < (totalPages-1) )
            //                {
            //                    ++pageNum;
            //                    var lo = (pageNum * choicesPerPage);
            //                    var hi = (lo + choicesPerPage);
            //                    currentPage = new List<string>();
            //                    for (int i = lo; i < hi; i++)
            //                    {
            //                        currentPage.Add(choices.ElementAt(i));
            //                    }
            //                    foreach (var prompt in defaultPrompts)
            //                    {
            //                        currentPage.Add(prompt);
            //                    }
            //                }
            //                break;
            //            case 0:
            //                //exit without returning a value
            //                isValueChosen = true;
            //                chosenChoice = -1;
            //                break;
            //        }
            //    }
            //}

            return hi-1;
            //string retVal = null;
            //int numRes = MenuHelper.Large_Pops.Count;

            //int promptRow = 0;
            //int pageIndex = 0;
            //bool reprintPage = true;
            //bool onLastPage = (MenuHelper.Large_Pops_Pages.Count == 1);
            //bool onFirstPage = true;
            //List<string> currPage = null;


            //bool go = true;
            //while (go)
            //{
            //    if (reprintPage)
            //    {
            //        MenuHelper.ClearAllInBorder();
            //        currPage = (List<string>)MenuHelper.Large_Pops_Pages[pageIndex];
            //        promptRow = MenuHelper.PrintResourceMenu(currPage, onLastPage, onFirstPage);
            //    }
            //    reprintPage = false;

            //    Console.SetCursorPosition(MenuHelper.Left_Align, promptRow);
            //    Console.Write(MenuHelper.Prompt);
            //    Console.CursorVisible = true;

            //    string input = "";
            //    int maxLen = 1;
            //    while (true)
            //    {
            //        char c = Console.ReadKey(true).KeyChar;
            //        if (c == '\r')
            //            break;
            //        if (c == '\b')
            //        {
            //            if (input != "")
            //            {
            //                input = input.Substring(0, input.Length - 1);
            //                Console.Write("\b \b");
            //            }
            //        }
            //        else if (input.Length < maxLen)
            //        {
            //            Console.Write(c);
            //            input += c;
            //        }
            //    }

            //    switch (input)
            //    {
            //        case "1":
            //        case "2":
            //        case "3":
            //        case "4":
            //        case "5":
            //        case "6":
            //        case "7":
            //            MenuHelper.ClearWithinBorder(promptRow + 1);
            //            int keyVal = Int32.Parse(input);
            //            if (keyVal <= currPage.Count)
            //                retVal = MenuHelper.Large_Pops[(pageIndex * 7) + keyVal - 1];
            //            go = false;
            //            break;
            //        case "8":
            //            MenuHelper.ClearWithinBorder(promptRow + 1);
            //            if (!onFirstPage)
            //            {
            //                --pageIndex;
            //                reprintPage = true;
            //                onLastPage = false;
            //                if (pageIndex == 0)
            //                    onFirstPage = true;
            //            }
            //            break;
            //        case "9":
            //            MenuHelper.ClearWithinBorder(promptRow + 1);
            //            if (!onLastPage)
            //            {
            //                ++pageIndex;
            //                reprintPage = true;
            //                onFirstPage = false;
            //                if (pageIndex == MenuHelper.Large_Pops_Pages.Count - 1)
            //                    onLastPage = true;
            //            }
            //            break;
            //        case "0":
            //            MenuHelper.ClearWithinBorder(promptRow + 1);
            //            go = false;
            //            break;
            //        default:
            //            Console.SetCursorPosition(MenuHelper.Left_Align, promptRow + 1);
            //            Console.Write(MenuHelper.Msg_Entry_Error);
            //            break;
            //    }
            //}

            //Console.CursorVisible = false;
            //return retVal;
        }
//------------------------------------------------------------------------------
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
            MenuHelper.ClearLine((Console.WindowHeight - 4));
            string retVal = null;
            int numRes = MenuHelper.Large_Pops.Count;

            int promptRow = 0;
            int pageIndex = 0;
            bool reprintPage = true;
            bool onLastPage = (MenuHelper.Large_Pops_Pages.Count == 1);
            bool onFirstPage = true;
            List<string> currPage = null;
            
            
            bool go = true;
            while (go)
            {
                if( reprintPage )
                {
                    MenuHelper.ClearAllInBorder();
                    currPage = (List<string>)MenuHelper.Large_Pops_Pages[pageIndex];
                    promptRow = MenuHelper.PrintResourceMenu(currPage,onLastPage,onFirstPage);                   
                }
                reprintPage = false;

                Console.SetCursorPosition(MenuHelper.Left_Align, promptRow);
                Console.Write(MenuHelper.Prompt);              
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
                        MenuHelper.ClearWithinBorder(promptRow + 1);
                        int keyVal = Int32.Parse(input);
                        if (keyVal <= currPage.Count)
                            retVal = MenuHelper.Large_Pops[(pageIndex * 7) + keyVal - 1];
                        go = false;
                        break;
                    case "8":
                        MenuHelper.ClearWithinBorder(promptRow + 1);
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
                        MenuHelper.ClearWithinBorder(promptRow + 1);
                        if (!onLastPage)
                        {
                            ++pageIndex;
                            reprintPage = true;
                            onFirstPage = false;
                            if (pageIndex == MenuHelper.Large_Pops_Pages.Count - 1)
                                onLastPage = true;
                        }
                        break;
                    case "0":
                        MenuHelper.ClearWithinBorder(promptRow + 1);
                        go = false;
                        break;
                    default:
                        Console.SetCursorPosition(MenuHelper.Left_Align, promptRow+1);
                        Console.Write(MenuHelper.Msg_Entry_Error);
                        break;
                }
            }

            Console.CursorVisible = false;
            return retVal;
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
        private static void ReinitializeConsole_NoPrinting()
        {
            Console.Clear();
            AdjustWindowSize(Valid_Sizes[Curr_Size_Index]);

            Current_Rows = Console.WindowHeight;
            Current_Cols = Console.WindowWidth;
            MenuHelper.ReInitialize();
            MenuHelper.DrawBorder();
            //return MenuText.PrintMenuFromList(prompts);
        }
//------------------------------------------------------------------------------
    } // end class
//------------------------------------------------------------------------------
////////////////////////////////////////////////////////////////////////////////
} 