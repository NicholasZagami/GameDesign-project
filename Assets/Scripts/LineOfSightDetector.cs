using UnityEngine;

public class LineOfSightDetector : MonoBehaviour
{
    [SerializeField]
    private LayerMask m_playerLayerMask;
    [SerializeField]
    private float m_detectionRange = 15.0f;
    [SerializeField]
    private float m_detectionHeight = 3f;

    [SerializeField] private bool showDebugVisuals = true;

    public GameObject PerformDetection(GameObject potentialTarget)
    {
        if (potentialTarget == null)
        {
            return null;
        }

        RaycastHit hit;
        Vector3 startPos = transform.position + Vector3.up * m_detectionHeight;
        Vector3 direction = potentialTarget.transform.position - startPos;
        
        bool hitSomething = Physics.Raycast(startPos, direction, out hit, m_detectionRange, m_playerLayerMask);

        if (hit.collider != null && hit.collider.gameObject == potentialTarget)
        {
            if (showDebugVisuals && this.enabled)
            {
                Debug.DrawLine(startPos, potentialTarget.transform.position, Color.green, 0.1f);
            }
            return hit.collider.gameObject;
        }
        else
        {
            if (showDebugVisuals && this.enabled && potentialTarget != null)
            {
                Debug.DrawLine(startPos, potentialTarget.transform.position, Color.red, 0.1f);
            }
            return null;
        }
    }

    private void OnDrawGizmos()
    {
        if (showDebugVisuals)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + Vector3.up * m_detectionHeight, 0.3f);
        }
    }
}