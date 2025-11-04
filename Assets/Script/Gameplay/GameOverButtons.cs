using UnityEngine;
using UnityEngine.UI;

public class GameOverButtons : MonoBehaviour
{
    public Button retryButton;
    public Button menuButton;
    public GameFlowManager gameFlowManager;
    public GameObject bubblesParticlePrefab;

    private void Start()
    {
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryPressed);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuPressed);
        }
    }

    private void OnRetryPressed()
    {
        Debug.Log("Retry pressed");
        
        // Spawn particle
        if (bubblesParticlePrefab != null)
        {
            GameObject particle = Instantiate(bubblesParticlePrefab);
            particle.transform.position = new Vector3(0, -8.7f, -1);
        }

        // Reload scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    private void OnMenuPressed()
    {
        Debug.Log("Menu pressed");
        
        // Spawn particle
        if (bubblesParticlePrefab != null)
        {
            GameObject particle = Instantiate(bubblesParticlePrefab);
            particle.transform.position = new Vector3(0, -8.7f, -1);
        }

        if (gameFlowManager != null)
        {
            // Ẩn game over UI
            GameOverManager gom = FindObjectOfType<GameOverManager>();
            if (gom != null && gom.gameOverOverlay != null)
            {
                gom.gameOverOverlay.gameObject.SetActive(false);
            }

            // Quay về menu
            gameFlowManager.ExitToMenu();
        }
        else
        {
            Debug.LogError("GameFlowManager not assigned!");
        }
    }
}