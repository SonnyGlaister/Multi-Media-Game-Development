using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TowerPlacer : MonoBehaviour
{
    public TowerController tower;
    public GameObject baseObject;
    public CinemachinePath path;
    public Collider ground;
    // public GameController gameController;

    public double minPathDist = 1;
    public double minTowerDist = 1;
    public double minBaseDist = 1;

    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.IsGameOver || !GameController.Instance.HasGameStarted) return;

        if (Input.GetMouseButtonDown(0))
        {
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
        if (!GameController.Instance.Purchase(tower.cost))
        {
            GameController.Instance.ShowMessage($"Not enough gold (need {tower.cost})");
            return;
        }

        Instantiate(tower, point, Quaternion.identity);
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
