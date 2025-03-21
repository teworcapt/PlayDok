using UnityEngine;

public enum ShopItemType { TimeMultiplier, PermanentTimeBoost, Cosmetic }

[CreateAssetMenu(fileName = "NewShopItem", menuName = "ScriptableObject/ShopItem")]
public class ShopItem : ScriptableObject
{
    public int itemNumber;
    public string itemName;
    public int price;
    public int amount;
    public Sprite itemIcon;
    public GameObject itemObject;
    public ShopItemType itemType;

    public float timeBoostPermanent;
    public float timeBoostPercentage;
}
