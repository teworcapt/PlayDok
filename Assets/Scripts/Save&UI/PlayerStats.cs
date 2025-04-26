using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    public int totalPatients = 0;
    public int patientsCured = 0;
    public int totalCuredPatients;
    public int dailyPenalties = 0;
    public int totalEarnings = 0;
    public float timeBoostPermanent = 0f;
    public List<int> itemsBought = new List<int>();
    public float Volume { get; set; } = 0.5f;
    public bool IsMuted { get; set; } = false;
    public int ResolutionIndex { get; set; } = 3;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("PlayerStats initialized.");

            // Load permanent time boost from PersistentDataManager
            PlayerPersistentData data = PersistentDataManager.LoadData();
            if (data != null)
            {
                timeBoostPermanent = data.permanentTimeBoost;
                Debug.Log($"Loaded permanent time boost: {timeBoostPermanent}");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPenalty(int amount)
    {
        dailyPenalties += amount;
    }

    public void AddCuredPatient()
    {
        patientsCured++;
        totalCuredPatients++;
    }

    public void AddTotalPatient() => totalPatients++;

    public void ResetDailyStats()
    {
        totalPatients = 0;
        patientsCured = 0;
        dailyPenalties = 0;
    }

    public void BuyItem(int itemID)
    {
        if (!itemsBought.Contains(itemID))
            itemsBought.Add(itemID);
    }

    public bool HasBoughtItem(int itemID) => itemsBought.Contains(itemID);

    public void SetCredits(int credits)
    {
        totalEarnings = credits;
        Debug.Log($"Credits set to: {credits}");
    }

    public int GetCredits() => totalEarnings;

    public void AddCredits(int amount)
    {
        totalEarnings += amount;
        Debug.Log($"Credits increased by {amount}, new total: {totalEarnings}");
    }

    public void SubtractCredits(int amount)
    {
        totalEarnings = Mathf.Max(0, totalEarnings - amount);
        Debug.Log($"Credits decreased by {amount}, new total: {totalEarnings}");
    }

    public void SetPenalties(int penalties)
    {
        dailyPenalties = penalties;
        Debug.Log($"Penalties set to: {penalties}");
    }

    public void ResetPermanentTimeBoosts()
    {
        timeBoostPermanent = 0f;
    }

    public void AddPermanentBoost(float boost)
    {
        timeBoostPermanent += boost;
        Debug.Log($"Permanent time boost increased by {boost}, new total: {timeBoostPermanent}");

        // Make sure we also update the persistent data
        PlayerPersistentData data = PersistentDataManager.LoadData();
        if (data != null)
        {
            data.permanentTimeBoost = timeBoostPermanent;
            PersistentDataManager.SaveData(data);
        }
    }
}