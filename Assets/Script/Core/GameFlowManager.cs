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
            readyVideo.Play();
        }
        
        if (readyCanvas != null)
        {
            readyCanvas.gameObject.SetActive(true);
        }

        // Spawn particle bọt biển (Z = -1 để render trên video)
        if (bubblesParticlePrefab != null)
        {
            GameObject particle = Instantiate(bubblesParticlePrefab);
            particle.transform.position = new Vector3(0, -8.7f, -1); // Gần camera
        }

        if (audioManager != null) audioManager.PlayGameplayMusic();

        // HIỆU ỨNG: Menu video fade out + scale
        Sequence menuTransition = DOTween.Sequence();
        
        if (menuVideo != null)
        {
            // Fade alpha
            menuTransition.Join(DOTween.To(() => menuVideo.targetCameraAlpha, 
                                          x => menuVideo.targetCameraAlpha = x, 
                                          0f, 0.5f));
            
            // Scale (nếu cần)
            menuTransition.Join(menuVideo.transform.DOScale(1.2f, 0.5f));
        }

        menuTransition.OnComplete(() =>
        {
            if (menuCanvas != null) menuCanvas.gameObject.SetActive(false);
            if (menuVideo != null) 
            {
                menuVideo.Stop();
                menuVideo.gameObject.SetActive(false);
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

        // BẬT GAMEPLAY VIDEO (alpha = 0 ban đầu)
        if (gameplayVideo != null) 
        {
            gameplayVideo.gameObject.SetActive(true);
            gameplayVideo.targetCameraAlpha = 0f;
            gameplayVideo.Play();
        }
        
        if (gameCanvas != null) gameCanvas.gameObject.SetActive(true);

        // Spawn particle
        if (bubblesParticlePrefab != null)
        {
            GameObject particle = Instantiate(bubblesParticlePrefab);
            particle.transform.position = new Vector3(0, -8.7f, -1);
        }

        // HIỆU ỨNG ĐỒNG THỜI
        Sequence transition = DOTween.Sequence();

        // Ready fade out + move up
        if (readyVideo != null)
        {
            transition.Join(DOTween.To(() => readyVideo.targetCameraAlpha, 
                                      x => readyVideo.targetCameraAlpha = x, 
                                      0f, 0.7f));
            
            Vector3 targetPos = readyVideo.transform.position + Vector3.up * 5f;
            transition.Join(readyVideo.transform.DOMove(targetPos, 0.7f));
        }

        // Gameplay fade in + move up
        if (gameplayVideo != null)
        {
            transition.Join(DOTween.To(() => gameplayVideo.targetCameraAlpha, 
                                      x => gameplayVideo.targetCameraAlpha = x, 
                                      1f, 0.7f));
            
            Vector3 startPos = gameplayVideo.transform.position;
            Vector3 targetPos = startPos + Vector3.up * 5f;
            gameplayVideo.transform.position = startPos - Vector3.up * 5f;
            transition.Join(gameplayVideo.transform.DOMove(targetPos, 0.7f));
        }

        transition.OnComplete(() =>
        {
            if (readyCanvas != null) readyCanvas.gameObject.SetActive(false);
            if (readyVideo != null) 
            {
                readyVideo.Stop();
                readyVideo.gameObject.SetActive(false);
            }

            // Kích hoạt dropper
            DropperController dropper = FindObjectOfType<DropperController>();
            if (dropper != null)
            {
                dropper.StartDropperSequence();
            }
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