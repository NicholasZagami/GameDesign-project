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
}
