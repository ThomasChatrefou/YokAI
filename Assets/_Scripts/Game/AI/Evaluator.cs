using YokAI.Main;
using YokAI.Properties;

namespace YokAI.AI
{
    public abstract class Evaluator
    {
        public uint BestMove { get; protected set; } = Move.INVALID;
        public uint NbPositionReached = 0;

        public bool CanRun { get; set; } = true;

        protected EvaluationParamSO _evaluationParamSo;
        public virtual int Search(int depth, ref YokAIBan ban)
        {
            return 0;
        }

        protected virtual int Evaluate(ref YokAIBan ban)
        {
            return 0;
        }
    }
}