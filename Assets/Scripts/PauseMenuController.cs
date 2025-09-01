using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuUI;

    [Header("Input System")]
    public PlayerInput playerInput;          // trascina il PlayerInput del player
    [Tooltip("Nome della action map usata per giocare (es. 'Player', 'Default'...)")]
    public string gameplayActionMap = "Player";   // <-- metti qui il NOME VERO della tua mappa di gioco
    [Tooltip("Nome della action map per l'UI (di solito 'UI')")]
    public string uiActionMap = "UI";

    [Header("Bloccare in pausa (opzionale ma consigliato)")]
    public Behaviour[] scriptsToDisable;     // es. PlayerMovement, MouseLook, ecc.

    private bool isPaused = false;
    private string previousActionMap = null;

    void Awake()
    {
        if (!playerInput) playerInput = FindAnyObjectByType<PlayerInput>();
        if (pauseMenuUI) pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI?.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isPaused = false;

        // riattiva script di gameplay
        foreach (var s in scriptsToDisable) if (s) s.enabled = true;

        // torna alla action map precedente (se c'era), altrimenti a gameplayActionMap
        if (playerInput != null)
        {
            string target = !string.IsNullOrEmpty(previousActionMap) ? previousActionMap : gameplayActionMap;
            TrySwitchActionMap(target);
        }
    }

    void Pause()
    {
        pauseMenuUI?.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isPaused = true;

        // disattiva script di gameplay (rete di sicurezza)
        foreach (var s in scriptsToDisable) if (s) s.enabled = false;

        // passa all'UI (salva da dove arrivi)
        if (playerInput != null)
        {
            previousActionMap = playerInput.currentActionMap != null ? playerInput.currentActionMap.name : null;
            TrySwitchActionMap(uiActionMap);
        }
    }

    private void TrySwitchActionMap(string mapName)
    {
        if (playerInput == null || string.IsNullOrEmpty(mapName)) return;

        var asset = playerInput.actions;
        if (asset == null) return;

        var map = asset.FindActionMap(mapName, throwIfNotFound: false);
        if (map != null)
        {
            playerInput.SwitchCurrentActionMap(mapName);
            // Debug.Log($"[Pause] Switched to action map: {mapName}");
        }
        else
        {
            // Se il nome è sbagliato, stampa le mappe disponibili e lascia attivi i fallback (scriptsToDisable)
            Debug.LogWarning($"[Pause] Action map '{mapName}' non trovata. Mappe disponibili: {ListActionMaps(asset)}");
        }
    }

    private string ListActionMaps(InputActionAsset asset)
    {
        if (asset == null) return "(nessuna)";
        var names = "";
        foreach (var m in asset.actionMaps)
        {
            if (!string.IsNullOrEmpty(names)) names += ", ";
            names += $"'{m.name}'";
        }
        return names;
    }
}