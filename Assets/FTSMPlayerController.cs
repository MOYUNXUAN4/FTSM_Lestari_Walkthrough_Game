using UnityEngine;
using UnityEngine.InputSystem;

public sealed class FTSMPlayerController : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private LineRenderer leftTeleportLine;
    [SerializeField] private LineRenderer rightInteractLine;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float lookSpeed = 2f;
    [SerializeField] private float rayDistance = 18f;
    [SerializeField] private LayerMask teleportMask;
    [SerializeField] private LayerMask interactMask;

    private float pitch;
    private Vector3 teleportTarget;
    private bool hasTeleportTarget;
    private Transform highlightedInteractable;
    private FTSMInteractionPromptUI interactionPrompt;

    private void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
        Canvas promptCanvas = playerCamera != null ? playerCamera.GetComponentInChildren<Canvas>(true) : null;
        interactionPrompt = FTSMInteractionPromptUI.GetOrCreate(promptCanvas);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;

        if (mouse == null || keyboard == null)
            return;

        if (keyboard.escapeKey.wasPressedThisFrame && FTSMStartMenu.TryTogglePause())
            return;

        if (FTSMStartMenu.Instance != null && FTSMStartMenu.Instance.IsPauseOpen)
            return;

        Look(mouse);
        Move(keyboard);
        UpdateTeleportRay(mouse);
        UpdateInteractRay();

        if (mouse.rightButton.wasReleasedThisFrame && hasTeleportTarget)
            Teleport();

        if (mouse.leftButton.wasPressedThisFrame)
            TryInteract();
    }

    private void Look(Mouse mouse)
    {
        Vector2 delta = mouse.delta.ReadValue();
        float yaw = delta.x * lookSpeed * 0.1f;
        pitch -= delta.y * lookSpeed * 0.1f;
        pitch = Mathf.Clamp(pitch, -75f, 75f);
        transform.Rotate(0f, yaw, 0f);
        playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void Move(Keyboard keyboard)
    {
        float x = 0f;
        float z = 0f;
        if (keyboard.aKey.isPressed) x -= 1f;
        if (keyboard.dKey.isPressed) x += 1f;
        if (keyboard.sKey.isPressed) z -= 1f;
        if (keyboard.wKey.isPressed) z += 1f;
        Vector3 input = new Vector3(x, 0f, z);
        input = Vector3.ClampMagnitude(input, 1f);
        Vector3 motion = transform.TransformDirection(input) * moveSpeed;
        motion.y = 0f;
        characterController.Move(motion * Time.deltaTime);
        FTSMAudioManager.ReportWalk(input.sqrMagnitude > 0.0001f && characterController.velocity.sqrMagnitude > 0.01f);
    }

    private void UpdateTeleportRay(Mouse mouse)
    {
        bool active = mouse.rightButton.isPressed;
        leftTeleportLine.enabled = active;
        hasTeleportTarget = false;
        if (!active)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Vector3 end = ray.origin + ray.direction * rayDistance;
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, teleportMask, QueryTriggerInteraction.Ignore))
        {
            end = hit.point;
            teleportTarget = hit.point;
            hasTeleportTarget = true;
        }

        leftTeleportLine.SetPosition(0, ray.origin);
        leftTeleportLine.SetPosition(1, end);
    }

    private void UpdateInteractRay()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Vector3 end = ray.origin + ray.direction * rayDistance;
        Color color = Color.white;
        bool hasHit = false;
        Transform interactableRoot = null;
        string interactableName = null;
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactMask, QueryTriggerInteraction.Collide))
        {
            end = hit.point;
            interactableRoot = GetInteractableRoot(hit.collider);
            interactableName = GetInteractableDisplayName(hit.collider);
            hasHit = interactableRoot != null;
            if (hasHit)
                color = Color.green;
        }

        UpdateHighlight(interactableRoot);
        UpdateInteractionPrompt(interactableName);

        if (rightInteractLine == null)
            return;

        rightInteractLine.enabled = hasHit;
        if (!hasHit)
            return;

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
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
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
        transform.position = targetPosition;
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

    private void OnDisable()
    {
        FTSMAudioManager.StopWalk();
        UpdateHighlight(null);
        UpdateInteractionPrompt(null);
    }
}
