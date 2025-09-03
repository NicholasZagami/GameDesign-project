using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class DoorInteraction : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("ID UNIVOCO porta, es: L1_Door_A")]
    public string uniqueID;

    [Header("Apertura")]
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool isOpen = false;

    [Header("Interazione")]
    public float interactionDistance = 2.5f;
    public Transform player;
    public GameObject interactionPrompt;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Coroutine _currentCoroutine;

    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockedSound;
    private AudioSource audioSource;

    [Header("Blocco porta con chiave")]
    public Item requiredKey; // ScriptableObject della chiave
    public bool isLocked = false; // Se la porta parte bloccata
    public bool consumeKeyOnUse = false; // Se vuoi rimuovere la chiave dopo l'uso

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // Setup rotazioni
        _closedRotation = transform.rotation;
        _openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));

        // Player fallback
        if (player == null)
        {
            var pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj) player = pObj.transform;
        }

        // Applica stato salvato (se presente)
        if (SaveManager.Instance != null && !string.IsNullOrEmpty(uniqueID))
        {
            bool wasOpen = SaveManager.Instance.IsDoorOpened(uniqueID);
            if (wasOpen)
            {
                // Se la porta era aperta, assicurati anche di sbloccarla
                isLocked = false;
                SetOpenImmediate(true);
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Prompt
        if (interactionPrompt != null)
            interactionPrompt.SetActive(distance <= interactionDistance);

        // Interazione
        if (distance <= interactionDistance && Keyboard.current.eKey.wasPressedThisFrame)
        {
            // Se è bloccata, verifica chiave
            if (isLocked)
            {
                if (PlayerHasRequiredKey())
                {
                    isLocked = false;
                    Debug.Log("Porta sbloccata con la chiave: " +
                              (requiredKey ? requiredKey.itemId : "(no id)"));

                    if (consumeKeyOnUse && requiredKey != null)
                        RemoveKeyById(requiredKey.itemId);
                }
                else
                {
                    Debug.Log("Porta bloccata. Chiave mancante: " +
                              (requiredKey ? requiredKey.itemId : "Nessuna"));

                    if (lockedSound != null && audioSource != null)
                        audioSource.PlayOneShot(lockedSound);

                    return;
                }
            }

            if (_currentCoroutine != null)
                StopCoroutine(_currentCoroutine);

            _currentCoroutine = StartCoroutine(ToggleDoor());
        }
    }

    private IEnumerator ToggleDoor()
    {
        // target = stato opposto
        bool targetOpen = !isOpen;
        Quaternion targetRotation = targetOpen ? _openRotation : _closedRotation;

        // Audio
        AudioClip clipToPlay = targetOpen ? openSound : closeSound;
        if (clipToPlay != null && audioSource != null)
            audioSource.PlayOneShot(clipToPlay);

        // Animazione
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                Time.unscaledDeltaTime * openSpeed
            );
            yield return null;
        }

        transform.rotation = targetRotation;
        isOpen = targetOpen;

        // Persistenza stato
        if (!string.IsNullOrEmpty(uniqueID) && SaveManager.Instance != null)
        {
            if (isOpen)
                SaveManager.Instance.RegisterDoorOpened(uniqueID);
            else
                SaveManager.Instance.UnregisterDoorOpened(uniqueID);
        }
    }

    /// <summary>
    /// Applica lo stato immediatamente (senza animazione), usato al Load.
    /// </summary>
    public void SetOpenImmediate(bool open)
    {
        if (_closedRotation == Quaternion.identity && _openRotation == Quaternion.identity)
        {
            // Nel raro caso Start non sia ancora passato
            _closedRotation = transform.rotation;
            _openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
        }

        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
            _currentCoroutine = null;
        }

        transform.rotation = open ? _openRotation : _closedRotation;
        isOpen = open;

        // Allinea la persistenza (utile se chiamato manualmente)
        if (!string.IsNullOrEmpty(uniqueID) && SaveManager.Instance != null)
        {
            if (isOpen)
                SaveManager.Instance.RegisterDoorOpened(uniqueID);
            else
                SaveManager.Instance.UnregisterDoorOpened(uniqueID);
        }
    }

    private bool PlayerHasRequiredKey()
    {
        if (requiredKey == null) return true; // nessuna chiave richiesta
        if (InventoryManager.Instance == null) return false;

        // Cerca per itemId, non per nome!
        var items = InventoryManager.Instance.GetItemsOfCategory(ItemCategory.Generic);
        foreach (var it in items)
            if (it != null && it.itemId == requiredKey.itemId)
                return true;

        return false;
    }

    private void RemoveKeyById(string keyId)
    {
        if (string.IsNullOrEmpty(keyId) || InventoryManager.Instance == null) return;

        var items = InventoryManager.Instance.GetItemsOfCategory(ItemCategory.Generic);
        foreach (var it in items)
        {
            if (it != null && it.itemId == keyId)
            {
                InventoryManager.Instance.RemoveItemFromBag(it);
                return;
            }
        }
    }
}