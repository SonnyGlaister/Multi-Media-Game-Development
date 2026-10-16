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
    // public GameController gameController;
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
    }

    // Update is called once per frame
    void Update()
    {
        isAboveRedDamageThreshold = IsAboveThreshold(GameController.Instance.redDamage, redDamageThreshold);
        if (isAboveRedDamageThreshold)
        {
            TowerController.redPerkPurchased = false;
        }
        isAboveYellowDamageThreshold = IsAboveThreshold(GameController.Instance.yellowDamage, yellowDamageThreshold);
        if (isAboveYellowDamageThreshold)
        {
            TowerController.yellowPerkPurchased = false;
        }
        SetButtonState(redPerk, Color.red, TowerController.redPerkPurchased, redPrice, isAboveRedDamageThreshold);
        SetButtonState(yellowPerk, Color.yellow, TowerController.yellowPerkPurchased, yellowPrice, isAboveYellowDamageThreshold);

    }

    void BuyRed()
    {
        if (!GameController.Instance.CanAfford(redPrice) || TowerController.redPerkPurchased)
        {
            return;
        }
        TowerController.redPerkPurchased = true;
        GameController.Instance.Currency -= redPrice;
    }

    void BuyYellow()
    {
        if (!GameController.Instance.CanAfford(yellowPrice) || TowerController.yellowPerkPurchased)
        {
            return;
        }
        TowerController.yellowPerkPurchased = true;
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

    void SetButtonState(Button perk, Color baseColor, bool isPurchased, int price, bool aboveThreshold )
    {
        if(aboveThreshold){
            perk.image.color = Color.black;
            perk.GetComponentInChildren<TextMeshProUGUI>().text = "Disabled";
            perk.interactable = false;
        } else if (isPurchased)
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
