using UnityEngine;

public class EnemyAudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Idle & Walk Clips")]
    public AudioClip[] ambientClips;
    public float ambientMinDelay = 5f;
    public float ambientMaxDelay = 12f;

    [Header("Attack Clips")]
    public AudioClip[] attackClips;

    [Header("Alert Clip")]
    public AudioClip alertClip;
    private bool hasPlayedAlert = false;

    private float nextAmbientTime;

    private Animator animator;
    private bool lastAttackState = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        ScheduleNextAmbient();
    }

    void Update()
    {
        // Suono ambientale casuale
        if (Time.time >= nextAmbientTime)
        {
            PlayRandomAmbient();
            ScheduleNextAmbient();
        }

        // Suono attacco se "attack" è diventato true
        bool isAttacking = animator.GetBool("Attack");
        if (isAttacking && !lastAttackState)
        {
            PlayAttackSound();
        }
        lastAttackState = isAttacking;
    }

    private void ScheduleNextAmbient()
    {
        nextAmbientTime = Time.time + Random.Range(ambientMinDelay, ambientMaxDelay);
    }

    private void PlayRandomAmbient()
    {
        if (ambientClips.Length > 0)
        {
            AudioClip clip = ambientClips[Random.Range(0, ambientClips.Length)];
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayAttackSound()
    {
        if (attackClips.Length > 0)
        {
            AudioClip clip = attackClips[Random.Range(0, attackClips.Length)];
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayAlertSound()
    {
        if (!hasPlayedAlert && alertClip != null)
        {
            audioSource.PlayOneShot(alertClip);
            hasPlayedAlert = true;
        }
    }

    public void ResetAlert()
    {
        hasPlayedAlert = false;
    }
}