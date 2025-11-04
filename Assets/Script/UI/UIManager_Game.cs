using UnityEngine;
using UnityEngine.UI;

public class UIManager_Game : MonoBehaviour
{
    public Button exitButton;
    public GameFlowManager gameFlowManager;

    private void Start()
    {
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitPressed);
        }
        else
        {
            Debug.LogWarning("exitButton chưa được gán!");
        }
    }

    public void OnExitPressed()
    {
        if (gameFlowManager != null)
        {
            gameFlowManager.ExitToMenu();
        }
        else
        {
            Debug.LogError("gameFlowManager = null!");
        }
    }
}