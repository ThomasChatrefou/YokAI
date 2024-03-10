namespace YokAI.Main
{
    public static class GameController
    {
        public static Ban CurrentBan;
        public static uint LastMove { get; private set; }
        public static int MoveNumber { get; private set; }
        public static uint PlayingColor { get { return CurrentBan.PlayingColor; } }

        public static string StartingPositionSFEN { get; set; }

        static GameController()
        {
            CurrentBan = new Ban();
        }

        public static void MakeMove()
        {

        }

        public static void TakeBack()
        {

        }

        public static void PassTurn()
        {

        }

        public static void GetAvailableMoves()
        {

        }

        public static void SetupEmptyPosition()
        {

        }

        public static void SetupStartPosition()
        {

        }

        public static void SaveCurrentPosition()
        {

        }
    }
}