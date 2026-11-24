using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathPreview : MonoBehaviour
{
    public static PathPreview Instance { get; private set; }

    [Tooltip("LineRenderer prefab for the whole route (faint).")]
    public LineRenderer routeLinePrefab;
    [Tooltip("LineRenderer prefab for the covered part (bright).")]
    public LineRenderer coverageLinePrefab;
    public float heightOffset = 0.15f;
    [Tooltip("Distance between route points. Smaller = smoother coverage edges.")]
    public float sampleStep = 0.25f;
    [Tooltip("Only matters if the route material has a dashed texture. 0 = still.")]
    public float scrollSpeed = 0f;

    private readonly List<List<Vector3>> routes = new();
    private readonly List<LineRenderer> routeLines = new();
    private readonly List<LineRenderer> coverageLines = new();
    private int coverageUsed;

    private void Awake() => Instance = this;

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        GameObject hq = GameObject.FindWithTag("Base");
        if (hq == null || !NavMesh.SamplePosition(hq.transform.position, out NavMeshHit end, 10f, NavMesh.AllAreas)) return;

        foreach (EnemySpawner spawner in FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None))
        {
            if (!NavMesh.SamplePosition(spawner.transform.position, out NavMeshHit start, 10f, NavMesh.AllAreas)) continue;
            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path)) continue;

            List<Vector3> points = Resample(path.corners);
            routes.Add(points);

            LineRenderer line = Instantiate(routeLinePrefab, transform);
            line.positionCount = points.Count;
            line.SetPositions(points.ToArray());
            routeLines.Add(line);
        }
    }

    private void Update()
    {
        GameController gc = GameController.Instance;
        bool placing = TowerPlacer.Instance != null && TowerPlacer.Instance.IsPlacing;
        TowerController selected = TowerSelection.Instance != null ? TowerSelection.Instance.Selected : null;
        bool setup = gc != null && !gc.IsGameOver && !gc.HasWaveStarted;

        bool show = placing || selected != null || setup;
        foreach (LineRenderer line in routeLines)
        {
            line.enabled = show;
            if (show && scrollSpeed != 0f) line.material.mainTextureOffset = new Vector2(-Time.time * scrollSpeed, 0f);
        }

        if (selected != null && !placing) ShowCoverage(selected.transform.position, selected.Range);
    }

    public float ShowCoverage(Vector3 center, float radius)
    {
        coverageUsed = 0;
        int inside = 0, total = 0;
        List<Vector3> run = new();

        foreach (List<Vector3> route in routes)
        {
            foreach (Vector3 p in route)
            {
                total++;
                Vector2 offset = new Vector2(p.x - center.x, p.z - center.z);
                if (offset.sqrMagnitude <= radius * radius)
                {
                    inside++;
                    run.Add(p);
                }
                else
                {
                    DrawRun(run);
                }
            }
            DrawRun(run);
        }

        for (int i = coverageUsed; i < coverageLines.Count; i++) coverageLines[i].enabled = false;
        return total > 0 ? (float)inside / total : 0f;
    }

    public void HideCoverage()
    {
        foreach (LineRenderer line in coverageLines) line.enabled = false;
    }

    private void DrawRun(List<Vector3> run)
    {
        if (run.Count >= 2)
        {
            if (coverageUsed == coverageLines.Count) coverageLines.Add(Instantiate(coverageLinePrefab, transform));
            LineRenderer line = coverageLines[coverageUsed++];
            line.enabled = true;
            line.positionCount = run.Count;
            line.SetPositions(run.ToArray());
        }
        run.Clear();
    }

    private List<Vector3> Resample(Vector3[] corners)
    {
        List<Vector3> points = new();
        for (int i = 0; i < corners.Length - 1; i++)
        {
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(corners[i], corners[i + 1]) / sampleStep));
            for (int s = 0; s < steps; s++)
                points.Add(Vector3.Lerp(corners[i], corners[i + 1], (float)s / steps) + Vector3.up * heightOffset);
        }
        if (corners.Length > 0) points.Add(corners[corners.Length - 1] + Vector3.up * heightOffset);
        return points;
    }
}
