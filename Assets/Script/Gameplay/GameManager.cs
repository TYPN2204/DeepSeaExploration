using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Jellyfish Prefabs theo cấp")]
    public GameObject[] jellyfishPrefabs;

    [Header("UI")]
    public Text scoreText;
    public Image nextJellyImage;           
    public Canvas gameCanvas;

    [Header("Gameplay")]
    public int currentScore = 0;
    public int highScore = 0;
    public int currentJellyfishLevel = 0;   
    public int nextJellyfishLevel = 0;
    
    [Header("Core")] 
    public AudioManager audioManager;

    // THÊM (Lỗi 5): Biến đếm số sứa đang merge/spawn
    public static int MergingCoroutines = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        audioManager = FindObjectOfType<AudioManager>();
    }

    private void Start()
    {
        highScore = PlayerPrefs.GetInt("Highscore", 0);
        // SỬA (Lỗi 3): Không gọi StartGame() ở đây, GameFlowManager sẽ gọi
    }

    private void PrepareInitialJellyfish()
    {
        int maxLevel = Mathf.Min(5, jellyfishPrefabs.Length);
        currentJellyfishLevel = Random.Range(0, maxLevel);
        
        PrepareNextJelly();
        
        Debug.Log($"Initial setup: currentJellyfishLevel = {currentJellyfishLevel}, nextJellyfishLevel = {nextJellyfishLevel}");
    }

    // THÊM (Lỗi 3): Hàm reset game, được gọi bởi GameFlowManager
    public void StartGame()
    {
        Debug.Log("GameManager.StartGame() called!");
        currentScore = 0;
        MergingCoroutines = 0; // Reset bộ đếm
        UpdateScoreUI();
        PrepareInitialJellyfish();
        
        // Đảm bảo game canvas đang bật
        if(gameCanvas != null) gameCanvas.gameObject.SetActive(true);
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
        int maxLevel = Mathf.Min(5, jellyfishPrefabs.Length);
        nextJellyfishLevel = Random.Range(0, maxLevel);

        Debug.Log($"PrepareNextJelly: nextJellyfishLevel = {nextJellyfishLevel}");
    }

    // SỬA (Lỗi 1, 5): Trả về GameObject để chạy SettleCheck
    public GameObject SpawnJellyfish(Vector3 position, int jellyLevel, bool playMergeAnim = false)
    {
        if (jellyLevel < 0 || jellyLevel >= jellyfishPrefabs.Length)
        {
            Debug.LogError($"Invalid jellyLevel: {jellyLevel}");
            return null;
        }

        GameObject obj = Instantiate(jellyfishPrefabs[jellyLevel], position, Quaternion.identity);
        Jellyfish jelly = obj.GetComponent<Jellyfish>();
        if (jelly != null)
        {
            jelly.jellyLevel = jellyLevel;
        }

        // Animation spawn khi merge
        if (playMergeAnim)
        {
            Vector3 newScale = obj.transform.localScale;
            obj.transform.localScale = Vector3.zero;
            
            // Animation scale
            obj.transform.DOScale(newScale, 0.3f).SetEase(Ease.OutBack);
            
            // Animation xoay
            obj.transform.DOShakeRotation(0.5f, new Vector3(0, 0, 10), 5, 90)
                         .SetEase(Ease.OutQuad);
        }
        
        Debug.Log($"Spawned jellyfish level {jellyLevel} at {position}");
        return obj; // Trả về sứa vừa tạo
    }

    public void OnDropJellyfish(Vector3 dropPosition)
    {
        Debug.Log($"OnDropJellyfish: Dropping level {currentJellyfishLevel} at {dropPosition}");

        // THÊM (Lỗi 5): Báo có sứa MỚI vừa RƠI
        MergingCoroutines++;
        Debug.Log($"Jelly dropped. Merging count: {MergingCoroutines}");

        // Spawn sứa (không anim merge) VÀ lấy về
        GameObject newJellyObj = SpawnJellyfish(dropPosition, currentJellyfishLevel, false);

        // Yêu cầu sứa vừa rơi chạy SettleCheck
        if(newJellyObj != null)
        {
            Jellyfish newJellyScript = newJellyObj.GetComponent<Jellyfish>();
            if(newJellyScript != null)
            {
                // Sứa mới sẽ tự giảm MergingCoroutines sau 0.5s
                newJellyScript.StartCoroutine(newJellyScript.SettleCheck(0.5f));
            }
        }

        currentJellyfishLevel = nextJellyfishLevel;
        PrepareNextJelly();
        
        Debug.Log($"After drop: currentJellyfishLevel = {currentJellyfishLevel}, nextJellyfishLevel = {nextJellyfishLevel}");
    }

    public void ExitToMenu()
    {
        // SỬA (Lỗi 3): Reset điểm VÀ bộ đếm
        currentScore = 0; 
        MergingCoroutines = 0;
        
        PlayerPrefs.SetInt("Highscore", highScore);
        if (gameCanvas != null) gameCanvas.gameObject.SetActive(false);
        
        Jellyfish[] allJellies = FindObjectsOfType<Jellyfish>();
        foreach (Jellyfish jelly in allJellies)
        {
            // Hủy tween và xóa
            jelly.transform.DOKill();
            Destroy(jelly.gameObject);
        }
    }

    public void TriggerGameOver()
    {
        Debug.Log("TriggerGameOver called!");
        
        if (audioManager != null) 
        {
            audioManager.PlayMenuMusic();
        }

        GameOverManager gameOverManager = FindObjectOfType<GameOverManager>();
        if (gameOverManager != null)
        {
            // Gửi điểm HIỆN TẠI
            gameOverManager.ShowGameOver(currentScore);
        }
        else
        {
            Debug.LogError("GameOverManager not found!");
        }
        
        // SỬA (Lỗi 3): Reset điểm VÀ bộ đếm (SAU KHI gửi điểm)
        currentScore = 0;
        MergingCoroutines = 0;
    }
}