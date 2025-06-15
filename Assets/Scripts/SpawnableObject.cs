using UnityEngine;

public enum PlacementType
{
    Wall,
    Corner,
    Center
}

[System.Serializable]
public class SpawnableObject
{
    public GameObject prefab;
    public PlacementType placementType;
}
