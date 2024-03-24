using YokAI.Main;
using YokAI.Notation;

namespace YokAI.AI
{
    public class AIController
    {
        private Evaluator _evaluator;

        public Evaluator Evaluator => _evaluator;

        public AIController(EvaluationParamSO evaluationParamSo)
        {
            _evaluator = new FirstEvaluator(evaluationParamSo);
        }

        public int EvaluateCurrentPosition(int depth, ref YokAIBan const_ban, out string bestMoveNotation)
        {
            _evaluator.NbPositionReached = 0;
            YokAIBan banCopy = const_ban;
            int eval = _evaluator.Search(depth, ref banCopy);
            bestMoveNotation = Decryptor.GetNotationFromMove(_evaluator.BestMove, ref const_ban);
            return eval;
        }
    }
}