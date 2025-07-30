using UnityEngine;

public class CameraModeSwitcher : MonoBehaviour
{
    public Transform cameraTransform;

    [Header("Offset solo per prima persona")]
    public Transform firstPersonView; // ← nuovo: empty davanti al viso
    public float transitionSpeed = 8f;

    private bool isFirstPerson = false;
    private Vector3 thirdPersonOriginalLocalPos;
    private Vector3 targetOffset;

    void Start()
    {
        // Salva la posizione iniziale della camera (terza persona)
        thirdPersonOriginalLocalPos = cameraTransform.localPosition;
        targetOffset = thirdPersonOriginalLocalPos;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;

            if (isFirstPerson)
                targetOffset = firstPersonView.localPosition; // ← usa il Transform come riferimento
            else
                targetOffset = thirdPersonOriginalLocalPos;
        }

        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            targetOffset,
            Time.deltaTime * transitionSpeed
        );
    }
}