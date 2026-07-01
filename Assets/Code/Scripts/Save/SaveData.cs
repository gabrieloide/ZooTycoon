using System.Collections.Generic;

namespace ZooTycoon.Save
{
    [System.Serializable]
    public class SaveData
    {
        public TimeSaveData time;
        public EconomySaveData economy;
        public PlayerSaveData player;
        public int buildSession;
        public List<string> purchasedLicenseIDs;
        public List<HabitatSaveData> habitats;
        public List<PathSaveData> paths;
    }

    [System.Serializable]
    public class TimeSaveData
    {
        public int day;
    }

    [System.Serializable]
    public class EconomySaveData
    {
        public float capital;
        public float loanDebt;
    }

    [System.Serializable]
    public class PlayerSaveData
    {
        public float stamina;
        public float posX;
        public float posY;
        public float posZ;
    }

    [System.Serializable]
    public class HabitatSaveData
    {
        public int id;
        public string biomeID;
        public int xMin;
        public int xMax;
        public int yMin;
        public int yMax;
        public int maxOcupation;
        public float totalBuildCost;
        public int buildSession;
        public List<AnimalSaveData> animals;
    }

    [System.Serializable]
    public class AnimalSaveData
    {
        public string animalID;
        public float currentAnnoyance;
        public bool hasEscaped;
        public float purchaseCost;
        public int buildSession;
    }

    [System.Serializable]
    public class PathSaveData
    {
        public float cellX;
        public float cellY;
        public float cost;
        public int session;
    }
}
