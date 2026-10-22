using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(FTSMRoomInteractable))]
[RequireComponent(typeof(XRSimpleInteractable))]
public sealed class FTSMXRRoomInteractableBridge : MonoBehaviour
{
    private FTSMRoomInteractable roomInteractable;
    private XRSimpleInteractable xrInteractable;

    private void Awake()
    {
        roomInteractable = GetComponent<FTSMRoomInteractable>();
        xrInteractable = GetComponent<XRSimpleInteractable>();
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
        if (roomInteractable == null)
            roomInteractable = GetComponent<FTSMRoomInteractable>();
        roomInteractable.Interact();
    }
}
