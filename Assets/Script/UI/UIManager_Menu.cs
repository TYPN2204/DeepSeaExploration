using UnityEngine;
using UnityEngine.UI;

public class UIManager_Menu : MonoBehaviour
{
    public Text highScoreText;
    public Button startButton;
    public GameFlowManager gameFlowManager;

    void Start()
    {
        UpdateHighScoreText(); // Gọi hàm chung

        if (startButton != null && gameFlowManager != null)
        {
            startButton.onClick.AddListener(gameFlowManager.OnStartGamePressed);
        }
    }

    // (Yêu cầu 2) Hàm này chạy mỗi khi GameObject được BẬT
    void OnEnable()
    {
        UpdateHighScoreText();
    }

    // Hàm chung để cập nhật high score
    void UpdateHighScoreText()
    {
        if (highScoreText != null)
        {
            highScoreText.text = PlayerPrefs.GetInt("Highscore", 0).ToString();
        }
    }
}