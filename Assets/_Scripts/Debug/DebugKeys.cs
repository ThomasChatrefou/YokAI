namespace YokAI.Debugging
{
    public static class TabNames
    {
        public const string GAME = "Game";
        public const string AI = "AI";
        public const string UNIT_TESTS = "Unit Tests";
    }

    public static class Labels
    {
        // Game Tab
        public const string GAME_INFOS = "Game Infos";
        public const string PLAYING_COLOR = "Playing Color : ";
        public const string MOVE_NUMBER = "Move Number : ";
        public const string LAST_MOVE = "Last Move : ";
        public const string AVAILABLE_MOVES = "Available Moves : ";
        public const string NEXT_MOVE = "Next Move";
        public const string LOAD_POSITION = "Load Position";

        // AI Tab
        public const string NB_POSITION_REACHED = "nb positions reached :";
    }

    public static class Buttons
    {
        public const string REFRESH = "Refresh";
        public const string VALIDATE = "Validate";
        public const string PASS_TURN = "Pass Turn";
        public const string TAKE_BACK = "Take Back";
        public const string LOAD_SFEN = "Load SFEN";
        public const string SAVE_SFEN = "Save SFEN";
        public const string EMPTY_POSITION = "Empty position";
        public const string START_POSITION = "Starting position";
        public const string DRAW = "Draw Ban in console";  //unused
    }

    public static class Feedbacks
    {
        public const string DONE = "Done";
        public const string INVALID = "Invalid";
    }

    public static class ColorName
    {
        public const string WHITE = "White"; // Sente in Shogi
        public const string BLACK = "Black"; // Gote in Shogi (but I am a Chess player...)
        public const string UNKNOWN = "Unknown";
    }
}