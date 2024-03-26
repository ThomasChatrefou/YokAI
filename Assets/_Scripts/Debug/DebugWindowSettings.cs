using System;
using NaughtyAttributes;
using UnityEngine;

namespace YokAI.Debugging
{
    [CreateAssetMenu(menuName = "ScriptableObjects/DebugWindowSettings", fileName = "DebugWindowSettings")]
    public class DebugWindowSettings : ScriptableObject
    {
        public float GameTabLabelWidth { get { return _gameTabLabelWidth; } }
        public float GameTabSectionsInterspace { get { return _gameTabSectionsInterspace; } }

        public float AiTabLabelWidth { get { return _aiTabLabelWidth; } }

        public event Action OnChanged;

        private void OnValueChanged()
        {
            OnChanged?.Invoke();
        }

        [BoxGroup("Game Tab")]
        [OnValueChanged(nameof(OnValueChanged))]
        [SerializeField][Range(10, 1000)] private float _gameTabLabelWidth = DefaultDebugWindowSettings.LABEL_WIDTH;
        [BoxGroup("Game Tab")]
        [OnValueChanged(nameof(OnValueChanged))]
        [SerializeField][Range(1, 100)] private float _gameTabSectionsInterspace = DefaultDebugWindowSettings.SECTIONS_INTERSPACE;

        [BoxGroup("AI Tab")]
        [OnValueChanged(nameof(OnValueChanged))]
        [SerializeField][Range(10, 1000)] private float _aiTabLabelWidth = DefaultDebugWindowSettings.LABEL_WIDTH;

    }

    public static class DefaultDebugWindowSettings
    {
        public const float LABEL_WIDTH = 100f;
        public const float SECTIONS_INTERSPACE = 10f;
    }
}