using UnityEngine;
using UnityEngine.UI;

public class UIManager_Menu : MonoBehaviour
{
    public Text highScoreText;
    public Button startButton;
    public GameFlowManager gameFlowManager;

    private void Start()
    {
        // Kiểm tra null trước khi sử dụng
        if (highScoreText != null)
        {
            int highscore = PlayerPrefs.GetInt("Highscore", 0);
            highScoreText.text =  highscore.ToString();
        }
        else
        {
            Debug.LogWarning("highScoreText chưa được gán trong Inspector!");
            
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartPressed);
        }
        else
        {
            Debug.LogWarning("startButton chưa được gán trong Inspector!");
        }

        if (gameFlowManager == null)
        {
            Debug.LogError("gameFlowManager chưa được gán trong Inspector!");
        }
    }

    public void OnStartPressed()
    {
        if (gameFlowManager != null)
        {
            gameFlowManager.OnStartGamePressed();
        }
        else
        {
            Debug.LogError("Không thể start game vì gameFlowManager = null!");
        }
    }
}