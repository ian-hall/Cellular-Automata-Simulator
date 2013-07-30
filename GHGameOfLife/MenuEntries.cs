﻿using System;

namespace GameOfLife
{
    public static class MenuEntries
    {
        public const ConsoleColor DefaultBG = ConsoleColor.Black;
        public const ConsoleColor DefaultFG = ConsoleColor.White;
        public const ConsoleColor PopColor = ConsoleColor.Cyan;
        
        public const String Welcome = "Welcome to the GAME OF LIFE!!!!one";
        public const String PlzChoose = "Please choose an option!";
        public const String DefPop = "1) Preloaded populations";
        public const String FilePop = "2) Load population from a file";
        public const String GetNext = "1) One generation at a time.";
        public const String Loop = "2) Just keep on loopin'";
        public const String MaxGen = "Gen to go to: ";
        public const String Choice = "Your choice: ";
        public const String Err = "Invalid entry.";

        public const String FileError1 = "Error loading file...";
        public const String FileError2 = "Loading default pop.";
        public const String Enter = "Press ENTER key";

        public const String NextPrompt = "Get next (y/n)? ";
        public const String Pause = "Press SPACE to pause";
        public const String Unpause = "Press SPACE to continue";
    }
}
