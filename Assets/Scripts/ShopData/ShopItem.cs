using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Item")]
public class ShopItem : ScriptableObject
{
    public string itemName;
    public int price;
    public bool isTimeMultiplier;
    public GameObject itemObject;
    public Sprite itemIcon;
}
