using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class Tutorial : MonoBehaviour
{
    [Header("Tutorial Steps")]
    public List<GameObject> tutorialPanels; // Lista dei pannelli
    public Button continueButton;

    [Header("Player Control")]
    public PlayerMovement playerMovement;
    public Transform cameraRoot;

    [Header("Trigger Settings")]
    public Collider attackTriggerZone;

    [Header("Trigger stanza attacco")]
    public Transform attackTriggerPoint; // assegnalo nell'Inspector
    public float triggerRadius = 2.5f;
    private bool attackPanelShown = false;

    [Header("Trigger stanza interazione")]
    public Transform interactTriggerPoint;
    public float interactTriggerRadius = 2.5f;
    private bool interactionPanelShown = false;


    [Header("Audio passi")]
    public AudioSource footstepAudio;  // assegna da Inspector



    private int currentPanelIndex = 0;
    private bool isPlayerTryingMovement = false;
    private bool canCheckMovement = false;
    private bool attackTutorialTriggered = false;

    void Start()
    {
        DisableAllPanels();
        ShowCurrentPanel();

        if (playerMovement != null)
            playerMovement.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (attackTriggerZone != null)
            attackTriggerZone.isTrigger = true;

        StartCoroutine(EnableMovementCheck());
    }

    void Update()
    {
        if (canCheckMovement && currentPanelIndex == 0 && !isPlayerTryingMovement)
        {
            bool inputDetected = Keyboard.current.wKey.isPressed ||
                                 Keyboard.current.aKey.isPressed ||
                                 Keyboard.current.sKey.isPressed ||
                                 Keyboard.current.dKey.isPressed;

            if (inputDetected)
            {
                isPlayerTryingMovement = true;
                continueButton.interactable = true;
            }
        }

        if (!attackPanelShown && currentPanelIndex == 1 && attackTriggerPoint != null)
        {
            float distance = Vector3.Distance(playerMovement.transform.position, attackTriggerPoint.position);
            if (distance <= triggerRadius)
            {
                attackPanelShown = true;
                ShowCurrentPanel();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                playerMovement.enabled = false;
                Debug.Log("▶️ Trigger attacco attivato, pannello mostrato");
            }
        }

        // ▶️ Trigger terza stanza: interazione con oggetti
        if (!interactionPanelShown && currentPanelIndex == 2 && interactTriggerPoint != null)
        {
            float distance = Vector3.Distance(playerMovement.transform.position, interactTriggerPoint.position);
            if (distance <= interactTriggerRadius)
            {
                interactionPanelShown = true;
                ShowCurrentPanel();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                playerMovement.enabled = false;
                Debug.Log("▶️ Trigger interazione attivato, pannello mostrato");
            }
        }
    }

    public void OnContinueClicked()
    {
        Debug.Log("OnContinueClicked chiamato");

        tutorialPanels[currentPanelIndex].SetActive(false);

        // Caso speciale: pannello 0 (movimento)
        if (currentPanelIndex == 0)
        {
            currentPanelIndex++; // Passa logicamente allo step 1
            EnablePlayerTemporarily();
            return;
        }

        // Caso speciale: pannello 1 (attacco)
        if (currentPanelIndex == 1)
        {
            currentPanelIndex++; // Passa logicamente allo step 2 (interazione)
            EnablePlayerTemporarily();
            return;
        }

        // Altri pannelli normali
        currentPanelIndex++;

        if (currentPanelIndex >= tutorialPanels.Count)
        {
            EndTutorial();
        }
        else
        {
            ShowCurrentPanel();
            continueButton.interactable = false;
        }
    }

    private void EnablePlayerTemporarily()
    {
        if (playerMovement != null)
            playerMovement.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ShowCurrentPanel()
    {
        tutorialPanels[currentPanelIndex].SetActive(true);

        if (playerMovement != null)
        {
            playerMovement.enabled = false;

            Animator anim = playerMovement.GetComponent<Animator>();
            if (anim != null)
                anim.SetBool("isMoving", false);
        }

        if (footstepAudio != null && footstepAudio.isPlaying)
            footstepAudio.Stop();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void DisableAllPanels()
    {
        foreach (var panel in tutorialPanels)
            panel.SetActive(false);
    }

    private IEnumerator EnableMovementCheck()
    {
        yield return new WaitForSeconds(1.5f);
        canCheckMovement = true;
    }

    private void EndTutorial()
    {
        if (playerMovement != null)
            playerMovement.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger entrato da: {other.name}");

        if (attackTutorialTriggered) return;

        if (attackTriggerZone != null && other.CompareTag("Player") && currentPanelIndex == 1)
        {
            Debug.Log("Trigger stanza 2 rilevato, pannello attacco in arrivo");
            attackTutorialTriggered = true;
            ShowCurrentPanel(); // Mostra pannello attacco (index 1)
        }
    }

}
