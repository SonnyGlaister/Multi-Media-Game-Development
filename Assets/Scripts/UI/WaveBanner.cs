using System.Collections;
using TMPro;
using UnityEngine;

public class WaveBanner : MonoBehaviour
{
    public RectTransform root;
    public CanvasGroup group;
    public TextMeshProUGUI label;
    public float holdTime = 0.9f;

    private Coroutine co;

    private void Awake()
    {
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    public void Play(string text, RectTransform shrinkTarget)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(Run(text, shrinkTarget));
    }

    private IEnumerator Run(string text, RectTransform target)
    {
        label.text = text;
        Vector3 centre = new Vector3(Screen.width * 0.5f, Screen.height * 0.62f, 0f);
        root.position = centre;

        yield return UITween.Run(0.25f, e =>
        {
            group.alpha = e;
            root.localScale = Vector3.one * Mathf.Lerp(1.4f, 1f, e);
        });

        yield return new WaitForSecondsRealtime(holdTime);

        Vector3 end = target != null ? target.position : centre;
        yield return UITween.Run(0.35f, e =>
        {
            root.position = Vector3.Lerp(centre, end, e);
            root.localScale = Vector3.one * Mathf.Lerp(1f, 0.25f, e);
            group.alpha = 1f - e;
        });

        if (target != null) UITween.Punch(this, target, 0.12f, 0.25f);
    }
}
