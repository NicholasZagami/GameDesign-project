using System.Collections;
using UnityEngine;

public class LootOpening : MonoBehaviour
{
    [Header("Movimento")]
    public Vector3 offset = new Vector3(0, -4f, 0); // movimento standard
    public float duration = 2f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool isOpening = false;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + offset;
    }

    public void OpenWall()
    {
        if (!isOpening)
        {
            StartCoroutine(MoveWall());
            isOpening = true;
        }
    }

    private IEnumerator MoveWall()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;

        // Disattiva completamente il muro
        gameObject.SetActive(false);
    }
}
