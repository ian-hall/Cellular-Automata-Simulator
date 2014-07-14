using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;


namespace GHGameOfLife
{
///////////////////////////////////////////////////////////////////////////////
    class Program
    {
        // Imports and junk for resizing the window
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        const short SWP_NOSIZE = 0x0001;
        const short SWP_NOZORDER = 0x0004;
        const int SWP_SHOWWINDOW = 0x0040;
        static IntPtr HWND_TOPMOST = new IntPtr(-1);
        // garbage

        enum PopType { Random, File, Premade, Build };

        // Don't go below these values or the text will be screwy
        static int Min_Cols = 70;
        static int Min_Rows = 30;
        // Don't go below these values or the text will be screwy

        static int Current_Cols, Current_Rows;

        static int Max_Cols, Max_Rows;    
        static IntPtr Current_Proc_Handle;
        static Screen Primary_Screen;
        static ScreenRes Primary_Res;

        static int DIFFERENT_SIZES = 5;  // The amount of different sizes allowed
        static ConsSize[] Cons_Sizes = new ConsSize[DIFFERENT_SIZES];
        static int Curr_Size_Index = 2; // Which size to default to, 2 is med
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

            Primary_Screen = System.Windows.Forms.Screen.PrimaryScreen;
            Primary_Res = new ScreenRes(Primary_Screen.Bounds.Width, Primary_Screen.Bounds.Height);

            
            //Current_Proc_Handle = Process.GetCurrentProcess().MainWindowHandle;
            Console.SetCursorPosition(0, 0);
            Console.Write("");
            Process[] cmdProc = Process.GetProcesses();
            Rect smallRect;
            Rect bigRect;
            foreach (Process p in cmdProc)
            {
                IntPtr currentHandle;

                if (p.MainWindowHandle == IntPtr.Zero)
                    continue;

                try
                {
                    currentHandle = p.MainWindowHandle;
                }
                catch
                {
                    continue;
                }

                Console.SetWindowSize(1, 1);
                Console.SetWindowPosition(0, 0);
                //Console.SetBufferSize(1, 1);
                System.Threading.Thread.Sleep(5);
                //Console.SetCursorPosition(0, 0);
                //Console.WriteLine("Testing Process: {0} {1}", p.ProcessName, p.MainWindowHandle);
                GetWindowRect(currentHandle, out smallRect);
                Console.SetWindowSize(initConsWidth,initConsHeight);
                Console.SetBufferSize(initBuffWidth, initBuffHeight);
                System.Threading.Thread.Sleep(5);
                GetWindowRect(currentHandle, out bigRect);
                //Console.WriteLine("SR:{0}\nBR:{1}", smallRect, bigRect);                

                if (smallRect.CompareTo(bigRect) < 0)
                {
                    Current_Proc_Handle = currentHandle;
                    break;
                }

            }

            if (Current_Proc_Handle == IntPtr.Zero)
            {
                Current_Proc_Handle = Process.GetCurrentProcess().MainWindowHandle;
                //Current_Proc = Process.GetCurrentProcess();
            }
            //Console.WriteLine("Using process: {0}", Current_Proc);
            //Console.ReadLine();
            InitializeConsole();
            bool exit = false;
            do
            {              
                MainMenu();

                MenuText.PromptForAnother();
                bool validKey = false;             
                while (!validKey)
                {
                    while (!Console.KeyAvailable)
                        System.Threading.Thread.Sleep(50);

                    ConsoleKey pressed = Console.ReadKey(false).Key;
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
            Console.BackgroundColor = MenuText.Default_BG;
            Console.ForegroundColor = MenuText.Default_FG;
            Console.Title = "Ian's Conway's Game of Life";        
            
            Max_Cols = Console.LargestWindowWidth;
            Max_Rows = Console.LargestWindowHeight;

            //int diffSizes = 5;
            int difWid = (Max_Cols - Min_Cols - 10) / (DIFFERENT_SIZES - 1);
            int difHeight = Math.Max(1, (Max_Rows - Min_Rows - 5) / (DIFFERENT_SIZES - 1));
            //Cons_Sizes = new ConsSize[diffSizes];

            // Initialize with the smallest window size and build from there
            // Keep around a 10:3 ratio for the window (col:row)
            // I am just moving the width because we have more play there than height
            // Unless you have some weird portrait set up I guess then enjoy your
            // small windows??
            Cons_Sizes[0] = new ConsSize(Min_Cols, Min_Rows);
            for (int i = 1; i < DIFFERENT_SIZES; i++)
            {
                Cons_Sizes[i] = new ConsSize(Cons_Sizes[i - 1].Cols + difWid, Cons_Sizes[i - 1].Rows + difHeight);
                while (Cons_Sizes[i].Ratio > (1.0 * 10 / 3))
                {
                    Cons_Sizes[i].Cols = (Cons_Sizes[i].Cols - 1);
                }
            }

            foreach (ConsSize cs in Cons_Sizes)
            {
                while (cs.Ratio < (1.0 * 10 / 3))
                {
                    cs.Cols = (cs.Cols + 1);
                }
            }

            ajustWindowSize(Primary_Res, Cons_Sizes[Curr_Size_Index]);
            Current_Rows = Console.WindowHeight;
            Current_Cols = Console.WindowWidth;
            MenuText.Initialize();

            char vert =     '║'; // '\u2551'
            char horiz =    '═'; // '\u2550'
            char topLeft =  '╔'; // '\u2554'
            char topRight = '╗'; // '\u2557'
            char botLeft =  '╚'; // '\u255A'
            char botRight = '╝'; // '\u255D'

            int borderTop = 4;
            int borderBottom = Current_Rows- 5;
            int borderLeft = 4;
            int borderRight = Current_Cols - 5;


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

            //MenuText.PrintWelcome();
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Displays the main menu. Pick how to load the population.
        /// Display the choice and ask for confirmation instead of just
        /// jumping to the next screen incase someone hits a wrong button
        /// </summary>
        ///                                          
        private static void MainMenu()
        {
            PopType pop = PopType.Random;
            string res = null;

            int numChoices = MenuText.NMenu_Choice;
            int currPromptRow = MenuText.PrintMainMenu();
            int choice = -1;

            //Only allow letters and numbers to be written as a choice
            //TODO: Add this to the resource selection menu
            string allCharDec = "abcdefghijklmnopqrstuvwxyz1234567890";

            bool validEntry = false;
            int newPromptRow = currPromptRow;
            bool resized = false;

            while (!validEntry)
            {
                if (resized)
                    currPromptRow = newPromptRow;

                resized = false;
                Console.CursorVisible = true;
                MenuText.ClearWithinBorder(currPromptRow);
                Console.SetCursorPosition(MenuText.Left_Align, currPromptRow);
                Console.Write(MenuText.Prompt);                   

                string input = "";
                int maxLen = 1;

                while (true)
                {
                    ConsoleKeyInfo cki = Console.ReadKey(true);
                    char c = cki.KeyChar;
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
                    //Need to reinitialize the console/menu positioning after changing window size
                    else if ((cki.Key == ConsoleKey.OemPlus || cki.Key == ConsoleKey.Add) && cki.Modifiers == ConsoleModifiers.Control)
                    {
                        if (Curr_Size_Index < Cons_Sizes.Count() - 1)
                        {
                            Curr_Size_Index++;
                        }
                        newPromptRow = ReInitializeConsole();
                        resized = true;
                        break;
                    }
                    else if ((cki.Key == ConsoleKey.OemMinus || cki.Key == ConsoleKey.Subtract) && cki.Modifiers == ConsoleModifiers.Control)
                    {
                        if (Curr_Size_Index > 0)
                        {
                            Curr_Size_Index--;
                        }
                        newPromptRow = ReInitializeConsole();
                        resized = true;
                        break;
                    }
                    else if (!allCharDec.Contains(c))
                    {
                        //This is here so these characters are not written
                    }
                    else if (input.Length < maxLen)
                    {
                        Console.Write(c);
                        input += c;
                    }
                    else
                        System.Threading.Thread.Sleep(50);
                }

                if (resized)
                    continue;
               
                if (IsValidNumber(input, numChoices))
                {
                    choice = Int32.Parse(input);
                    validEntry = true;
                }
                else
                {
                    Console.SetCursorPosition(MenuText.Left_Align, currPromptRow + 1);
                    Console.Write(MenuText.Err);
                    continue;
                }

                Console.CursorVisible = false;
                
                switch (choice)
                {
                    case 1:
                        pop = PopType.Random;
                        validEntry = true;
                        break;
                    case 2:
                        pop = PopType.File;
                        validEntry = true;
                        break;
                    case 3:
                        pop = PopType.Premade;
                        res = PromptForRes();
                        if (res != null)
                            validEntry = true;
                        else
                        {
                            MenuText.PrintMainMenu();
                            validEntry = false;
                        }
                        break;
                    case 4:
                        pop = PopType.Build;
                        validEntry = true;
                        break;
                    case 5:
                        validEntry = true;
                        return;
                    default:
                        Console.SetCursorPosition(MenuText.Left_Align, currPromptRow + 2);
                        Console.Write(MenuText.Err);
                        validEntry = false;
                        break;
                }
            }
           
            //Clear the current options
            MenuText.ClearAllInBoarder();
            
            //Move out into the main loop maybe
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
            GoL game = new GoL(Current_Rows - 10, Current_Cols - 10);
            switch (pop)
            {
                case PopType.Random:
                    game.BuildDefaultPop();
                    break;
                case PopType.File:
                    game.BuildFromFile();
                    break;
                case PopType.Premade:
                    game.BuildFromResource(res);
                    break;
                case PopType.Build:
                    game.BuildFromUser();
                    break;
            }

            game.RunGame();
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
            int numRes = MenuText.Res_Names.Count;
            int resToLoad = -1;

            int numPrinted;
            int promptRow = MenuText.PrintResourceMenu(out numPrinted);
            
            
            bool validEntry = false;
            while (!validEntry)
            {
                MenuText.ClearWithinBorder(promptRow);
                Console.SetCursorPosition(MenuText.Left_Align, promptRow);
                Console.Write(MenuText.Prompt);
                Console.CursorVisible = true;

                string input = "";
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
                    Console.SetCursorPosition(MenuText.Left_Align, promptRow+1);
                    Console.Write(MenuText.Err);
                    continue;
                }

            }

            if (resToLoad < MenuText.Res_Names.Count)
            {
                retVal = MenuText.Res_Names[resToLoad].ToString();
            }

            Console.CursorVisible = false;
            return retVal;
        }
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
        /// <summary>
        /// Reinitialize the console after resizing
        /// </summary>
        /// <returns>Returns the new row to print the response text on</returns>
        private static int ReInitializeConsole()
        {
            Console.Clear();
            ajustWindowSize(Primary_Res, Cons_Sizes[Curr_Size_Index]);

            Current_Rows = Console.WindowHeight;
            Current_Cols = Console.WindowWidth;
            MenuText.ReInitialize();

            char vert = '║';        // '\u2551'
            char horiz = '═';       // '\u2550'
            char topLeft = '╔';     // '\u2554'
            char topRight = '╗';    // '\u2557'
            char botLeft = '╚';     // '\u255A'
            char botRight = '╝';    // '\u255D'

            int borderTop = 4;
            int borderBottom = Current_Rows - 5;
            int borderLeft = 4;
            int borderRight = Current_Cols - 5;


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

            return MenuText.PrintMainMenu();
            
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Makes sure the string can be converted to a valid int.
        /// Also makes sure it is in range of the number of choices presnted
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static Boolean IsValidNumber(string s, int numPrinted)
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
            int initConsolePosLeft = initValues[4];
            int initConsolePosTop = initValues[5];

            MenuText.ClearLine(Current_Rows - 2);
            Console.SetCursorPosition(0, Current_Rows - 2);
            
            //Console.Write("Press any key to exit...");
            //while (!Console.KeyAvailable)
            //    System.Threading.Thread.Sleep(50);
            
            Console.SetWindowSize(1, 1);
            Console.SetWindowPosition(initConsolePosLeft, initConsolePosTop);
            Console.SetWindowSize(initConsoleWidth, initConsHeight);
            Console.SetBufferSize(initBuffWidth, initBuffHeight);      
            Console.ResetColor();
            Console.CursorVisible = true;
        }
//------------------------------------------------------------------------------
        private static void ajustWindowSize(ScreenRes primaryRes, ConsSize size)
        {              
            //Resize the console window
            Console.SetWindowSize(1, 1);
            Console.SetWindowPosition(0, 0);
            Console.SetBufferSize(size.Cols, size.Rows);
            Console.SetWindowSize(size.Cols, size.Rows);
            Console.SetCursorPosition(0, 0);
            Console.Write("");

            //Center on the screen
            // Some kind of bug where the out Rect comes back with a height of 74,
            // So just loop until it gets the correct position.
            // Also check if the out Rect is zero before doing this to avoid
            // an infinite loop.
            
            //IntPtr windowHandle = Current_Proc.MainWindowHandle;
            Rect consRect;
            GetWindowRect(Current_Proc_Handle, out consRect);
            if (!consRect.IsZero())
            {
                while ((consRect.Bottom - consRect.Top) < 100)
                {
                    GetWindowRect(Current_Proc_Handle, out consRect);
                }
            }

            //int consWidth = consRect.Right - consRect.Left;
            //int consHeight = consRect.Bottom - consRect.Top;
            int widthOffset = (primaryRes.Width / 2) - (consRect.Width / 2);
            int heightOffset = (primaryRes.Height / 2) - (consRect.Height / 2);
            SetWindowPos(Current_Proc_Handle, HWND_TOPMOST, widthOffset, heightOffset, 0, 0, SWP_NOZORDER | SWP_NOSIZE | SWP_SHOWWINDOW);

            /*
            ConsoleTraceListener ctl = new ConsoleTraceListener(true);
            ctl.WriteLine(String.Format("Top Left: {0,-5} Top Right: {1,-5} Window Rect:{2}", widthOffset, heightOffset, consRect));
             */ 
        }
//------------------------------------------------------------------------------
    } // end class
//------------------------------------------------------------------------------
////////////////////////////////////////////////////////////////////////////////
//------------------------------------------------------------------------------
////////////////////////////////////////////////////////////////////////////////
} 