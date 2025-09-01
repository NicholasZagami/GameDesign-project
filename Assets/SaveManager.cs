using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string savePath;
    public SaveData CurrentSave { get; private set; } = new SaveData();

    [Header("Known Items (trascina qui TUTTI gli Item ScriptableObject)")]
    public List<Item> knownItems = new();

    private Item GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        for (int i = 0; i < knownItems.Count; i++)
        {
            var it = knownItems[i];
            if (it != null && it.itemId == id) return it;
        }
        return null;
    }

    public bool HasSave()
    {
        return System.IO.File.Exists(System.IO.Path.Combine(Application.persistentDataPath, "save.json"));
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "save.json");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        // Posizione player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            CurrentSave.playerX = pos.x;
            CurrentSave.playerY = pos.y;
            CurrentSave.playerZ = pos.z;
        }

        // inventario
        CurrentSave.inventoryItemIds.Clear();

        var inv = InventoryManager.Instance ?? FindObjectOfType<InventoryManager>();
        if (inv == null)
        {
            Debug.LogWarning("[Save] InventoryManager NON trovato nella scena corrente.");
        }
        else
        {
            int i = 0;
            foreach (var it in inv.GetAllItems())
            {
                // log diagnostico per capire cosa c’è davvero negli slot
                Debug.Log($"[Save] SlotItem #{i}: name='{it.itemName}', id='{it.itemId}'");
                if (string.IsNullOrEmpty(it.itemId))
                    Debug.LogWarning($"[Save] L'item '{it.itemName}' ha itemId VUOTO: non potrà essere ricreato al load.");

                CurrentSave.inventoryItemIds.Add(it.itemId);
                i++;
            }
        }
        Debug.Log($"[Save] Inventory count={CurrentSave.inventoryItemIds.Count}, ids= {string.Join(", ", CurrentSave.inventoryItemIds)}");


        // Scena corrente
        CurrentSave.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        string json = JsonUtility.ToJson(CurrentSave, true);
        File.WriteAllText(savePath, json);

        Debug.Log("Game salvato in: " + savePath);
        Debug.Log("[Save] Inventory items: " + string.Join(", ", CurrentSave.inventoryItemIds));
    }

    public void LoadGame()
    {
        Debug.Log("[Save] LoadGame() chiamato");

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Nessun salvataggio trovato!");
            return;
        }

        string json = File.ReadAllText(savePath);
        CurrentSave = JsonUtility.FromJson<SaveData>(json);

        // Carica scena
        UnityEngine.SceneManagement.SceneManager.LoadScene(CurrentSave.sceneName);

        // Applica dopo il load
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        StartCoroutine(ApplyAfterLoad());
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private System.Collections.IEnumerator ApplyAfterLoad()
    {
        Debug.Log("[Save] ApplyAfterLoad avviata");

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // 1) Attendi che il Player esista (se spawn ritardato)
        GameObject player = null;
        for (int i = 0; i < 300 && player == null; i++) // ~5s a 60fps
        {
            player = GameObject.FindGameObjectWithTag("Player");
            yield return null;
        }

        // 2) Posiziona il player e riabilita i componenti chiave
        if (player != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;
            player.transform.position = new Vector3(CurrentSave.playerX, CurrentSave.playerY, CurrentSave.playerZ);
            if (cc) cc.enabled = true;

            // Stop a eventuali derive fisiche
            var rb = player.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
            }

            // Se usi il New Input System, riabilita l�input (in caso fosse stato disabilitato dalla pausa)
            var pi = player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (pi && !pi.enabled) pi.enabled = true;
        }

        // Aspetta InventoryManager
        InventoryManager inv = null;
        for (int i = 0; i < 300 && inv == null; i++)
        {
            inv = InventoryManager.Instance;
            yield return null;
        }

        // Ora ricostruisci inventario
        if (inv != null)
        {
            inv.ClearAll();
            foreach (var id in CurrentSave.inventoryItemIds)
            {
                var item = GetById(id); // cerca tra knownItems
                if (item != null)
                {
                    inv.AddItemToBag(item);
                    Debug.Log($"[Load] aggiunto item {id} -> {item.itemName}");
                }
                else
                {
                    Debug.LogWarning($"[Load] itemId {id} non trovato nei knownItems");
                }
            }
        }
        Debug.Log("[Load] Ricostruzione inventory, ids salvati: " + string.Join(", ", CurrentSave.inventoryItemIds));

        // 4) Rimuovi nemici gi� uccisi
        foreach (var enemy in FindObjectsOfType<EnemySave>())
            if (CurrentSave.defeatedEnemies.Contains(enemy.uniqueID))
                Destroy(enemy.gameObject);

        // 5) Rimuovi oggetti gi� raccolti (istanze di scena, es. pozioni)
        foreach (var item in FindObjectsOfType<ItemSave>())
            if (CurrentSave.collectedItems.Contains(item.uniqueID))
                Destroy(item.gameObject);

        // 6) I bauli si autoregolano in OnEnable/Start leggendo IsWorldEventCompleted(...)
    }

    public void RegisterEnemyDefeat(string enemyID)
    {
        if (!CurrentSave.defeatedEnemies.Contains(enemyID))
            CurrentSave.defeatedEnemies.Add(enemyID);
    }

    public void RegisterItemCollected(string itemID)
    {
        if (!CurrentSave.collectedItems.Contains(itemID))
            CurrentSave.collectedItems.Add(itemID);
    }

    public void ClearSave()
    {
        if (File.Exists(savePath))
            File.Delete(savePath);

        CurrentSave = new SaveData();
    }

    // SaveManager.cs (aggiungi questi metodi)
    public void RegisterWorldEventCompleted(string eventId)
    {
        if (!string.IsNullOrEmpty(eventId) && !CurrentSave.completedWorldEvents.Contains(eventId))
            CurrentSave.completedWorldEvents.Add(eventId);
    }

    public bool IsWorldEventCompleted(string eventId)
    {
        return !string.IsNullOrEmpty(eventId) && CurrentSave.completedWorldEvents.Contains(eventId);
    }

    public void RegisterDoorOpened(string doorId)
    {
        if (!string.IsNullOrEmpty(doorId) && !CurrentSave.openedDoors.Contains(doorId))
            CurrentSave.openedDoors.Add(doorId);
    }
    public void UnregisterDoorOpened(string doorId)
    {
        if (!string.IsNullOrEmpty(doorId))
            CurrentSave.openedDoors.Remove(doorId);
    }
    public bool IsDoorOpened(string doorId)
    {
        return !string.IsNullOrEmpty(doorId) && CurrentSave.openedDoors.Contains(doorId);
    }
}
