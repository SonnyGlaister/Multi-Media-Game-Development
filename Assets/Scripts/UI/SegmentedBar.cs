using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Shows value/max as segments or continuous fill
public class SegmentedBar : MonoBehaviour
{
    [Tooltip("A disabled child Image used as the template for each segment.")]
    public Image segmentTemplate;
    [Tooltip("Object with a HorizontalLayoutGroup that segments are created under.")]
    public Transform segmentParent;
    [Tooltip("Optional Filled image used when max > maxSegments.")]
    public Image continuousFill;
    public int maxSegments = 12;
    public Color emptyColor = new Color(1f, 1f, 1f, 0.08f);

    private readonly List<Image> segments = new();
    private int builtFor = -1;

    public void Set(int value, int max, Color fill)
    {
        max = Mathf.Max(1, max);
        bool useSegments = segmentTemplate != null && max <= maxSegments;

        if (continuousFill != null)
        {
            continuousFill.gameObject.SetActive(!useSegments);
            continuousFill.fillAmount = Mathf.Clamp01((float)value / max);
            continuousFill.color = fill;
        }
        if (segmentParent != null) segmentParent.gameObject.SetActive(useSegments);
        if (!useSegments) return;

        if (builtFor != max) Rebuild(max);
        for (int i = 0; i < segments.Count; i++) segments[i].color = i < value ? fill : emptyColor;
    }

    public Image SegmentAt(int index) => index >= 0 && index < builtFor && index < segments.Count ? segments[index] : null;

    private void Rebuild(int max)
    {
        while (segments.Count < max)
        {
            Image s = Instantiate(segmentTemplate, segmentParent);
            s.gameObject.SetActive(true);
            segments.Add(s);
        }
        for (int i = 0; i < segments.Count; i++) segments[i].gameObject.SetActive(i < max);
        builtFor = max;
    }
}