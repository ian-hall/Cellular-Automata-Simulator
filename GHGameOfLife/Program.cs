using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Core_Automata.Rules;


namespace Core_Automata
{

    class Program
    {
        // Don't go below these values or the text will be screwy
        static readonly int MinCols = 100;
        static readonly int MinRows = 30;
        // Don't go below these values or the text will be screwy

        static int CurrentCols, CurrentRows;
        
        //seriously don't remember how i came up with these numbers
        static int MaxCols = 175;
        static int MaxRows = 52;  
  
        static readonly int NumSizes = 3;  // The amount of different sizes allowed
        static BoardSize[] ValidSizes = new BoardSize[NumSizes];
        static int CurrentSizeIdx = 0; // Default to smallest size

        [STAThread]
        static void Main(string[] args)
        {
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

        /// <summary>
        /// Initializes the console for display. 
        /// </summary>
        private static void InitializeConsole()
        {
            Console.OutputEncoding = Encoding.Unicode;
            Console.BackgroundColor = MenuHelper.DefaultBG;
            Console.ForegroundColor = MenuHelper.DefaultFG;
            Console.Title = "Ian's Automata Whatever";        
            
            
            MaxCols = (Console.LargestWindowWidth < MaxCols)? Console.LargestWindowWidth : MaxCols;
            MaxRows = (Console.LargestWindowHeight < MaxRows)? Console.LargestWindowHeight : MaxRows;

            int difWid = (MaxCols - MinCols) / (NumSizes - 1);
            int difHeight = Math.Max(1, (MaxRows - MinRows) / (NumSizes - 1));

            // Initialize with the smallest window size and build from there
            // keeping the window ratio near that of the max window size ratio
            // I am just moving the width because we have more play there than height
            // unless you have some weird portrait set up I guess then enjoy your
            // small windows??

            BoardSize max = new BoardSize(MaxCols, MaxRows);
            double consRatio = max.Ratio;

            // Don't actually allow use of the full area to account for something probably
            ValidSizes[NumSizes - 1] = new BoardSize(MaxCols, MaxRows - 4);
            for (int i = NumSizes - 2; i >= 0; i--)
            {
                int tempCols = Math.Max(MinCols, ValidSizes[i + 1].Cols - difWid);
                int tempRows = Math.Max(MinRows, ValidSizes[i + 1].Rows - difHeight);
                ValidSizes[i] = new BoardSize(tempCols, tempRows);
            }


            // Check the ratios and adjust as needed
            foreach (BoardSize cs in ValidSizes)
            {
                while (cs.Ratio > consRatio)
                {
                    if ((cs.Cols - 1) <= MinCols)
                    {
                        cs.Rows = Math.Min(MaxRows, cs.Rows + 1);
                    }
                    else
                    {
                        cs.Cols = Math.Max(MinCols, cs.Cols - 1);
                    }
                }

                while (cs.Ratio < consRatio)
                {
                    if ((cs.Cols + 1) >= MaxCols)
                    {
                        cs.Rows = Math.Max(MinRows, cs.Rows = cs.Rows - 1);
                    }
                    else
                    {
                        cs.Cols = Math.Min(MaxCols, (cs.Cols + 1));
                    }

                }
            }

            AdjustWindowSize(ValidSizes[CurrentSizeIdx]);
            CurrentRows = Console.WindowHeight;
            CurrentCols = Console.WindowWidth;
            MenuHelper.DrawBorder();
        }
        
        /// <summary>
        /// Displays the main menu for starting the game running.
        /// </summary>
        /// 
        ///         Prompt for 1d or 2d
        ///             if 1d   -> prompt for which rule
        ///                     -> prompt for type of starting population
        ///             if 2d   -> prompt for which rule (once implemented)
        ///                     -> prompt for type of starting population
        ///                         -> prompt for resource if needed
        private static void NewMenu()
        {
            var typePrompts = new List<string>() {"1) 1D Automata",
                                                  "2) 2D Automata",
                                                  "3) Exit"};

            var ruleTypes1DStrings = MenuHelper.EnumToChoiceStrings(Rules1D.RuleNames);

            var initTypes1D = Enum.GetValues(typeof(Automata1D.BuildTypes));
            var initTypes1DStrings = MenuHelper.EnumToChoiceStringsWithBack(initTypes1D);

            var ruleTypes2DStrings = MenuHelper.EnumToChoiceStrings(Rules2D.RuleNames);

            var initTypes2D = Enum.GetValues(typeof(Automata2D.BuildTypes));
            var initTypes2DStrings = MenuHelper.EnumToChoiceStringsWithBack(initTypes2D);

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
                    numChoices = MenuHelper.PrintPagedMenu(currentPrompts,promptPage, out onLastPage);
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
                            if (CurrentSizeIdx < ValidSizes.Count() - 1)
                            {
                                CurrentSizeIdx++;
                            }
                            ReinitializeConsoleWithoutPrinting();
                            consoleResized = true;
                            break;
                        case ConsoleKey.OemMinus:
                        case ConsoleKey.Subtract:
                            if (CurrentSizeIdx > 0)
                            {
                                CurrentSizeIdx--;
                            }
                            ReinitializeConsoleWithoutPrinting();
                            consoleResized = true;
                            break;
                    }
                    if (consoleResized)
                    {
                        continue;
                    }
                }
                //After checking for the resize we should process the input
                //start with choosing type, then rules, then board init type
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
                                var intendedIndex = ((promptPage * MenuHelper.ChoicesPerPage) + keyVal) - 1;
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
                        var ruleVal1D = Rules1D.RuleNames[ruleChoice];
                        var initVal1D = (Automata1D.BuildTypes)(initChoice - 1);
                        MenuHelper.ClearAllInBorder();
                        var autoBoard1D = Automata1D.InitializeAutomata(CurrentRows - 10, CurrentCols - 10, initVal1D, ruleVal1D);
                        ConsoleRunHelper.ConsoleAutomataRunner(autoBoard1D);
                        break;
                    case 2:
                        //2D automata
                        var ruleVal2D = Rules2D.RuleNames[ruleChoice];
                        var initVal2D = (Automata2D.BuildTypes)(initChoice - 1);
                        MenuHelper.ClearAllInBorder();
                        var autoBoard2D = Automata2D.InitializeAutomata(CurrentRows - 10, CurrentCols - 10, initVal2D, ruleVal2D, res2D);
                        ConsoleRunHelper.ConsoleAutomataRunner(autoBoard2D);
                        break;
                    default:
                        //Start over if stuffs broke
                        NewMenu();
                        break;
                }
            }
        }
        
        /// <summary>
        /// Display a list of all resources built in to the program
        /// </summary>
        /// <returns>The string key value of the resource to load</returns>
        /// TODO: Have multiple resources specific to certain life rules
        ///       as well as one common resource list for all rules
        private static string PromptForRes()
        {
            //Clear the resize options since we are not supporting resizing here.
            MenuHelper.ClearLine((Console.WindowHeight - 4));

            int numRes = MenuHelper.LargePops.Count;
            int choiceVal = -1;
            int pageIndex = 0;
            bool onLastPage = false;            
            
            bool isValueChosen = false;
            while (!isValueChosen)
            {
                choiceVal = MenuHelper.PrintPagedMenu(MenuHelper.LargePops, pageIndex, out onLastPage);

                while(!Console.KeyAvailable)
                {
                    System.Threading.Thread.Sleep(50);
                }

                var keyInfo = Console.ReadKey(true);
                var charIn = keyInfo.KeyChar;

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
                            var intendedIndex = ((pageIndex * MenuHelper.ChoicesPerPage) + keyVal) - 1;
                            if (intendedIndex < numRes)
                            {
                                //This means we have a valid choice, return this value
                                choiceVal = intendedIndex;
                                isValueChosen = true;
                            }
                            break;
                        case 8:
                            //Go to the previous page if one is available
                            if (pageIndex > 0)
                            {
                                --pageIndex;                              
                            }
                            break;
                        case 9:
                            //Go to the next page if one is available
                            if (!onLastPage)
                            {
                                ++pageIndex;
                            }
                            break;
                        case 0:
                            //exit without returning a value
                            isValueChosen = true;
                            choiceVal = -1;
                            break;
                    }
                }
            }

            Console.CursorVisible = false;
            if(choiceVal != -1)
            {
                return MenuHelper.LargePops[choiceVal];
            }
            else
            {
                return null;
            }
        }

        private static void AdjustWindowSize(BoardSize size)
        {              
            //Resize the console window
            Console.SetWindowSize(1, 1);
            Console.SetWindowPosition(0, 0);
            Console.SetBufferSize(size.Cols, size.Rows);
            Console.SetWindowSize(size.Cols, size.Rows);
            Console.SetCursorPosition(0, 0);
        }

        /// <summary>
        /// Reinitialize the console after resizing
        /// </summary>
        private static void ReinitializeConsoleWithoutPrinting()
        {
            Console.Clear();
            AdjustWindowSize(ValidSizes[CurrentSizeIdx]);

            CurrentRows = Console.WindowHeight;
            CurrentCols = Console.WindowWidth;
            MenuHelper.ReInitialize();
            MenuHelper.DrawBorder();
        }

    } // end class

} 