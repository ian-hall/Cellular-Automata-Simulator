using System;
using System.Text;

namespace GHGameOfLife
{
    /// <summary>
    /// Static class that keeps getting the next generation of the board.
    /// Will either prompt the user to step through or loop 
    /// </summary>
    static class GoLRunner
    {
//------------------------------------------------------------------------------
        /// <summary>
        /// Runs the game
        /// </summary>
        /// <param name="b">The board to start with</param>
        /// TODO: Add a status display
        public static void NewRunStyle(GoLBoard b)
        {
            if (!b._Initialized)
            {
                Console.ForegroundColor = MenuText.InfoColor;
                Console.Write("ERROR");
                return;
            }
            
            int printRow = MenuText.PrintControls();
            /*int opt1Left = (Console.WindowWidth / 2) -
                                        (MenuText.RunOptions1.Length / 2);
            int opt2Left = (Console.WindowWidth / 2) -
                                        (MenuText.RunOptions2.Length / 2);
            int opt3Left = (Console.WindowWidth / 2) -
                                        (MenuText.RunOptions3.Length / 2);

            Console.SetCursorPosition(opt1Left, printRow++);
            Console.Write(MenuText.RunOptions1);
            Console.SetCursorPosition(opt2Left, printRow++);
            Console.Write(MenuText.RunOptions2);
            Console.SetCursorPosition(opt3Left, printRow);
            Console.Write(MenuText.RunOptions3);*/
            bool go = true;
            bool continuous = false;
            bool paused = true;
            MenuText.printStatus(continuous, paused);
            while (go)
            {
                // If it isnt running, and no keys are pressed
                while (!Console.KeyAvailable && !continuous)
                {
                    System.Threading.Thread.Sleep(50);
                }
                // if it IS running, and no keys are pressed
                while (!Console.KeyAvailable && continuous)
                {
                    b.Next();
                    //b.Print();
                    b.TestPrint();
                    System.Threading.Thread.Sleep(33);
                }
                //if PAUSE is pressed while it is not running
                ConsoleKey pressed = Console.ReadKey(true).Key;
                if (pressed == ConsoleKey.Spacebar && !continuous)
                {
                    b.Next();
                    //b.Print();
                    b.TestPrint();
                }

                /// if paused while running, wait until space
                /// is pressed again to start going
                if (pressed == ConsoleKey.Spacebar && continuous)
                {
                    bool keyPressed = false;
                    paused = true;
                    MenuText.printStatus(continuous, paused);
                    while (!keyPressed)
                    {
                        while (!Console.KeyAvailable)
                        {
                            System.Threading.Thread.Sleep(50);
                        }

                        ConsoleKey pauseEntry = Console.ReadKey(true).Key;
                        if (pauseEntry == ConsoleKey.Spacebar) //unpause
                        {
                            keyPressed = true;
                            paused = false;
                            MenuText.printStatus(continuous, paused);
                        }
                        else if (pauseEntry == ConsoleKey.Escape)
                        {
                            keyPressed = true;
                            go = false;
                        }
                        else if (pauseEntry == ConsoleKey.R)
                        {
                            continuous = false;
                            keyPressed = true;
                            MenuText.printStatus(continuous, paused);
                        }
                    }
                }

                if (pressed == ConsoleKey.R)
                {
                    continuous  = !continuous;
                    paused = !paused;
                    MenuText.printStatus(continuous, paused);
                }

                if (pressed == ConsoleKey.Escape)
                {
                    go = false;
                }
            }
            //MenuText.ClearLine(printRow);
            //MenuText.ClearLine(printRow + 1);
            Console.CursorVisible = false;
        }
//------------------------------------------------------------------------------
        /*private static void printStatus(bool running, bool paused)
        {
            Console.ForegroundColor = MenuText.InfoColor;
            MenuText.ClearLine(3);
            if (running)
            {
                Console.SetCursorPosition(5, 3);
                Console.Write("AUTO");
            }
            if (paused)
            {
                Console.SetCursorPosition(10, 3);
                Console.Write("PAUSED");
            }          
            Console.ForegroundColor = MenuText.DefaultFG;
        }*/
 //------------------------------------------------------------------------------
    } // end class
}