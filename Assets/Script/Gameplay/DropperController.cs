using UnityEngine;
using DG.Tweening;

public class DropperController : MonoBehaviour
{
    public Transform dropperSprite;
    public Transform heldJellySprite;

    public void StartDropperSequence()
    {
        dropperSprite.DOMoveY(0, 0.5f); // bay từ trên xuống
        heldJellySprite.localScale = Vector3.zero;
        heldJellySprite.DOScale(Vector3.one, 0.4f); // hiệu ứng phóng to jellyfish
    }

    public void DropJellyfish()
    {
        heldJellySprite.gameObject.SetActive(false);
        // Spawn prefab Jellyfish tại vị trí dropper
        GameManager.Instance.SpawnJellyfish(dropperSprite.position, GameManager.Instance.currentJellyLevel);
    }
}