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

    [Header("Cosmetic Items")]
    public CanvasGroup dogCanvasGroup;
    public CanvasGroup plantCanvasGroup;

    [Header("Shop Items")]
    public List<ShopItem> shopItems = new List<ShopItem>();

    private DailySaveData dailyData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        GameObject dogObject = GameObject.Find("Dog");
        if (dogObject != null)
        {
            dogCanvasGroup = dogObject.GetComponent<CanvasGroup>();
            if (dogCanvasGroup != null)
            {
                SetAlpha(dogCanvasGroup, 0);
            }
        }

        GameObject plantObject = GameObject.Find("Potted Plant");
        if (plantObject != null)
        {
            plantCanvasGroup = plantObject.GetComponent<CanvasGroup>();
            if (plantCanvasGroup != null)
            {
                SetAlpha(plantCanvasGroup, 0);
            }
        }
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

        int currentDayIndex = SaveManager.GetDayIndex() + 1;
        if (dailyData.day != currentDayIndex)
        {
            ResetDailyItems();
            ResetDailyShopItems();
            dailyData.day = currentDayIndex;
        }

        Debug.Log($"Loaded Daily Data - Day: {dailyData.day}, Credits: {dailyData.credits}");
        SetCredits(dailyData.credits);
        UpdateCreditsUI();
        PopulateShop();

        foreach (int itemNumber in dailyData.purchasedItems)
        {
            ShopItem purchasedItem = shopItems.Find(x => x.itemNumber == itemNumber);
            if (purchasedItem != null)
            {
                if (purchasedItem.itemName == "Dog")
                    SetAlpha(dogCanvasGroup, 1);
                else if (purchasedItem.itemName == "Potted Plant")
                    SetAlpha(plantCanvasGroup, 1);
            }
        }
    }

    private void ResetDailyItems()
    {
        dailyData.purchasedItems = dailyData.purchasedItems.FindAll(itemNumber =>
        {
            ShopItem shopItem = shopItems.Find(x => x.itemNumber == itemNumber);
            return shopItem != null &&
                   (shopItem.itemType == ShopItemType.Cosmetic || shopItem.itemType == ShopItemType.PermanentTimeBoost);
        });
    }

    private void ResetDailyShopItems()
    {
        foreach (ShopItem item in shopItems)
        {
            if (item.itemType == ShopItemType.CurrentDayTimeBoost)
            {
                item.amount = item.defaultAmount;
            }
        }
    }

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

    private void PopulateShop()
    {
        foreach (ShopItem item in shopItems)
        {
            if (dailyData.purchasedItems.Contains(item.itemNumber))
                continue;

            if (item.itemType == ShopItemType.CurrentDayTimeBoost && item.amount <= 0)
                continue;

            GameObject shopRow = Instantiate(shopItemPrefab, shopItemList);

            TMP_Text itemNameText = shopRow.transform.Find("ItemName").GetComponent<TMP_Text>();
            TMP_Text itemPriceText = shopRow.transform.Find("ItemPrice").GetComponent<TMP_Text>();
            TMP_Text itemAmountText = shopRow.transform.Find("ItemAmount").GetComponent<TMP_Text>();
            TMP_Text itemDescriptionText = shopRow.transform.Find("ItemDescription")?.GetComponent<TMP_Text>();
            Image itemImage = shopRow.transform.Find("ItemIcon").GetComponent<Image>();

            if (itemNameText != null)
                itemNameText.text = item.itemName;
            if (itemPriceText != null)
                itemPriceText.text = item.price.ToString();
            if (itemAmountText != null)
                itemAmountText.text = item.amount.ToString();
            if (itemDescriptionText != null)
                itemDescriptionText.text = item.itemDescription;
            if (itemImage != null && item.itemIcon != null)
                itemImage.sprite = item.itemIcon;

            Button buyButton = shopRow.transform.Find("Buybtn").GetComponent<Button>();
            if (buyButton != null)
                buyButton.onClick.AddListener(() => BuyItem(item, shopRow));
        }
    }

    public void BuyItem(ShopItem item, GameObject shopRow)
    {
        if (dailyData.credits >= item.price)
        {
            dailyData.credits -= item.price;

            if (item.itemType == ShopItemType.CurrentDayTimeBoost)
                item.amount--;

            if (item.itemType == ShopItemType.CurrentDayTimeBoost && item.amount <= 0)
            {
                dailyData.purchasedItems.Add(item.itemNumber);
                Destroy(shopRow);
            }
            else
            {
                TMP_Text itemAmountText = shopRow.transform.Find("ItemAmount").GetComponent<TMP_Text>();
                if (itemAmountText != null)
                    itemAmountText.text = item.amount.ToString();
            }

            Debug.Log($"Bought {item.itemName}!");

            switch (item.itemType)
            {
                case ShopItemType.PermanentTimeBoost:
                    TimerManager.Instance.ApplyPermanentTimeBoost(item.timeBoostPermanent);
                    if (item.itemName == "Dog")
                        SetAlpha(dogCanvasGroup, 1);
                    break;
                case ShopItemType.CurrentDayTimeBoost:
                    TimerManager.Instance.ExtendCurrentDayTimer(item.timeBoostCurrentDay);
                    break;
                case ShopItemType.Cosmetic:
                    if (item.itemName == "Potted Plant")
                        SetAlpha(plantCanvasGroup, 1);
                    break;
            }
            UpdateCreditsUI();
        }
        else
        {
            Debug.Log("Not enough credits!");
        }
    }

    public void SetAlpha(CanvasGroup canvasGroup, float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
            canvasGroup.interactable = (alpha == 1);
            canvasGroup.blocksRaycasts = (alpha == 1);
        }
    }
    public List<int> GetPurchasedItemIDs()
    {
        return dailyData.purchasedItems;
    }

}
