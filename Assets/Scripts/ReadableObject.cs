using UnityEngine;

public class ReadableObject : MonoBehaviour
{
    [Header("UI")]
    public GameObject readableCanvas;     // Pannello del libro
    public GameObject interactionPrompt;  // Scritta "Premi E per interagire"

    private bool isPlayerInRange = false;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            bool isActive = readableCanvas.activeSelf;
            readableCanvas.SetActive(!isActive);

            // Nascondi il prompt quando il libro è aperto
            if (interactionPrompt != null)
                interactionPrompt.SetActive(isActive == true); // Se stava leggendo, lo nasconde
        }

        if (readableCanvas.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            readableCanvas.SetActive(false);

            // Rendi di nuovo visibile il prompt
            if (isPlayerInRange && interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            // Mostra il prompt solo se il libro non è già visibile
            if (interactionPrompt != null && !readableCanvas.activeSelf)
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

            if (readableCanvas.activeSelf)
                readableCanvas.SetActive(false);
        }
    }
}