using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TowerDragButton : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private TowerController towerPrefab;
    private Image icon;
    private bool isDragging;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (towerPrefab == null) return;

        bool canAffordTower = GameController.Instance.CanAfford(towerPrefab.cost);
        icon.color = canAffordTower ? Color.white : Color.gray;
    }

    public void Setup(TowerController prefab)
    {
        towerPrefab = prefab;
        icon = GetComponent<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!TowerPlacer.Instance.CanDrag()) return;

        isDragging = true;
        TowerPlacer.Instance.BeginDrag(towerPrefab);
        TowerPlacer.Instance.Drag(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        TowerPlacer.Instance.Drag(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        GameObject hovered = eventData.pointerCurrentRaycast.gameObject;
        bool droppedOnButton = hovered != null && hovered.GetComponentInParent<Button>() != null;
        TowerPlacer.Instance.EndDrag(eventData.position, droppedOnButton);
    }
}
