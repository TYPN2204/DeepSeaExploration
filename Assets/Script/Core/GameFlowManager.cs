using UnityEngine;
using UnityEngine.Video;
using DG.Tweening;

public class GameFlowManager : MonoBehaviour
{
    [Header("Video Players")]
    public VideoPlayer menuVideo;
    public VideoPlayer readyVideo;
    public VideoPlayer gameplayVideo;
    
    [Header("Canvases")]
    public Canvas menuCanvas;
    public Canvas readyCanvas;
    public Canvas gameCanvas;
    
    [Header("Managers")]
    public AudioManager audioManager;
    private GameOverManager gameOverManager; 
    
    [Header("Effects")]
    public GameObject bubblesParticlePrefab;
    public Camera mainCamera;

    private CanvasGroup menuCanvasGroup;
    private CanvasGroup readyCanvasGroup;

    private void Start()
    {
        gameOverManager = FindObjectOfType<GameOverManager>();

        if (menuCanvas != null)
        {
            menuCanvasGroup = menuCanvas.GetComponent<CanvasGroup>();
            if (menuCanvasGroup == null)
                menuCanvasGroup = menuCanvas.gameObject.AddComponent<CanvasGroup>();
            menuCanvasGroup.alpha = 1;
        }
        if (readyCanvas != null)
        {
            readyCanvasGroup = readyCanvas.GetComponent<CanvasGroup>();
            if (readyCanvasGroup == null)
                readyCanvasGroup = readyCanvas.gameObject.AddComponent<CanvasGroup>();
            readyCanvasGroup.alpha = 1;
        }

        // TẮT HẾT VIDEO VÀ CANVAS TRƯỚC
        if (menuVideo != null) { menuVideo.Stop(); menuVideo.gameObject.SetActive(false); }
        if (readyVideo != null) { readyVideo.Stop(); readyVideo.gameObject.SetActive(false); }
        if (gameplayVideo != null) { gameplayVideo.Stop(); gameplayVideo.gameObject.SetActive(false); }
        
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(false);
        if (readyCanvas != null) readyCanvas.gameObject.SetActive(false);
        if (gameCanvas != null) gameCanvas.gameObject.SetActive(false);

        // CHỈ BẬT MENU
        if (menuVideo != null) 
        {
            menuVideo.gameObject.SetActive(true);
            menuVideo.transform.position = new Vector3(0, 0, 100);
            menuVideo.transform.localScale = Vector3.one;
            menuVideo.targetCameraAlpha = 1f;
            menuVideo.Play();
            Debug.Log("Menu video started");
        }
        
        if (audioManager != null) audioManager.PlayMenuMusic();
        
        if (menuCanvas != null) 
        {
            menuCanvas.gameObject.SetActive(true);
            if(menuCanvasGroup != null) menuCanvasGroup.alpha = 1; 
            Debug.Log("Menu canvas activated");
        }
    }

    public void SpawnBubbles()
    {
        if (bubblesParticlePrefab != null)
        {
            GameObject particle = Instantiate(bubblesParticlePrefab);
            particle.transform.position = new Vector3(0, -8.7f, -1);
        }
        if (audioManager != null)
        {
            audioManager.PlayBubbleSound();
        }
    }

    public void OnStartGamePressed()
    {
        Debug.Log("=== TRANSITION: Menu → Ready ===");
        
        if (readyVideo != null) 
        {
            readyVideo.gameObject.SetActive(true);
            readyVideo.targetCameraAlpha = 1f;
            readyVideo.transform.position = new Vector3(0, 0, 90);
            readyVideo.Play();
        }
        
        if (readyCanvas != null)
        {
            readyCanvas.gameObject.SetActive(true);
        }

        SpawnBubbles();
        if (audioManager != null) audioManager.PlayGameplayMusic();

        // (Code fade out Menu Video/Canvas giữ nguyên)
        Sequence menuTransition = DOTween.Sequence();
        float fadeDuration = 0.3f; 
        if (menuVideo != null)
        {
            menuTransition.Join(DOTween.To(() => menuVideo.targetCameraAlpha, x => menuVideo.targetCameraAlpha = x, 0f, fadeDuration));
            menuTransition.Join(menuVideo.transform.DOScale(1.2f, fadeDuration));
        }
        if (menuCanvasGroup != null)
        {
            menuTransition.Join(menuCanvasGroup.DOFade(0f, fadeDuration));
        }
        menuTransition.OnComplete(() =>
        {
            if (menuCanvas != null) menuCanvas.gameObject.SetActive(false);
            if (menuVideo != null) 
            {
                menuVideo.Stop();
                menuVideo.gameObject.SetActive(false);
                menuVideo.transform.localScale = Vector3.one;
                menuVideo.targetCameraAlpha = 1f;
            }
        });

        if (readyVideo != null)
        {
            readyVideo.loopPointReached += OnReadyVideoEnd;
        }
    }

    void OnReadyVideoEnd(VideoPlayer vp)
    {
        Debug.Log("=== TRANSITION: Ready → Gameplay ===");
        vp.loopPointReached -= OnReadyVideoEnd;

        if (gameplayVideo != null) 
        {
            gameplayVideo.gameObject.SetActive(true);
            gameplayVideo.targetCameraAlpha = 0f; 
            gameplayVideo.transform.position = new Vector3(0, -10, 80);
            gameplayVideo.Play();
        }
        
        if (gameCanvas != null) gameCanvas.gameObject.SetActive(true);

        // SỬA LỖI 2: Ẩn UI của game trước khi delay
        DropperController dropper = FindObjectOfType<DropperController>();
        if (dropper != null)
        {
            dropper.ResetDropper(); // Ẩn UI ngay lập tức
        }
        
        SpawnBubbles();

        // (Code cross-fade 2 video giữ nguyên)
        Sequence transition = DOTween.Sequence();
        float transitionDuration = 0.7f; 
        if (readyVideo != null)
        {
            transition.Join(DOTween.To(() => readyVideo.targetCameraAlpha, x => readyVideo.targetCameraAlpha = x, 0f, transitionDuration));
            transition.Join(readyVideo.transform.DOMoveY(10f, transitionDuration));
        }
        if (gameplayVideo != null)
        {
            transition.Join(gameplayVideo.transform.DOMoveY(0f, transitionDuration));
            transition.Join(DOTween.To(() => gameplayVideo.targetCameraAlpha, x => gameplayVideo.targetCameraAlpha = x, 1f, transitionDuration));
        }

        transition.OnComplete(() =>
        {
            if (readyCanvas != null) readyCanvas.gameObject.SetActive(false);
            if (readyVideo != null) 
            {
                readyVideo.Stop();
                readyVideo.gameObject.SetActive(false);
                readyVideo.transform.position = new Vector3(0, 0, 90);
                readyVideo.targetCameraAlpha = 1f;
            }

            // SỬA LỖI 2/3: Delay 3 giây, SAU ĐÓ mới reset game VÀ BẬT dropper
            DOVirtual.DelayedCall(3f, () =>
            {
                if(GameManager.Instance != null)
                {
                    GameManager.Instance.StartGame();
                }
                
                // (dropper đã được tìm thấy ở trên)
                if (dropper != null)
                {
                    dropper.StartDropperSequence();
                }
            });
        });
    }

    public void ExitToMenu()
    {
        Debug.Log("=== EXIT TO MENU ===");
        
        if (gameOverManager != null)
        {
            gameOverManager.HideGameOver(true); // Ẩn ngay lập tức
        }
        
        if(GameManager.Instance != null)
        {
            GameManager.Instance.ExitToMenu();
        }
        
        DropperController dropper = FindObjectOfType<DropperController>();
        if (dropper != null)
        {
            dropper.ResetDropper();
        }

        SpawnBubbles(); 

        if (gameCanvas != null) gameCanvas.gameObject.SetActive(false);
        if (gameplayVideo != null)
        {
            gameplayVideo.Stop();
            gameplayVideo.gameObject.SetActive(false);
        }

        // Bật menu
        if (menuVideo != null)
        {
            menuVideo.gameObject.SetActive(true);
            menuVideo.transform.position = new Vector3(0, 0, 100);
            menuVideo.transform.localScale = Vector3.one;
            menuVideo.targetCameraAlpha = 1f;
            menuVideo.Play();
        }
        
        if (menuCanvas != null) 
        {
            menuCanvas.gameObject.SetActive(true);
            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.alpha = 1;
                menuCanvasGroup.interactable = true;
                menuCanvasGroup.blocksRaycasts = true;
            }
        }
        if (audioManager != null) audioManager.PlayMenuMusic();
    }

    // SỬA LỖI (Ảnh): Thêm kiểm tra "null"
    private void OnDestroy()
    {
        if (readyVideo != null)
        {
            readyVideo.loopPointReached -= OnReadyVideoEnd;
        }
    }
}