using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class TowerPlacer : MonoBehaviour
{
    public enum Mode { None, Dragging, Clicking }

    public static TowerPlacer Instance { get; private set; }

    [Header("Towers (build drawer + skill tree branches use this order)")]
    public TowerDefinition[] towers;

    [Header("Scene")]
    [Tooltip("HQ. Found by the 'Base' tag if empty.")]
    public GameObject baseObject;
    [Tooltip("Collider of the buildable ground (the Plane in Level1, the Terrain in Level2).")]
    public Collider ground;

    [Header("Rules")]
    public float minPathDist = 0.55f;
    public float minTowerDist = 0.75f;
    public float minBaseDist = 0.75f;

    [Header("Look")]
    [Tooltip("Optional see-through materials for the ghost. Empty = just tint it.")]
    public Material ghostValidMaterial;
    public Material ghostInvalidMaterial;
    [Tooltip("Flat circle sprite, 1 unit wide, shown around HQ and towers while placing.")]
    public SpriteRenderer noBuildDiscPrefab;

    public Mode CurrentMode { get; private set; }
    public bool IsPlacing => CurrentMode != Mode.None;
    public TowerDefinition Pending { get; private set; }
    public int LastPlacementFrame { get; private set; } = -1;

    private Camera cam;
    private GameObject ghost;
    private TowerController ghostTower;
    private MeshRenderer[] ghostRenderers;
    private bool? ghostValid;
    private readonly Dictionary<TowerDefinition, int> placedCount = new();
    private readonly List<SpriteRenderer> noBuildDiscs = new();

    private void Awake()
    {
        Instance = this;
        cam = Camera.main;
        if (baseObject == null) baseObject = GameObject.FindWithTag("Base");
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // Cost
    public int PlacedCount(TowerDefinition tower) => placedCount.TryGetValue(tower, out int n) ? n : 0;
    public int CostOf(TowerDefinition tower) => tower.CostFor(PlacedCount(tower));
    public int NextCostOf(TowerDefinition tower) => tower.CostFor(PlacedCount(tower) + 1);

    public void OnTowerSold(TowerDefinition tower)
    {
        if (tower != null && PlacedCount(tower) > 0) placedCount[tower]--;
    }

    public bool CanPlace()
    {
        GameController gc = GameController.Instance;
        bool treeOpen = SkillTreeMenu.Instance != null && SkillTreeMenu.Instance.IsOpen;
        return gc != null && gc.HasGameStarted && !gc.IsGameOver && !treeOpen;
    }

    private void Update()
    {
        if (!IsPlacing) return;
        if (!CanPlace() || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            Cancel();
            return;
        }
        if (CurrentMode != Mode.Clicking) return;

        UpdateGhost(Input.mousePosition);

        if (Input.GetMouseButtonDown(0) && !PointerOverUI())
        {
            bool placed = TryPlace(Input.mousePosition);
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (placed && !shift) Cancel();
        }
    }

    // TowerCard functions
    public void BeginPlacement(TowerDefinition tower, Mode mode)
    {
        if (tower == null || tower.prefab == null || mode == Mode.None || !CanPlace()) return;
        Cancel();
        if (TowerSelection.Instance != null) TowerSelection.Instance.Deselect();

        Pending = tower;
        CurrentMode = mode;
        CreateGhost(tower);
        ShowPlacementHelpers();
    }

    public void Drag(Vector2 screen)
    {
        if (CurrentMode == Mode.Dragging) UpdateGhost(screen);
    }

    public void EndDrag(Vector2 screen)
    {
        if (CurrentMode != Mode.Dragging) return;
        if (!PointerOverUI()) TryPlace(screen);
        Cancel();
    }

    public void Cancel()
    {
        if (!IsPlacing) return;

        if (ghost != null) Destroy(ghost);
        ghost = null;
        ghostTower = null;
        ghostRenderers = null;
        ghostValid = null;
        Pending = null;
        CurrentMode = Mode.None;

        TooltipManager.HideFor(this);        ClearNoBuildDiscs();
        foreach (TowerController t in TowerController.Towers) t.HideRange();
    }

    // Ghost tower
    private void CreateGhost(TowerDefinition tower)
    {
        ghostTower = Instantiate(tower.prefab);
        ghost = ghostTower.gameObject;
        ghost.name = "Ghost_" + tower.displayName;
        ghost.tag = "Untagged";
        ghostTower.enabled = false; // doesn't attack
        ghostTower.Init(tower);
        foreach (Collider c in ghost.GetComponentsInChildren<Collider>()) c.enabled = false;
        ghostRenderers = ghost.GetComponentsInChildren<MeshRenderer>();
        ghost.SetActive(false);
    }

    private void UpdateGhost(Vector2 screen)
    {
        if (ghost == null) return;

        if (PointerOverUI() || !TryGetGroundPoint(screen, out Vector3 point))
        {
            ghost.SetActive(false);
            TooltipManager.HideFor(this);
            return;
        }

        ghost.SetActive(true);
        ghost.transform.position = point;

        GameController gc = GameController.Instance;
        GameStrings s = GameStrings.Current;
        UITheme t = UITheme.Current;

        int cost = CostOf(Pending);
        string error = GetPlacementError(point);
        if (error == null && !gc.CanAfford(cost))
            error = string.Format(s.errNeedSkillPoints, cost - gc.SkillPoints);
        bool valid = error == null;

        SetGhostLook(valid);
        ghostTower.ShowRange(valid ? t.validPlacement : t.invalidPlacement);

        float range = ghostTower.Stats.range;

        if (TooltipManager.Instance != null)
            TooltipManager.Instance.Show(this,
                $"{ghostTower.DisplayName}  ·  {cost} {s.skillPointsName}",
                error ?? $"Range {ghostTower.Stats.range:0.#}",
                valid ? t.validPlacement : t.invalidPlacement);
    }

    private void SetGhostLook(bool valid)
    {
        if (ghostValid == valid) return;
        ghostValid = valid;

        Material m = valid ? ghostValidMaterial : ghostInvalidMaterial;
        Color tint = valid ? UITheme.Current.validPlacement : UITheme.Current.invalidPlacement;
        foreach (MeshRenderer r in ghostRenderers)
        {
            if (m != null)
            {
                Material[] mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = m;
                r.sharedMaterials = mats;
            }
            else
            {
                r.material.color = tint;
            }
        }
    }

    // Building rings + neighbor discs
    private void ShowPlacementHelpers()
    {
        foreach (TowerController t in TowerController.Towers) t.ShowRange(UITheme.Current.neighbourRange);

        ClearNoBuildDiscs();
        if (noBuildDiscPrefab == null) return;
        if (baseObject != null) AddNoBuildDisc(baseObject.transform.position, minBaseDist);
        foreach (TowerController t in TowerController.Towers) AddNoBuildDisc(t.transform.position, minTowerDist);
    }

    private void AddNoBuildDisc(Vector3 at, float radius)
    {
        SpriteRenderer disc = Instantiate(noBuildDiscPrefab, GroundUnder(at) + Vector3.up * 0.05f, noBuildDiscPrefab.transform.rotation);
        disc.transform.localScale = Vector3.one * radius * 2f;
        disc.color = UITheme.Current.noBuild;
        noBuildDiscs.Add(disc);
    }

    private void ClearNoBuildDiscs()
    {
        foreach (SpriteRenderer d in noBuildDiscs) if (d != null) Destroy(d.gameObject);
        noBuildDiscs.Clear();
    }

    // Placing
    private bool TryPlace(Vector2 screen)
    {
        if (Pending == null || !TryGetGroundPoint(screen, out Vector3 point)) return false;
        GameController gc = GameController.Instance;
        GameStrings s = GameStrings.Current;

        string error = GetPlacementError(point);
        if (error != null)
        {
            gc.ShowMessage(error, MessageType.Warning);
            return false;
        }

        int cost = CostOf(Pending);
        if (!gc.Purchase(cost))
        {
            gc.ShowMessage(string.Format(s.errNeedSkillPoints, cost - gc.SkillPoints), MessageType.Warning);
            return false;
        }

        TowerController tower = Instantiate(Pending.prefab, point, Quaternion.identity);
        tower.Init(Pending);
        tower.PaidCost = cost;
        placedCount[Pending] = PlacedCount(Pending) + 1;
        LastPlacementFrame = Time.frameCount;

        if (CurrentMode == Mode.Clicking) ShowPlacementHelpers();
        return true;
    }

    // Rules (returns null for valid spot)
    public string GetPlacementError(Vector3 point)
    {
        GameStrings s = GameStrings.Current;
        if (IsTooCloseToPath(point)) return s.errOnPath;
        if (IsTooCloseToBase(point)) return s.errNearBase;
        if (IsTooCloseToTower(point)) return s.errNearTower;
        return null;
    }

    private bool IsTooCloseToPath(Vector3 point)
    {
        if (!NavMesh.SamplePosition(point, out NavMeshHit hit, minPathDist, NavMesh.AllAreas)) return false;
        return Vector2.Distance(new Vector2(point.x, point.z), new Vector2(hit.position.x, hit.position.z)) < minPathDist;
    }

    private bool IsTooCloseToTower(Vector3 point)
    {
        foreach (TowerController t in TowerController.Towers)
        {
            Vector3 p = t.transform.position;
            if (Vector2.Distance(new Vector2(point.x, point.z), new Vector2(p.x, p.z)) < minTowerDist) return true;
        }
        return false;
    }

    private bool IsTooCloseToBase(Vector3 point)
    {
        if (baseObject == null) return false;
        Vector3 b = baseObject.transform.position;
        return Vector2.Distance(new Vector2(point.x, point.z), new Vector2(b.x, b.z)) < minBaseDist;
    }

    // Helpers
    private bool TryGetGroundPoint(Vector2 screen, out Vector3 point)
    {
        Ray ray = cam.ScreenPointToRay(screen);
        if (ground != null && ground.Raycast(ray, out RaycastHit hit, 1000f))
        {
            point = hit.point;
            return true;
        }
        point = Vector3.zero;
        return false;
    }

    private Vector3 GroundUnder(Vector3 p)
    {
        if (ground == null) return p;
        Ray down = new Ray(new Vector3(p.x, ground.bounds.max.y + 10f, p.z), Vector3.down);
        return ground.Raycast(down, out RaycastHit hit, 1000f) ? hit.point : p;
    }

    private static bool PointerOverUI() =>
        EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
}
