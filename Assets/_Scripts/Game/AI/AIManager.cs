using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using _Scripts.Utilities;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using YokAI.Main;
using YokAI.Notation;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace YokAI.AI
{
    public class AIManager : Singleton<AIManager>
    {
        [Header("AI")]
        
        [Label("AI vs IA")]
        [SerializeField] private bool AIvsAI;
        public AIController playerOne;
        public AIController playerTwo;
        
        [SerializeField] private EvaluationParamSO whiteEvaluationParamSo;
        [EnableIf(nameof(AIvsAI))]
        [SerializeField] private EvaluationParamSO blackEvaluationParamSo;

        [Header("AI General Params")] 
        
        [SerializeField][Range(1,10)] private int _maxEvaluationDepth = 5;
        [SerializeField] private int maxThinkTime; // int represent miliseconds
        [SerializeField] private bool isAIStart;

        private AIController playingPlayer;
        
        [Button("NextTurn")]
        public void NextTurn()
        { 
            playerOne.Evaluator.CanRun = true;
            PlayAI(playerOne);
        }
        
        private void Awake()
        {
            playerOne = new AIController(whiteEvaluationParamSo);
            if (AIvsAI) 
                playerTwo = new AIController(blackEvaluationParamSo);
        }

        private void Start()
        {
            if (Random.value > .5f || isAIStart)
            {
                PlayAI(playerOne);
                playingPlayer = playerOne;
            }
            else if(AIvsAI)
            {
                PlayAI(playerTwo);
                playingPlayer = playerTwo;
            }
        }

        private async void PlayAI(AIController player)
        {
            await Task.Run(async () =>
            {
                player.Evaluator.NbPositionReached = 0;
                var searchTask = Task.Run(() => player.Evaluator.Search(_maxEvaluationDepth, ref GameController.Ban));

                await Task.Delay(maxThinkTime);
                
                player.Evaluator.CanRun = false;
                
                await searchTask;
                
                GameController.IsGenerationDirty = true;
                uint bestMove = player.Evaluator.BestMove;
                GameController.TryMakeMove(bestMove);
                Debug.Log("Best Move found is : " + Decryptor.GetNotationFromMove(bestMove, GameController.Ban));
            });

            StartCoroutine(BoardManager.Instance.AIMovePiece(player.Evaluator.BestMove, .2f));
        }
    }
}