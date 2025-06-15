using UnityEngine;

public class RoomBehavior : MonoBehaviour
{
    public GameObject[] walls; // 0-Up 1-Down 2-Right 3-Left
    public GameObject[] doors;

    //AnchorPoints
    public Transform[] wallAnchors;
    public Transform[] centerAnchors;
    public Transform[] cornerAnchors;
    public Transform spawnArea;

    public void UpdateRoom(bool[] status)
    {
        for (int i = 0; i < status.Length; i++)
        {
            doors[i].SetActive(status[i]);
            walls[i].SetActive(!status[i]);
        }
    }

    public void PopulateRoomAnchored(SpawnableObject[] spawnables)
    {
        foreach (var obj in spawnables)
        {
            Transform[] anchors = GetAnchorsByType(obj.placementType);
            if (anchors.Length == 0) continue;

            Transform anchor = anchors[Random.Range(0, anchors.Length)];
            Instantiate(obj.prefab, anchor.position, anchor.rotation, transform);
        }
    }

    private Transform[] GetAnchorsByType(PlacementType type)
    {
        return type switch
        {
            PlacementType.Wall => wallAnchors,
            PlacementType.Corner => cornerAnchors,
            PlacementType.Center => centerAnchors,
            _ => new Transform[0]
        };
    }
}

