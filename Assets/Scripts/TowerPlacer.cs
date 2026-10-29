using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class TowerPlacer : MonoBehaviour
{
    public static TowerPlacer Instance { get; private set; }

    public TowerController[] towerPrefabs;
    public GameObject baseObject;
    public CinemachinePath path;
    public Collider ground;

    public double minPathDist = 1;
    public double minTowerDist = 1;
    public double minBaseDist = 1;
    public GameObject TowerSelectBar;

    public Color validColor = Color.green;
    public Color invalidColor = Color.red;

    private Camera cam;
    private TowerController draggedTower;
    // Ghost is a tower copy that isn't a real tower (can't shoot)
    private GameObject ghostTower;
    private Renderer[] ghostRenderers;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        cam = Camera.main;
        Button[] selectButtons = TowerSelectBar.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < selectButtons.Length; i++)
        {
            TowerDragButton dragButton = selectButtons[i].gameObject.AddComponent<TowerDragButton>();
            dragButton.Setup(towerPrefabs[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Cancel in-progress drag if the game ends or the skill tree opens
        if (ghostTower != null && !CanDrag())
        {
            CancelDrag();
        }
    }

    public bool CanDrag()
    {
        return !GameController.Instance.IsGameOver && GameController.Instance.HasGameStarted && !SkillTreeMenu.Instance.isOpen;
    }

    // Called when a tower drag starts
    public void BeginDrag(TowerController prefab)
    {
        draggedTower = prefab;

        ghostTower = Instantiate(prefab.gameObject);
        ghostTower.tag = "Untagged";
        ghostTower.GetComponent<TowerController>().enabled = false;
        foreach (Collider c in ghostTower.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }
        ghostRenderers = ghostTower.GetComponentsInChildren<Renderer>();
        ghostTower.SetActive(false);
    }

    public void Drag(Vector2 screenPosition)
    {
        if (ghostTower == null) return;

        if (!TryGetGroundPoint(screenPosition, out Vector3 point))
        {
            ghostTower.SetActive(false);
            return;
        }

        ghostTower.SetActive(true);
        ghostTower.transform.position = point;

        bool isValid = GetPlacementError(point) == null && GameController.Instance.CanAfford(draggedTower.cost);
        Color tint = isValid ? validColor : invalidColor;
        foreach (Renderer r in ghostRenderers)
        {
            r.material.color = tint;
        }

    }

    // Called when a drag ends (successful or cancelled)
    public void EndDrag(Vector2 screenPosition, bool droppedOnButton)
    {
        if (ghostTower == null) return;

        TowerController prefab = draggedTower;
        CancelDrag();

        if (droppedOnButton) return;
        if (!TryGetGroundPoint(screenPosition, out Vector3 point)) return;

        TryPlaceTower(prefab, point);
    }

    public void CancelDrag()
    {
        if (ghostTower != null) Destroy(ghostTower);
        ghostTower = null;
        ghostRenderers = null;
        draggedTower = null;
    }

    private void TryPlaceTower(TowerController prefab, Vector3 point)
    {
        string error = GetPlacementError(point);
        if (error != null)
        {
            GameController.Instance.ShowMessage(error);
            return;
        }

        if (!GameController.Instance.Purchase(prefab.cost))
        {
            GameController.Instance.ShowMessage($"Not enough gold");
            return;
        }

        Instantiate(prefab, point, Quaternion.identity);
    }

    private bool TryGetGroundPoint(Vector2 screenPosition, out Vector3 point)
    {
        Ray ray = cam.ScreenPointToRay(screenPosition);
        if (ground.Raycast(ray, out RaycastHit hit, 1000f))
        {
            point = hit.point;
            return true;
        }
        point = Vector3.zero;
        return false;
    }

    // Null if the tower spot is valid, otherwise shows message
    private string GetPlacementError(Vector3 point)
    {
        if (IsTooCloseToPath(point)) return "Can't build on the path";
        if (IsTooCloseToTower(point)) return "Can't build on another tower";
        if (IsTooCloseToBase(point)) return "Can't build on the base";
        return null;
    }

    private bool IsTooCloseToPath(Vector3 point)
    {
        float pathPos = path.FindClosestPoint(point, 0, -1, 10);
        Vector3 closest = path.EvaluatePosition(pathPos);

        Vector2 point2D = new Vector2(point.x, point.z);
        Vector2 path2D = new Vector2(closest.x, closest.z);
        return Vector2.Distance(point2D, path2D) < minPathDist;
    }

    private bool IsTooCloseToTower(Vector3 point)
    {
        foreach (GameObject t in GameObject.FindGameObjectsWithTag("Tower"))
        {
            Vector3 towerPos = t.transform.position;

            Vector2 point2D = new Vector2(point.x, point.z);
            Vector2 tower2D = new Vector2(towerPos.x, towerPos.z);

            if (Vector3.Distance(point2D, tower2D) < minTowerDist)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsTooCloseToBase(Vector3 point)
    {
        Vector3 basePos = baseObject.transform.position;

        Vector2 point2D = new Vector2(point.x, point.z);
        Vector2 base2D = new Vector2(basePos.x, basePos.z);

        return Vector2.Distance(point2D, base2D) < minBaseDist;
    }
}
