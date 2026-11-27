using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeView : MonoBehaviour
{
    public PerkButton buttonPrefab;
    public Transform columnsParent;
    [Tooltip("Optional TMP label prefab shown at the top of each column.")]
    public TextMeshProUGUI headerPrefab;

    void Start()
    {
        foreach (TowerDefinition tower in PerkManager.Instance.Branches)
        {
            if (tower == null || tower.perks.Length == 0) continue;

            // One column per tower faction
            GameObject column = new GameObject(tower.displayName + "Column", typeof(RectTransform), typeof(VerticalLayoutGroup));
            column.transform.SetParent(columnsParent, false);

            VerticalLayoutGroup layout = column.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            if (headerPrefab != null)
            {
                TextMeshProUGUI header = Instantiate(headerPrefab, column.transform);
                header.text = tower.displayName;
                header.color = tower.color;
            }

            // One button per perk, first perk at the top
            for (int i = 0; i < tower.perks.Length; i++)
            {
                Instantiate(buttonPrefab, column.transform).Setup(tower, i);
            }
        }
    }
}
