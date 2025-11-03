using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// GẮNG SCRIPT NÀY VÀO CHÍNH DROPPER IMAGE OBJECT
public class DropperController : MonoBehaviour
{
    [Header("UI References")]
    public Image currentJellyfishImage;          // Sứa hiện tại trong dropper (sắp thả)
    public Image nextJellyfishImage;             // Sứa tiếp theo (preview ở góc)

    [Header("Movement Settings")]
    public float minX = -300f;                   
    public float maxX = 300f;                    
    public float dropperY = 444.7f;              

    [Header("Drop Settings")]
    public Transform spawnPoint;                 
    public float dropCooldown = 0.5f;            
    public KeyCode dropKey = KeyCode.Space;      

    private bool canMove = false;
    private bool canDrop = true;
    private Canvas parentCanvas;
    private RectTransform rectTransform;
    private Image dropperImage;

    // CACHE: Lưu sprite và scale của current jelly để tránh lẫn lộn
    private Sprite cachedCurrentSprite;
    private Vector3 cachedCurrentScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        dropperImage = GetComponent<Image>();
        
        if (dropperImage != null)
        {
            dropperImage.raycastTarget = true;
        }
    }

    private void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        
        if (rectTransform != null)
        {
            Vector2 pos = rectTransform.anchoredPosition;
            pos.y = dropperY;
            pos.x = 0;
            rectTransform.anchoredPosition = pos;
        }

        Debug.Log("DropperController ready");
    }

    private void Update()
    {
        if (!canMove || rectTransform == null || parentCanvas == null) return;

        // Di chuyển theo chuột
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

        // Thả jellyfish
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(dropKey)) && canDrop)
        {
            DropJellyfish();
        }
    }

    public void StartDropperSequence()
    {
        Debug.Log("StartDropperSequence called!");
        
        // Load current jellyfish TRƯỚC (để có sprite và scale)
        LoadCurrentJellyfish();
        
        // Animation dropper xuất hiện từ ngoài camera (Y cao hơn)
        if (rectTransform != null)
        {
            Vector2 startPos = rectTransform.anchoredPosition;
            startPos.y = dropperY + 500f; // Bắt đầu từ ngoài camera
            rectTransform.anchoredPosition = startPos;

            // Tween xuống vị trí dropperY
            rectTransform.DOAnchorPosY(dropperY, 0.5f).SetEase(Ease.OutBack);
        }

        // Cho phép di chuyển sau animation
        DOVirtual.DelayedCall(0.5f, () => 
        { 
            canMove = true;
            Debug.Log("Can move now!");
        });
    }

    public void LoadCurrentJellyfish()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance is null!");
            return;
        }

        int currentLevel = GameManager.Instance.currentJellyfishLevel;

        Debug.Log($"Loading CURRENT jellyfish level: {currentLevel}");

        // Lấy thông tin từ prefab
        if (currentLevel >= 0 && currentLevel < GameManager.Instance.jellyfishPrefabs.Length)
        {
            GameObject prefab = GameManager.Instance.jellyfishPrefabs[currentLevel];
            if (prefab != null)
            {
                SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    // CACHE sprite và scale
                    cachedCurrentSprite = sr.sprite;
                    cachedCurrentScale = prefab.transform.localScale;

                    Debug.Log($"Cached current jelly: {cachedCurrentSprite.name}, scale: {cachedCurrentScale}");

                    // Hiển thị sprite
                    if (currentJellyfishImage != null)
                    {
                        currentJellyfishImage.sprite = cachedCurrentSprite;
                        
                        // ANIMATION: Scale từ 0 lên scale của prefab
                        currentJellyfishImage.transform.localScale = Vector3.zero;
                        currentJellyfishImage.transform.DOScale(cachedCurrentScale, 0.4f).SetEase(Ease.OutBack);
                    }
                }
            }
        }

        // Cập nhật next jelly preview
        UpdateNextJellyfishPreview();
    }

    private void UpdateNextJellyfishPreview()
    {
        if (GameManager.Instance == null || nextJellyfishImage == null) return;

        int previewLevel = GameManager.Instance.nextJellyfishLevel;
        
        Debug.Log($"Updating NEXT jellyfish preview level: {previewLevel}");

        if (previewLevel >= 0 && previewLevel < GameManager.Instance.jellyfishPrefabs.Length)
        {
            GameObject prefab = GameManager.Instance.jellyfishPrefabs[previewLevel];
            if (prefab != null)
            {
                SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    nextJellyfishImage.sprite = sr.sprite;
                    
                    // Next jelly nhỏ hơn 20%
                    Vector3 prefabScale = prefab.transform.localScale;
                    nextJellyfishImage.transform.localScale = prefabScale * 0.8f;
                    
                    Debug.Log($"Updated next jelly preview: {sr.sprite.name}");
                }
            }
        }
    }

    public void DropJellyfish()
    {
        if (!canDrop) return;

        Debug.Log($"Dropping jellyfish: {cachedCurrentSprite.name} at X: {rectTransform.anchoredPosition.x}");
        canDrop = false;

        // Animation rơi
        if (currentJellyfishImage != null)
        {
            currentJellyfishImage.transform.DOScale(0, 0.2f).SetEase(Ease.InBack);
        }

        // Spawn jellyfish vật lý
        Vector3 worldSpawnPos = CalculateWorldSpawnPosition();
        GameManager.Instance.OnDropJellyfish(worldSpawnPos);

        // Load jellyfish mới sau cooldown
        DOVirtual.DelayedCall(dropCooldown, () =>
        {
            LoadCurrentJellyfish();
            canDrop = true;
        });
    }

    private Vector3 CalculateWorldSpawnPosition()
    {
        if (spawnPoint != null)
        {
            Vector3 pos = spawnPoint.position;
            
            // Tính X dựa trên vị trí dropper UI
            // Chuyển đổi từ UI space (-300 to 300) sang World space (-3 to 3)
            float uiX = rectTransform.anchoredPosition.x;
            float worldX = (uiX / 300f) * 3f; // Scale theo tỷ lệ bình nước
            
            pos.x = worldX;
            pos.z = 0; // Đảm bảo Z = 0 (gần camera)
            
            Debug.Log($"Spawn: UI X={uiX} → World pos={pos}");
            return pos;
        }

        // Fallback: dùng center màn hình
        Vector3 centerScreen = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(centerScreen);
        worldPos.z = 0;
        
        Debug.LogWarning("No spawnPoint set, using screen center");
        return worldPos;
    }
}