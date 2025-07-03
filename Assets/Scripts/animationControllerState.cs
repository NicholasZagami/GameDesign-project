﻿using UnityEngine;
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
    public AudioClip hitClip;

    private bool isMoving;
    private bool isAttacking = false;

    public GameObject inventoryUI;
    private bool isInventoryOpen = false;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float damageAmount = 20f;
    public LayerMask enemyLayer;


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
            PerformAttack();

            StartCoroutine(AttackCooldown());
        }
    }

    private IEnumerator AttackCooldown()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.7f); // durata dell'attacco
        isAttacking = false;
    }

    private void PerformAttack()
    {
        float radius = 1f;
        Vector3 attackCenter = transform.position + transform.forward * 1.0f + Vector3.up * 1.2f;


        Collider[] hits = Physics.OverlapSphere(attackCenter, radius, enemyLayer);
        bool didHit = false;

        foreach (Collider hit in hits)
        {
            HealthBar enemyHealth = hit.GetComponent<HealthBar>();
            if (enemyHealth == null)
                enemyHealth = hit.GetComponentInParent<HealthBar>();

            if (enemyHealth != null)
            {
                Vector3 dirToEnemy = (hit.bounds.center - attackCenter);
                float distToEnemy = dirToEnemy.magnitude;

                // Raycast verso il nemico per verificare ostacoli
                if (Physics.Raycast(attackCenter, dirToEnemy.normalized, out RaycastHit hitInfo, distToEnemy))
                {
                    if (hitInfo.collider != hit && !hitInfo.collider.CompareTag("Enemy"))
                    {
                        Debug.Log("Attacco bloccato da ostacolo: " + hitInfo.collider.name);
                        Debug.DrawRay(attackCenter, dirToEnemy.normalized * distToEnemy, Color.red, 1f);
                        continue; // ostacolo in mezzo → non colpisce
                    }
                }

                // Nessun ostacolo → colpisci il nemico
                enemyHealth.TakeDamage(damageAmount);
                Debug.Log("Nemico colpito!");

                if (hitClip != null && weaponAudio != null)
                    weaponAudio.PlayOneShot(hitClip);

                didHit = true;
                break; // colpisce solo un nemico
            }
        }

        // Se non ha colpito nulla
        if (!didHit && attackClip != null && weaponAudio != null)
        {
            weaponAudio.PlayOneShot(attackClip);
        }

        // Ray per debug visuale (verde = tentativo valido)
        Debug.DrawRay(attackCenter, transform.forward * radius, Color.green, 1f);
    }
}
