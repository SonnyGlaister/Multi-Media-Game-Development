using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One shared tooltip that follows the mouse
public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    public RectTransform panel;
    public CanvasGroup group;
    public TextMeshProUGUI header;
    public TextMeshProUGUI body;
    public Image accent;
    public Vector2 offset = new Vector2(20f, -20f);

    private Canvas canvas;
    private RectTransform canvasRect;
    private object owner;
    private bool visible;

    private void Awake()
    {
        Instance = this;
        canvas = GetComponentInParent<Canvas>().rootCanvas;
        canvasRect = (RectTransform)canvas.transform;
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Show(object who, string title, string text, Color? accentColor = null)
    {
        owner = who;
        bool hasTitle = !string.IsNullOrEmpty(title);
        header.gameObject.SetActive(hasTitle);
        header.text = title;
        body.text = text;
        if (accent != null) accent.color = accentColor ?? UITheme.Current.info;
        visible = true;
        group.alpha = 1f;
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
        Follow(Input.mousePosition);
    }

    public void Hide(object who)
    {
        if (who != owner) return;
        owner = null;
        visible = false;
        group.alpha = 0f;
    }

    public static void HideFor(object who)
    {
        if (Instance != null) Instance.Hide(who);
    }

    private void LateUpdate()
    {
        if (visible) Follow(Input.mousePosition);
    }

    private void Follow(Vector2 screen)
    {
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, uiCam, out Vector2 local);
        Vector2 size = panel.rect.size;
        Vector2 half = canvasRect.rect.size * 0.5f;

        Vector2 pos = local + offset;
        if (pos.x + size.x > half.x) pos.x = local.x - offset.x - size.x; // flip left
        if (pos.y - size.y < -half.y) pos.y = local.y - offset.y + size.y; // flip up
        panel.anchoredPosition = pos;
    }
}
