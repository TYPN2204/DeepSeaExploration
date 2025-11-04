using UnityEngine;
using DG.Tweening;

public class Jellyfish : MonoBehaviour
{
    public int jellyLevel;
    private bool hasMerged = false; // Tránh merge nhiều lần

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Kiểm tra va chạm với jellyfish khác
        Jellyfish other = collision.gameObject.GetComponent<Jellyfish>();
        
        if (other != null && !hasMerged && !other.hasMerged)
        {
            // Chỉ merge nếu cùng level
            if (other.jellyLevel == this.jellyLevel)
            {
                // Tính vị trí trung điểm
                Vector3 mergePosition = (transform.position + other.transform.position) / 2f;
                
                Debug.Log($"Merging 2 jellyfish level {jellyLevel} at {mergePosition}");
                
                // Đánh dấu đã merge
                hasMerged = true;
                other.hasMerged = true;
                
                // Thực hiện merge
                MergeWith(other, mergePosition);
            }
        }
    }

    public void MergeWith(Jellyfish other, Vector3 mergePosition)
    {
        // Disable physics
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;
        
        Rigidbody2D otherRb = other.GetComponent<Rigidbody2D>();
        if (otherRb != null) otherRb.simulated = false;

        // Tính điểm
        int scoreGained = 100 * (jellyLevel + 1);
        GameManager.Instance.UpdateScore(scoreGained);

        // ANIMATION MERGE (chậm hơn để dễ nhìn)
        float mergeDuration = 0.4f;

        Sequence mergeSeq = DOTween.Sequence();
        
        // Cả 2 jellyfish di chuyển về trung điểm + thu nhỏ về 0
        mergeSeq.Join(transform.DOMove(mergePosition, mergeDuration).SetEase(Ease.InOutQuad));
        mergeSeq.Join(transform.DOScale(0, mergeDuration).SetEase(Ease.InBack));
        
        mergeSeq.Join(other.transform.DOMove(mergePosition, mergeDuration).SetEase(Ease.InOutQuad));
        mergeSeq.Join(other.transform.DOScale(0, mergeDuration).SetEase(Ease.InBack));
        
        mergeSeq.OnComplete(() =>
        {
            Destroy(gameObject);
            Destroy(other.gameObject);
        });

        // Spawn jellyfish mới SAU KHI animation gần xong
        DOVirtual.DelayedCall(mergeDuration * 0.6f, () =>
        {
            SpawnMergedJellyfish(mergePosition, jellyLevel + 1);
        });
    }

    private void SpawnMergedJellyfish(Vector3 position, int newLevel)
    {
        if (newLevel >= GameManager.Instance.jellyfishPrefabs.Length)
        {
            Debug.Log("Max level reached!");
            return;
        }

        // Spawn jellyfish mới
        GameObject newJellyObj = Instantiate(GameManager.Instance.jellyfishPrefabs[newLevel], position, Quaternion.identity);
        Jellyfish newJelly = newJellyObj.GetComponent<Jellyfish>();
        
        if (newJelly != null)
        {
            newJelly.jellyLevel = newLevel;
        }

        // Lấy scale gốc từ prefab
        Vector3 targetScale = GameManager.Instance.jellyfishPrefabs[newLevel].transform.localScale;

        // ANIMATION: Scale từ 0 → overshoot → target (bong bóng núng nính)
        newJellyObj.transform.localScale = Vector3.zero;
        newJellyObj.transform.DOScale(targetScale * 1.15f, 0.25f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            // Co lại về scale bình thường
            newJellyObj.transform.DOScale(targetScale, 0.15f).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                // Đung đưa nhẹ
                newJellyObj.transform.DOPunchRotation(new Vector3(0, 0, 8), 0.4f, 8, 0.5f);
            });
        });

        // HIỆU ỨNG PHÁT SÁNG (tăng brightness sprite)
        SpriteRenderer sr = newJellyObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Lưu màu gốc
            Color originalColor = sr.color;
            
            // Phát sáng: Tăng brightness (color * 2)
            sr.color = originalColor * 2f;
            
            // Về màu bình thường
            sr.DOColor(originalColor, 0.5f).SetEase(Ease.OutQuad);
        }

        Debug.Log($"Spawned merged jellyfish level {newLevel}");
    }
}