using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
    public Transform cameraRoot; // Assegna PlayerCameraRoot da Inspector
    public float speed = 2f;
    public float mouseSensitivity = 5f;

    private CharacterController controller;
    private Animator animator;
    private InputSystem_Actions inputActions;
    private Vector2 mouseDelta;
    private float cameraPitch = 0f;

    private AudioSource footstepAudio;
    private AudioSource weaponAudio;
    public AudioClip attackClip;
    private bool isMoving;
    private bool isAttacking = false;

    public GameObject inventoryUI;
    private bool isInventoryOpen = false;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        footstepAudio = GetComponent<AudioSource>();
        Transform maceTransform = transform.Find("root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/WeaponSocket/Mace");

        if (maceTransform == null)
        {
            Debug.LogError("Mace non trovato! Controlla il percorso.");
        }
        else
        {
            weaponAudio = maceTransform.GetComponent<AudioSource>();
            if (weaponAudio == null)
                Debug.LogError("AudioSource mancante su Mace!");
        }
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Update()
    {
        // Apertura/chiusura inventario
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            isInventoryOpen = !isInventoryOpen;
            inventoryUI.SetActive(isInventoryOpen);

            if (isInventoryOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        // Se l'inventario è aperto, non permettere movimenti né rotazione
        if (isInventoryOpen)
        {
            animator.SetBool("isMoving", false);
            if (footstepAudio.isPlaying) footstepAudio.Stop();
            return;
        }

        // --- Input Mouse Look ---
        mouseDelta = inputActions.Player.Look.ReadValue<Vector2>();
        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -45f, 75f);
        cameraRoot.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);

        // --- Movimento ---
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        isMoving = moveInput.magnitude > 0.1f;

        if (!isAttacking)
        {
            controller.Move(move * speed * Time.deltaTime);
        }

        animator.SetBool("isMoving", isMoving && !isAttacking);

        // Audio passi
        if (isMoving && !footstepAudio.isPlaying && !isAttacking)
            footstepAudio.Play();
        else if ((!isMoving || isAttacking) && footstepAudio.isPlaying)
            footstepAudio.Stop();

        // --- Attacco ---
        if (inputActions.Player.Attack.triggered && !isAttacking)
        {
            animator.SetTrigger("Attack");

            if (attackClip != null && weaponAudio != null)
            {
                weaponAudio.PlayOneShot(attackClip);
            }

            StartCoroutine(AttackCooldown());
        }
    }

    private IEnumerator AttackCooldown()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.7f); // durata dell'attacco
        isAttacking = false;
    }
}
