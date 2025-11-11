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

    private EdgeCollider2D tankCollider;
    private float tankMinX;
    private float tankMaxX;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        audioManager = FindObjectOfType<AudioManager>();
        
        GameObject tank = GameObject.Find("JellyfishTankCollider");
        if (tank != null) 
        {
            tankCollider = tank.GetComponent<EdgeCollider2D>();
            if (tankCollider != null)
            {
                tankMinX = tankCollider.bounds.min.x;
                tankMaxX = tankCollider.bounds.max.x;
            }
        }
        if (tankCollider == null)
        {
            Debug.LogError("Không tìm thấy 'JellyfishTankCollider'! Sứa có thể bị tràn.");
        }
    }

    private void Start()
    {
        highScore = PlayerPrefs.GetInt("Highscore", 0);
    }

    private void PrepareInitialJellyfish()
    {
        int maxLevel = Mathf.Min(5, jellyfishPrefabs.Length);
        currentJellyfishLevel = Random.Range(0, maxLevel);
        
        PrepareNextJelly();
        
        Debug.Log($"Initial setup: currentJellyfishLevel = {currentJellyfishLevel}, nextJellyfishLevel = {nextJellyfishLevel}");
    }

    public void StartGame()
    {
        Debug.Log("GameManager.StartGame() called!");
        currentScore = 0;
        UpdateScoreUI();
        PrepareInitialJellyfish();
        
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

    public GameObject SpawnJellyfish(Vector3 position, int jellyLevel, bool playMergeAnim = false)
    {
        if (jellyLevel < 0 || jellyLevel >= jellyfishPrefabs.Length)
        {
            Debug.LogError($"Invalid jellyLevel: {jellyLevel}");
            return null;
        }
        
        GameObject prefab = jellyfishPrefabs[jellyLevel];

        // (Code "Safe Spawn" - kẹp X trong thành bình)
        if (tankCollider != null)
        {
            CircleCollider2D prefabCollider = prefab.GetComponentInChildren<CircleCollider2D>();
            if (prefabCollider != null)
            {
                float scale = prefab.transform.localScale.x;
                float radius = prefabCollider.radius * scale;
                float childLocalX = prefabCollider.transform.localPosition.x;
                float colliderOffsetX = prefabCollider.offset.x;
                float totalOffset = (childLocalX + colliderOffsetX) * scale;

                float safeMinX = tankMinX - totalOffset + radius;
                float safeMaxX = tankMaxX - totalOffset - radius;

                position.x = Mathf.Clamp(position.x, safeMinX, safeMaxX);
            }
        }

        GameObject obj = Instantiate(prefab, position, Quaternion.identity);
        Jellyfish jelly = obj.GetComponent<Jellyfish>();
        if (jelly != null)
        {
            jelly.jellyLevel = jellyLevel;
        }

        if (playMergeAnim)
        {
            // SỬA LỖI "ĐÁ VĂNG":
            // 1. Lấy Rigidbody
            Rigidbody2D newRb = obj.GetComponentInChildren<Rigidbody2D>();
            if (newRb != null)
            {
                // 2. Tắt vật lý (isKinematic) TRƯỚC KHI scale
                newRb.isKinematic = true; 
            }

            // 3. Chạy animation
            Vector3 newScale = obj.transform.localScale;
            obj.transform.localScale = Vector3.zero;
            
            obj.transform.DOScale(newScale, 0.3f).SetEase(Ease.OutBack);
            
            // 4. Chạy anim xoay VÀ BẬT LẠI VẬT LÝ khi xong
            obj.transform.DOShakeRotation(0.5f, new Vector3(0, 0, 10), 5, 90)
                         .SetEase(Ease.OutQuad)
                         .OnComplete(() => {
                             // 5. Bật lại vật lý SAU KHI anim xong
                             if (newRb != null)
                             {
                                 newRb.isKinematic = false;
                             }
                         });
        }
        
        Debug.Log($"Spawned jellyfish level {jellyLevel} at {position}");
        return obj; 
    }

    public void OnDropJellyfish(Vector3 dropPosition)
    {
        Debug.Log($"OnDropJellyfish: Dropping level {currentJellyfishLevel} at {dropPosition}");

        SpawnJellyfish(dropPosition, currentJellyfishLevel, false);
        
        currentJellyfishLevel = nextJellyfishLevel;
        PrepareNextJelly();
        
        Debug.Log($"After drop: currentJellyfishLevel = {currentJellyfishLevel}, nextJellyfishLevel = {nextJellyfishLevel}");
    }

    public bool CanDrop()
    {
        Jellyfish[] allJellies = FindObjectsOfType<Jellyfish>();
        
        foreach (Jellyfish jelly in allJellies)
        {
            if (jelly == null) continue; 
            
            if (jelly.IsMerging)
            {
                return false; 
            }
                
            if (jelly.IsMoving())
            {
                return false; 
            }
        }
        
        return true; 
    }

    public void ExitToMenu()
    {
        currentScore = 0; 
        
        PlayerPrefs.SetInt("Highscore", highScore);
        if (gameCanvas != null) 
        {
            gameCanvas.gameObject.SetActive(false);
        }
        
        Jellyfish[] allJellies = FindObjectsOfType<Jellyfish>();
        foreach (Jellyfish jelly in allJellies)
        {
            if (jelly != null)
            {
                jelly.transform.DOKill();
                Destroy(jelly.gameObject);
            }
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
            gameOverManager.ShowGameOver(currentScore);
        }
        else
        {
            Debug.LogError("GameOverManager not found!");
        }
        
        currentScore = 0;
    }
}