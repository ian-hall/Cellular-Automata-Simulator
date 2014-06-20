using System;
using System.Text;

namespace GHGameOfLife
{
    /// <summary>
    /// Static class that keeps getting the next generation of the board.
    /// Will either prompt the user to step through or loop 
    /// Probably should add this to the GolBoard class
    /// or combine it or something
    /// </summary>
    static class GoLRunner
    {
        private static int[] Speeds = {132,100,66,50,33};
        private static int Curr_Speed_Index = 2; //Start at a 66ms wait
//------------------------------------------------------------------------------
        /// <summary>
        /// Runs the game
        /// Starts paused, with stepping enabled
        /// </summary>
        /// <param name="b">The board to start with</param>
        public static void RunIt(GoLBoard b)
        {
            if (!b.IsInitialized)
            {
                Console.ForegroundColor = MenuText.Info_Color;
                Console.Write("ERROR");
                return;
            }

            MenuText.PrintRunControls();

            bool go = true;
            bool continuous = false;
            bool paused = true;
            bool wrapping = b.Wrap;
            MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
            while (go)
            {
                // If it isnt running, and no keys are pressed
                while (!Console.KeyAvailable && !continuous)
                {
                    System.Threading.Thread.Sleep(10);
                }
                // if it IS running, and no keys are pressed
                while (!Console.KeyAvailable && continuous)
                {
                    b.Next();
                    b.Print();
                    System.Threading.Thread.Sleep(Speeds[Curr_Speed_Index]);
                }
                //if PAUSE is pressed while it is not running
                ConsoleKey pressed = Console.ReadKey(true).Key;
                if (pressed == ConsoleKey.Spacebar && !continuous)
                {
                    b.Next();
                    b.Print();
                }

                /// if paused while running, wait until space
                /// is pressed again to start going
                if (pressed == ConsoleKey.Spacebar && continuous)
                {
                    bool exitPauseLoop = false;
                    paused = true;
                    MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                    while (/*paused*/!exitPauseLoop)
                    {
                        while (!Console.KeyAvailable)
                        {
                            System.Threading.Thread.Sleep(50);
                        }

                        ConsoleKey pauseEntry = Console.ReadKey(true).Key;
                        if (pauseEntry == ConsoleKey.Spacebar) //unpause
                        {
                            exitPauseLoop = true;
                            paused = false;
                            MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                        }
                        else if (pauseEntry == ConsoleKey.Escape) //exit
                        {
                            go = false;
                            paused = false;
                            exitPauseLoop = true;
                        }
                        else if (pauseEntry == ConsoleKey.R) // stop looping
                        {
                            continuous = !continuous;
                            exitPauseLoop = true;
                            //paused = !paused;
                            MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                        }
                        else if (pauseEntry == ConsoleKey.W) // toggle wrapping
                        {
                            wrapping = !wrapping;
                            b.Wrap = wrapping;
                            MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                        }
                        //These two change speed
                        else if (pauseEntry == ConsoleKey.OemMinus || pauseEntry == ConsoleKey.Subtract)
                        {
                            if (Curr_Speed_Index >= 1)
                                Curr_Speed_Index -= 1;
                            MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                        }
                        else if (pauseEntry == ConsoleKey.OemPlus || pauseEntry == ConsoleKey.Add)
                        {
                            if (Curr_Speed_Index <= 3)
                                Curr_Speed_Index += 1;
                            MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                        }
                    }
                }
               
                if (pressed == ConsoleKey.OemMinus || pressed == ConsoleKey.Subtract)
                {
                    if (Curr_Speed_Index >= 1)
                        Curr_Speed_Index -= 1;
                    MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                }
                
                if (pressed == ConsoleKey.OemPlus || pressed == ConsoleKey.Add)
                {
                    if (Curr_Speed_Index <= 3)
                        Curr_Speed_Index += 1;
                    MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                }
                
                if (pressed == ConsoleKey.R)
                {
                    continuous  = !continuous;
                    paused = !paused;
                    MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                }
                
                if (pressed == ConsoleKey.W)
                {
                    wrapping = !wrapping;
                    b.Wrap = wrapping;
                    MenuText.PrintStatus(continuous, paused, wrapping, Curr_Speed_Index);
                }

                if (pressed == ConsoleKey.Escape)
                {
                    go = false;
                }
            }

            Console.CursorVisible = false;
        }
 //------------------------------------------------------------------------------
    } // end class
}