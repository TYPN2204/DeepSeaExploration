using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

// GẮNG SCRIPT NÀY VÀO CHÍNH DROPPER IMAGE OBJECT
public class DropperController : MonoBehaviour
{
    [Header("UI References")]
    public Image currentJellyImage;              // Sứa hiện tại trong dropper
    public Image nextJellyImage;                 // Sứa tiếp theo (preview)

    [Header("Movement Settings")]
    public float minX = -300f;                   // Giới hạn trái
    public float maxX = 300f;                    // Giới hạn phải
    public float dropperY = 444.7f;              // Vị trí Y của dropper (cố định)

    [Header("Drop Settings")]
    public Transform spawnPoint;                 // Vị trí spawn jellyfish vật lý
    public float dropCooldown = 0.5f;            // Thời gian chờ giữa các lần thả
    public KeyCode dropKey = KeyCode.Space;      // Phím thả (Space)

    private bool canMove = false;
    private bool canDrop = true;
    private Canvas parentCanvas;
    private RectTransform rectTransform;
    private Image dropperImage;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        dropperImage = GetComponent<Image>();
        
        // Đảm bảo Image có raycastTarget = true
        if (dropperImage != null)
        {
            dropperImage.raycastTarget = true;
        }
    }

    private void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        
        // Đảm bảo dropper ở vị trí ban đầu
        if (rectTransform != null)
        {
            Vector2 pos = rectTransform.anchoredPosition;
            pos.y = dropperY;
            pos.x = 0; // Giữa màn hình
            rectTransform.anchoredPosition = pos;
        }

        Debug.Log("DropperController ready - Can move: " + canMove);
    }

    private void Update()
    {
        if (!canMove || rectTransform == null || parentCanvas == null) return;

        // DI CHUYỂN THEO CHUỘT (PC/Touchpad)
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition,
            parentCanvas.worldCamera,
            out localPoint
        );

        // Clamp vị trí X trong giới hạn
        float clampedX = Mathf.Clamp(localPoint.x, minX, maxX);

        // Cập nhật vị trí dropper (chỉ di chuyển ngang)
        Vector2 newPos = rectTransform.anchoredPosition;
        newPos.x = clampedX;
        newPos.y = dropperY; // Luôn giữ Y cố định
        rectTransform.anchoredPosition = newPos;

        // THẢLEFT MOUSE hoặc SPACE
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(dropKey)) && canDrop)
        {
            DropJellyfish();
        }
    }

    public void StartDropperSequence()
    {
        Debug.Log("StartDropperSequence called!");
        
        // Animation dropper xuất hiện
        if (rectTransform != null)
        {
            // Di chuyển từ trên cao xuống
            Vector2 startPos = rectTransform.anchoredPosition;
            startPos.y = dropperY + 300f; // Bắt đầu từ trên cao
            rectTransform.anchoredPosition = startPos;

            // Tween xuống vị trí dropperY
            rectTransform.DOAnchorPosY(dropperY, 0.5f).SetEase(Ease.OutBack);
        }

        // Animation jellyfish hiện tại phóng to
        if (currentJellyImage != null)
        {
            currentJellyImage.transform.localScale = Vector3.zero;
            currentJellyImage.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                // Sau khi animation xong, set đúng scale từ prefab
                SetCorrectScale();
            });
        }

        // Cho phép di chuyển sau khi animation xong
        DOVirtual.DelayedCall(0.5f, () => 
        { 
            canMove = true;
            Debug.Log("Can move now!");
        });

        // Load jellyfish hiện tại từ GameManager
        LoadCurrentJelly();
    }

    public void LoadCurrentJelly()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance is null!");
            return;
        }

        // Hiển thị sprite của jellyfish hiện tại
        if (currentJellyImage != null && GameManager.Instance.nextJellyLevel < GameManager.Instance.jellyfishPrefabs.Length)
        {
            GameObject prefab = GameManager.Instance.jellyfishPrefabs[GameManager.Instance.nextJellyLevel];
            if (prefab != null)
            {
                SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    currentJellyImage.sprite = sr.sprite;
                    Debug.Log("Loaded jelly sprite: " + sr.sprite.name);
                }
            }
        }

        // Cập nhật NextJelly preview
        UpdateNextJellyPreview();
    }

    private void SetCorrectScale()
    {
        if (GameManager.Instance == null || currentJellyImage == null) return;

        if (GameManager.Instance.nextJellyLevel < GameManager.Instance.jellyfishPrefabs.Length)
        {
            GameObject prefab = GameManager.Instance.jellyfishPrefabs[GameManager.Instance.nextJellyLevel];
            if (prefab != null)
            {
                // LẤY SCALE TỪ PREFAB (scale gốc của prefab)
                Vector3 prefabScale = prefab.transform.localScale;
                currentJellyImage.transform.localScale = prefabScale;
                
                Debug.Log($"Set CurrentJelly scale to: {prefabScale} from prefab {prefab.name}");
            }
        }
    }

    private void UpdateNextJellyPreview()
    {
        if (GameManager.Instance == null || nextJellyImage == null) return;

        // Lấy previewJellyLevel từ GameManager (đã được prepare)
        int previewLevel = GameManager.Instance.previewJellyLevel;
        
        if (previewLevel >= 0 && previewLevel < GameManager.Instance.jellyfishPrefabs.Length)
        {
            GameObject prefab = GameManager.Instance.jellyfishPrefabs[previewLevel];
            if (prefab != null)
            {
                SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    nextJellyImage.sprite = sr.sprite;
                    
                    // Set scale cho preview
                    Vector3 prefabScale = prefab.transform.localScale;
                    nextJellyImage.transform.localScale = prefabScale * 0.8f; // Nhỏ hơn 20%
                    
                    Debug.Log($"Updated NextJelly preview: {sr.sprite.name}, scale: {prefabScale}");
                }
            }
        }
    }

    public void DropJellyfish()
    {
        if (!canDrop) return;

        Debug.Log("Dropping jellyfish at X: " + rectTransform.anchoredPosition.x);
        canDrop = false;

        // Animation jellyfish rơi (scale xuống 0)
        if (currentJellyImage != null)
        {
            currentJellyImage.transform.DOScale(0, 0.2f).SetEase(Ease.InBack);
        }

        // Spawn jellyfish vật lý vào game world
        Vector3 worldSpawnPos = CalculateWorldSpawnPosition();
        GameManager.Instance.OnDropJellyfish(worldSpawnPos);

        // Load jellyfish mới sau cooldown
        DOVirtual.DelayedCall(dropCooldown, () =>
        {
            LoadCurrentJelly();
            
            // Animation jellyfish mới phóng to
            if (currentJellyImage != null)
            {
                currentJellyImage.transform.localScale = Vector3.zero;
                currentJellyImage.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    SetCorrectScale();
                });
            }

            canDrop = true;
        });
    }

    private Vector3 CalculateWorldSpawnPosition()
    {
        // Nếu có spawnPoint riêng, dùng nó
        if (spawnPoint != null)
        {
            // Điều chỉnh X của spawnPoint theo vị trí dropper
            Vector3 pos = spawnPoint.position;
            
            // Tính tỷ lệ X dựa trên vị trí UI (-300 đến 300 UI → -3 đến 3 World)
            float xRatio = rectTransform.anchoredPosition.x / 300f;
            pos.x = xRatio * 3f; // Điều chỉnh 3f theo kích thước bình nước
            
            Debug.Log($"Spawn position: UI X={rectTransform.anchoredPosition.x}, World X={pos.x}");
            return pos;
        }

        // Fallback: chuyển đổi từ UI sang world
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(rectTransform.position);
        worldPos.z = 0;
        return worldPos;
    }
}