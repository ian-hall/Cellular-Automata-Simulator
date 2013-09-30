using System;

namespace GameOfLife
{
    public static class MenuEntries
    {
        public const ConsoleColor DefaultBG = ConsoleColor.Black;
        public const ConsoleColor DefaultFG = ConsoleColor.White;
        public const ConsoleColor PopColor  = ConsoleColor.Cyan;
        public const ConsoleColor DeadColor = ConsoleColor.Gray;
        
        public const String Welcome     = "Welcome to the GAME OF LIFE!!!!";
        public const String PlsChoose   = "Please choose an option!";
        public const String PopChoice1  = "1) Random population";
        public const String PopChoice2  = "2) Load population from a file";
        public const String PopChoice3  = "3) Load a premade population";
        public const String Choice      = "Your choice: ";
        public const String Err         = "**Invalid entry**";

        public const String FileError1  = "Error loading file...";
        public const String FileError2  = "Loading random pop.";
        public const String Enter       = "Press ENTER to confirm";

        public const String NextPrompt  = "Get next generation (y/n)? ";
        public const String Pause       = "Press SPACE to pause";
        public const String Unpause     = "Press SPACE to continue " + 
                                                            "or ESC to exit";

        public const String RunOptions1 = "Press SPACE to step through, " +
                                                        "or pause if running.";
        public const String RunOptions2 = "Press R to toggle auto stepping.";
        public const String RunOptions3 = "Press ESC to exit";
//------------------------------------------------------------------------------
        public static void clearLine(int row)
        {
            Console.SetCursorPosition(0, row);
            Console.Write("".PadRight(Console.WindowWidth));
        }
//------------------------------------------------------------------------------
    }
}
