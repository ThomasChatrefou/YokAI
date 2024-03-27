using _Scripts.Utilities;
using NaughtyAttributes;
using UnityEngine;
using YokAI.AI;
using YokAI.Debugging;
using YokAI.Main;
using YokAI.Properties;
using YGrid = YokAI.Main.Grid;
using YColor = YokAI.Properties.Color;

namespace YokAI.UnitTests
{
    [CreateAssetMenu(menuName = "ScriptableObjects/UnitTests/PositionLoader", fileName = "Test_PositionLoader")]
    public class PositionLoaderTest : ScriptableObject
    {
        [Button]
        public void TestStartPosition()
        {
            GameController.SetupYokaiNoMori();
        }

        [Button]
        public void TestGridCopy()
        {
            YGrid first = YGrid.Create();
            first[0] = 1;
            YGrid second = first;
            second[1] = 2;
            Debug.Log($"First : {first[0]} {first[1]}");
            Debug.Log($"Second : {second[0]} {second[1]}");
        }

        [Button]
        public void TestBanCopy()
        {
            GameController.SetupYokaiNoMori();

            YokAIBan copy = GameController.GetBanCopy();
            uint moveOnCopy = GameController.CreateMove(
                movingPieceId: 4, // white pawn
                startCoordX: 1,
                startCoordY: 1,
                targetCoordX: 1,
                targetCoordY: 2);
            copy.MakeMove(moveOnCopy);

            string mainBanPlayingColor = YokAIDebugger.GetPlayingColor();
            string copyPlayingColor = copy.PlayingColor == YColor.WHITE ? ColorName.WHITE : (copy.PlayingColor == YColor.BLACK ? ColorName.BLACK : ColorName.UNKNOWN);

            Debug.Log($"Main : " + mainBanPlayingColor);
            Debug.Log($"Copy : " + copyPlayingColor);
        }

        [Button]
        public void TestSorting()
        {
            uint[] array = new uint[] {2,5,3,8,9,1,0};
            
            debugArray(array);
            //array.SortMoves(MoveComparator.CompareMove);
            Debug.Log("Sort");
            debugArray(array);

        }
        
        static void debugArray(uint[] arr)
        {
            var str = "";
            
            int n = arr.Length;
            for (int i = 0; i < n; ++i)
            {
                str += arr[i] + "; ";
            }
            Debug.Log(str);
        }

        [Button]
        public void TestMoves()
        {
            uint[] moves = {Move.Create(0, 15, 0,0,false, false, false),
                            Move.Create(0, 15, 0,0,true, false, false),
                            Move.Create(1, 15, 0,0,false, true, false),
                            Move.Create(0, 3, 0,0,false, false, false)};

            debugArray(moves);
            //moves.SortMoves(MoveComparator.CompareMove);
            debugArray(moves);
            
        }
    }
}
