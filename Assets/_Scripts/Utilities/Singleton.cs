using UnityEngine;
    
namespace _Scripts.Utilities
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static bool IsValid { get; private set; } = false;

        public static T Instance
        {
            get
            {
                _instance ??= FindAnyObjectByType<T>() ?? new GameObject(typeof(T).Name + " " + nameof(Singleton<T>)).AddComponent<T>();
                IsValid = true;
                //DontDestroyOnLoad(_instance);

                return _instance;
            }
        }
        private static T _instance;

        private void OnDestroy()
        {
            _instance = null;
            IsValid = false;
        }
    }
}