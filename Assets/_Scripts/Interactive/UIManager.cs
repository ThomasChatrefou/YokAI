using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using YokAI;
using PColor = YokAI.Properties.Color;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Text;

    private void Start()
    {
        BoardManager.Instance.OnMate += EndGame;
    }

    public void EndGame(uint color)
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        Text.color = color == PColor.WHITE ? BoardManager.Instance.WhiteColor : BoardManager.Instance.BlackColor;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
