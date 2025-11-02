using UnityEngine;
using UnityEngine.Video;
using DG.Tweening;

public class GameFlowManager : MonoBehaviour
{
    public VideoPlayer menuVideo;
    public VideoPlayer readyVideo;
    public VideoPlayer gameplayVideo;
    public Canvas menuCanvas;
    public Canvas readyCanvas;
    public Canvas gameCanvas;
    public GameObject bubblesParticlePrefab; // Đổi thành Prefab
    public AudioManager audioManager;
    public Camera mainCamera;

    private CanvasGroup menuCanvasGroup;
    private CanvasGroup readyCanvasGroup;

    private void Start()
    {
        // Thêm CanvasGroup nếu chưa có
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
            menuVideo.Play();
        }
        if (audioManager != null) audioManager.PlayMenuMusic();
        
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(true);
        if (readyCanvas != null) readyCanvas.gameObject.SetActive(false);
        if (gameCanvas != null) gameCanvas.gameObject.SetActive(false);
    }

    public void OnStartGamePressed()
    {
        // ===== TẤT CẢ CHẠY SONG SONG =====
        
        // 1. Spawn particle NGAY (không chờ gì cả)
        if (bubblesParticlePrefab != null)
        {
            Instantiate(bubblesParticlePrefab);
        }

        // 2. Fade menu canvas (song song)
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.DOFade(0, 0.5f);
        }

        // 3. Chuyển video NGAY (không chờ scale xong)
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(false);
        if (menuVideo != null) menuVideo.Stop();
        
        // Hiển thị ready canvas và phát video NGAY
        if (readyCanvas != null)
        {
            Debug.Log("Null ready canvas");
            readyCanvas.gameObject.SetActive(true);
        }
        if (readyVideo != null) 
        {
            Debug.Log("Null ready video");
            readyVideo.Play();
        }
        if (audioManager != null) audioManager.PlayGameplayMusic();

        // Đăng ký sự kiện khi ready video kết thúc
        if (readyVideo != null)
        {
            readyVideo.loopPointReached += OnReadyVideoEnd;
        }
    }

    void OnReadyVideoEnd(VideoPlayer vp)
    {
        vp.loopPointReached -= OnReadyVideoEnd;

        // ===== TẤT CẢ CHẠY SONG SONG =====
        
        // 1. Spawn particle NGAY
        if (bubblesParticlePrefab != null)
        {
            Instantiate(bubblesParticlePrefab);
        }

        // 2. Fade ready canvas (song song)
        if (readyCanvasGroup != null)
        {
            readyCanvasGroup.DOFade(0, 0.5f);
        }

        // 3. Chuyển sang gameplay NGAY (không chờ camera)
        if (readyCanvas != null)
        {
            Debug.Log("Null ready canvas");
            readyCanvas.gameObject.SetActive(false);
        }

        if (gameplayVideo != null)
        {
            Debug.Log("Null gameplay video");
            gameplayVideo.Play();
        }
        if (gameCanvas != null) gameCanvas.gameObject.SetActive(true);
        
        // Kích hoạt gameplay
        DropperController dropper = FindObjectOfType<DropperController>();
        if (dropper != null)
        {
            dropper.StartDropperSequence();
        }
    }

    private void OnDestroy()
    {
        if (readyVideo != null)
        {
            readyVideo.loopPointReached -= OnReadyVideoEnd;
        }
    }
}