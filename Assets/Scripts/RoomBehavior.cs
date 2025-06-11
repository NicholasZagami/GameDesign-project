using UnityEngine;

public class RoomBehavior : MonoBehaviour
{
    public GameObject[] walls; // 0-Up 1-Down 2-Right 3-Left
    public GameObject[] doors;

    public bool[] testStatus;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateRoom(testStatus);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateRoom(bool[] status)
    {
        for (int i = 0; i < status.Length; i++)
        {
            doors[i].SetActive(status[i]);
            walls[i].SetActive(!status[i]);
        }
    }
}

