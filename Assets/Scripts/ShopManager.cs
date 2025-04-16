using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("UI References")]
    public Transform shopItemList;
    public GameObject shopItemPrefab;
    public TMP_Text creditsText;
    public CanvasGroup dogCanvasGroup;
    public CanvasGroup plantCanvasGroup;
    public CanvasGroup starsCanvasGroup;

    [Header("Shop Items")]
    public List<ShopItem> shopItems = new List<ShopItem>();

    [Header("Daily Reset Items")]
    public List<ShopItem> dailyResetItems = new List<ShopItem>();

    private GameSaveData gameSaveData;
    private List<int> currentDayPurchasedItems = new List<int>();
    private Dictionary<int, int> currentItemAmounts = new Dictionary<int, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitializeCanvasGroups();
    }

    private void Start()
    {
        LoadGameData();
        UpdateCreditsUI();
        RefreshShopDisplay();
        ApplyCosmeticVisualsFromSaveData();
    }

    private void InitializeCanvasGroups()
    {
        dogCanvasGroup = GameObject.Find("Dog")?.GetComponent<CanvasGroup>();
        plantCanvasGroup = GameObject.Find("Potted Plant")?.GetComponent<CanvasGroup>();
        starsCanvasGroup = GameObject.Find("Stars")?.GetComponent<CanvasGroup>();

        if (dogCanvasGroup != null) SetAlpha(dogCanvasGroup, 0);
        if (plantCanvasGroup != null) SetAlpha(plantCanvasGroup, 0);
        if (starsCanvasGroup != null) SetAlpha(starsCanvasGroup, 0);
    }

    private void LoadGameData()
    {
        int currentDayIndex = GetCurrentDayIndex();
        gameSaveData = SaveManager.LoadGame(currentDayIndex);
        currentDayPurchasedItems.Clear();
    }

    private int GetCurrentDayIndex()
    {
        if (!string.IsNullOrEmpty(LoadSaveManager.CurrentLoadedDay))
        {
            string[] daysOfWeek = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            int currentDayIndex = System.Array.IndexOf(daysOfWeek, LoadSaveManager.CurrentLoadedDay);
            if (currentDayIndex == -1) currentDayIndex = 0;
            return currentDayIndex;
        }
        else
        {
            int currentDayIndex = SaveManager.GetCurrentDayIndex();
            return currentDayIndex;
        }
    }

    private void RefreshShopDisplay()
    {
        ClearShop();
        PopulateShop();
    }

    private void ApplyCosmeticVisualsFromSaveData()
    {
        foreach (int itemNumber in gameSaveData.purchasedItems)
        {
            ShopItem purchasedItem = shopItems.Find(x => x.itemNumber == itemNumber);
            if (purchasedItem != null)
                ApplyCosmeticVisual(purchasedItem.itemNumber);
        }
    }

    private void UpdateCreditsUI()
    {
        if (creditsText != null)
            creditsText.text = $"Credits: {PlayerStats.Instance.GetCredits()}";
    }

    private void ClearShop()
    {
        if (shopItemList == null)
            return;

        foreach (Transform child in shopItemList)
            Destroy(child.gameObject);
    }

    private void PopulateShop()
    {
        if (shopItemList == null || shopItemPrefab == null)
            return;

        foreach (ShopItem item in shopItems)
        {
            if ((item.itemType == ShopItemType.Cosmetic || item.itemType == ShopItemType.PermanentTimeBoost) &&
                PlayerStats.Instance.itemsBought.Contains(item.itemNumber))
                continue;

            if (item.itemType == ShopItemType.CurrentDayTimeBoost)
            {
                if (!currentItemAmounts.ContainsKey(item.itemNumber))
                    currentItemAmounts[item.itemNumber] = item.defaultAmount;

                if (currentItemAmounts[item.itemNumber] <= 0)
                    continue;
            }

            GameObject shopRow = Instantiate(shopItemPrefab, shopItemList);
            if (shopRow == null)
                continue;

            SetupShopItemUI(shopRow, item);
        }
    }

    private void SetupShopItemUI(GameObject shopRow, ShopItem item)
    {
        TMP_Text nameText = shopRow.transform.Find("ItemName")?.GetComponent<TMP_Text>();
        if (nameText != null) nameText.text = item.itemName;

        TMP_Text priceText = shopRow.transform.Find("ItemPrice")?.GetComponent<TMP_Text>();
        if (priceText != null) priceText.text = item.price.ToString();

        TMP_Text amountText = shopRow.transform.Find("ItemAmount")?.GetComponent<TMP_Text>();
        if (amountText != null)
        {
            int amountToShow = (item.itemType == ShopItemType.CurrentDayTimeBoost) ? currentItemAmounts[item.itemNumber] : item.amount;
            amountText.text = amountToShow.ToString();
        }

        TMP_Text descText = shopRow.transform.Find("ItemDescription")?.GetComponent<TMP_Text>();
        if (descText != null) descText.text = item.itemDescription;

        Image itemImage = shopRow.transform.Find("ItemIcon")?.GetComponent<Image>();
        if (itemImage != null && item.itemIcon != null) itemImage.sprite = item.itemIcon;

        Button buyButton = shopRow.transform.Find("Buybtn")?.GetComponent<Button>();
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => BuyItem(item, shopRow));
        }
    }

    public void BuyItem(ShopItem item, GameObject shopRow)
    {
        int currentCredits = PlayerStats.Instance.GetCredits();
        if (currentCredits >= item.price)
        {
            PlayerStats.Instance.SubtractCredits(item.price);

            if (item.itemType == ShopItemType.CurrentDayTimeBoost)
            {
                currentItemAmounts[item.itemNumber]--;
                if (!currentDayPurchasedItems.Contains(item.itemNumber))
                    currentDayPurchasedItems.Add(item.itemNumber);

                if (currentItemAmounts[item.itemNumber] <= 0)
                    Destroy(shopRow);
                else
                {
                    TMP_Text itemAmountText = shopRow.transform.Find("ItemAmount")?.GetComponent<TMP_Text>();
                    if (itemAmountText != null)
                        itemAmountText.text = currentItemAmounts[item.itemNumber].ToString();
                }
            }
            else
            {
                Destroy(shopRow);
            }

            ApplyItemEffects(item);
            SavePurchaseData(item);
            UpdateCreditsUI();
            SaveShopData();
        }
    }

    private void ApplyItemEffects(ShopItem item)
    {
        switch (item.itemType)
        {
            case ShopItemType.PermanentTimeBoost:
                TimerManager.Instance.ApplyPermanentTimeBoost(item.timeBoostPermanent);
                PlayerStats.Instance.AddPermanentBoost(item.timeBoostPermanent);
                break;

            case ShopItemType.CurrentDayTimeBoost:
                TimerManager.Instance.ExtendCurrentDayTimer(item.timeBoostCurrentDay);
                break;
        }

        ApplyCosmeticVisual(item.itemNumber);
    }

    private void SavePurchaseData(ShopItem item)
    {
        if (item.itemType != ShopItemType.CurrentDayTimeBoost)
        {
            if (!gameSaveData.purchasedItems.Contains(item.itemNumber))
                gameSaveData.purchasedItems.Add(item.itemNumber);

            if (!PlayerStats.Instance.itemsBought.Contains(item.itemNumber))
                PlayerStats.Instance.itemsBought.Add(item.itemNumber);
        }
    }

    private void SaveShopData()
    {
        gameSaveData.credits = PlayerStats.Instance.GetCredits();
        SaveManager.SaveGame(gameSaveData);
    }

    public void ApplyCosmeticVisual(int itemNumber)
    {
        if (itemNumber == 5 && dogCanvasGroup != null)
            SetAlpha(dogCanvasGroup, 1);
        if (itemNumber == 3 && plantCanvasGroup != null)
            SetAlpha(plantCanvasGroup, 1);
        if (itemNumber == 4 && starsCanvasGroup != null)
            SetAlpha(starsCanvasGroup, 1);
    }

    public void SetAlpha(CanvasGroup group, float value)
    {
        group.alpha = value;
        group.blocksRaycasts = value > 0;
        group.interactable = value > 0;
    }

    public void UpdateCosmeticVisuals()
    {
        if (dogCanvasGroup != null) SetAlpha(dogCanvasGroup, 0);
        if (plantCanvasGroup != null) SetAlpha(plantCanvasGroup, 0);
        if (starsCanvasGroup != null) SetAlpha(starsCanvasGroup, 0);

        foreach (int itemID in PlayerStats.Instance.itemsBought)
        {
            ShopItem item = shopItems.Find(i => i.itemNumber == itemID);
            if (item == null) continue;

            if (item.itemType == ShopItemType.Cosmetic)
            {
                if (item.itemName == "Dog" && dogCanvasGroup != null)
                    SetAlpha(dogCanvasGroup, 1);
                else if (item.itemName == "Potted Plant" && plantCanvasGroup != null)
                    SetAlpha(plantCanvasGroup, 1);
                else if (item.itemName == "Stars" && starsCanvasGroup != null)
                    SetAlpha(starsCanvasGroup, 1);
            }
        }
    }

    public void ResetDailyItems()
    {
        foreach (ShopItem item in shopItems)
        {
            if (item.itemType == ShopItemType.CurrentDayTimeBoost)
                currentItemAmounts[item.itemNumber] = item.defaultAmount;
        }

        currentDayPurchasedItems.Clear();
        SaveShopData();
        RefreshShopDisplay();
    }
}
