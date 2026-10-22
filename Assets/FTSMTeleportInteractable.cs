using UnityEngine;

public sealed class FTSMTeleportInteractable : MonoBehaviour
{
    [SerializeField] private string displayName = "Teleport";
    [SerializeField] private float forwardOffset = 1800f;
    [SerializeField] private float verticalOffset;
    [SerializeField] private bool useColliderSurface = true;

    public string DisplayName => displayName;

    public bool TryGetTeleportPosition(float currentY, out Vector3 targetPosition)
    {
        Vector3 anchor = transform.position;

        if (useColliderSurface)
        {
            Collider hitCollider = GetComponent<Collider>();
            if (hitCollider == null)
                hitCollider = GetComponentInChildren<Collider>();
            if (hitCollider != null)
                anchor = hitCollider.bounds.center;
        }

        Vector3 flatForward = transform.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude < 0.0001f)
            flatForward = Vector3.forward;
        else
            flatForward.Normalize();

        targetPosition = anchor - flatForward * forwardOffset;
        targetPosition.y = currentY + verticalOffset;
        return true;
    }
}
