using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton

    [Header("Jellyfish Prefabs theo cấp")]
    public GameObject[] jellyfishPrefabs; // Kéo các prefab cấp 0-10 vào Inspector

    [Header("UI")]
    public Text scoreText;
    public Image nextJellyImage;
    public Canvas gameCanvas;

    [Header("Dropper & Spawn")]
    public Transform dropperTransform;
    public Transform spawnPoint; // Vị trí thả jellyfish

    [Header("Gameplay")]
    public int currentScore = 0;
    public int highScore = 0;
    public int currentJellyLevel = 0;
    public int nextJellyLevel = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        highScore = PlayerPrefs.GetInt("Highscore", 0);
        UpdateScoreUI();
        PrepareNextJelly();
    }

    public void StartGame()
    {
        currentScore = 0;
        UpdateScoreUI();
        PrepareNextJelly();
    }

    public void UpdateScore(int addScore)
    {
        currentScore += addScore;
        UpdateScoreUI();
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("Highscore", highScore);
        }
    }

    public void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore;
    }

    public void PrepareNextJelly()
    {
        // Random cấp jelly tiếp theo (hoặc logic riêng của bạn)
        nextJellyLevel = Random.Range(0, jellyfishPrefabs.Length);
        // Hiển thị sprite jellyfish kế tiếp trong UI
        if (nextJellyImage != null && jellyfishPrefabs[nextJellyLevel] != null)
        {
            Sprite sprite = jellyfishPrefabs[nextJellyLevel].GetComponent<SpriteRenderer>().sprite;
            nextJellyImage.sprite = sprite;
        }
    }

    public void SpawnJellyfish(Vector3 position, int jellyLevel)
    {
        if (jellyLevel < 0 || jellyLevel >= jellyfishPrefabs.Length) return;
        GameObject obj = Instantiate(jellyfishPrefabs[jellyLevel], position, Quaternion.identity);

        Jellyfish jelly = obj.GetComponent<Jellyfish>();
        if (jelly != null) jelly.jellyLevel = jellyLevel;
    }

    // Gọi khi dropper thả jellyfish
    public void OnDropJellyfish()
    {
        // Tạo jellyfish ở vị trí dropper/spawnPoint với cấp nextJellyLevel
        Vector3 pos = spawnPoint != null ? spawnPoint.position : dropperTransform.position;
        SpawnJellyfish(pos, nextJellyLevel);

        // Gán lại jelly tiếp theo cho dropper
        PrepareNextJelly();
    }

    // Xử lý khi merge thành công
    public void OnMergeJellyfish(int mergedLevel, Vector3 position)
    {
        // Tăng điểm, có thể dựa trên cấp jellyfish
        UpdateScore((mergedLevel + 1) * 10);

        // Spawn jellyfish mới
        SpawnJellyfish(position, mergedLevel + 1);

        // Có thể thêm hiệu ứng, animation, particle tại đây
    }

    public void ExitToMenu()
    {
        PlayerPrefs.SetInt("Highscore", highScore);
        // Ẩn Canvas game, chuyển về menu (tuỳ thuộc GameFlowManager)
        if (gameCanvas != null) gameCanvas.gameObject.SetActive(false);
    }

    public void GameOver()
    {
        PlayerPrefs.SetInt("Highscore", highScore);
        // Reload lại scene (hoặc gọi coroutine chuyển cảnh)
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
}