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

    public bool isAboveYellowDamageThreshold;
    public bool isAboveRedDamageThreshold;

    // Start is called before the first frame update
    void Start()
    {
        redPerk.onClick.AddListener(BuyRed);
        yellowPerk.onClick.AddListener(BuyYellow);
        redPerk1.onClick.AddListener(BuyRed1);
        yellowPerk1.onClick.AddListener(BuyYellow1);
    }

    // Update is called once per frame
    void Update()
    {
        isAboveRedDamageThreshold = IsAboveThreshold(GameController.Instance.redDamage, redDamageThreshold);
        if (isAboveRedDamageThreshold)
        {
            TowerController.redPerkPurchased = false;
            TowerController.redPerkPurchased1 = false;
        }
        isAboveYellowDamageThreshold = IsAboveThreshold(GameController.Instance.yellowDamage, yellowDamageThreshold);
        if (isAboveYellowDamageThreshold)
        {
            TowerController.yellowPerkPurchased = false;
            TowerController.yellowPerkPurchased1 = false;
        }
        SetButtonState(redPerk, Color.red, TowerController.redPerkPurchased, redPrice, isAboveRedDamageThreshold,1,0);
        SetButtonState(yellowPerk, Color.yellow, TowerController.yellowPerkPurchased, yellowPrice, isAboveYellowDamageThreshold,1,0);
        SetButtonState(redPerk1, Color.red, TowerController.redPerkPurchased1, redPrice, isAboveRedDamageThreshold,2,RSeqTower);
        SetButtonState(yellowPerk1, Color.yellow, TowerController.yellowPerkPurchased1, yellowPrice, isAboveYellowDamageThreshold,2,YSeqTower);

    }

    void BuyRed()
    {
        if (!GameController.Instance.CanAfford(redPrice) || TowerController.redPerkPurchased)
        {
            return;
        }
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
        TowerController.redPerkPurchased1 = true;
        GameController.Instance.Currency -= redPrice;
    }

    void BuyYellow1()
    {
        if (!GameController.Instance.CanAfford(yellowPrice) || TowerController.yellowPerkPurchased1)
        {
            return;
        }
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

    void SetButtonState(Button perk, Color baseColor, bool isPurchased, int price, bool aboveThreshold, int version, int tower )
    {
        if(aboveThreshold){
            perk.image.color = Color.black;
            perk.GetComponentInChildren<TextMeshProUGUI>().text = "Disabled";
            perk.interactable = false;
        } else if (version == 2 & tower == 0)
        {
            perk.image.color = Color.gray;
            perk.interactable = false;
        }
        else if (isPurchased)
        {
            perk.image.color = Color.green;
            perk.interactable = false;
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
