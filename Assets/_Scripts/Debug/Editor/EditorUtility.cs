namespace YokAI.Debugging
{
    public static class EditorUtility
    {
        public const float PADDING = 4f;

        public static float GetUniformWidthFromWindow(float windowWidth, int nbElementsInLine)
        {
            return (windowWidth - nbElementsInLine * PADDING) / nbElementsInLine;
        }
    }
}