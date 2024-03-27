using UnityEngine;
using UnityEngine.Serialization;

namespace YokAI.AI
{
    
    [CreateAssetMenu(fileName = "Evaluation Param", menuName = "ScriptableObjects/EvaluationParam", order = 1)]
    public class EvaluationParamSO : ScriptableObject
    {
        [Tooltip("Calculates the material (number of pieces and there associated importance) on the board for the given player")]
        public bool materialEval;
    }
}