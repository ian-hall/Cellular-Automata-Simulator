namespace Core_Automata
{
    /// <summary>
    /// abstract class for handling ConsoleAutomata whatevers.
    /// </summary>
    /// Members include:
    ///         __Is_Initialized -> bool representing board initialization
    ///         __Num_Rows,__Num_Cols -> ints representing the dimensions of the game board
    ///         __Orig_Console_Height,__Orig_Console_Width -> ints representing the size of the console on game initializtion
    ///         __Generation -> current generation of the game    
    ///         abstract Board -> 2D array of bools representing the current game board

    abstract class ConsoleAutomata
    {
        protected bool _prInitialized;
        protected int _prRows;
        protected int _prCols;
        protected int _consoleHeight;
        protected int _consoleWidth;
        protected int _generation;

        public bool Is_Initialized { get { return this._prInitialized; } protected set { this._prInitialized = value; } }
        public int Rows { get { return this._prRows; } protected set { this._prRows = value; } }
        public int Cols { get { return this._prCols; } protected set { this._prCols = value; } }
        public bool Is_Wrapping { get; set; }
        public abstract bool[,] BoardCopy { get; }

        protected ConsoleAutomata(int rows, int cols)
        {
            this.Rows = rows;
            this.Cols = cols;
            this._consoleHeight = System.Console.WindowHeight;
            this._consoleWidth = System.Console.WindowWidth;
            this._generation = 1;
            this.Is_Wrapping = true;
            this.Is_Initialized = false;
        }

        /// <summary>
        /// Generates the next generation for the board
        /// </summary>
        public abstract void NextGeneration();

        /// <summary>
        /// Prints the board to the console window
        /// </summary>
        public abstract void PrintBoard();

    }

}
