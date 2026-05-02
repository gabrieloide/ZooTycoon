using UnityEngine;
using System.Collections.Generic;
using ZooTycoon.Data;
public class AnimalManager : MonoBehaviour
{
    public static AnimalManager Instance { get; private set; }
    [SerializeField] private List<AnimalData> animalDataList = new();
    private Dictionary<string, AnimalData> animalDataDict = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        foreach (var animalData in animalDataList)
        {
            animalDataDict.Add(animalData.animalID, animalData);
        }
    }
    public AnimalData GetAnimalData(string id) => animalDataDict[id];
    public List<AnimalData> GetAnimalDataList() => animalDataList;
}