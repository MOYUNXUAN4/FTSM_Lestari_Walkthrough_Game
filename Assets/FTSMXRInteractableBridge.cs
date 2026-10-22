using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(FTSMInteractable))]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public sealed class FTSMXRInteractableBridge : MonoBehaviour
{
    private FTSMInteractable ftsmInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable xrInteractable;

    private void Awake()
    {
        ftsmInteractable = GetComponent<FTSMInteractable>();
        xrInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
    }

    private void OnEnable()
    {
        if (xrInteractable == null)
            xrInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        xrInteractable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDisable()
    {
        if (xrInteractable != null)
            xrInteractable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (ftsmInteractable == null)
            ftsmInteractable = GetComponent<FTSMInteractable>();
        ftsmInteractable.Interact();
    }
}
