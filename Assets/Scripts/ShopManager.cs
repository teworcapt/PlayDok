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

    private DailySaveData dailyData;

    /* -------------------- Initialization -------------------- */

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        string currentDay = !string.IsNullOrEmpty(LoadSaveManager.CurrentLoadedDay)
                                ? LoadSaveManager.CurrentLoadedDay
                                : SaveManager.GetCurrentDay();

        dailyData = LoadSaveManager.LoadDayData(currentDay) ?? new DailySaveData
        {
            day = SaveManager.GetDayIndex() + 1,
            credits = 0,
            dailyPenalties = 0,
            purchasedItems = new List<int>()
        };

        Debug.Log($"Loaded Daily Data - Day: {dailyData.day}, Credits: {dailyData.credits}");
        SetCredits(dailyData.credits);
        UpdateCreditsUI();
        PopulateShop();
    }

    /* -------------------- UI Updates -------------------- */

    public void SetCredits(int amount)
    {
        dailyData.credits = amount;
        UpdateCreditsUI();
        Debug.Log($"Credits updated to: {dailyData.credits}");
    }

    private void UpdateCreditsUI()
    {
        if (creditsText != null)
            creditsText.text = $"Credits: {dailyData.credits}";
    }

    /* -------------------- Shop Population -------------------- */

    private void PopulateShop()
    {
        foreach (ShopItem item in shopItems)
        {
            if (dailyData.purchasedItems.Contains(item.itemNumber))
                continue;

            GameObject shopRow = Instantiate(shopItemPrefab, shopItemList);
            TMP_Text itemNameText = shopRow.transform.Find("ItemName").GetComponent<TMP_Text>();
            TMP_Text itemPriceText = shopRow.transform.Find("ItemPrice").GetComponent<TMP_Text>();
            TMP_Text itemAmountText = shopRow.transform.Find("ItemAmount").GetComponent<TMP_Text>();
            TMP_Text itemDescriptionText = shopRow.transform.Find("ItemDescription")?.GetComponent<TMP_Text>();
            Image itemImage = shopRow.transform.Find("ItemIcon").GetComponent<Image>();

            Button buyButton = shopRow.transform.Find("Buybtn").GetComponent<Button>();
            if (buyButton != null)
                buyButton.onClick.AddListener(() => BuyItem(item, shopRow));
        }
    }

    /* -------------------- Purchasing System -------------------- */

    public void BuyItem(ShopItem item, GameObject shopRow)
    {
        if (dailyData.credits >= item.price)
        {
            dailyData.credits -= item.price;
            item.amount--;

            if (item.amount <= 0)
            {
                dailyData.purchasedItems.Add(item.itemNumber);
                Destroy(shopRow);
            }
            else
            {
                shopRow.transform.Find("ItemAmount").GetComponent<TMP_Text>().text = item.amount.ToString();
            }

            Debug.Log($"Bought {item.itemName}! Remaining: {item.amount}");

            switch (item.itemType)
            {
                case ShopItemType.PermanentTimeBoost:
                    TimerManager.Instance.ApplyPermanentTimeBoost(item.timeBoostPermanent);
                    break;
                case ShopItemType.CurrentDayTimeBoost:
                    TimerManager.Instance.ExtendCurrentDayTimer(item.timeBoostCurrentDay);
                    break;
                case ShopItemType.Cosmetic:
                    item.itemObject?.SetActive(true);
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
