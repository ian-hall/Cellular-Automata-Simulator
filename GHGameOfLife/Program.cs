using System;
using System.IO;
using System.Text;


namespace GameOfLife
{
    class Program
    {
        enum PopType { DEFAULT, FILE };
        enum RunType { NEXTGEN, LOOP };

        // Don't go below these values or the text will be screwy
        const int MIN_WIDTH = 50;
        const int MIN_HEIGHT = 30;
        // Don't go below these values or the text will be screwy

        static int CONSOLE_WIDTH = 50; // Console width
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
            MainMenu(validWindowSize);
            ResetConsole(initialValues);
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Initializes the console for display. 
        /// </summary>
        private static bool InitializeConsole()
        {        
            /* Need to check the current window/buffer size before applying the
             * new size. An exception is thrown if the buffer somehow stays 
             * small and the window grows...
             */

            if (CONSOLE_WIDTH < MIN_WIDTH || CONSOLE_HEIGHT < MIN_HEIGHT)
                return false;

            Console.BackgroundColor = MenuEntries.DefaultBG;
            Console.ForegroundColor = MenuEntries.DefaultFG;
            Console.Title = "Ian's Game of Life";
            Console.SetWindowSize(CONSOLE_WIDTH, CONSOLE_HEIGHT);
            Console.SetWindowPosition(0, 0);
            Console.SetBufferSize(CONSOLE_WIDTH, CONSOLE_HEIGHT);                                 
            Console.CursorVisible = false;
            Console.Clear();

            //hurr unicode
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

            /* TODO: Add some pretty color to this
             */ 

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
        private static void MainMenu( bool validWindowSize )
        {
            if (!validWindowSize)
            {
                Console.WriteLine("Error: Window Size Too Small");
                return;
            }

            PopType pop = PopType.DEFAULT;
            RunType run = RunType.LOOP;
            
            int welcomeLeft = (Console.WindowWidth / 2) - 
                                            (MenuEntries.Welcome.Length / 2);
            Console.SetCursorPosition(welcomeLeft, 8);
            Console.Write(MenuEntries.Welcome);

            int windowCenter = Console.WindowHeight / 2;

            Console.SetCursorPosition(welcomeLeft, windowCenter-4);
            Console.Write(MenuEntries.PlzChoose);

            Console.SetCursorPosition(welcomeLeft + 4, windowCenter-3);
            Console.Write(MenuEntries.DefPop);
            Console.SetCursorPosition(welcomeLeft + 4, windowCenter-2);
            Console.Write(MenuEntries.FilePop);

            Boolean validEntry = false;
            while (!validEntry)
            {
                Console.SetCursorPosition(welcomeLeft, windowCenter + 2);
                Console.Write(MenuEntries.Choice);
                Console.CursorVisible = true;
                int input = 
                        (int)Char.GetNumericValue(Console.ReadKey().KeyChar);
                Console.CursorVisible = false;
                if (input == 1)
                {
                    pop = PopType.DEFAULT;
                    validEntry = true;
                }
                else if (input == 2)
                {
                    pop = PopType.FILE;
                    validEntry = true;
                }
                else
                {
                    Console.SetCursorPosition(welcomeLeft, windowCenter + 3);
                    Console.Write(MenuEntries.Err);
                    continue;
                }
            }
           
            //Clear the current options and...
            Console.SetCursorPosition(welcomeLeft+4, windowCenter - 3);
            Console.Write("".PadRight(MenuEntries.DefPop.Length));
            Console.SetCursorPosition(welcomeLeft+4, windowCenter - 2);
            Console.Write("".PadRight(MenuEntries.FilePop.Length));
            Console.SetCursorPosition(welcomeLeft, windowCenter + 2);
            Console.Write("".PadRight(MenuEntries.Choice.Length+2));
            Console.SetCursorPosition(welcomeLeft, windowCenter + 3);
            Console.Write("".PadRight(MenuEntries.Err.Length));

            // ...add the new options!
            Console.SetCursorPosition(welcomeLeft + 4, windowCenter - 3);
            Console.Write(MenuEntries.GetNext);
            Console.SetCursorPosition(welcomeLeft + 4, windowCenter - 2);
            Console.Write(MenuEntries.Loop);

            validEntry = false;
            while (!validEntry)
            {
                Console.SetCursorPosition(welcomeLeft, windowCenter + 2);
                Console.Write(MenuEntries.Choice);
                Console.CursorVisible = true;
                int input = 
                        (int)Char.GetNumericValue(Console.ReadKey().KeyChar);
                Console.CursorVisible = false;
                if (input == 1)
                {
                    run = RunType.NEXTGEN;
                    validEntry = true;
                }
                else if (input == 2)
                {
                    run = RunType.LOOP;
                    validEntry = true;
                }
                else
                {
                    Console.SetCursorPosition(welcomeLeft, windowCenter + 3);
                    Console.Write(MenuEntries.Err);
                    continue;
                }
            }

            // Clear everything again for the next prompt
            Console.SetCursorPosition(welcomeLeft, windowCenter - 4);
            Console.Write("".PadRight(MenuEntries.PlzChoose.Length));
            Console.SetCursorPosition(welcomeLeft + 4, windowCenter - 3);
            Console.Write("".PadRight(MenuEntries.GetNext.Length));
            Console.SetCursorPosition(welcomeLeft + 4, windowCenter - 2);
            Console.Write("".PadRight(MenuEntries.Loop.Length));
            Console.SetCursorPosition(welcomeLeft, windowCenter + 2);
            Console.Write("".PadRight(MenuEntries.Choice.Length + 2));
            Console.SetCursorPosition(welcomeLeft, windowCenter + 3);
            Console.Write("".PadRight(MenuEntries.Err.Length));

            if (run == RunType.LOOP)
            {
                int loopTo = 25;
                validEntry = false;
                int distToBorder = (CONSOLE_WIDTH - 5) - welcomeLeft;
                while (!validEntry)
                {
                    Console.SetCursorPosition(welcomeLeft, windowCenter);
                    Console.Write("".PadRight(distToBorder));
                    Console.SetCursorPosition(welcomeLeft, windowCenter + 1);
                    Console.Write(MenuEntries.Enter);
                    Console.SetCursorPosition(welcomeLeft, windowCenter);
                    Console.Write(MenuEntries.MaxGen);                    
                    Console.CursorVisible = true;
                                
                    String input = "";
                    //int maxLen = distToBorder - MenuEntries.MaxGen.Length;
                    int maxLen = Int32.MaxValue.ToString().Length-1;
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
                    Console.CursorVisible = false;
                    if (IsValidNumber(input))
                    {
                        loopTo = Int32.Parse(input);
                        validEntry = true;
                    }
                    else
                    {
                        Console.SetCursorPosition(welcomeLeft, 
                                                            windowCenter + 3);
                        Console.Write(MenuEntries.Err);
                        continue;
                    }
                }
                Console.SetCursorPosition(welcomeLeft, windowCenter + 3);
                Console.Write("".PadRight(MenuEntries.Err.Length));
                RunGame(pop, run, loopTo);
            }
            else
            {
                RunGame(pop, run);
            }
        }
        //----------------------------------------------------------------------
        /// <summary>
        /// This like runs the game or something
        /// </summary>
        /// <param name="pop"></param>
        /// <param name="type"></param>
        /// <param name="maxPop"></param>
        private static void RunGame(PopType pop, RunType type, int maxPop = -1)
        {
            GoLBoard initial = new GoLBoard(CONSOLE_HEIGHT - 10, 
                                                            CONSOLE_WIDTH - 10);
            switch (pop)
            {
                case PopType.DEFAULT:
                    initial.BuildDefaultPop();
                    break;
                case PopType.FILE:
                    initial.BuildFromFile();
                    break;
            }

            initial.Print();

            switch(type)
            {
                case RunType.NEXTGEN:
                    GoLRunner.NextGeneration(initial);
                    break;
                case RunType.LOOP:
                    GoLRunner.JustLoop(initial, maxPop);
                    break;              
            }

        }

//------------------------------------------------------------------------------
        private static void RunDefault()
        {
            GoLBoard b = new GoLBoard(CONSOLE_HEIGHT - 10, CONSOLE_WIDTH - 10);
            b.BuildDefaultPop();
            b.Print();
            GoLRunner.NextGeneration(b);
            //GoLRunner.JustLoop(b);
        }
//------------------------------------------------------------------------------
        private static void RunFromFile()
        {
            GoLBoard b = new GoLBoard(CONSOLE_HEIGHT - 10, CONSOLE_WIDTH - 10);
            b.BuildFromFile();
            b.Print();
            GoLRunner.NextGeneration(b);
            //GoLRunner.JustLoop(b);
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Makes sure the string can be converted to a valid int.
        /// This is used to get the generation to loop to.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static Boolean IsValidNumber(String s)
        {
            try
            {
                int val = Int32.Parse(s);
                if (val >= 0)
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

            Console.SetWindowSize(1, 1);
            Console.SetWindowPosition(initConsolePosLeft, initConsolePosTop);
            Console.SetWindowSize(initConsoleWidth, initConsHeight);
            Console.SetBufferSize(initBuffWidth, initBuffHeight);      
            Console.ResetColor();
            Console.WriteLine("Press any key to exit...");
            while (!Console.KeyAvailable)
                System.Threading.Thread.Sleep(50);
            Console.CursorVisible = true;
        }
//------------------------------------------------------------------------------
    }
}
