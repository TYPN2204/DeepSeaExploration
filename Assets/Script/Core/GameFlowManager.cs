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
    
    [Header("Effects & Audio")]
    public GameObject bubblesParticlePrefab;
    public AudioManager audioManager;
    public Camera mainCamera;

    private CanvasGroup menuCanvasGroup;
    private CanvasGroup readyCanvasGroup;

    private void Start()
    {
        // Thêm CanvasGroup
        if (menuCanvas != null)
        {
            menuCanvasGroup = menuCanvas.GetComponent<CanvasGroup>();
            if (menuCanvasGroup == null)
            {
                menuCanvasGroup = menuCanvas.gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (readyCanvas != null)
        {
            readyCanvasGroup = readyCanvas.GetComponent<CanvasGroup>();
            if (readyCanvasGroup == null)
            {
                readyCanvasGroup = readyCanvas.gameObject.AddComponent<CanvasGroup>();
            }
        }

        // TẮT HẾT VIDEO TRƯỚC
        if (menuVideo != null) 
        {
            menuVideo.Stop();
            menuVideo.gameObject.SetActive(false);
        }
        if (readyVideo != null) 
        {
            readyVideo.Stop();
            readyVideo.gameObject.SetActive(false);
        }
        if (gameplayVideo != null) 
        {
            gameplayVideo.Stop();
            gameplayVideo.gameObject.SetActive(false);
        }

        // Setup ban đầu - CHỈ BẬT MENU
        if (menuVideo != null) 
        {
            menuVideo.gameObject.SetActive(true);
            menuVideo.targetCameraAlpha = 1f;
            menuVideo.transform.position = new Vector3(0, 0, 100);
            menuVideo.transform.localScale = Vector3.one;
            menuVideo.Play();
        }
        
        if (audioManager != null) audioManager.PlayMenuMusic();
        
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(true);
        if (readyCanvas != null) readyCanvas.gameObject.SetActive(false);
        if (gameCanvas != null) gameCanvas.gameObject.SetActive(false);
    }

    public void OnStartGamePressed()
    {
        Debug.Log("=== TRANSITION: Menu → Ready ===");
        
        // BẬT READY VIDEO NGAY (phía sau menu)
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

        // Spawn particle bọt biển tại Y = -8.7
        if (bubblesParticlePrefab != null)
        {
            GameObject particle = Instantiate(bubblesParticlePrefab);
            particle.transform.position = new Vector3(0, -8.7f, -1);
        }

        if (audioManager != null) audioManager.PlayGameplayMusic();

        // HIỆU ỨNG: Menu video fade out + scale to 1.2x
        Sequence menuTransition = DOTween.Sequence();
        
        if (menuVideo != null)
        {
            // Fade alpha từ 1 → 0
            menuTransition.Join(DOTween.To(() => menuVideo.targetCameraAlpha, 
                                          x => menuVideo.targetCameraAlpha = x, 
                                          0f, 0.5f));
            
            // Scale từ 1 → 1.2
            menuTransition.Join(menuVideo.transform.DOScale(1.2f, 0.5f));
        }

        menuTransition.OnComplete(() =>
        {
            if (menuCanvas != null) menuCanvas.gameObject.SetActive(false);
            if (menuVideo != null) 
            {
                menuVideo.Stop();
                menuVideo.gameObject.SetActive(false);
                // Reset lại cho lần sau
                menuVideo.transform.localScale = Vector3.one;
                menuVideo.targetCameraAlpha = 1f;
            }
        });

        // Đăng ký event
        if (readyVideo != null)
        {
            readyVideo.loopPointReached += OnReadyVideoEnd;
        }
    }

    void OnReadyVideoEnd(VideoPlayer vp)
    {
        Debug.Log("=== TRANSITION: Ready → Gameplay ===");
        vp.loopPointReached -= OnReadyVideoEnd;

        // BẬT GAMEPLAY VIDEO tại vị trí dưới (0, -10, 80)
        if (gameplayVideo != null) 
        {
            gameplayVideo.gameObject.SetActive(true);
            gameplayVideo.targetCameraAlpha = 1f;
            gameplayVideo.transform.position = new Vector3(0, -10, 80);
            gameplayVideo.Play();
        }
        
        if (gameCanvas != null) gameCanvas.gameObject.SetActive(true);

        // Spawn particle tại Y = -8.7
        if (bubblesParticlePrefab != null)
        {
            GameObject particle = Instantiate(bubblesParticlePrefab);
            particle.transform.position = new Vector3(0, -8.7f, -1);
        }

        // HIỆU ỨNG ĐỒNG THỜI
        Sequence transition = DOTween.Sequence();

        // 1. Ready fade out + move lên (0,0,90) → (0,10,90)
        if (readyVideo != null)
        {
            transition.Join(DOTween.To(() => readyVideo.targetCameraAlpha, 
                                      x => readyVideo.targetCameraAlpha = x, 
                                      0f, 0.7f));
            
            transition.Join(readyVideo.transform.DOMoveY(10f, 0.7f));
        }

        // 2. Gameplay move lên (0,-10,80) → (0,0,80)
        if (gameplayVideo != null)
        {
            transition.Join(gameplayVideo.transform.DOMoveY(0f, 0.7f));
        }

        transition.OnComplete(() =>
        {
            if (readyCanvas != null) readyCanvas.gameObject.SetActive(false);
            if (readyVideo != null) 
            {
                readyVideo.Stop();
                readyVideo.gameObject.SetActive(false);
                // Reset lại
                readyVideo.transform.position = new Vector3(0, 0, 90);
                readyVideo.targetCameraAlpha = 1f;
            }

            // SAU 3 GIÂY mới kích hoạt dropper
            DOVirtual.DelayedCall(3f, () =>
            {
                DropperController dropper = FindObjectOfType<DropperController>();
                if (dropper != null)
                {
                    dropper.StartDropperSequence();
                }
            });
        });
    }

    private void OnDestroy()
    {
        if (readyVideo != null)
        {
            readyVideo.loopPointReached -= OnReadyVideoEnd;
        }
    }
}