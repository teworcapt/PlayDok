using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    public int totalPatients;
    public int patientsCured;
    public int totalCuredPatients;
    public int totalEarnings;
    public int dailyPenalties;
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
        totalEarnings = 0;
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

    public void SetPenalties(int penalties)
    {
        dailyPenalties = penalties;
        Debug.Log($"Penalties set to: {penalties}");
    }
}
