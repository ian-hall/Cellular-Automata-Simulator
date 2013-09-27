using System;
using System.Text;

namespace GameOfLife
{
    /// <summary>
    /// Static class that keeps getting the next generation of the board.
    /// Will either prompt the user to step through or just loop to a 
    /// certain generation.
    /// </summary>
    static class GoLRunner
    {   
//------------------------------------------------------------------------------
        /// <summary>
        /// Goes through the board one generation at a time.
        /// Asks the user if they want to continue or not after each generation.
        /// Returns true when the user is done.
        /// </summary>
        /// <param name="b">The board to get the next generation of</param>
        public static void NextGeneration(GoLBoard b)
        {
            Boolean getNext = true;
            
            int printRow = (Console.WindowHeight) - 3;
            int promoptLeft = (Console.WindowWidth / 2) - 
                                            (MenuEntries.NextPrompt.Length / 2);
            int errLeft = (Console.WindowWidth / 2) - 
                                                   (MenuEntries.Err.Length / 2);

            while (getNext)
            {
                Boolean validEntry = false;
                while (!validEntry)
                {
                    MenuEntries.clearLine(printRow);
                    Console.SetCursorPosition(promoptLeft, printRow);
                    Console.Write(MenuEntries.NextPrompt);   
                    Console.CursorVisible = true;
                    char input = Console.ReadKey().KeyChar;
                    Console.CursorVisible = false;
                    if (input == 'y' || input == 'Y')
                    {
                        getNext = true;
                        validEntry = true;
                    }
                    else if (input == 'n' || input == 'N')
                    {
                        getNext = false;
                        validEntry = true;
                    }
                    else
                    {
                        Console.SetCursorPosition(errLeft, printRow + 1);
                        Console.Write(MenuEntries.Err);
                        continue;
                    }
                }
                MenuEntries.clearLine(printRow+1);
                if (getNext)
                {
                    b.Next();
                    b.Print();
                }
            }
            MenuEntries.clearLine(printRow);
            MenuEntries.clearLine(printRow + 1);
            Console.CursorVisible = false;
        }
//------------------------------------------------------------------------------
        /// <summary>
        /// Just loops through the board until it has done the supplied number
        /// of loops or until it is stopped.
        /// </summary>
        /// <param name="b">The board to get the next generation of</param>
        /// <param name="loops">Which generation to go to</param>
        public static void JustLoop(GoLBoard b, int maxGen)
        {
            int printRow = (Console.WindowHeight) - 3;
            int pauseLeft = (Console.WindowWidth / 2) - 
                                                 (MenuEntries.Pause.Length / 2);
            int unpauseLeft = (Console.WindowWidth / 2) - 
                                               (MenuEntries.Unpause.Length / 2);

            MenuEntries.clearLine(printRow);
            Console.SetCursorPosition(pauseLeft, printRow);
            Console.Write(MenuEntries.Pause);     

            for (int i = 0; i < maxGen; i++)
            {
                //If the user is pressing a button...
                if (Console.KeyAvailable)
                {
                    //Check if it is the space bar...
                    if (Console.ReadKey(true).Key == ConsoleKey.Spacebar)
                    {
                        //If it is, wait until the bar is pressed again
                        //To start going again
                        MenuEntries.clearLine(printRow);
                        Console.SetCursorPosition(unpauseLeft, printRow);
                        Console.Write(MenuEntries.Unpause);  
                        Boolean keyPressed = false;

                        while (!keyPressed)
                        {
                            while (!Console.KeyAvailable)
                            {
                                System.Threading.Thread.Sleep(50);
                            }
                            
                            ConsoleKey pressed = Console.ReadKey(true).Key;
                            if (pressed == ConsoleKey.Spacebar)
                            {
                                keyPressed = true;
                                MenuEntries.clearLine(printRow);                            
                                Console.SetCursorPosition(pauseLeft, printRow);
                                Console.Write(MenuEntries.Pause);  
                            }
                            // Early Exit
                            else if (pressed == ConsoleKey.Escape)
                            {
                                keyPressed = true;
                                i = maxGen;                              
                            }
                        }
                        
                    }
                }
                b.Next();
                b.Print();
                System.Threading.Thread.Sleep(33);
            }
            MenuEntries.clearLine(printRow);
        }
//------------------------------------------------------------------------------
        public static void NewRunStyle(GoLBoard b)
        {
            int printRow = (Console.WindowHeight) - 4;
            int opt1Left = (Console.WindowWidth / 2) -
                                            (MenuEntries.RunOptions1.Length / 2);
            int opt2Left = (Console.WindowWidth / 2) -
                                            (MenuEntries.RunOptions2.Length / 2);
            int opt3Left = (Console.WindowWidth / 2) -
                                            (MenuEntries.RunOptions3.Length / 2);

            Console.SetCursorPosition(opt1Left, printRow++);
            Console.Write(MenuEntries.RunOptions1);
            Console.SetCursorPosition(opt2Left, printRow++);
            Console.Write(MenuEntries.RunOptions2);
            Console.SetCursorPosition(opt3Left, printRow);
            Console.Write(MenuEntries.RunOptions3);
            bool keepRunning = true;
            while (keepRunning)
            {
                
                if (Console.KeyAvailable)
                {
                    //Check if it is the space bar...
                    if (Console.ReadKey(true).Key == ConsoleKey.Spacebar)
                    {
                        //If it is, wait until the bar is pressed again
                        //To start going again
                        Boolean keyPressed = false;

                        while (!keyPressed)
                        {
                            while (!Console.KeyAvailable)
                            {
                                System.Threading.Thread.Sleep(50);
                            }

                            ConsoleKey pressed = Console.ReadKey(true).Key;
                            if (pressed == ConsoleKey.Spacebar)
                            {
                                keyPressed = true;
                            }
                            // Early Exit
                            else if (pressed == ConsoleKey.Escape)
                            {
                                keyPressed = true;
                                keepRunning = false;
                            }
                        }

                    }
                }
                b.Next();
                b.Print();
                System.Threading.Thread.Sleep(33);
            }
            MenuEntries.clearLine(printRow);
            MenuEntries.clearLine(printRow + 1);
            Console.CursorVisible = false;
        }
//------------------------------------------------------------------------------
    } // end class
}