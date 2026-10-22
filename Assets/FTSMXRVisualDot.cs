using UnityEngine;

public sealed class FTSMXRVisualDot : MonoBehaviour
{
    [SerializeField] private Transform targetHand;
    [SerializeField] private LayerMask interactMask;
    [SerializeField] private float rayDistance = 18f;
    [SerializeField] private float visibleScale = 180f;
    [SerializeField] private float hiddenScale = 0.001f;
    [SerializeField] private Color idleColor = Color.white;
    [SerializeField] private Color hitColor = Color.green;

    private Renderer cachedRenderer;
    private MaterialPropertyBlock propertyBlock;
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");
    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        transform.localScale = Vector3.one * hiddenScale;
    }

    private void LateUpdate()
    {
        if (targetHand == null)
        {
            Hide();
            return;
        }

        Ray ray = new Ray(targetHand.position, targetHand.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactMask, QueryTriggerInteraction.Collide))
        {
            Hide();
            return;
        }

        transform.position = hit.point + hit.normal * 6f;
        transform.rotation = Quaternion.LookRotation(hit.normal);
        transform.localScale = Vector3.one * visibleScale;

        Transform interactableRoot = GetInteractableRoot(hit.collider);
        ApplyColor(interactableRoot != null ? hitColor : idleColor);
    }

    private void Hide()
    {
        transform.localScale = Vector3.one * hiddenScale;
    }

    private void ApplyColor(Color color)
    {
        if (cachedRenderer == null)
            return;

        cachedRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(ColorProperty, color);
        propertyBlock.SetColor(BaseColorProperty, color);
        cachedRenderer.SetPropertyBlock(propertyBlock);
    }

    private static Transform GetInteractableRoot(Collider hitCollider)
    {
        FTSMTeleportInteractable teleportInteractable = hitCollider.GetComponentInParent<FTSMTeleportInteractable>();
        if (teleportInteractable != null)
            return teleportInteractable.transform;

        FTSMRoomInteractable roomInteractable = hitCollider.GetComponentInParent<FTSMRoomInteractable>();
        if (roomInteractable != null)
            return roomInteractable.transform;

        FTSMInteractable interactable = hitCollider.GetComponentInParent<FTSMInteractable>();
        return interactable != null ? interactable.transform : null;
    }
}
