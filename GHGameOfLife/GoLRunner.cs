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
        private static int[] _Speeds = {132,100,66,50,33};
        private static int _SpeedIndex = 2; //Start at a 66ms wait
//------------------------------------------------------------------------------
        /// <summary>
        /// Runs the game
        /// Starts paused, with stepping enabled
        /// </summary>
        /// <param name="b">The board to start with</param>
        /// TODO: Add a status display
        public static void RunIt(GoLBoard b)
        {
            if (!b.IsInitialized)
            {
                Console.ForegroundColor = MenuText.InfoColor;
                Console.Write("ERROR");
                return;
            }

            MenuText.PrintRunControls();

            bool go = true;
            bool continuous = false;
            bool paused = true;
            bool wrapping = b.Wrap;
            MenuText.PrintStatus(continuous, paused, wrapping, _SpeedIndex);
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
                    System.Threading.Thread.Sleep(_Speeds[_SpeedIndex]);
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
                    //bool keyPressed = false;
                    paused = true;
                    MenuText.PrintStatus(continuous, paused, wrapping, _SpeedIndex);
                    while (/*!keyPressed &&*/ paused)
                    {
                        while (!Console.KeyAvailable)
                        {
                            System.Threading.Thread.Sleep(50);
                        }

                        ConsoleKey pauseEntry = Console.ReadKey(true).Key;
                        if (pauseEntry == ConsoleKey.Spacebar) //unpause
                        {
                            //keyPressed = true;
                            paused = false;
                            MenuText.PrintStatus(continuous, paused, wrapping, _SpeedIndex);
                        }
                        else if (pauseEntry == ConsoleKey.Escape) //exit
                        {
                            //keyPressed = true;
                            go = false;
                            paused = false;
                        }
                        else if (pauseEntry == ConsoleKey.R) // stop looping
                        {
                            continuous = false;
                            //keyPressed = true;
                            paused = false;
                            MenuText.PrintStatus(continuous, paused, wrapping, _SpeedIndex);
                        }
                        else if (pauseEntry == ConsoleKey.W) // toggle wrapping
                        {
                            wrapping = !wrapping;
                            b.Wrap = wrapping;
                            //keyPressed = true;
                            MenuText.PrintStatus(continuous, paused, wrapping, _SpeedIndex);
                        }
                        //These two change speed
                        else if (pauseEntry == ConsoleKey.OemMinus || pauseEntry == ConsoleKey.Subtract)
                        {
                            if (_SpeedIndex >= 1)
                                _SpeedIndex -= 1;
                            MenuText.PrintStatus(continuous, paused, wrapping, _SpeedIndex);
                        }
                        else if (pauseEntry == ConsoleKey.OemPlus || pauseEntry == ConsoleKey.Add)
                        {
                            if (_SpeedIndex <= 3)
                                _SpeedIndex += 1;
                            MenuText.PrintStatus(continuous, paused, wrapping, _SpeedIndex);
                        }
                    }
                }
               
                if (pressed == ConsoleKey.OemMinus || pressed == ConsoleKey.Subtract)
                {
                    if (_SpeedIndex >= 1)
                        _SpeedIndex -= 1;
                    MenuText.PrintStatus(continuous, paused, wrapping, _SpeedIndex);
                }
                
                if (pressed == ConsoleKey.OemPlus || pressed == ConsoleKey.Add)
                {
                    if (_SpeedIndex <= 3)
                        _SpeedIndex += 1;
                    MenuText.PrintStatus(continuous, paused, wrapping, _SpeedIndex);
                }
                
                if (pressed == ConsoleKey.R)
                {
                    continuous  = !continuous;
                    paused = !paused;
                    MenuText.PrintStatus(continuous, paused, wrapping, _SpeedIndex);
                }
                
                if (pressed == ConsoleKey.W)
                {
                    wrapping = !wrapping;
                    b.Wrap = wrapping;
                    MenuText.PrintStatus(continuous, paused, wrapping, _SpeedIndex);
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
    } // end class
}