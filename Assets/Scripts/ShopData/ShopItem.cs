using UnityEngine;

public enum ShopItemType { PermanentTimeBoost, CurrentDayTimeBoost, Cosmetic }

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Scriptable Objects/Shop Item")]
public class ShopItem : ScriptableObject
{
    public int itemNumber;
    public string itemName;
    public int price;
    public int amount;
    public int defaultAmount;
    public Sprite itemIcon;
    public ShopItemType itemType;
    public string itemDescription;
    public float timeBoostPermanent;
    public float timeBoostCurrentDay;
}