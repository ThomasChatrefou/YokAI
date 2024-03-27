namespace _Scripts.Utilities
{
    public static class ArrayExtension
    {
        public delegate bool Comparator(uint firstVal, uint secondVal);

        public static void SortMoves(this uint[] moves, Comparator comparator)
        {
            for (int i = 1; i < moves.Length; ++i)
            {
                uint buffer = moves[i];
                int j = i - 1;

                while (j >= 0 && comparator(moves[j], buffer))
                {
                    moves[j + 1] = moves[j];
                    j = j - 1;
                }

                moves[j + 1] = buffer;
            }
        }
    }
}