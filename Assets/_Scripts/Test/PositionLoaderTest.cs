using NaughtyAttributes;
using UnityEngine;
using YokAI.Main;

namespace YokAI.UnitTests
{
    [CreateAssetMenu(menuName = "Test/PositionLoader", fileName = "Test_PositionLoader")]
    public class PositionLoaderTest : ScriptableObject
    {
        [Button]
        public void TestStartPosition()
        {
            GameController.SetupYokaiNoMoriPosition();
        }
    }
}
