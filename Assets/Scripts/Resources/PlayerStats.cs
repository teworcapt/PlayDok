using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    public int totalPatients;
    public int patientsCured;
    public int penalties;
    public int totalEarnings;
    public float timeBoostPermanent = 0f;

    public List<int> itemsBought = new List<int>();

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
            return;
        }
    }

    public void AddPenalty(int amount)
    {
        penalties += amount;
    }

    public void AddCuredPatient()
    {
        patientsCured++;
    }

    public void AddTotalPatient()
    {
        totalPatients++;
    }

    public void ResetDailyStats()
    {
        totalPatients = 0;
        patientsCured = 0;
        penalties = 0;
        totalEarnings = 0;
    }

    public void BuyItem(int itemID)
    {
        if (!itemsBought.Contains(itemID))
        {
            itemsBought.Add(itemID);
        }
    }

    public bool HasBoughtItem(int itemID)
    {
        return itemsBought.Contains(itemID);
    }
}
