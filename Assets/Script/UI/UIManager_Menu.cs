using UnityEngine;
using UnityEngine.UI;

public class UIManager_Menu : MonoBehaviour
{
    public Text highScoreText;
    public Button startButton;
    public GameFlowManager gameFlowManager;

    private void Start()
    {
        int highscore = PlayerPrefs.GetInt("Highscore", 0);
        highScoreText.text = "High Score: " + highscore;
        startButton.onClick.AddListener(OnStartPressed);
    }

    public void OnStartPressed()
    {
        gameFlowManager.OnStartGamePressed();
    }
}