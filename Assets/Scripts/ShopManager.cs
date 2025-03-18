using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Shop UI")]
    public Transform shopItemList;
    public GameObject shopItemPrefab;

    [Header("Shop Items")]
    public List<ShopItem> shopItems = new List<ShopItem>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PopulateShop();
    }

    private void PopulateShop()
    {
        foreach (ShopItem item in shopItems)
        {
            GameObject shopRow = Instantiate(shopItemPrefab, shopItemList);

            TMP_Text itemNameText = shopRow.transform.Find("ItemName").GetComponent<TMP_Text>();
            TMP_Text itemPriceText = shopRow.transform.Find("ItemPrice").GetComponent<TMP_Text>();
            Image itemImage = shopRow.transform.Find("ItemIcon").GetComponent<Image>();

            if (itemNameText) itemNameText.text = item.itemName;
            if (itemPriceText) itemPriceText.text = "$" + item.price;
            if (itemImage) itemImage.sprite = item.itemIcon;

            Button buyButton = shopRow.transform.Find("BuyButton").GetComponent<Button>();
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(() => BuyItem(item));
            }
        }
    }

    public void BuyItem(ShopItem item)
    {
        PlayerData data = SaveManager.LoadData();

        if (data.credits >= item.price)
        {
            data.credits -= item.price;
            SaveManager.SaveData(data);
            Debug.Log($"Bought {item.itemName}!");

            if (item.isTimeMultiplier)
            {
                TimerManager.Instance.ExtendDayTimer();
            }
            else if (item.itemObject != null)
            {
                item.itemObject.SetActive(true);
            }
        }
        else
        {
            Debug.Log("Not enough credits!");
        }
    }

    public void AddCredits(int amount)
    {
        PlayerData data = SaveManager.LoadData();
        data.credits += amount;
        SaveManager.SaveData(data);
        Debug.Log($"Credits added: {amount}. Current Balance: {data.credits}");
    }

}
