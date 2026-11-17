using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillTreeMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject SkillTree;
    public static SkillTreeMenu Instance;
    public Button SkillTreePress;
    public bool isOpen = false;
    public GameObject SkillTreeUI;
    private Vector3[] WholeScreen = {new Vector3(1, 1, 1),new Vector3(367f, 360.6667f, 0) };
    private Vector3[] Corner = { new Vector3(.2f, .2f, .2f), new Vector3(650f, 80f, 0) };
    void Start()
    {
        SkillTree.SetActive(false);
        ChangeCorner();

    }
    private void Awake()
    {
        Instance = this;
    }
    private void ChangeCorner()
    {
        SkillTreeUI.transform.localScale = Corner[0];
        SkillTreeUI.transform.position = Corner[1];
    }
    private void ChangeWholeScreen()
    {
        SkillTreeUI.transform.localScale = WholeScreen[0];
        SkillTreeUI.transform.position = WholeScreen[1];
    }

    public void SkillTreePressed()
    {
        if (!isOpen)
        {
            ChangeWholeScreen();
            isOpen = true;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
        if (!GameController.Instance.HasGameStarted) { return; }
        SkillTree.SetActive(true);
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (isOpen)
            {
                ChangeCorner();

                isOpen = false;
            }
            else
            {
                ChangeWholeScreen();

                isOpen = true;
            }
            return;
        }
    }
}