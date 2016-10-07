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
        protected bool pr_Initialized;
        protected int pr_Rows;
        protected int pr_Cols;
        protected int Console_Height;
        protected int Console_Width;
        protected int Generation;

        public bool Is_Initialized { get { return this.pr_Initialized; } protected set { this.pr_Initialized = value; } }
        public int Rows { get { return this.pr_Rows; } protected set { this.pr_Rows = value; } }
        public int Cols { get { return this.pr_Cols; } protected set { this.pr_Cols = value; } }
        public bool Is_Wrapping { get; set; }
        public abstract bool[,] Board_Copy { get; }
//-----------------------------------------------------------------------------
        protected ConsoleAutomata(int rows, int cols)
        {
            this.Rows = rows;
            this.Cols = cols;
            this.Console_Height = System.Console.WindowHeight;
            this.Console_Width = System.Console.WindowWidth;
            this.Generation = 1;
            this.Is_Wrapping = true;
            this.Is_Initialized = false;
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
