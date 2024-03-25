using NaughtyAttributes;
using UnityEngine;

namespace YokAI.AI
{
    public abstract class AIPlayModeConfig : ScriptableObject
    {
        [SerializeField][BoxGroup("Evaluators")] private EvaluationParamSO _aiEvaluationParamSo;
        [SerializeField] private bool _randomFirstPlayer;
        [SerializeField][HideIf(nameof(_randomFirstPlayer))] private bool _aiPlaysFirst;

        public virtual void Setup(ref AIController whiteController, ref AIController blackController)
        {
            if ((_randomFirstPlayer && Random.value > .5f) || (!_randomFirstPlayer && _aiPlaysFirst))
            {
                whiteController = new AIController(_aiEvaluationParamSo);
                blackController = _opponent != null ? new AIController(_opponent) : null;
            }
            else
            {
                whiteController = _opponent != null ? new AIController(_opponent) : null;
                blackController = new AIController(_aiEvaluationParamSo);
            }
        }

        protected EvaluationParamSO _opponent = null;
    }
}