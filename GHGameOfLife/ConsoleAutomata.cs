namespace GHGameOfLife
{
    /// <summary>
    /// abstract class for handling ConsoleAutomata whatevers.
    /// Members include:
    ///         __Is_Initialized -> bool representing board initialization
    ///         __Num_Rows,__Num_Cols -> ints representing the dimensions of the game board
    ///         __Orig_Console_Height,__Orig_Console_Width -> ints representing the size of the console on game initializtion
    ///         __Generation -> current generation of the game    
    ///         abstract Board -> 2D array of bools representing the current game board
    /// </summary>
///////////////////////////////////////////////////////////////////////////////
    abstract class ConsoleAutomata
    {
        protected bool __Is_Initialized;
        protected int __Num_Rows;
        protected int __Num_Cols;
        protected int __Orig_Console_Height;
        protected int __Orig_Console_Width;
        protected int __Generation;

        public bool Is_Initialized { get { return this.__Is_Initialized; } }
        public int Rows { get { return this.__Num_Rows; } }
        public int Cols { get { return this.__Num_Cols; } }
        public int Console_Height { get { return this.__Orig_Console_Height; } }
        public int Console_Width { get { return this.__Orig_Console_Width; } }
        public bool Is_Wrapping { get; set; }
        public abstract bool[,] Board { get; }
//-----------------------------------------------------------------------------
        protected ConsoleAutomata(int rows, int cols)
        {
            this.__Num_Rows = rows;
            this.__Num_Cols = cols;
            this.__Orig_Console_Height = System.Console.WindowHeight;
            this.__Orig_Console_Width = System.Console.WindowWidth;
            this.__Generation = 1;
            this.Is_Wrapping = true;
            this.__Is_Initialized = false;
        }
//-----------------------------------------------------------------------------
        /// <summary>
        /// Generates the next generation for the board
        /// </summary>
        public abstract void NextGeneration();
//-----------------------------------------------------------------------------
        /// <summary>
        /// Prints the board to the console window
        /// </summary>
        public abstract void PrintBoard();
//-----------------------------------------------------------------------------
    }
///////////////////////////////////////////////////////////////////////////////
}
