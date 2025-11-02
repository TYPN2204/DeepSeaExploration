using UnityEngine;
using DG.Tweening;

public class Jellyfish : MonoBehaviour
{
    public int jellyLevel;

    public void MergeWith(Jellyfish other, Vector3 mergePosition)
    {
        // Thu nhỏ cả hai về 0 và di chuyển tới vị trí merge
        Sequence mergeSeq = DOTween.Sequence();
        mergeSeq.Append(transform.DOMove(mergePosition, 0.3f));
        mergeSeq.Join(transform.DOScale(0, 0.3f));
        mergeSeq.AppendCallback(() => Destroy(gameObject));

        mergeSeq.Play();
        other.PlayMergeEffect(mergePosition, jellyLevel + 1);
    }

    public void PlayMergeEffect(Vector3 pos, int newLevel)
    {
        // Spawn jellyfish mới tại vị trí merge, scale từ 0 lên 1
        GameObject newJelly = Instantiate(GameManager.Instance.jellyfishPrefabs[newLevel], pos, Quaternion.identity);
        newJelly.transform.localScale = Vector3.zero;
        newJelly.transform.DOScale(1, 0.3f).OnComplete(() =>
        {
            // Hiệu ứng đung đưa
            newJelly.transform.DOLocalRotate(new Vector3(0, 0, 10), 0.2f).SetLoops(4, LoopType.Yoyo);
        });
    }
}