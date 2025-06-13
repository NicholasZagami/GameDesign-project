using UnityEngine;

public class RoomBehavior : MonoBehaviour
{
    public GameObject[] walls; // 0-Up 1-Down 2-Right 3-Left
    public GameObject[] doors;

    public Transform spawnArea;

    public void UpdateRoom(bool[] status)
    {
        for (int i = 0; i < status.Length; i++)
        {
            doors[i].SetActive(status[i]);
            walls[i].SetActive(!status[i]);
        }
    }

    public void PopulateRoom(GameObject[] spawnables, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject toSpawn = spawnables[Random.Range(0, spawnables.Length)];

            Vector3 position = new Vector3(
                Random.Range(-3f, 3f),
                0f,
                Random.Range(-3f, 3f)
            );

            Instantiate(toSpawn, spawnArea.position + position, Quaternion.identity, transform);
        }
    }
}

