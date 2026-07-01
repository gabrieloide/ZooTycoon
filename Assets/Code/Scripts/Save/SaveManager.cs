using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ZooTycoon.Core;
using ZooTycoon.Save;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [SerializeField] private HabitatBuilder habitatBuilder;
    [SerializeField] private PathBuilder pathBuilder;
    [SerializeField] private LicenseManager licenseManager;

    private string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private IEnumerator Start()
    {
        yield return null;
        if (HasSaveFile())
            Load();
    }

    public bool HasSaveFile() => File.Exists(SavePath);

    public void Save()
    {
        var data = BuildSaveData();
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public void Load()
    {
        if (!File.Exists(SavePath)) return;
        string json = File.ReadAllText(SavePath);
        var data = JsonUtility.FromJson<SaveData>(json);
        ApplySaveData(data);
    }

    private SaveData BuildSaveData()
    {
        var player = FindPlayer();
        var data = new SaveData
        {
            time = new TimeSaveData { day = TimeManager.Instance.GetCurrentDay() },
            economy = new EconomySaveData
            {
                capital = EconomyManager.Instance.Capital,
                loanDebt = EconomyManager.Instance.LoanDebt
            },
            player = new PlayerSaveData
            {
                stamina = player != null ? player.stamina : 0f,
                posX = player != null ? player.transform.position.x : 0f,
                posY = player != null ? player.transform.position.y : 0f,
                posZ = player != null ? player.transform.position.z : 0f
            },
            buildSession = BuildController.CurrentSession,
            purchasedLicenseIDs = licenseManager.GetPurchasedIDs(),
            habitats = new List<HabitatSaveData>(),
            paths = new List<PathSaveData>()
        };

        foreach (var habitat in HabitatManager.GetAllHabitats())
        {
            var habitatSave = new HabitatSaveData
            {
                id = habitat.id,
                biomeID = habitat.biome != null ? habitat.biome.biomeID : null,
                xMin = habitat.xMin,
                xMax = habitat.xMax,
                yMin = habitat.yMin,
                yMax = habitat.yMax,
                maxOcupation = habitat.maxOcupation,
                totalBuildCost = habitat.totalBuildCost,
                buildSession = habitat.buildSession,
                animals = new List<AnimalSaveData>()
            };

            foreach (var animal in habitat.GetAnimals())
            {
                if (animal == null || animal.data == null) continue;
                habitatSave.animals.Add(new AnimalSaveData
                {
                    animalID = animal.data.animalID,
                    currentAnnoyance = animal.currentAnnoyance,
                    hasEscaped = animal.hasEscaped,
                    purchaseCost = animal.purchaseCost,
                    buildSession = animal.buildSession
                });
            }

            data.habitats.Add(habitatSave);
        }

        foreach (var cell in PathManager.GetAll())
        {
            if (!PathManager.TryGetData(cell, out float cost, out int session)) continue;
            data.paths.Add(new PathSaveData
            {
                cellX = cell.x,
                cellY = cell.y,
                cost = cost,
                session = session
            });
        }

        return data;
    }

    private void ApplySaveData(SaveData data)
    {
        HabitatManager.Clear();

        TimeManager.Instance.LoadState(data.time.day);
        EconomyManager.Instance.LoadState(data.economy.capital, data.economy.loanDebt);
        BuildController.LoadSession(data.buildSession);
        licenseManager.LoadPurchasedLicenses(data.purchasedLicenseIDs);

        foreach (var habitatSave in data.habitats)
        {
            var biome = licenseManager.GetBiomeData(habitatSave.biomeID);
            var habitat = habitatBuilder.RebuildHabitat(habitatSave, biome);
            if (habitat == null) continue;

            foreach (var animalSave in habitatSave.animals)
            {
                var animalData = AnimalManager.Instance.GetAnimalData(animalSave.animalID);
                habitat.AddAnimal(animalData);
                var animals = habitat.GetAnimals();
                var animal = animals[animals.Count - 1];
                animal.currentAnnoyance = animalSave.currentAnnoyance;
                animal.purchaseCost = animalSave.purchaseCost;
                animal.buildSession = animalSave.buildSession;
                if (animalSave.hasEscaped)
                {
                    animal.hasEscaped = true;
                    animal.transform.SetParent(null);
                }
            }

            if (habitat.HasEscapedAnimals())
                habitat.BreakFence();
        }

        foreach (var habitat in HabitatManager.GetAllHabitats())
            habitat.RefreshNeighborTension();

        foreach (var pathSave in data.paths)
            pathBuilder.RebuildTile(new Vector2(pathSave.cellX, pathSave.cellY), pathSave.cost, pathSave.session);

        var player = FindPlayer();
        if (player != null)
        {
            player.stamina = data.player.stamina;
            var rb = player.GetComponent<Rigidbody>();
            rb.position = new Vector3(data.player.posX, data.player.posY, data.player.posZ);
            rb.linearVelocity = Vector3.zero;
        }
    }

    private PlayerMovement FindPlayer()
    {
        var playerObject = GameObject.FindGameObjectWithTag("Player");
        return playerObject != null ? playerObject.GetComponent<PlayerMovement>() : null;
    }
}
