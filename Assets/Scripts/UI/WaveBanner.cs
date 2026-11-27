using DG.Tweening;
using TMPro;
using UnityEngine;

public class WaveBanner : MonoBehaviour
{
    public RectTransform root;
    public CanvasGroup group;
    public TextMeshProUGUI label;
    public float holdTime = 0.9f;

    private Sequence sequence;

    private void Awake()
    {
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    // Pops in at the centre, holds, then flies into the wave pill and shrinks away
    public void Play(string text, RectTransform shrinkTarget)
    {
        sequence?.Kill();

        label.text = text;
        Vector3 centre = new Vector3(Screen.width * 0.5f, Screen.height * 0.62f, 0f);
        Vector3 end = shrinkTarget != null ? shrinkTarget.position : centre;
        root.position = centre;
        root.localScale = Vector3.one * 1.4f;
        group.alpha = 0f;

        sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(group.DOFade(1f, 0.25f))
                .Join(root.DOScale(1f, 0.25f))
                .AppendInterval(holdTime)
                .Append(root.DOMove(end, 0.35f))
                .Join(root.DOScale(0.25f, 0.35f))
                .Join(group.DOFade(0f, 0.35f));

        if (shrinkTarget != null)
            sequence.AppendCallback(() => shrinkTarget.DOPunchScale(Vector3.one * 0.12f, 0.25f, 1, 0f).SetUpdate(true));
    }
}
