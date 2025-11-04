using UnityEngine;
using UnityEngine.UI;

public static class ImageSizeHelper
{
    // Set Image size để match với sprite pixel size
    public static void SetNativeSize(Image image, Sprite sprite, Vector3 prefabScale)
    {
        if (image == null || sprite == null) return;

        // Lấy kích thước pixel gốc của sprite
        float pixelWidth = sprite.rect.width;
        float pixelHeight = sprite.rect.height;
        float pixelsPerUnit = sprite.pixelsPerUnit;

        // Tính size theo world units
        float worldWidth = pixelWidth / pixelsPerUnit;
        float worldHeight = pixelHeight / pixelsPerUnit;

        // Áp dụng scale từ prefab
        worldWidth *= prefabScale.x;
        worldHeight *= prefabScale.y;

        // Set RectTransform size
        RectTransform rt = image.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(worldWidth * 100f, worldHeight * 100f); // *100 vì UI scale
        }
    }
}