using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// GẮN SCRIPT NÀY VÀO CANVAS_GAMEOVER HOẶC 1 OBJECT QUẢN LÝ
public class GameOverManager : MonoBehaviour
{
    [Header("Main Canvas (Kéo vào)")]
    public Canvas gameOverCanvas; // (Lỗi 2) Kéo Canvas_GameOver vào đây

    [Header("UI Components (Kéo vào)")]
    public Image gameOverImage;         
    public Text currentScoreText;       
    public Text highestScoreText;       
    public Button retryButton;          
    
    private CanvasGroup canvasGroup;
    private GameFlowManager gameFlowManager;

    void Awake()
    {
        gameFlowManager = FindObjectOfType<GameFlowManager>();

        if (gameOverCanvas == null)
        {
            // Nếu không gán, thử tự lấy
            gameOverCanvas = GetComponent<Canvas>();
            if (gameOverCanvas == null)
            {
                Debug.LogError("GameOverCanvas chưa được gán!");
                return;
            }
        }

        // Tự động tìm/thêm CanvasGroup
        canvasGroup = gameOverCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameOverCanvas.gameObject.AddComponent<CanvasGroup>();
        }
        
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetryPressed);
        }

        // Ẩn UI lúc bắt đầu
        HideGameOver(true);
    }

    // Được gọi bởi GameManager.TriggerGameOver()
    public void ShowGameOver(int finalScore)
    {
        if (gameOverCanvas == null) return;

        // 1. Tắt Gameplay Canvas
        if (GameManager.Instance != null && GameManager.Instance.gameCanvas != null)
        {
            GameManager.Instance.gameCanvas.gameObject.SetActive(false);
        }

        // 2. Chạy hiệu ứng bọt biển
        if (gameFlowManager != null)
        {
            gameFlowManager.SpawnBubbles();
        }

        // 3. Cập nhật Text
        if (currentScoreText != null)
        {
            currentScoreText.text = finalScore.ToString();
        }
        
        int highScore = PlayerPrefs.GetInt("Highscore", 0);
        if (highestScoreText != null)
        {
            highestScoreText.text = highScore.ToString();
        }

        // 4. Chuẩn bị UI Game Over (Ẩn)
        if (gameOverImage != null)
        {
            Color imgColor = gameOverImage.color;
            imgColor.a = 0f;
            gameOverImage.color = imgColor;
        }
        
        canvasGroup.alpha = 0f;
        gameOverCanvas.gameObject.SetActive(true); // Bật GameObject

        // 5. Chạy animation Fade In
        canvasGroup.DOFade(1f, 0.4f).SetEase(Ease.OutQuad);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        if (gameOverImage != null)
        {
            float targetAlpha = 230f / 255f; // Alpha 230
            gameOverImage.DOFade(targetAlpha, 0.5f).SetEase(Ease.OutQuad).SetDelay(0.1f);
        }
    }

    // Nút "Chơi Lại" (Retry)
    private void OnRetryPressed()
    {
        Debug.Log("Retry Button Pressed - Exiting to Menu...");
        
        // SỬA (Lỗi 3): Nút retry gọi ExitToMenu
        // ExitToMenu sẽ (1) Ẩn GameOver, (2) Chạy bọt biển, (3) Về Menu
        if (gameFlowManager != null)
        {
            gameFlowManager.ExitToMenu();
        }
    }

    // Ẩn UI Game Over
    public void HideGameOver(bool immediate = false)
    {
        if (gameOverCanvas == null || canvasGroup == null) return;
        
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (immediate)
        {
            canvasGroup.alpha = 0f;
            gameOverCanvas.gameObject.SetActive(false);
        }
        else
        {
            canvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
            {
                gameOverCanvas.gameObject.SetActive(false);
            });
        }
    }
}