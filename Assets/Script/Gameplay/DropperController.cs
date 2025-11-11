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
    
    private bool isDropperActive = false; 
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
        
        ResetDropper(); 
    }

    private void Update()
    {
        if (!isDropperActive || rectTransform == null || parentCanvas == null) return;

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

        // SỬA: Logic thả MỚI
        if (isDropperActive && 
            (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            // Hỏi GameManager trước khi thả
            // (Thêm kiểm tra GameManager.Instance != null để tránh lỗi)
            if (GameManager.Instance != null && GameManager.Instance.CanDrop())
            {
                DropJellyfish();
            }
            else
            {
                Debug.Log("CANNOT DROP: Jellies are moving or merging.");
            }
        }
    }

    public void StartDropperSequence()
    {
        Debug.Log("StartDropperSequence called!");
        
        if (nextJellyfishImage != null) nextJellyfishImage.gameObject.SetActive(true);
        if (scoreText != null) scoreText.gameObject.SetActive(true);

        LoadCurrentJellyfish();
        
        if (rectTransform != null)
        {
            rectTransform.DOAnchorPosY(dropperY, 0.6f).SetEase(Ease.OutQuad);
        }

        DOVirtual.DelayedCall(0.6f, () => 
        { 
            isDropperActive = true; 
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

    public void DropJellyfish()
    {
        Debug.Log($"Dropping jellyfish: {cachedCurrentSprite.name} at X: {rectTransform.anchoredPosition.x}");
        
        if (currentJellyfishImage != null)
        {
            currentJellyfishImage.enabled = false; 
            currentJellyfishImage.transform.DOKill(); 
        }

        Vector3 worldSpawnPos = CalculateWorldSpawnPosition();
        
        GameManager.Instance.OnDropJellyfish(worldSpawnPos);

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

    public void ResetDropper()
    {
        Debug.Log("Resetting Dropper...");
        isDropperActive = false; 
        
        if(rectTransform != null) rectTransform.DOKill();
        if (currentJellyfishImage != null)
        {
            currentJellyfishImage.transform.DOKill();
            currentJellyfishImage.enabled = false;
        }

        if (nextJellyfishImage != null) nextJellyfishImage.gameObject.SetActive(false);
        if (scoreText != null) scoreText.gameObject.SetActive(false);

        if (rectTransform != null)
        {
            Vector2 pos = rectTransform.anchoredPosition;
            pos.y = dropperY + 700f; 
            pos.x = 0;
            rectTransform.anchoredPosition = pos;
        }
    }
}