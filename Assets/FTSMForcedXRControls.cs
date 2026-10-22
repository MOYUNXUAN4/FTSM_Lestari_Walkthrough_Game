using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public sealed class FTSMForcedXRControls : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera xrCamera;
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;
    [SerializeField] private LineRenderer leftTeleportLine;
    [SerializeField] private LineRenderer rightInteractLine;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float rayDistance = 18f;
    [SerializeField] private LayerMask teleportMask;
    [SerializeField] private LayerMask interactMask;

    private InputAction moveAction;
    private InputAction leftTriggerAction;
    private InputAction rightTriggerAction;
    private Vector3 teleportTarget;
    private bool hasTeleportTarget;
    private bool wasLeftTriggerPressed;
    private Transform highlightedInteractable;
    private FTSMInteractionPromptUI interactionPrompt;

    private void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        if (xrCamera == null)
            xrCamera = GetComponentInChildren<Camera>();

        Canvas promptCanvas = xrCamera != null ? xrCamera.GetComponentInChildren<Canvas>(true) : null;
        interactionPrompt = FTSMInteractionPromptUI.GetOrCreate(promptCanvas);

        moveAction = new InputAction("FTSM Move", InputActionType.Value, expectedControlType: "Vector2");
        moveAction.AddBinding("<XRController>{LeftHand}/thumbstick");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        leftTriggerAction = new InputAction("FTSM Left Teleport Trigger", InputActionType.Button);
        leftTriggerAction.AddBinding("<XRController>{LeftHand}/triggerPressed");
        leftTriggerAction.AddBinding("<Mouse>/rightButton");

        rightTriggerAction = new InputAction("FTSM Right Interact Trigger", InputActionType.Button);
        rightTriggerAction.AddBinding("<XRController>{RightHand}/triggerPressed");
        rightTriggerAction.AddBinding("<Mouse>/leftButton");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        leftTriggerAction.Enable();
        rightTriggerAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        leftTriggerAction.Disable();
        rightTriggerAction.Disable();
        FTSMAudioManager.StopWalk();
        UpdateHighlight(null);
        UpdateInteractionPrompt(null);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && FTSMStartMenu.TryTogglePause())
            return;

        if (FTSMStartMenu.Instance != null && FTSMStartMenu.Instance.IsPauseOpen)
            return;

        Move();
        UpdateTeleportRay();
        UpdateInteractRay();

        bool leftPressed = leftTriggerAction.IsPressed();
        if (wasLeftTriggerPressed && !leftPressed && hasTeleportTarget)
            Teleport();
        wasLeftTriggerPressed = leftPressed;

        if (rightTriggerAction.WasPressedThisFrame())
            TryInteract();
    }

    private void Move()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        if (input.sqrMagnitude > 1f)
            input.Normalize();

        Vector3 forward = xrCamera.transform.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 right = xrCamera.transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 motion = (forward * input.y + right * input.x) * moveSpeed;
        motion.y = 0f;
        characterController.Move(motion * Time.deltaTime);
        FTSMAudioManager.ReportWalk(input.sqrMagnitude > 0.0001f && characterController.velocity.sqrMagnitude > 0.01f);
    }

    private void UpdateTeleportRay()
    {
        bool active = leftTriggerAction.IsPressed();
        if (leftTeleportLine != null)
            leftTeleportLine.enabled = active;
        hasTeleportTarget = false;
        if (!active || leftHand == null)
            return;

        Ray ray = new Ray(leftHand.position, leftHand.forward);
        Vector3 end = ray.origin + ray.direction * rayDistance;
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, teleportMask, QueryTriggerInteraction.Ignore))
        {
            end = hit.point;
            teleportTarget = hit.point;
            hasTeleportTarget = true;
        }

        if (leftTeleportLine != null)
        {
            leftTeleportLine.SetPosition(0, ray.origin);
            leftTeleportLine.SetPosition(1, end);
            Color color = hasTeleportTarget ? Color.cyan : Color.red;
            leftTeleportLine.startColor = color;
            leftTeleportLine.endColor = color;
        }
    }

    private void UpdateInteractRay()
    {
        if (rightHand == null)
            return;

        Ray ray = new Ray(rightHand.position, rightHand.forward);
        Vector3 end = ray.origin + ray.direction * rayDistance;
        Color color = Color.white;
        Transform interactableRoot = null;
        string interactableName = null;
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactMask, QueryTriggerInteraction.Collide))
        {
            end = hit.point;
            interactableRoot = GetInteractableRoot(hit.collider);
            interactableName = GetInteractableDisplayName(hit.collider);
            if (interactableRoot != null)
                color = Color.green;
        }

        UpdateHighlight(interactableRoot);
        UpdateInteractionPrompt(interactableName);

        if (rightInteractLine == null)
            return;

        rightInteractLine.enabled = true;
        rightInteractLine.SetPosition(0, ray.origin);
        rightInteractLine.SetPosition(1, end);
        rightInteractLine.startColor = color;
        rightInteractLine.endColor = color;
    }

    private void Teleport()
    {
        characterController.enabled = false;
        transform.position = new Vector3(teleportTarget.x, transform.position.y, teleportTarget.z);
        characterController.enabled = true;
    }

    private void TryInteract()
    {
        if (rightHand == null)
            return;

        Ray ray = new Ray(rightHand.position, rightHand.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactMask, QueryTriggerInteraction.Collide))
            return;

        FTSMTeleportInteractable teleportInteractable = hit.collider.GetComponentInParent<FTSMTeleportInteractable>();
        if (teleportInteractable != null && teleportInteractable.isActiveAndEnabled)
        {
            FTSMAudioManager.PlayTouch();
            TryTeleportToInteractable(teleportInteractable);
            return;
        }

        FTSMRoomInteractable roomInteractable = hit.collider.GetComponentInParent<FTSMRoomInteractable>();
        if (roomInteractable != null)
        {
            FTSMAudioManager.PlayRoomInteraction(roomInteractable);
            roomInteractable.Interact();
            return;
        }

        FTSMInteractable interactable = hit.collider.GetComponentInParent<FTSMInteractable>();
        if (interactable != null)
        {
            FTSMAudioManager.PlayTouch();
            interactable.Interact();
        }
    }

    private Transform GetInteractableRoot(Collider hitCollider)
    {
        FTSMTeleportInteractable teleportInteractable = hitCollider.GetComponentInParent<FTSMTeleportInteractable>();
        if (teleportInteractable != null && teleportInteractable.isActiveAndEnabled)
            return teleportInteractable.transform;

        FTSMRoomInteractable roomInteractable = hitCollider.GetComponentInParent<FTSMRoomInteractable>();
        if (roomInteractable != null)
            return roomInteractable.transform;

        FTSMInteractable interactable = hitCollider.GetComponentInParent<FTSMInteractable>();
        return interactable != null ? interactable.transform : null;
    }

    private void UpdateHighlight(Transform newTarget)
    {
        if (highlightedInteractable == newTarget)
            return;

        if (highlightedInteractable != null)
            FTSMInteractableHighlighter.SetHighlighted(highlightedInteractable, false);

        highlightedInteractable = newTarget;

        if (highlightedInteractable != null)
            FTSMInteractableHighlighter.SetHighlighted(highlightedInteractable, true);
    }

    private void UpdateInteractionPrompt(string displayName)
    {
        if (interactionPrompt == null)
            return;

        if (string.IsNullOrEmpty(displayName))
            interactionPrompt.Hide();
        else
            interactionPrompt.Show(displayName);
    }

    private string GetInteractableDisplayName(Collider hitCollider)
    {
        FTSMTeleportInteractable teleportInteractable = hitCollider.GetComponentInParent<FTSMTeleportInteractable>();
        if (teleportInteractable != null && teleportInteractable.isActiveAndEnabled)
            return teleportInteractable.DisplayName;

        FTSMRoomInteractable roomInteractable = hitCollider.GetComponentInParent<FTSMRoomInteractable>();
        if (roomInteractable != null)
            return GetRoomInteractionDisplayName(roomInteractable);

        FTSMInteractable interactable = hitCollider.GetComponentInParent<FTSMInteractable>();
        if (interactable != null)
            return GetDeviceDisplayName(interactable.DeviceType, interactable.transform.name);

        return null;
    }

    private void TryTeleportToInteractable(FTSMTeleportInteractable teleportInteractable)
    {
        if (teleportInteractable == null)
            return;

        if (!teleportInteractable.TryGetTeleportPosition(transform.position.y, out Vector3 targetPosition))
            return;

        characterController.enabled = false;
        transform.position = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
        characterController.enabled = true;
    }

    private static string GetRoomInteractionDisplayName(FTSMRoomInteractable roomInteractable)
    {
        string objectName = roomInteractable.transform.name.ToLowerInvariant();
        if (objectName.Contains("printer"))
            return "Printer";
        if (objectName.Contains("coffee"))
            return "Coffee Machine";
        if (objectName.Contains("microwave"))
            return "Microwave";
        if (objectName.Contains("tv"))
            return "TV";

        switch (roomInteractable.InteractionType)
        {
            case FTSMRoomInteractionType.ToggleLight:
                return "Light";
            case FTSMRoomInteractionType.ToggleFan:
                return "Fan";
            case FTSMRoomInteractionType.ToggleAirConditioner:
                return "Air Conditioner";
            case FTSMRoomInteractionType.ToggleComputer:
                return "Computer";
            case FTSMRoomInteractionType.ToggleDoor:
                return "Door";
            default:
                return null;
        }
    }

    private static string GetDeviceDisplayName(FTSMDeviceType deviceType, string objectName)
    {
        string normalizedObjectName = objectName.ToLowerInvariant();
        if (normalizedObjectName.Contains("printer"))
            return "Printer";
        if (normalizedObjectName.Contains("coffee"))
            return "Coffee Machine";
        if (normalizedObjectName.Contains("microwave"))
            return "Microwave";
        if (normalizedObjectName.Contains("tv"))
            return "TV";

        switch (deviceType)
        {
            case FTSMDeviceType.Computer:
                return "Computer";
            case FTSMDeviceType.Laptop:
                return "Laptop";
            case FTSMDeviceType.Projector:
                return "Projector";
            case FTSMDeviceType.AirConditioner:
                return "Air Conditioner";
            case FTSMDeviceType.LightSwitch:
                return "Light Switch";
            case FTSMDeviceType.FanSwitch:
                return "Fan Switch";
            case FTSMDeviceType.ExitDoor:
                return "Exit";
            default:
                return null;
        }
    }

    private void OnDestroy()
    {
        FTSMAudioManager.StopWalk();
        UpdateHighlight(null);
        UpdateInteractionPrompt(null);
    }
}
