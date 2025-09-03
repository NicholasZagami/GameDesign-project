using UnityEngine;

public class PortalInteraction : MonoBehaviour
{
    [Header("UI")]
    public GameObject interactionPrompt; 

    [Header("Scene Management")]
    public SceneManagement sceneManager; 
    public int sceneIndexToLoad = 1;     

    [Header("Final Portal")]
    public bool isFinalPortal = false;   
    public GameObject finalLorePanel;    
    public bool pauseOnFinalPanel = true;         
    public bool unlockCursorOnFinalPanel = true;  
    public int mainMenuSceneIndex = 0;           

    private bool isPlayerInRange = false;
    private bool hasActivated = false; 

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (finalLorePanel != null)
            finalLorePanel.SetActive(false);
    }

    void Update()
    {
        if (isPlayerInRange && !hasActivated && Input.GetKeyDown(KeyCode.E))
        {
            hasActivated = true;

            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);

            if (isFinalPortal)
            {
                // Mostra pannello finale invece di cambiare scena
                if (finalLorePanel != null)
                    finalLorePanel.SetActive(true);

                if (pauseOnFinalPanel) Time.timeScale = 0f;

                if (unlockCursorOnFinalPanel)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
            else
            {
                // Portale "normale" → vai alla scena successiva
                if (sceneManager != null)
                {
                    sceneManager.LoadSceneByIndex(sceneIndexToLoad);
                }
                else
                {
                    Debug.LogWarning("SceneManagement non assegnato al portale.");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (!hasActivated && interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }

    // Bottone: "Torna al menu"
    public void ReturnToMainMenu()
    {
        // Ripristina tempo/cursore prima del cambio scena
        if (pauseOnFinalPanel) Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (sceneManager != null)
        {
            sceneManager.LoadSceneByIndex(mainMenuSceneIndex);
        }
        else
        {
            Debug.LogWarning("SceneManagement non assegnato. Impossibile tornare al menu.");
        }
    }

    // (Opzionale) Bottone: "Chiudi pannello"
    public void CloseFinalPanel()
    {
        if (finalLorePanel != null)
            finalLorePanel.SetActive(false);

        if (pauseOnFinalPanel) Time.timeScale = 1f;

        // Permetti una nuova attivazione se desideri
        hasActivated = false;

        // Se il player è ancora nell’area, ri-mostra il prompt
        if (isPlayerInRange && interactionPrompt != null)
            interactionPrompt.SetActive(true);
    }
}