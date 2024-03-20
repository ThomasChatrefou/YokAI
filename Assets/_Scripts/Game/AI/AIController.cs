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

        public int EvaluateCurrentPosition(int depth, YokAIBan ban, out string bestMoveNotation)
        {
            _evaluator.NbPositionReached = 0;
            int eval = _evaluator.Search(depth, ref ban);
            bestMoveNotation = Decryptor.GetNotationFromMove(_evaluator.BestMove, ban);
            return eval;
        }
    }
}