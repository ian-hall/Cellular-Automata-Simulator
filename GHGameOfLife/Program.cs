using System;
using System.IO;
using System.Text;
using System.Reflection;


namespace GHGameOfLife
{
    class Program
    {
        enum PopType { RANDOM, FILE, PREMADE, BUILD };

        // Don't go below these values or the text will be screwy
        const int MIN_WIDTH = 70;
        const int MIN_HEIGHT = 30;
        // Don't go below these values or the text will be screwy

        static int CONSOLE_WIDTH = 70; // Console width
        static int CONSOLE_HEIGHT = 30; // Console height
//------------------------------------------------------------------------------
        [STAThread]
        static void Main(string[] args)
        {
            
            int initBuffWidth = Console.BufferWidth;
            int initBuffHeight = Console.BufferHeight;
            int initConsWidth = Console.WindowWidth;
            int initConsHeight = Console.WindowHeight;            
            int initConsPosLeft = Console.WindowLeft;
            int initConsPosTop = Console.WindowTop;

            int[] initialValues = new int[] { initBuffWidth, initBuffHeight, 
                                              initConsWidth, initConsHeight, 
                                              initConsPosLeft, initConsPosTop };

            bool validWindowSize = InitializeConsole();

            if (!validWindowSize)
            {
                Console.WriteLine("Problem with console size");
            }
            else
            {
                MenuText.Initialize();
                MainMenu();
                //TODO Prompt to go again before resetting the console and closing               
            }
            ResetConsole(initialValues);
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Initializes the console for display. 
        /// </summary>
        private static bool InitializeConsole()
        {
            Console.BackgroundColor = MenuText.DefaultBG;
            Console.ForegroundColor = MenuText.DefaultFG;
            Console.Title = "Ian's Game of Life";
            
            /* Need to check the current window/buffer size before applying the
             * new size. Exits if the sizes are off.
             * TODO: Probably make it resize instead of exiting if there is a
             * problem here.
             */
            if (CONSOLE_WIDTH < MIN_WIDTH || CONSOLE_HEIGHT < MIN_HEIGHT)
                return false;
            if (CONSOLE_WIDTH > Console.LargestWindowWidth ||
                                CONSOLE_HEIGHT > Console.LargestWindowHeight)
                return false;


            Console.SetWindowSize(CONSOLE_WIDTH, CONSOLE_HEIGHT);
            Console.SetWindowPosition(0, 0);
            Console.SetBufferSize(CONSOLE_WIDTH, CONSOLE_HEIGHT);                                 
            Console.CursorVisible = false;
            Console.Clear();

            char vert =     '║'; // '\u2551'
            char horiz =    '═'; // '\u2550'
            char topLeft =  '╔'; // '\u2554'
            char topRight = '╗'; // '\u2557'
            char botLeft =  '╚'; // '\u255A'
            char botRight = '╝'; // '\u255D'

            int borderTop = 4;
            int borderBottom = CONSOLE_HEIGHT - 5;
            int borderLeft = 4;
            int borderRight = CONSOLE_WIDTH - 5;


            // This draws the nice little border on the screen...
            Console.SetCursorPosition(borderLeft, borderTop);
            Console.Write(topLeft);
            for (int i = borderLeft; i < borderRight; i++)
                Console.Write(horiz);
            Console.SetCursorPosition(borderRight,borderTop);
            Console.Write(topRight);
            for (int i = borderTop+1; i < borderBottom; i++)
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

            return true;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Displays the main menu. Pick how to load the population and whether
        /// you want to individually go through generations or just let it go
        /// for a certain number of generations.
        /// </summary>
        /// <param name="validWindowSize">Makes sure the console window
        ///                                          is of adaquate size</param>
        ///                                          
        private static void MainMenu()
        {
            PopType pop = PopType.RANDOM;
            string res = null;

            int numChoices;
            int promptRow = MenuText.PrintMainMenu(out numChoices);
            int choice = -1;

            bool validEntry = false;
            while (!validEntry)
            {
                Console.CursorVisible = true;
                MenuText.ClearWithinBorder(promptRow);
                Console.SetCursorPosition(MenuText.LeftAlign, promptRow);
                Console.Write(MenuText.Choice);                     

                String input = "";
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

                if (IsValidNumber(input, numChoices))
                {
                    choice = Int32.Parse(input);
                    validEntry = true;
                }
                else
                {
                    Console.SetCursorPosition(MenuText.LeftAlign, promptRow + 1);
                    Console.Write(MenuText.Err);
                    continue;
                }

                Console.CursorVisible = false;
                
                switch (choice)
                {
                    case 1:
                        pop = PopType.RANDOM;
                        validEntry = true;
                        break;
                    case 2:
                        pop = PopType.FILE;
                        validEntry = true;
                        break;
                    case 3:
                        pop = PopType.PREMADE;
                        res = PromptForRes();
                        if (res != null)
                            validEntry = true;
                        else
                        {
                            MenuText.PrintMainMenu(out numChoices);
                            validEntry = false;
                        }
                        break;
                    case 4:
                        pop = PopType.BUILD;
                        validEntry = true;
                        break;
                    default:
                        Console.SetCursorPosition(MenuText.LeftAlign, promptRow + 2);
                        Console.Write(MenuText.Err);
                        break;
                }
            }
           
            //Clear the current options
            MenuText.ClearMenuOptions();
            
            RunGame(pop,res);
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// This starts the game going by getting the starting population loaded
        /// </summary>
        /// <param name="pop">The type of population to build</param>
        /// <param name="res">Resource to load, if needed</param>
        /// 
        private static void RunGame(PopType pop, string res = null)
        {
            GoLBoard initial = new GoLBoard(CONSOLE_HEIGHT - 10, 
                                                            CONSOLE_WIDTH - 10);
            switch (pop)
            {
                case PopType.RANDOM:
                    initial.BuildDefaultPop();
                    break;
                case PopType.FILE:
                    initial.BuildFromFile();
                    break;
                case PopType.PREMADE:
                    initial.BuildFromResource(res);
                    break;
                case PopType.BUILD:
                    initial.BuildFromUser();
                    break;
            }

            initial.Print();

            GoLRunner.RunIt(initial);

        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Display a list of all resources built in to the program
        /// TODO: Probably make this better support more resources or something
        /// </summary>
        /// <returns>The string key value of the resource to load</returns>
        private static string PromptForRes()
        {
            string retVal = null;
            int numRes = MenuText.ResNames.Count;
            int resToLoad = -1;

            int numPrinted;
            int promptRow = MenuText.PrintResourceMenu(out numPrinted);
            
            
            bool validEntry = false;
            while (!validEntry)
            {
                MenuText.ClearWithinBorder(promptRow);
                Console.SetCursorPosition(MenuText.LeftAlign, promptRow);
                Console.Write(MenuText.Choice);
                Console.CursorVisible = true;

                String input = "";
                int maxLen = 2; // Wont display more than 99 choices...
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

                if (IsValidNumber(input,numPrinted))
                {
                    //Menu starts at 1, but resources start at 0
                    resToLoad = Int32.Parse(input)-1;
                    validEntry = true;
                }
                else
                {
                    Console.SetCursorPosition(MenuText.LeftAlign, promptRow+1);
                    Console.Write(MenuText.Err);
                    continue;
                }

            }

            if (resToLoad < MenuText.ResNames.Count)
            {
                retVal = MenuText.ResNames[resToLoad].ToString();
            }

            Console.CursorVisible = false;
            return retVal;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Makes sure the string can be converted to a valid int.
        /// Also makes sure it is in range of the number of choices presnted
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static Boolean IsValidNumber(String s,int numPrinted)
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
        private static void ResetConsole( int[] initValues)
        {
            int initBuffWidth = initValues[0];
            int initBuffHeight = initValues[1];
            int initConsoleWidth = initValues[2];
            int initConsHeight = initValues[3];
            int initConsolePosLeft = initValues[4];
            int initConsolePosTop = initValues[5];

            MenuText.ClearLine(CONSOLE_HEIGHT - 2);
            Console.SetCursorPosition(0, CONSOLE_HEIGHT - 2);
            Console.Write("Press any key to exit...");
            while (!Console.KeyAvailable)
                System.Threading.Thread.Sleep(50);
            
            Console.SetWindowSize(1, 1);
            Console.SetWindowPosition(initConsolePosLeft, initConsolePosTop);
            Console.SetWindowSize(initConsoleWidth, initConsHeight);
            Console.SetBufferSize(initBuffWidth, initBuffHeight);      
            Console.ResetColor();
            Console.CursorVisible = true;
        }
//------------------------------------------------------------------------------
    } // end class
}