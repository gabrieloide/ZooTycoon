using System.Collections.Generic;
using UnityEngine;
using ZooTycoon.Core;
using ZooTycoon.Data;

public class LicenseManager : MonoBehaviour
{
    public static LicenseManager Instance { get; private set; }

    [SerializeField] private List<LicenseData> allLicenses = new();

    [Header("Event Channels Out")]
    [SerializeField] private LicenseEventChannelSO onLicensePurchasedChannel;

    private readonly HashSet<string> purchasedIDs = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
        onLicensePurchasedChannel?.Raise(license);
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
