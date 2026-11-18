using System.Collections.Generic;
using UnityEngine;

public class FloatingTextPool : MonoBehaviour
{
    public static FloatingTextPool Instance { get; private set; }
    public FloatingText prefab;

    private readonly Stack<FloatingText> pool = new();

    private void Awake() => Instance = this;

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void SpawnScreen(string text, Color color, Vector2 screenPosition, float scale = 1f) =>
        Get().Play(text, color, screenPosition, null, scale, Release);

    public void SpawnWorld(string text, Color color, Vector3 worldPosition, float scale = 1f) =>
        Get().Play(text, color, Vector2.zero, worldPosition, scale, Release);

    private FloatingText Get()
    {
        if (pool.Count > 0) return pool.Pop();
        FloatingText ft = Instantiate(prefab, transform);
        ft.label.raycastTarget = false;
        return ft;
    }

    private void Release(FloatingText ft) => pool.Push(ft);
}
