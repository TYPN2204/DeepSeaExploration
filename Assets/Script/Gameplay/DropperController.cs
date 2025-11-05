using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DropperController : MonoBehaviour
{
    [Header("UI References")]
    public Image currentJellyfishImage;          
    public Image nextJellyfishImage;             
    public Text scoreText;

    [Header("Movement Settings")]
    public float minX = -300f; 
    public float maxX = 300f;  
    public float dropperY = 444.7f; 

    [Header("Drop Settings")]
    public Transform spawnPoint; 
    
    // SỬA LỖI 4/5: Đổi tên biến
    private bool isDropperActive = false; // Tắt di chuyển/thả ban đầu
    private Canvas parentCanvas;
    private RectTransform rectTransform;
    private Image dropperImage;

    private Sprite cachedCurrentSprite;
    private Vector3 cachedCurrentScale;
    
    private Camera mainCamera;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        dropperImage = GetComponent<Image>();
        mainCamera = Camera.main; 
        
        if (dropperImage != null)
        {
            dropperImage.raycastTarget = true;
        }
    }

    private void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        
        // SỬA LỖI 2: Gọi ResetDropper để ẩn mọi thứ
        ResetDropper(); 
    }

    private void Update()
    {
        // SỬA LỖI 4/5: Logic Update MỚI
        // 1. Chỉ di chuyển dropper khi nó active
        if (!isDropperActive || rectTransform == null || parentCanvas == null) return;

        // (Code di chuyển dropper theo chuột giữ nguyên)
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition,
            parentCanvas.worldCamera, 
            out localPoint
        );
        float clampedX = Mathf.Clamp(localPoint.x, minX, maxX);
        Vector2 newPos = rectTransform.anchoredPosition;
        newPos.x = clampedX;
        newPos.y = dropperY;
        rectTransform.anchoredPosition = newPos;

        // 2. Kiểm tra điều kiện thả
        // CHỈ thả khi: Active, Không có merge, và Người chơi nhấn nút
        if (isDropperActive && 
            GameManager.MergingCoroutines == 0 && 
            (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            DropJellyfish();
        }
    }

    public void StartDropperSequence()
    {
        Debug.Log("StartDropperSequence called!");
        
        // SỬA LỖI 2: HIỆN UI
        if (nextJellyfishImage != null) nextJellyfishImage.gameObject.SetActive(true);
        if (scoreText != null) scoreText.gameObject.SetActive(true);
        // (currentJellyfishImage sẽ hiện trong LoadCurrentJellyfish)

        LoadCurrentJellyfish();
        
        if (rectTransform != null)
        {
            rectTransform.DOAnchorPosY(dropperY, 0.6f).SetEase(Ease.OutQuad);
        }

        DOVirtual.DelayedCall(0.6f, () => 
        { 
            isDropperActive = true; // Sẵn sàng
            Debug.Log("Dropper is Active!");
        });
    }

    public void LoadCurrentJellyfish()
    {
        if (GameManager.Instance == null) return;

        int currentLevel = GameManager.Instance.currentJellyfishLevel;
        Debug.Log($"Loading CURRENT jellyfish level: {currentLevel}");

        if (currentLevel >= 0 && currentLevel < GameManager.Instance.jellyfishPrefabs.Length)
        {
            GameObject prefab = GameManager.Instance.jellyfishPrefabs[currentLevel];
            if (prefab != null)
            {
                // SỬA LỖI 1 (Phòng hờ): Dùng GetComponentInChildren
                SpriteRenderer sr = prefab.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    cachedCurrentSprite = sr.sprite;
                    cachedCurrentScale = prefab.transform.localScale;

                    if (currentJellyfishImage != null)
                    {
                        currentJellyfishImage.enabled = true; 
                        currentJellyfishImage.sprite = cachedCurrentSprite;
                        currentJellyfishImage.preserveAspect = true;
                        currentJellyfishImage.SetNativeSize(); 
                        
                        currentJellyfishImage.transform.localScale = Vector3.zero;
                        currentJellyfishImage.transform.DOScale(cachedCurrentScale, 0.4f).SetEase(Ease.OutBack);
                    }
                }
            }
        }
        UpdateNextJellyfishPreview();
    }

    private void UpdateNextJellyfishPreview()
    {
        if (GameManager.Instance == null || nextJellyfishImage == null) return;
        int previewLevel = GameManager.Instance.nextJellyfishLevel;
        if (previewLevel >= 0 && previewLevel < GameManager.Instance.jellyfishPrefabs.Length)
        {
            GameObject prefab = GameManager.Instance.jellyfishPrefabs[previewLevel];
            if (prefab != null)
            {
                // SỬA LỖI 1 (Phòng hờ): Dùng GetComponentInChildren
                SpriteRenderer sr = prefab.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    nextJellyfishImage.sprite = sr.sprite;
                    Vector3 prefabScale = prefab.transform.localScale;
                    nextJellyfishImage.preserveAspect = true;
                    nextJellyfishImage.SetNativeSize(); 
                    
                    nextJellyfishImage.transform.localScale = Vector3.zero;
                    nextJellyfishImage.transform.DOScale(prefabScale, 0.3f).SetEase(Ease.OutBack);
                }
            }
        }
    }

    // SỬA LỖI 4/5: Logic thả MỚI
    public void DropJellyfish()
    {
        Debug.Log($"Dropping jellyfish: {cachedCurrentSprite.name} at X: {rectTransform.anchoredPosition.x}");
        
        // 1. Ẩn sứa trên dropper ngay lập tức (Lỗi 4)
        if (currentJellyfishImage != null)
        {
            currentJellyfishImage.enabled = false; 
            currentJellyfishImage.transform.DOKill(); 
        }

        // 2. Tính vị trí thả (Lỗi 4)
        Vector3 worldSpawnPos = CalculateWorldSpawnPosition();
        
        // 3. Gọi GameManager (sẽ tăng MergingCoroutines lên 1)
        GameManager.Instance.OnDropJellyfish(worldSpawnPos);

        // 4. Load sứa MỚI lên dropper ngay lập tức
        // (Bạn sẽ thấy sứa mới, nhưng không thể thả vì MergingCoroutines > 0)
        LoadCurrentJellyfish();
    }

    private Vector3 CalculateWorldSpawnPosition()
    {
        if (spawnPoint == null || mainCamera == null)
        {
            Debug.LogError("SpawnPoint hoặc MainCamera chưa được gán!");
            return Vector3.zero;
        }

        Vector3 spawnPointScreenPos = mainCamera.WorldToScreenPoint(spawnPoint.position);
        float mouseX = Input.mousePosition.x;
        Vector3 dropScreenPosition = new Vector3(mouseX, spawnPointScreenPos.y, spawnPointScreenPos.z);
        Vector3 worldSpawnPos = mainCamera.ScreenToWorldPoint(dropScreenPosition);
        worldSpawnPos.z = 0; 
        
        return worldSpawnPos;
    }

    // SỬA LỖI 2/3: Hàm Reset (gọi bởi GameFlowManager)
    public void ResetDropper()
    {
        Debug.Log("Resetting Dropper...");
        isDropperActive = false; // Tắt di chuyển/thả
        
        if(rectTransform != null) rectTransform.DOKill();
        if (currentJellyfishImage != null)
        {
            currentJellyfishImage.transform.DOKill();
            currentJellyfishImage.enabled = false;
        }

        // ẨN UI
        if (nextJellyfishImage != null) nextJellyfishImage.gameObject.SetActive(false);
        if (scoreText != null) scoreText.gameObject.SetActive(false);

        // ĐẶT DROPPER NGOÀI CAMERA
        if (rectTransform != null)
        {
            Vector2 pos = rectTransform.anchoredPosition;
            pos.y = dropperY + 700f; // Vị trí ẩn
            pos.x = 0;
            rectTransform.anchoredPosition = pos;
        }
    }
}