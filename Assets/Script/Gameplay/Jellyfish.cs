using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Jellyfish : MonoBehaviour
{
    [Header("Data")]
    public int jellyLevel; 

    public bool HasMerged { get; private set; } = false;
    public bool IsMerging { get; private set; } = false;
    
    private Rigidbody2D rb;
    private float moveThreshold = 0.1f; 

    void Awake()
    {
        rb = GetComponentInChildren<Rigidbody2D>();
        
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        else
        {
            Debug.LogError($"Jellyfish {gameObject.name} không tìm thấy Rigidbody2D ở con!");
        }
    }

    public bool IsMoving()
    {
        if (rb == null) return false;
        return rb.velocity.magnitude > moveThreshold; 
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (this.HasMerged) return; 
        
        Jellyfish other = collision.gameObject.GetComponent<Jellyfish>();
        if (other == null && collision.transform.parent != null)
        {
            other = collision.transform.parent.GetComponent<Jellyfish>();
        }

        if (other != null && other.jellyLevel == this.jellyLevel && !other.HasMerged) 
        {
            // SỬA LỖI "VĂNG": Tắt vật lý và vận tốc NGAY LẬP TỨC
            // để ngăn chúng nảy ra trước khi merge
            if (rb != null) 
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0;
                rb.isKinematic = true;
            }
            Rigidbody2D otherRb = other.GetComponentInChildren<Rigidbody2D>();
            if (otherRb != null) 
            {
                otherRb.velocity = Vector2.zero;
                otherRb.angularVelocity = 0;
                otherRb.isKinematic = true;
            }
            
            // Chỉ 1 sứa (ID nhỏ hơn) chạy logic
            if (this.GetInstanceID() < other.GetInstanceID())
            {
                StartCoroutine(MergeAnimation(other));
            }
        }
    }

    IEnumerator MergeAnimation(Jellyfish other)
    {
        this.HasMerged = true;
        other.HasMerged = true; 
        
        this.IsMerging = true;
        other.IsMerging = true;

        // (Code tắt vật lý đã chạy ở trên, nhưng để đây cho chắc)
        if (rb != null) rb.isKinematic = true;
        Rigidbody2D otherRb = other.GetComponentInChildren<Rigidbody2D>();
        if (otherRb != null) otherRb.isKinematic = true;
        
        Collider2D col = GetComponentInChildren<Collider2D>();
        if (col != null) col.enabled = false;
        
        Collider2D otherCol = other.GetComponentInChildren<Collider2D>();
        if (otherCol != null) otherCol.enabled = false;

        Transform higherJelly = (transform.position.y > other.transform.position.y) ? this.transform : other.transform;
        Transform lowerJelly = (higherJelly == this.transform) ? other.transform : this.transform;
        Vector3 mergePosition = lowerJelly.position;

        float animTime = 0.4f; 

        higherJelly.DOMove(mergePosition, animTime).SetEase(Ease.InQuad);
        transform.DOScale(Vector3.zero, animTime).SetEase(Ease.InBack);
        other.transform.DOScale(Vector3.zero, animTime).SetEase(Ease.InBack);

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.audioManager != null)
            {
                GameManager.Instance.audioManager.PlayMergeSound(this.jellyLevel);
            }
            
            int scoreGained = (this.jellyLevel + 1) * 10;
            GameManager.Instance.UpdateScore(scoreGained);
        }

        yield return new WaitForSeconds(animTime);

        if (GameManager.Instance != null)
        {
            int newLevel = this.jellyLevel + 1;
            if (newLevel < GameManager.Instance.jellyfishPrefabs.Length)
            {
                GameManager.Instance.SpawnJellyfish(mergePosition, newLevel, true);
            }
        }
        
        Destroy(gameObject);
        Destroy(other.gameObject);
    }
}