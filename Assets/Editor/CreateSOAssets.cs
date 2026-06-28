using UnityEditor;
using UnityEngine;
using ZooTycoon.Core;

public static class CreateSOAssets
{
    [MenuItem("ZooTycoon/Create SO Assets")]
    public static void Create()
    {
        CreateAsset<FloatEventChannelSO>("Assets/Code/Core/Events/Economy_OnCapitalChanged.asset");
        CreateAsset<FloatEventChannelSO>("Assets/Code/Core/Events/Economy_OnLoanDebtChanged.asset");
        CreateAsset<DailySummaryEventChannelSO>("Assets/Code/Core/Events/Economy_OnDailySummaryReady.asset");
        CreateAsset<IntEventChannelSO>("Assets/Code/Core/Events/Visitor_OnCountChanged.asset");
        CreateAsset<LicenseEventChannelSO>("Assets/Code/Core/Events/License_OnPurchased.asset");
        CreateAsset<FloatVariable>("Assets/Code/Core/Variables/Economy_Capital.asset");
        CreateAsset<FloatVariable>("Assets/Code/Core/Variables/Economy_LoanDebt.asset");
        CreateAsset<HabitatRuntimeSet>("Assets/Code/Core/Sets/Habitats.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("SO Assets created.");
    }

    private static void CreateAsset<T>(string path) where T : ScriptableObject
    {
        if (AssetDatabase.LoadAssetAtPath<T>(path) != null) return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<T>(), path);
    }
}
