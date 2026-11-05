using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TowerDragButton : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public Color affordableColor = Color.white;
    public Color unaffordableColor = Color.red;

    private TowerController towerPrefab;
    private Image icon;
    private TextMeshProUGUI costLabel;
    private bool isDragging;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (towerPrefab == null) return;

        bool canAfford = GameController.Instance.CanAfford(towerPrefab.currentCost);
        icon.color = canAfford ? affordableColor : unaffordableColor;
        if (costLabel != null)
        {
            costLabel.text = towerPrefab.currentCost.ToString();
            costLabel.color = canAfford ? Color.black : Color.red;
        }
    }

    public void Setup(TowerController prefab)
    {
        towerPrefab = prefab;

        Transform iconChild = transform.Find("Icon");
        icon = iconChild != null ? iconChild.GetComponent<Image>() : GetComponent<Image>();
        if (prefab.icon != null) icon.sprite = prefab.icon;

        Transform costChild = transform.Find("CostLabel");
        if (costChild != null) costLabel = costChild.GetComponent<TextMeshProUGUI>();
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
