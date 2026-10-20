using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SkillTree : MonoBehaviour
{

    // There's got to be a way to access this game object's children instead of assigning them to public variables
    public Button redPerk;

    public Button yellowPerk;

    public Button redPerk1;

    public Button yellowPerk1;
    // public GameController gameController;
    int RSeqTower = 0;
    int YSeqTower = 0;

    public int redPrice;
    public int yellowPrice;

    public int redDamageThreshold = 1;
    public int yellowDamageThreshold = 1;
    public static int Rbought = 0;
    public static int Ybought = 0;
    public bool isAboveYellowDamageThreshold;
    public bool isAboveRedDamageThreshold;

    // Start is called before the first frame update
    void Start()
    {
        RSeqTower = 0;
        YSeqTower = 0;
        Rbought = 0;
        Ybought = 0;
        redPerk.onClick.AddListener(BuyRed);
        yellowPerk.onClick.AddListener(BuyYellow);
        redPerk1.onClick.AddListener(BuyRed1);
        yellowPerk1.onClick.AddListener(BuyYellow1);
    }

    // Update is called once per frame
    void Update()
    {

        isAboveRedDamageThreshold = IsAboveThreshold(GameController.Instance.redDamageHid, redDamageThreshold);
        if (isAboveRedDamageThreshold)
        {
            if (TowerController.redPerkPurchased1)
            {
                TowerController.redPerkPurchased1 = false;
                GameController.Instance.redDamageHid= GameController.Instance.redDamageHid - redDamageThreshold;
            } else 
            { 
                TowerController.redPerkPurchased = false;
            }
        }
        isAboveYellowDamageThreshold = IsAboveThreshold(GameController.Instance.yellowDamageHid, yellowDamageThreshold);
        if (isAboveYellowDamageThreshold)
        {
            if (TowerController.yellowPerkPurchased1)
            {
                TowerController.yellowPerkPurchased1 = false;
                GameController.Instance.yellowDamageHid = GameController.Instance.yellowDamageHid - yellowDamageThreshold;
            }
            else
            {
                TowerController.yellowPerkPurchased = false;
            }

        }
        SetButtonState(redPerk, Color.red, TowerController.redPerkPurchased, redPrice, isAboveRedDamageThreshold,1,0,Rbought, "FireRate+ ");
        SetButtonState(yellowPerk, Color.yellow, TowerController.yellowPerkPurchased, yellowPrice, isAboveYellowDamageThreshold,1,0, Ybought, "Range+ ");
        SetButtonState(redPerk1, Color.red, TowerController.redPerkPurchased1, redPrice, isAboveRedDamageThreshold,2,RSeqTower, Rbought, "FireRate+ ");
        SetButtonState(yellowPerk1, Color.yellow, TowerController.yellowPerkPurchased1, yellowPrice, isAboveYellowDamageThreshold,2,YSeqTower, Ybought, "Range+ ");

    }

    void BuyRed()
    {
        if (!GameController.Instance.CanAfford(redPrice) || TowerController.redPerkPurchased)
        {
            return;
        }
        Rbought++;
        TowerController.redPerkPurchased = true;
        GameController.Instance.Currency -= redPrice;
        RSeqTower = 1;
    }

    void BuyYellow()
    {
        if (!GameController.Instance.CanAfford(yellowPrice) || TowerController.yellowPerkPurchased)
        {
            return;
        }
        Ybought++;
        TowerController.yellowPerkPurchased = true;
        GameController.Instance.Currency -= yellowPrice;
        YSeqTower = 1;

    }

    void BuyRed1()
    {
        if (!GameController.Instance.CanAfford(redPrice) || TowerController.redPerkPurchased1)
        {
            return;
        }
        Rbought++;
        TowerController.redPerkPurchased1 = true;
        GameController.Instance.Currency -= redPrice;
    }

    void BuyYellow1()
    {
        if (!GameController.Instance.CanAfford(yellowPrice) || TowerController.yellowPerkPurchased1)
        {
            return;
        }
        Ybought++;
        TowerController.yellowPerkPurchased1 = true;
        GameController.Instance.Currency -= yellowPrice;

    }

    bool IsAboveThreshold(int damage, int threshold)
    {
        if (damage >= threshold)
        {
            return true;
        }
        return false;
    }

    void SetButtonState(Button perk, Color baseColor, bool isPurchased, int price, bool aboveThreshold, int version, int tower ,int boughtNum, string name)
    {
        perk.GetComponentInChildren<TextMeshProUGUI>().text = (name + "(" + price + ")");
        if (aboveThreshold){
            perk.image.color = Color.black;
            perk.GetComponentInChildren<TextMeshProUGUI>().text = "Disabled";
            perk.interactable = false;
        } else if (!isPurchased & version == 2 & tower == 1 & boughtNum == 2 )
        {
            perk.image.color = Color.black;
            perk.GetComponentInChildren<TextMeshProUGUI>().text = "disabled";
            perk.interactable = false;
        }
        else if (version == 2 & tower == 0)
        {
            perk.image.color = Color.gray;
            perk.interactable = false;
        }
        else if (boughtNum >= version)
        {
            perk.image.color = Color.green;
            perk.interactable = false;
            perk.GetComponentInChildren<TextMeshProUGUI>().text = "Bought";
            return;
        } else if (!GameController.Instance.CanAfford(price))
        {
            perk.image.color = Color.gray;
            perk.interactable = false;
        } else
        {
            perk.image.color = baseColor;
            perk.interactable = true;
        }
    }
}
