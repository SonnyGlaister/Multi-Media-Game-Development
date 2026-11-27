using UnityEngine;
using UnityEngine.UI;

public class SkillTreeView : MonoBehaviour
{
    public PerkButton buttonPrefab;
    public Transform columnsParent;

    void Start()
    {
        foreach (FactionDefinition f in GameController.Instance.Factions)
        {
            if (f.perks.Length == 0) continue;

            // One column per faction
            GameObject column = new GameObject(f.displayName + "Column", typeof(RectTransform), typeof(VerticalLayoutGroup));
            column.transform.SetParent(columnsParent, false);

            VerticalLayoutGroup layout = column.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // One button per tier, tier 1 at the top
            for (int tier = 0; tier < f.perks.Length; tier++)
            {
                Instantiate(buttonPrefab, column.transform).Setup(f, tier);
            }
        }
    }
}