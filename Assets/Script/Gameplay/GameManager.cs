using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Jellyfish Prefabs theo cấp")]
    public GameObject[] jellyfishPrefabs;

    [Header("UI")]
    public Text scoreText;
    public Image nextJellyImage;           // Preview sứa tiếp theo
    public Canvas gameCanvas;

    [Header("Gameplay")]
    public int currentScore = 0;
    public int highScore = 0;
    public int nextJellyLevel = 0;          // Cấp sứa sắp thả
    public int previewJellyLevel = 0;       // PUBLIC để DropperController truy cập

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
            scoreText.text = currentScore.ToString();
    }

    public void PrepareNextJelly()
    {
        // Random cấp jelly tiếp theo (giới hạn 0-4 cho đầu game dễ hơn)
        int maxLevel = Mathf.Min(5, jellyfishPrefabs.Length);
        previewJellyLevel = Random.Range(0, maxLevel);

        // Hiển thị sprite jellyfish preview trong UI
        if (nextJellyImage != null && previewJellyLevel < jellyfishPrefabs.Length)
        {
            GameObject prefab = jellyfishPrefabs[previewJellyLevel];
            if (prefab != null)
            {
                SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    nextJellyImage.sprite = sr.sprite;
                }
            }
        }
    }

    public void SpawnJellyfish(Vector3 position, int jellyLevel)
    {
        if (jellyLevel < 0 || jellyLevel >= jellyfishPrefabs.Length) return;

        GameObject obj = Instantiate(jellyfishPrefabs[jellyLevel], position, Quaternion.identity);
        Jellyfish jelly = obj.GetComponent<Jellyfish>();
        if (jelly != null)
        {
            jelly.jellyLevel = jellyLevel;
        }
    }

    // Gọi khi dropper thả jellyfish (với vị trí tùy chỉnh)
    public void OnDropJellyfish(Vector3 dropPosition)
    {
        // Spawn jellyfish vật lý tại vị trí dropper
        SpawnJellyfish(dropPosition, nextJellyLevel);

        // Chuyển preview thành current cho lần drop tiếp theo
        nextJellyLevel = previewJellyLevel;

        // Prepare jellyfish mới cho preview
        PrepareNextJelly();
    }

    // Xử lý khi merge thành công
    public void OnMergeJellyfish(int mergedLevel, Vector3 position)
    {
        // Tăng điểm dựa trên cấp
        UpdateScore((mergedLevel + 1) * 10);

        // Spawn jellyfish mới (cấp cao hơn)
        if (mergedLevel + 1 < jellyfishPrefabs.Length)
        {
            SpawnJellyfish(position, mergedLevel + 1);
        }
    }

    public void ExitToMenu()
    {
        PlayerPrefs.SetInt("Highscore", highScore);
        if (gameCanvas != null) gameCanvas.gameObject.SetActive(false);
    }

    public void GameOver()
    {
        PlayerPrefs.SetInt("Highscore", highScore);
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }
}