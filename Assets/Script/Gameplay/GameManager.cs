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
    public int currentJellyfishLevel = 0;   // Sứa HIỆN TẠI (đang trong dropper, sắp thả)
    public int nextJellyfishLevel = 0;      // Sứa TIẾP THEO (preview)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        highScore = PlayerPrefs.GetInt("Highscore", 0);
        UpdateScoreUI();
        
        // Khởi tạo lần đầu: random cả current và next
        PrepareInitialJellyfish();
    }

    private void PrepareInitialJellyfish()
    {
        // Random sứa đầu tiên cho dropper
        int maxLevel = Mathf.Min(5, jellyfishPrefabs.Length);
        currentJellyfishLevel = Random.Range(0, maxLevel);
        
        // Random sứa tiếp theo
        PrepareNextJelly();
        
        Debug.Log($"Initial setup: currentJellyfishLevel = {currentJellyfishLevel}, nextJellyfishLevel = {nextJellyfishLevel}");
    }

    public void StartGame()
    {
        currentScore = 0;
        UpdateScoreUI();
        PrepareInitialJellyfish();
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
        // Random sứa tiếp theo (preview)
        int maxLevel = Mathf.Min(5, jellyfishPrefabs.Length);
        nextJellyfishLevel = Random.Range(0, maxLevel);

        Debug.Log($"PrepareNextJelly: nextJellyfishLevel = {nextJellyfishLevel}");

        // Hiển thị sprite trong NextJelly UI (nếu có - thường do DropperController xử lý)
        // Không cần update ở đây nữa vì DropperController sẽ tự update
    }

    public void SpawnJellyfish(Vector3 position, int jellyLevel)
    {
        if (jellyLevel < 0 || jellyLevel >= jellyfishPrefabs.Length)
        {
            Debug.LogError($"Invalid jellyLevel: {jellyLevel}");
            return;
        }

        GameObject obj = Instantiate(jellyfishPrefabs[jellyLevel], position, Quaternion.identity);
        Jellyfish jelly = obj.GetComponent<Jellyfish>();
        if (jelly != null)
        {
            jelly.jellyLevel = jellyLevel;
        }
        
        Debug.Log($"Spawned jellyfish level {jellyLevel} at {position}");
    }

    // Gọi khi dropper thả jellyfish
    public void OnDropJellyfish(Vector3 dropPosition)
    {
        Debug.Log($"OnDropJellyfish: Dropping level {currentJellyfishLevel} at {dropPosition}");
        
        // Spawn jellyfish vật lý với currentJellyfishLevel
        SpawnJellyfish(dropPosition, currentJellyfishLevel);

        // Chuyển next → current cho lần thả tiếp theo
        currentJellyfishLevel = nextJellyfishLevel;

        // Random sứa mới cho next
        PrepareNextJelly();
        
        Debug.Log($"After drop: currentJellyfishLevel = {currentJellyfishLevel}, nextJellyfishLevel = {nextJellyfishLevel}");
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