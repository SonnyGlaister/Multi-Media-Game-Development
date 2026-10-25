using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TowerPlacer : MonoBehaviour
{
    public TowerController tower;
    public TowerController[] towerPrefabs;
    public GameObject baseObject;
    public CinemachinePath path;
    public Collider ground;
    // public GameController gameController;

    public double minPathDist = 1;
    public double minTowerDist = 1;
    public double minBaseDist = 1;
    private Button Tower1Select;
    private Button Tower2Select;
    private Button[] SelectButtons;
    public GameObject TowerSelectBar;
    private int selectedTowerIndex;

    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        SelectButtons = TowerSelectBar.GetComponentsInChildren<Button>(true);
        Tower1Select = SelectButtons[0];
        Tower2Select = SelectButtons[1];

        Tower1Select.onClick.AddListener(() => SelectTower(1));
        Tower2Select.onClick.AddListener(() => SelectTower(2));

        SelectTower(1);
    }

    // Update is called once per frame
    void Update()
    {
        
        if (GameController.Instance.IsGameOver || !GameController.Instance.HasGameStarted || SkillTreeMenu.Instance.isOpen) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverButton())
            {
                Debug.Log("Tried to place tower on button");
                return;
            }

            PlaceTower();
        }
    }

    public void PlaceTower()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        ground.Raycast(ray, out hit, 1000f);
        Vector3 point = hit.point;

        if (IsTooCloseToPath(point)) {
            GameController.Instance.ShowMessage("Can't build on the path");
            return;
        }
        if (IsTooCloseToTower(point)) {
            GameController.Instance.ShowMessage("Can't build on another tower");
            return;
        }
        if (IsTooCloseToBase(point)) {
            GameController.Instance.ShowMessage("Can't build on the base");
            return;
        }

        TowerController selectedTower = towerPrefabs[selectedTowerIndex];

        if (!GameController.Instance.Purchase(selectedTower.cost))
        {
            GameController.Instance.ShowMessage($"Not enough gold (need {tower.cost})");
            return;
        }

        Instantiate(selectedTower, point, Quaternion.identity);
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

    private void SelectTower(int towerNumber)
    {
        SelectButtons[selectedTowerIndex].image.color = Color.white;
        selectedTowerIndex = towerNumber -1;
        SelectButtons[selectedTowerIndex].image.color = Color.gray;
        
    }

    // Gotta be a simpler way to do this, Look into IsPointerOverGameObject
    private bool IsPointerOverButton()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            Button button = result.gameObject.GetComponentInParent<Button>();

            if (button != null && button.interactable)
            {
                return true;
            }
        }
        return false;
    }
}
