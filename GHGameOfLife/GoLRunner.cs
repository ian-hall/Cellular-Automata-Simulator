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
        /// </summary>
        /// <param name="b">The board to start with</param>
        /// TODO: Add a status display
        public static void RunIt(GoLBoard b)
        {
            if (!b._Initialized)
            {
                Console.ForegroundColor = MenuText.InfoColor;
                Console.Write("ERROR");
                return;
            }

            int printRow = MenuText.PrintControls();

            bool go = true;
            bool continuous = false;
            bool paused = true;
            MenuText.printStatus(continuous, paused, _SpeedIndex);
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
                    bool keyPressed = false;
                    paused = true;
                    MenuText.printStatus(continuous, paused, _SpeedIndex);
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
                            MenuText.printStatus(continuous, paused, _SpeedIndex);
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
                            MenuText.printStatus(continuous, paused, _SpeedIndex);
                        }
                        else if (pauseEntry == ConsoleKey.OemMinus || pauseEntry == ConsoleKey.Subtract)
                        {
                            if (_SpeedIndex >= 1)
                                _SpeedIndex -= 1;
                            MenuText.printStatus(continuous, paused, _SpeedIndex);
                        }
                        else if (pauseEntry == ConsoleKey.OemPlus || pauseEntry == ConsoleKey.Add)
                        {
                            if (_SpeedIndex <= 3)
                                _SpeedIndex += 1;
                            MenuText.printStatus(continuous, paused, _SpeedIndex);
                        }
                    }
                }
               
                if (pressed == ConsoleKey.OemMinus || pressed == ConsoleKey.Subtract)
                {
                    if (_SpeedIndex >= 1)
                        _SpeedIndex -= 1;
                    MenuText.printStatus(continuous, paused, _SpeedIndex);
                }
                
                if (pressed == ConsoleKey.OemPlus || pressed == ConsoleKey.Add)
                {
                    if (_SpeedIndex <= 3)
                        _SpeedIndex += 1;
                    MenuText.printStatus(continuous, paused, _SpeedIndex);
                }
                
                if (pressed == ConsoleKey.R)
                {
                    continuous  = !continuous;
                    paused = !paused;
                    MenuText.printStatus(continuous, paused, _SpeedIndex);
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