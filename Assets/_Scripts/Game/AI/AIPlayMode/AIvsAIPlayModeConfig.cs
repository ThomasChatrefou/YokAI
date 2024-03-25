using NaughtyAttributes;
using UnityEngine;

namespace YokAI.AI
{
    [CreateAssetMenu(fileName = "AIvsAIPlayModeConfig", menuName = "AIPlayMode/AIvsAI", order = 2)]
    public class AIvsAIPlayModeConfig : AIPlayModeConfig
    {
        [SerializeField][BoxGroup("Evaluators")] private EvaluationParamSO _opponentEvaluationParamSo;

        public override void Setup(ref AIController whiteController, ref AIController blackController)
        {
            _opponent = _opponentEvaluationParamSo;
            base.Setup(ref whiteController, ref blackController);
        }
    }
}
