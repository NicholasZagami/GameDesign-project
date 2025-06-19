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

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        footstepAudio = GetComponent<AudioSource>();
        //weaponAudio = transform.Find("WeaponSocket/Mace").GetComponent<AudioSource>();
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Update()
    {
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
                Debug.Log("Weapon Audio: " + weaponAudio);
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
