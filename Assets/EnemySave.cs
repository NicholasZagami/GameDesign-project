using UnityEngine;

public class EnemySave : MonoBehaviour
{
    public string uniqueID; // assegna a mano o genera automaticamente

    public void OnDeath()
    {
        SaveManager.Instance?.RegisterEnemyDefeat(uniqueID);
    }
}
