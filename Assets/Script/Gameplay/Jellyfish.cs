using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Jellyfish : MonoBehaviour
{
    [Header("Data")]
    public int jellyLevel; 

    private bool hasMerged = false;
    private Rigidbody2D rb;

    void Awake()
    {
        // SỬA LỖI (Ảnh): Dùng GetComponentInChildren để tìm Rigidbody2D ở con
        rb = GetComponentInChildren<Rigidbody2D>();
        
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        else
        {
            // Báo lỗi nếu không tìm thấy Rigidbody2D
            Debug.LogError($"Jellyfish {gameObject.name} không tìm thấy Rigidbody2D ở con!");
        }
    }

    public IEnumerator SettleCheck(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (GameManager.MergingCoroutines > 0)
        {
            GameManager.MergingCoroutines--;
        }
        Debug.Log($"SettleCheck complete. Merging count: {GameManager.MergingCoroutines}");
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasMerged) return;
        Jellyfish other = collision.gameObject.GetComponent<Jellyfish>();

        if (other != null && other.jellyLevel == this.jellyLevel && !other.hasMerged)
        {
            if (this.GetInstanceID() < other.GetInstanceID())
            {
                StartCoroutine(MergeAnimation(other));
            }
        }
    }

    IEnumerator MergeAnimation(Jellyfish other)
    {
        this.hasMerged = true;
        other.hasMerged = true;

        GameManager.MergingCoroutines++;
        Debug.Log($"Merge started. Merging count: {GameManager.MergingCoroutines}");

        if (rb != null) rb.isKinematic = true;
        
        // SỬA LỖI: Phải tìm Rigidbody của "other"
        Rigidbody2D otherRb = other.GetComponentInChildren<Rigidbody2D>();
        if (otherRb != null) otherRb.isKinematic = true;
        
        // SỬA LỖI: Tìm Collider ở con
        Collider2D col = GetComponentInChildren<Collider2D>();
        if (col != null) col.enabled = false;
        
        Collider2D otherCol = other.GetComponentInChildren<Collider2D>();
        if (otherCol != null) otherCol.enabled = false;

        Transform higherJelly = (transform.position.y > other.transform.position.y) ? this.transform : other.transform;
        Transform lowerJelly = (higherJelly == this.transform) ? other.transform : this.transform;
        Vector3 mergePosition = lowerJelly.position;

        float animTime = 0.4f; // Giữ nguyên 0.4s

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
                GameManager.MergingCoroutines++;
                Debug.Log($"Spawning new jelly. Merging count: {GameManager.MergingCoroutines}");

                GameObject newJellyObj = GameManager.Instance.SpawnJellyfish(mergePosition, newLevel, true);
                
                if(newJellyObj != null)
                {
                    Jellyfish newJellyScript = newJellyObj.GetComponent<Jellyfish>();
                    if(newJellyScript != null)
                    {
                        newJellyScript.StartCoroutine(newJellyScript.SettleCheck(0.5f));
                    }
                }
            }
        }
        
        if (GameManager.MergingCoroutines > 0)
        {
            GameManager.MergingCoroutines--;
        }
        Debug.Log($"Merge anim complete. Merging count: {GameManager.MergingCoroutines}");

        Destroy(gameObject);
        Destroy(other.gameObject);
    }
}