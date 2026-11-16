using System;
using UnityEngine;

public static class RuntimeSprites
{
    private static Sprite vignette;

    public static Sprite Vignette => vignette != null ? vignette : vignette = Make(256, d =>
        Mathf.SmoothStep(0f, 1f, (d - 0.55f) / 0.75f));

    private static Sprite Make(int size, Func<float, float> alphaAtDistance)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };
        Color32[] px = new Color32[size * size];
        float half = (size - 1) * 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - half) * (x - half) + (y - half) * (y - half)) / half;
                px[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(alphaAtDistance(d)) * 255));
            }
        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}