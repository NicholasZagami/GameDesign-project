using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuActions : MonoBehaviour
{
    [Header("UI (opzionali)")]
    public Button continueButton;   // trascina il bottone "Continua/Carica"

    void Start()
    {
        // Assicurati che il SaveManager esista e abilita/disabilita il bottone
        if (continueButton != null)
            continueButton.interactable = (SaveManager.Instance != null && SaveManager.Instance.HasSave());
    }

    public void NewGame(string firstSceneName) // es. "Tutorial" o "FirstLevel"
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.ClearSave(); // pulisci eventuali salvataggi vecchi
        Time.timeScale = 1f; // nel dubbio
        SceneManager.LoadScene(firstSceneName);
    }

    public void ContinueGame()
    {
        if (SaveManager.Instance == null) { Debug.LogWarning("[Menu] SaveManager mancante"); return; }
        if (!SaveManager.Instance.HasSave()) { Debug.LogWarning("[Menu] Nessun salvataggio"); return; }

        Debug.Log("[Menu] Continua premuto: carico dal file…");
        SaveManager.Instance.LoadGame();
    }
}