using UnityEngine;
using UnityEngine.Serialization;

namespace YokAI.AI
{
    
    [CreateAssetMenu(fileName = "Evaluation Param", menuName = "ScriptableObjects/EvaluationParam", order = 1)]
    public class EvaluationParamSO : ScriptableObject
    {
        public bool materialEval;
    }
}