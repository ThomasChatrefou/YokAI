namespace YokAI.Debugging
{
    public abstract class DebuggerTab
    {
        public float WinWidth => _window.position.width;

        public static T Create<T>(DebugWindow window) where T : DebuggerTab, new()
        {
            T newTab = new() { _window = window };
            return newTab;
        }

        public abstract void Open();

        protected DebugWindow _window;
    }
}