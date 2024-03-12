using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using YokAI;
using PColor = YokAI.PieceProperties.Color;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private TextMeshProUGUI Text;

    public event Action<uint> EndGame;

    private void Start()
    {
        EndGame += OnEndGame;
    }

    private void OnEndGame(uint color)
    {
        gameObject.SetActive(true);
        Text.color = color == PColor.WHITE ? NewBanManager.instance.WhiteColor : NewBanManager.instance.BlackColor;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
