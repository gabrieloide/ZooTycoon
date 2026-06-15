using System;
using System.Collections.Generic;
using UnityEngine;
using ZooTycoon.Core;
using ZooTycoon.Data;

public class LicenseManager : MonoBehaviour
{
    public static LicenseManager Instance { get; private set; }

    [SerializeField] private List<LicenseData> allLicenses = new();

    private readonly HashSet<string> purchasedIDs = new();

    public static event Action<LicenseData> OnLicensePurchased;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        foreach (var license in allLicenses)
        {
            if (license != null && license.cost <= 0)
                Unlock(license);
        }
    }

    public bool IsPurchased(LicenseData license) => license != null && purchasedIDs.Contains(license.licenseID);

    public bool TryPurchase(LicenseData license)
    {
        if (license == null || IsPurchased(license)) return false;
        if (license.cost > 0)
        {
            if (EconomyManager.Instance == null || !EconomyManager.Instance.CanAfford(license.cost)) return false;
            EconomyManager.Instance.Spend(license.cost);
        }
        Unlock(license);
        return true;
    }

    private void Unlock(LicenseData license)
    {
        purchasedIDs.Add(license.licenseID);
        OnLicensePurchased?.Invoke(license);
    }

    public List<LicenseData> GetAllLicenses() => allLicenses;

    public List<BiomeDefinition> GetUnlockedBiomes()
    {
        var result = new List<BiomeDefinition>();
        foreach (var license in allLicenses)
        {
            if (license != null && purchasedIDs.Contains(license.licenseID))
                foreach (var biome in license.unlockedBiomes)
                    if (biome != null && !result.Contains(biome))
                        result.Add(biome);
        }
        return result;
    }
}
