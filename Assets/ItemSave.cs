using UnityEngine;

public class ItemSave : MonoBehaviour
{
    public string uniqueID; // es. L1_Potion_01

    private void OnEnable()
    {
        var s = SaveManager.Instance?.CurrentSave;
        if (s != null && s.collectedItems.Contains(uniqueID))
            Destroy(gameObject);
    }

    public void OnCollected()
    {
        SaveManager.Instance?.RegisterItemCollected(uniqueID);
        Destroy(gameObject);
    }
}
