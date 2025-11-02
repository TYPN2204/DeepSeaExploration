using UnityEngine;
using UnityEngine.Video;
using DG.Tweening;

public class GameFlowManager : MonoBehaviour
{
    public VideoPlayer menuVideo;
    public VideoPlayer readyVideo;
    public VideoPlayer gameplayVideo;
    public Canvas menuCanvas;
    public Canvas gameCanvas;
    public ParticleSystem bubblesParticle;
    public AudioManager audioManager;
    public Camera mainCamera;

    private void Start()
    {
        menuVideo.Play();
        audioManager.PlayMenuMusic();
        menuCanvas.gameObject.SetActive(true);
        gameCanvas.gameObject.SetActive(false);
    }

    public void OnStartGamePressed()
    {
        // Hiệu ứng bọt biển
        bubblesParticle.Play();

        // Fade và scale video menu
        menuCanvas.GetComponent<CanvasGroup>().DOFade(0, 0.5f);
        menuVideo.transform.DOScale(1.2f, 0.5f).OnComplete(() =>
        {
            menuCanvas.gameObject.SetActive(false);
            menuVideo.Stop();
            readyVideo.Play();
            audioManager.PlayGameplayMusic();
        });

        // Khi video ready kết thúc
        readyVideo.loopPointReached += OnReadyVideoEnd;
    }

    void OnReadyVideoEnd(VideoPlayer vp)
    {
        bubblesParticle.Play();
        readyVideo.GetComponent<CanvasGroup>().DOFade(0, 0.5f);

        // Di chuyển camera xuống gameplay
        mainCamera.transform.DOMoveY(-30f, 0.7f).OnComplete(() =>
        {
            gameplayVideo.Play();
            gameCanvas.gameObject.SetActive(true);
            // Gọi DropperController xuất hiện
            FindObjectOfType<DropperController>().StartDropperSequence();
        });
    }
}