using NaughtyAttributes;
using UnityEngine;

namespace YokAI
{
    namespace Test
    {
        [CreateAssetMenu(menuName = "Test/PositionLoader", fileName = "Test_PositionLoader")]
        public class PositionLoaderTest : ScriptableObject
        {
            [Button]
            public void TestStartPosition()
            {
                PositionLoader.LoadPositionFromSFEN(PositionLoader.STARTING_POSITION);
                BanDebugger.DebugLog();
            }
        }
    }
}
