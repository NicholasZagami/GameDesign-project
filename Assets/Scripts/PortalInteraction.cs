using UnityEngine;

public class PortalInteraction : MonoBehaviour
{
    [Header("UI")]
    public GameObject interactionPrompt; // UI da mostrare (es: "Premi E per interagire")

    [Header("Scene Management")]
    public SceneManagement sceneManager; // Script da assegnare via Inspector
    public int sceneIndexToLoad = 1;     // Indice della scena da caricare

    private bool isPlayerInRange = false;

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (interactionPrompt != null)
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
}