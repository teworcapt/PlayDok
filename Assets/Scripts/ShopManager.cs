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
    public TMP_Text creditsText;

    [Header("Shop Items")]
    public List<ShopItem> shopItems = new List<ShopItem>();

    private PlayerData playerData;

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
        playerData = SaveManager.LoadData();
        UpdateCreditsUI();
        PopulateShop();
    }

    private void SaveCredits()
    {
        SaveManager.SaveData(playerData);
    }

    private void UpdateCreditsUI()
    {
        if (creditsText != null)
        {
            creditsText.text = $"Credits: {playerData.GetCredits()}";
        }
    }

    public void AddCredits(int amount)
    {
        playerData.SetCredits(playerData.GetCredits() + amount);
        SaveCredits();
        UpdateCreditsUI();
    }

    public void SetCredits(int amount)
    {
        playerData.SetCredits(amount);
        SaveCredits();
        UpdateCreditsUI();
    }

    private void PopulateShop()
    {
        foreach (ShopItem item in shopItems)
        {
            if (playerData.purchasedItems.Contains(item.itemNumber))
            {
                continue;
            }

            GameObject shopRow = Instantiate(shopItemPrefab, shopItemList);

            TMP_Text itemNameText = shopRow.transform.Find("ItemName").GetComponent<TMP_Text>();
            TMP_Text itemPriceText = shopRow.transform.Find("ItemPrice").GetComponent<TMP_Text>();
            TMP_Text itemAmountText = shopRow.transform.Find("ItemAmount").GetComponent<TMP_Text>();
            Image itemImage = shopRow.transform.Find("ItemIcon").GetComponent<Image>();

            if (itemNameText) itemNameText.text = item.itemName;
            if (itemPriceText) itemPriceText.text = item.price.ToString();
            if (itemAmountText) itemAmountText.text = item.amount.ToString();
            if (itemImage) itemImage.sprite = item.itemIcon;

            Button Buybtn = shopRow.transform.Find("Buybtn").GetComponent<Button>();
            if (Buybtn != null)
            {
                Buybtn.onClick.AddListener(() => BuyItem(item, shopRow));
            }
        }
    }

    public void BuyItem(ShopItem item, GameObject shopRow)
    {
        if (playerData.GetCredits() >= item.price)
        {
            playerData.SetCredits(playerData.GetCredits() - item.price);
            item.amount--;

            if (item.amount <= 0)
            {
                playerData.purchasedItems.Add(item.itemNumber);
                Destroy(shopRow);
            }
            else
            {
                TMP_Text itemAmountText = shopRow.transform.Find("ItemAmount").GetComponent<TMP_Text>();
                if (itemAmountText != null)
                {
                    itemAmountText.text = item.amount.ToString();
                }
            }

            SaveCredits();
            Debug.Log($"Bought {item.itemName}! Remaining: {item.amount}");

            switch (item.itemType)
            {
                case ShopItemType.TimeMultiplier:
                    TimerManager.Instance.ExtendDayTimer(item.timeBoostPermanent);
                    break;

                case ShopItemType.PermanentTimeBoost:
                    TimerManager.Instance.ApplyPermanentTimeBoost(item.timeBoostPermanent);
                    break;

                case ShopItemType.Cosmetic:
                    if (item.itemObject != null)
                    {
                        item.itemObject.SetActive(true);
                    }
                    break;
            }

            UpdateCreditsUI();
        }
        else
        {
            Debug.Log("Not enough credits!");
        }
    }
}
