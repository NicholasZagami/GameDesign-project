using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuActions : MonoBehaviour
{
    public void Save()
    {
        if (SaveManager.Instance == null) { Debug.LogWarning("SaveManager mancante."); return; }
        SaveManager.Instance.SaveGame();
        Debug.Log("[UI] Salvataggio richiesto");
    }

    public void Load()
    {
        if (SaveManager.Instance == null) { Debug.LogWarning("SaveManager mancante."); return; }
        SaveManager.Instance.LoadGame();
        Debug.Log("[UI] Caricamento richiesto");
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // metti il nome esatto della tua scena Menu
    }
}