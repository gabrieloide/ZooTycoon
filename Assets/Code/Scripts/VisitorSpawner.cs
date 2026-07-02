using System.Collections.Generic;
using UnityEngine;
using ZooTycoon.Core;
using ZooTycoon.Data;

public class VisitorSpawner : MonoBehaviour
{
    public static VisitorSpawner Instance { get; private set; }

    [Header("Pool")]
    [SerializeField] private GameObject visitorPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private VisitorConfig visitorConfig;

    public Transform SpawnPoint => spawnPoint;

    [Header("Event Channels")]
    [SerializeField] private VoidEventChannelSO onWorkDayEndChannel;
    [SerializeField] private IntEventChannelSO onVisitorCountChangedChannel;

    private readonly List<GameObject> pool = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        if (visitorPrefab == null || visitorConfig == null) return;

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        for (int i = 0; i < visitorConfig.maxNpcVisitors; i++)
        {
            var npc = Instantiate(visitorPrefab, spawnPos, Quaternion.identity);
            npc.GetComponent<VisitorNPC>()?.SetConfig(visitorConfig);
            npc.SetActive(false);
            pool.Add(npc);
        }
    }

    private void OnEnable()
    {
        onVisitorCountChangedChannel?.Subscribe(OnVisitorCountChanged);
        onWorkDayEndChannel?.Subscribe(OnDayClose);
    }

    private void OnDisable()
    {
        onVisitorCountChangedChannel?.Unsubscribe(OnVisitorCountChanged);
        onWorkDayEndChannel?.Unsubscribe(OnDayClose);
    }

    private void OnVisitorCountChanged(int current)
    {
        if (visitorConfig == null || pool.Count == 0) return;

        int target = Mathf.Clamp(
            Mathf.RoundToInt((float)current / visitorConfig.maxVisitors * visitorConfig.maxNpcVisitors),
            0, pool.Count);

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        for (int i = 0; i < pool.Count; i++)
        {
            bool shouldBeActive = i < target;
            if (shouldBeActive && !pool[i].activeSelf)
                pool[i].transform.position = spawnPos;
            pool[i].SetActive(shouldBeActive);
        }
    }

    private void OnDayClose()
    {
        foreach (var npc in pool)
            if (npc != null) npc.SetActive(false);
    }
}
