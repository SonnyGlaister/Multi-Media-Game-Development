using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TowerIconRenderer : MonoBehaviour
{
    public GameObject towerPrefab;

    [ContextMenu("Render Icon")]
    void Render()
    {
        GameObject tower = Instantiate(towerPrefab, new Vector3(0, -1000, 0), Quaternion.identity);

        Camera cam = new GameObject("IconCam").AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 1.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.clear;
        cam.transform.position = tower.transform.position + new Vector3(0, 4, -10);
        cam.transform.LookAt(tower.transform);
        cam.targetTexture = new RenderTexture(256, 256, 24);
        cam.Render();

        RenderTexture.active = cam.targetTexture;
        Texture2D tex = new Texture2D(256, 256);
        tex.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
        File.WriteAllBytes(Application.dataPath + $"/Sprites/Towers/{towerPrefab.name}Icon.png", tex.EncodeToPNG());
        RenderTexture.active = null;

        DestroyImmediate(cam.gameObject);
        DestroyImmediate(tower);
    }
}