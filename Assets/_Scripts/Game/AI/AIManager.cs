using System;
using System.Threading.Tasks;
using _Scripts.Utilities;
using NaughtyAttributes;
using UnityEngine;
using YokAI.Main;
using YokAI.Notation;
using Debug = UnityEngine.Debug;

namespace YokAI.AI
{
    public class AIManager : Singleton<AIManager>
    {
        [SerializeField] private EvaluationParamSO _defaultEvaluationParamSo;
        [SerializeField] private AIPlayModeConfig _playMode;

        [Header("AI General Params")] 
        
        [SerializeField][Range(1,10)] private int _maxEvaluationDepth = 5;
        [SerializeField][Tooltip("in ms")] private int _maxThinkTime = 1000;

        private AIController _defaultAI;
        private AIController _playerOne;
        private AIController _playerTwo;

        [HideInInspector] public bool IsAISet;

        [Button]
        public void MakeMove()
        {
            _defaultAI.Evaluator.CanRun = true;
            PlayAI(_defaultAI);
        }
        
        private void Awake()
        {
            _defaultAI = new AIController(_defaultEvaluationParamSo);
        }

        private void Start()
        {
            if (_playMode == null)
            {
                IsAISet = false;
                return;
            }

            _playMode.Setup(ref _playerOne, ref _playerTwo);
            if (_playerOne != null)
            {
                PlayAI(_playerOne);
            }
            IsAISet = true;
        }

        private void OnEnable()
        {
            GameController.OnMoveMade += OnMoveMade;
            BoardManager.Instance.OnAutoMoveMade += OnMoveMade;
            BoardManager.Instance.OnDisabled += OnBoardDisabled;
        }

        private void OnBoardDisabled()
        {
            GameController.OnMoveMade -= OnMoveMade;
            BoardManager.Instance.OnAutoMoveMade -= OnMoveMade;
            BoardManager.Instance.OnDisabled -= OnBoardDisabled;
        }
        
        private void OnMoveMade()
        {
            if (!IsAISet) return;
            if (!BoardManager.Instance.IsReady) return;

            if (GameController.PlayingColor == Properties.Color.WHITE && _playerOne != null)
            {
                PlayAI(_playerOne);
            }
            if (GameController.PlayingColor == Properties.Color.BLACK && _playerTwo != null)
            {
                PlayAI(_playerTwo);
            }
        }

        private async void PlayAI(AIController player)
        {
            if (GameController.IsGameSet)
            {
                BoardManager.Instance.IsReady = false;
            }

            await Task.Run(async () =>
            {
                player.Evaluator.NbPositionReached = 0;
                YokAIBan simulationBan = GameController.GetBanCopy();

                var searchTask = Task.Run(() => player.Evaluator.Search(_maxEvaluationDepth, ref simulationBan));

                await Task.Delay(_maxThinkTime);
                
                player.Evaluator.CanRun = false;
                
                await searchTask;

                // We should check if the game is still there at the end of computation as user may have stopped everything since it has been launched
                if (GameController.IsGameSet)
                {
                    uint bestMove = player.Evaluator.BestMove;
                    Debug.Log("Best Move found is : " + Decryptor.GetNotationFromMove(bestMove, ref GameController.GetBan()));
                
                    GameController.TryMakeMove(bestMove);
                }
            });

            if (GameController.IsGameSet)
            {
                BoardManager.Instance.AutoMovePiece(player.Evaluator.BestMove);
            }
        }
    }
}