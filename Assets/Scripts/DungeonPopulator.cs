using UnityEngine;

public class DungeonPopulator : MonoBehaviour
{
    public SpawnableObject[] spawnables;
    public int maxObjectsPerRoom = 3;

    private void Start()
    {
        Debug.Log("Popolamento dungeon iniziato...");
        foreach (Transform room in transform)
        {
            PopulateRoom(room);
        }
    }

    private void PopulateRoom(Transform room)
    {
        Debug.Log($"Popolamento stanza: {room.name}");

        // Recupera tutti gli anchor della stanza
        Transform[] allAnchors = room.GetComponentsInChildren<Transform>();

        // Spawn fino a maxObjectsPerRoom oggetti casuali
        for (int i = 0; i < maxObjectsPerRoom; i++)
        {
            SpawnableObject obj = spawnables[Random.Range(0, spawnables.Length)];
            Transform[] anchors = FindAnchorsByType(allAnchors, obj.placementType);

            if (anchors.Length == 0)
            {
                Debug.LogWarning($"Nessun anchor per {obj.placementType} nella stanza {room.name}");
                continue;
            }

            Transform anchor = anchors[Random.Range(0, anchors.Length)];
            Instantiate(obj.prefab, anchor.position, anchor.rotation, room);
            Debug.Log($"Spawnato {obj.prefab.name} su {anchor.name} nella stanza {room.name}");
        }
    }

    private Transform[] FindAnchorsByType(Transform[] all, PlacementType type)
    {
        string keyword = type switch
        {
            PlacementType.Wall => "WallAnchor",
            PlacementType.Center => "FloorCenter",
            PlacementType.Corner => "CornerAnchor",
            _ => ""
        };

        var result = new System.Collections.Generic.List<Transform>();
        foreach (var t in all)
        {
            if (t.name.Contains(keyword))
                result.Add(t);
        }

        return result.ToArray();
    }
}
