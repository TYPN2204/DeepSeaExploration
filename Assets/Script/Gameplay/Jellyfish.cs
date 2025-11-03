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
        // Disable physics để không bị ảnh hưởng trong quá trình merge
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;
        
        Rigidbody2D otherRb = other.GetComponent<Rigidbody2D>();
        if (otherRb != null) otherRb.simulated = false;

        // Tính điểm: 100 x (level + 1)
        // Level 0 = cấp 1 → 100 điểm
        // Level 1 = cấp 2 → 200 điểm
        int scoreGained = 100 * (jellyLevel + 1);
        GameManager.Instance.UpdateScore(scoreGained);

        // ANIMATION MERGE (nhanh để không bị tràn khoảng trống)
        float mergeDuration = 0.25f;

        Sequence mergeSeq = DOTween.Sequence();
        
        // Cả 2 jellyfish di chuyển về trung điểm + thu nhỏ về 0
        mergeSeq.Join(transform.DOMove(mergePosition, mergeDuration).SetEase(Ease.InQuad));
        mergeSeq.Join(transform.DOScale(0, mergeDuration).SetEase(Ease.InBack));
        
        mergeSeq.Join(other.transform.DOMove(mergePosition, mergeDuration).SetEase(Ease.InQuad));
        mergeSeq.Join(other.transform.DOScale(0, mergeDuration).SetEase(Ease.InBack));
        
        mergeSeq.OnComplete(() =>
        {
            // Xóa 2 jellyfish cũ
            Destroy(gameObject);
            Destroy(other.gameObject);
        });

        // Spawn jellyfish mới (level + 1) NGAY SAU KHI animation bắt đầu
        DOVirtual.DelayedCall(mergeDuration * 0.5f, () =>
        {
            SpawnMergedJellyfish(mergePosition, jellyLevel + 1);
        });
    }

    private void SpawnMergedJellyfish(Vector3 position, int newLevel)
    {
        // Kiểm tra có level tiếp theo không
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

        // ANIMATION: Scale từ 0 lên scale gốc
        newJellyObj.transform.localScale = Vector3.zero;
        newJellyObj.transform.DOScale(targetScale, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            // Hiệu ứng đung đưa sau khi phóng to xong
            newJellyObj.transform.DOPunchRotation(new Vector3(0, 0, 10), 0.4f, 10, 1);
        });

        Debug.Log($"Spawned merged jellyfish level {newLevel} with scale {targetScale}");
    }
}