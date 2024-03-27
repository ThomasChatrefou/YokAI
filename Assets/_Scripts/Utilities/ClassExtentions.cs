using YokAI.Main;

namespace _Scripts.Utilities
{
    public static class ArrayExtension
    {
        public delegate bool Comparator(uint firstVal, uint secondVal, YokAIBan ban);

        public static void SortMoves(this uint[] moves, Comparator comparator, YokAIBan ban)
        {
            for (int i = 1; i < moves.Length; ++i)
            {
                uint buffer = moves[i];
                int j = i - 1;

                while (j >= 0 && comparator(moves[j], buffer, ban))
                {
                    moves[j + 1] = moves[j];
                    j = j - 1;
                }

                moves[j + 1] = buffer;
            }
        }
    }
}