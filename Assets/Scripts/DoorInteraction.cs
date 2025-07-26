using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class DoorInteraction : MonoBehaviour
{
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool isOpen = false;
    public float interactionDistance = 2.5f;
    public Transform player;
    public GameObject interactionPrompt;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Coroutine _currentCoroutine;

    public AudioClip openSound;
    public AudioClip closeSound;
    private AudioSource audioSource;

    [Header("Blocco porta con chiave")]
    public Item requiredKey;      // ScriptableObject della chiave (es. ChiavePortaBoss)
    public bool isLocked = false;  // Se la porta parte bloccata
    public bool consumeKeyOnUse = false; // Se vuoi rimuovere la chiave dopo l'uso


    void Start()
    {
        _closedRotation = transform.rotation;
        _openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Mostra/Nasconde il prompt
        if (interactionPrompt != null)
            interactionPrompt.SetActive(distance <= interactionDistance);

        if (distance <= interactionDistance && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isLocked)
            {
                if (PlayerHasRequiredKey())
                {
                    isLocked = false;
                    Debug.Log("Porta sbloccata con la chiave: " + requiredKey.itemName);

                    if (consumeKeyOnUse)
                        RemoveKeyByName(requiredKey.itemName);
                }
                else
                {
                    Debug.Log("Porta bloccata. Chiave mancante: " + (requiredKey != null ? requiredKey.itemName : "Nessuna"));
                    return;
                }
            }

            if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(ToggleDoor());
        }
    }

    private IEnumerator ToggleDoor()
    {
        Quaternion targetRotation = isOpen ? _closedRotation : _openRotation;
        isOpen = !isOpen;

        // Suono apertura o chiusura
        AudioClip clipToPlay = isOpen ? openSound : closeSound;
        if (clipToPlay != null && audioSource != null)
            audioSource.PlayOneShot(clipToPlay);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }
        transform.rotation = targetRotation;
    }

    private bool PlayerHasRequiredKey()
    {
        if (requiredKey == null) return true;
        if (InventoryManager.Instance == null) return false;

        var allGenericItems = InventoryManager.Instance.GetItemsOfCategory(ItemCategory.Generic);

        foreach (var item in allGenericItems)
        {
            if (item != null)
            {
                Debug.Log("Oggetto generico presente nell'inventario: " + item.itemName);
                if (item.itemName == requiredKey.itemName)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void RemoveKeyByName(string keyName)
    {
        var genericItems = InventoryManager.Instance.GetItemsOfCategory(ItemCategory.Generic);
        Debug.Log("genericItem " + genericItems);

        foreach (var item in genericItems)
        {
            if (item != null && item.itemName == keyName)
            {
                InventoryManager.Instance.RemoveItemFromBag(item);
                return;
            }
        }
    }
}
