using UnityEngine;

public class RoomPopulator : MonoBehaviour
{
    public SpawnableObject[] testObjects;

    void Start()
    {
        Debug.Log("RoomPopulator: Start chiamato");
        RoomBehavior[] rooms = FindObjectsOfType<RoomBehavior>();
        foreach (var room in rooms)
        {
            room.PopulateRoomAnchored(testObjects);
        }
    }
}
