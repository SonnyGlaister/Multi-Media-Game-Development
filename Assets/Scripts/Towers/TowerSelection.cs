using UnityEngine;
using UnityEngine.EventSystems;

public class TowerSelection : MonoBehaviour
{
    public static TowerSelection Instance { get; private set; }

    [Tooltip("The layer the tower prefabs are on.")]
    public LayerMask towerLayers;

    public TowerController Selected { get; private set; }

    private TowerController hovered;
    private Camera cam;

    private void Awake()
    {
        Instance = this;
        cam = Camera.main;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        GameController gc = GameController.Instance;
        TowerPlacer placer = TowerPlacer.Instance;
        bool treeOpen = SkillTreeMenu.Instance != null && SkillTreeMenu.Instance.IsOpen;
        bool placing = placer != null && (placer.IsPlacing || placer.LastPlacementFrame == Time.frameCount);
        bool gameOver = gc == null || gc.IsGameOver;

        if (treeOpen || gameOver) Deselect();
        bool blocked = treeOpen || gameOver || placing;
        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        SetHovered(blocked || overUI ? null : TowerUnderMouse());
        if (blocked) return;

        if (Selected != null && Input.GetKeyDown(KeyCode.Escape))
        {
            Deselect();
            return;
        }

        if (Input.GetMouseButtonDown(0) && !overUI)
        {
            if (hovered != null) Select(hovered);
            else Deselect();
        }
    }

    public void Select(TowerController tower)
    {
        if (tower == Selected) return;
        Deselect();
        Selected = tower;
        TooltipManager.HideFor(this);
        tower.ShowRange(UITheme.Current.selectedRange);
    }

    public void Deselect()
    {
        if (Selected == null) return;
        Selected.HideRange();
        Selected = null;
    }

    private void SetHovered(TowerController tower)
    {
        if (tower == hovered) return;

        bool placing = TowerPlacer.Instance != null && TowerPlacer.Instance.IsPlacing;
        if (hovered != null && hovered != Selected && !placing) hovered.HideRange();
        TooltipManager.HideFor(this);

        hovered = tower;
        if (hovered == null || hovered == Selected) return;

        hovered.ShowRange(UITheme.Current.hoverRange);
        if (TooltipManager.Instance != null)
            TooltipManager.Instance.Show(this, hovered.DisplayName,
                string.Format(GameStrings.Current.towerHoverHint, hovered.Definition.displayName, hovered.Tier),
                hovered.Definition.color);
    }

    private TowerController TowerUnderMouse()
    {
        if (cam == null) return null;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit, 500f, towerLayers)
            ? hit.collider.GetComponentInParent<TowerController>()
            : null;
    }
}
