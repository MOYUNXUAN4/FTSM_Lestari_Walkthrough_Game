using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(FTSMTeleportInteractable))]
[RequireComponent(typeof(XRSimpleInteractable))]
public sealed class FTSMXRTeleportInteractableBridge : MonoBehaviour
{
    [SerializeField] private Transform xrRigRoot;

    private FTSMTeleportInteractable teleportInteractable;
    private XRSimpleInteractable xrInteractable;
    private CharacterController xrCharacterController;

    private void Awake()
    {
        teleportInteractable = GetComponent<FTSMTeleportInteractable>();
        xrInteractable = GetComponent<XRSimpleInteractable>();
        CacheRigReferences();
    }

    private void OnEnable()
    {
        if (xrInteractable == null)
            xrInteractable = GetComponent<XRSimpleInteractable>();
        xrInteractable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDisable()
    {
        if (xrInteractable != null)
            xrInteractable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (teleportInteractable == null)
            teleportInteractable = GetComponent<FTSMTeleportInteractable>();

        CacheRigReferences();
        if (teleportInteractable == null || xrRigRoot == null)
            return;

        float currentY = xrRigRoot.position.y;
        if (!teleportInteractable.TryGetTeleportPosition(currentY, out Vector3 targetPosition))
            return;

        bool hadController = xrCharacterController != null && xrCharacterController.enabled;
        if (hadController)
            xrCharacterController.enabled = false;

        xrRigRoot.position = targetPosition;

        if (hadController)
            xrCharacterController.enabled = true;
    }

    private void CacheRigReferences()
    {
        if (xrRigRoot == null)
        {
            GameObject xrRig = GameObject.Find("XR Origin (XR Rig)");
            if (xrRig != null)
                xrRigRoot = xrRig.transform;
        }

        if (xrRigRoot != null && xrCharacterController == null)
            xrCharacterController = xrRigRoot.GetComponent<CharacterController>();
    }
}
