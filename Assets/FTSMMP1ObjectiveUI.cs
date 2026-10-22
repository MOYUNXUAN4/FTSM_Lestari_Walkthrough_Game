using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class FTSMMP1ObjectiveUI : MonoBehaviour
{
    [SerializeField] private Transform trackedPlayer;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Font uiFont;
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Text titleText;
    [SerializeField] private VerticalLayoutGroup objectivesLayout;
    [SerializeField] private ObjectiveRowUi[] rows = new ObjectiveRowUi[MaxObjectives];

    [Header("Layout")]
    [SerializeField] private Vector2 panelSize = new Vector2(338f, 206f);
    [SerializeField] private Vector2 panelOffset = new Vector2(20f, -18f);
    [SerializeField] private Color backgroundColor = new Color(0.03f, 0.06f, 0.12f, 0.52f);
    [SerializeField] private float titleHeight = 24f;
    [SerializeField] private float titleTopInset = 8f;
    [SerializeField] private float sideInset = 14f;
    [SerializeField] private float objectivesTopOffset = 34f;
    [SerializeField] private int titleFontSize = 22;
    [SerializeField] private int objectiveFontSize = 16;
    [SerializeField] private int detailFontSize = 10;
    [SerializeField] private float titleLineSpacing = 1f;
    [SerializeField] private float objectiveLineSpacing = 1f;
    [SerializeField] private float detailLineSpacing = 1f;
    [SerializeField] private float rowSpacing = -1f;
    [SerializeField] private float rowHeight = 18f;
    [SerializeField] private float rowHeightWithDetail = 24f;
    [SerializeField] private float detailIndent = 14f;
    [SerializeField] private float detailTopOffset = 10f;
    [SerializeField] private float detailHeight = 12f;
    [SerializeField] private float roomSwitchDistance = 36000f;

    [Header("Effects")]
    [SerializeField] private bool backgroundOutlineEnabled = false;
    [SerializeField] private Color backgroundOutlineColor = new Color(0f, 0f, 0f, 0.35f);
    [SerializeField] private Vector2 backgroundOutlineDistance = new Vector2(2f, -2f);
    [SerializeField] private bool backgroundShadowEnabled = true;
    [SerializeField] private Color backgroundShadowColor = new Color(0f, 0f, 0f, 0.22f);
    [SerializeField] private Vector2 backgroundShadowDistance = new Vector2(2f, -2f);
    [SerializeField] private bool textOutlineEnabled = true;
    [SerializeField] private Color textOutlineColor = new Color(0f, 0f, 0f, 0.38f);
    [SerializeField] private Vector2 textOutlineDistance = new Vector2(1f, -1f);
    [SerializeField] private bool textShadowEnabled = false;
    [SerializeField] private Color textShadowColor = new Color(0f, 0f, 0f, 0.5f);
    [SerializeField] private Vector2 textShadowDistance = new Vector2(1f, -1f);

    private const int MaxObjectives = 8;

    private static readonly Color TitleColor = Color.white;
    private static readonly Color ObjectiveColor = new Color(1f, 1f, 1f, 0.96f);
    private static readonly Color DetailColor = new Color(1f, 1f, 1f, 0.72f);
    private static readonly Color CompleteColor = new Color(0.45f, 1f, 0.5f, 1f);
    private static readonly string Checkmark = "<color=#72FF89>✓</color>";

    private readonly List<RoomUiDefinition> roomDefinitions = new List<RoomUiDefinition>(4);

    private RoomUiDefinition activeRoom;
    private bool initialized;
    private bool subscribed;

    [System.Serializable]
    private sealed class ObjectiveRowUi
    {
        public GameObject rowObject;
        public RectTransform rowRect;
        public LayoutElement layoutElement;
        public Text objectiveText;
        public Text detailText;
    }

    private sealed class ObjectiveState
    {
        public string Label;
        public string Detail;
        public FTSMRoomInteractable MasterTarget;
        public readonly List<FTSMRoomInteractable> Targets = new List<FTSMRoomInteractable>(4);
    }

    private sealed class RoomUiDefinition
    {
        public string Title;
        public Transform Root;
        public Bounds Bounds;
        public Bounds FloorBounds;
        public Vector3 Center;
        public float ActivationDistance;
        public ObjectiveState[] Objectives;
    }

    private void Reset()
    {
        AutoBindReferences();
        ApplyLayoutSettings();
    }

    private void OnValidate()
    {
        AutoBindReferences();
        ApplyLayoutSettings();
    }

    private void Awake()
    {
        AutoBindReferences();
        ApplyLayoutSettings();
    }

    private void Start()
    {
        RebuildTrackedInteractables();
        initialized = true;
        RefreshUi();
    }

    private void Update()
    {
        if (!initialized)
            return;

        if (UpdateActiveRoom())
            RefreshUi();
    }

    private void OnEnable()
    {
        if (subscribed)
            return;

        FTSMRoomInteractable.StateChanged += HandleStateChanged;
        subscribed = true;

        if (initialized)
            RefreshUi();
    }

    private void OnDisable()
    {
        if (!subscribed)
            return;

        FTSMRoomInteractable.StateChanged -= HandleStateChanged;
        subscribed = false;
    }

    private void HandleStateChanged(FTSMRoomInteractable interactable, bool isOn)
    {
        if (!initialized || interactable == null)
            return;

        switch (interactable.InteractionType)
        {
            case FTSMRoomInteractionType.ToggleComputer:
            case FTSMRoomInteractionType.ToggleAirConditioner:
            case FTSMRoomInteractionType.ToggleFan:
            case FTSMRoomInteractionType.ToggleLight:
                RefreshUi();
                break;
        }
    }

    private void AutoBindReferences()
    {
        if (targetCamera == null)
            targetCamera = GetComponentInParent<Camera>();

        if (trackedPlayer == null)
        {
            FTSMPlayerController playerController = GetComponentInParent<FTSMPlayerController>();
            if (playerController != null)
                trackedPlayer = playerController.transform;
            else if (targetCamera != null)
                trackedPlayer = targetCamera.transform.root;
        }

        if (panelRoot == null)
            panelRoot = GetComponent<RectTransform>();
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        if (titleText == null)
            titleText = FindText(transform, "Title");

        Transform objectivesRoot = transform.Find("Objectives");
        if (objectivesLayout == null && objectivesRoot != null)
            objectivesLayout = objectivesRoot.GetComponent<VerticalLayoutGroup>();

        if (rows == null || rows.Length != MaxObjectives)
            rows = new ObjectiveRowUi[MaxObjectives];

        if (objectivesRoot == null)
            return;

        for (int i = 0; i < MaxObjectives; i++)
        {
            if (rows[i] == null)
                rows[i] = new ObjectiveRowUi();

            Transform rowTransform = i < objectivesRoot.childCount ? objectivesRoot.GetChild(i) : null;
            rows[i].rowObject = rowTransform != null ? rowTransform.gameObject : null;
            rows[i].rowRect = rowTransform != null ? rowTransform.GetComponent<RectTransform>() : null;
            rows[i].layoutElement = rowTransform != null ? rowTransform.GetComponent<LayoutElement>() : null;
            rows[i].objectiveText = rowTransform != null ? FindText(rowTransform, $"Objective {i + 1}") : null;
            rows[i].detailText = rowTransform != null ? FindText(rowTransform, $"Detail {i + 1}") : null;
        }
    }

    private void ApplyLayoutSettings()
    {
        if (panelRoot != null)
        {
            panelRoot.anchorMin = new Vector2(0f, 1f);
            panelRoot.anchorMax = new Vector2(0f, 1f);
            panelRoot.pivot = new Vector2(0f, 1f);
            panelRoot.anchoredPosition = panelOffset;
            panelRoot.sizeDelta = panelSize;
        }

        if (backgroundImage != null)
            backgroundImage.color = backgroundColor;

        ApplyGraphicEffect(backgroundImage, backgroundOutlineEnabled, backgroundOutlineColor, backgroundOutlineDistance, backgroundShadowEnabled, backgroundShadowColor, backgroundShadowDistance);

        RectTransform titleRect = titleText != null ? titleText.rectTransform : null;
        if (titleRect != null)
            SetTopStretchRect(titleRect, sideInset, titleTopInset, titleHeight);

        if (titleText != null)
        {
            titleText.font = uiFont != null ? uiFont : titleText.font;
            titleText.fontSize = titleFontSize;
            titleText.color = TitleColor;
            titleText.lineSpacing = titleLineSpacing;
        }

        ApplyGraphicEffect(titleText, textOutlineEnabled, textOutlineColor, textOutlineDistance, textShadowEnabled, textShadowColor, textShadowDistance);

        RectTransform objectivesRect = objectivesLayout != null ? objectivesLayout.GetComponent<RectTransform>() : null;
        if (objectivesRect != null)
        {
            float objectivesHeight = Mathf.Max(0f, panelSize.y - objectivesTopOffset);
            SetTopStretchRect(objectivesRect, sideInset, objectivesTopOffset, objectivesHeight);
        }

        if (objectivesLayout != null)
            objectivesLayout.spacing = rowSpacing;

        if (rows == null)
            return;

        for (int i = 0; i < rows.Length; i++)
        {
            ObjectiveRowUi row = rows[i];
            if (row == null)
                continue;

            if (row.objectiveText != null)
            {
                row.objectiveText.font = uiFont != null ? uiFont : row.objectiveText.font;
                row.objectiveText.fontSize = objectiveFontSize;
                row.objectiveText.color = ObjectiveColor;
                row.objectiveText.lineSpacing = objectiveLineSpacing;
            }

            if (row.detailText != null)
            {
                row.detailText.font = uiFont != null ? uiFont : row.detailText.font;
                row.detailText.fontSize = detailFontSize;
                row.detailText.color = DetailColor;
                row.detailText.lineSpacing = detailLineSpacing;
            }

            if (row.layoutElement != null)
            {
                row.layoutElement.preferredHeight = rowHeightWithDetail;
                row.layoutElement.minHeight = rowHeightWithDetail;
            }

            if (row.rowRect != null)
            {
                row.rowRect.anchorMin = new Vector2(0f, 1f);
                row.rowRect.anchorMax = new Vector2(1f, 1f);
                row.rowRect.pivot = new Vector2(0f, 1f);
                row.rowRect.anchoredPosition = Vector2.zero;
                row.rowRect.sizeDelta = new Vector2(0f, rowHeightWithDetail);
            }

            if (row.objectiveText != null)
            {
                SetTopStretchRect(row.objectiveText.rectTransform, 0f, 0f, rowHeight);
                ApplyGraphicEffect(row.objectiveText, textOutlineEnabled, textOutlineColor, textOutlineDistance, textShadowEnabled, textShadowColor, textShadowDistance);
            }

            if (row.detailText != null)
            {
                SetTopStretchRect(row.detailText.rectTransform, detailIndent, detailTopOffset, detailHeight);
                ApplyGraphicEffect(row.detailText, textOutlineEnabled, textOutlineColor, textOutlineDistance, textShadowEnabled, textShadowColor, textShadowDistance);
            }
        }

        if (panelRoot != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRoot);
    }

    private void RebuildTrackedInteractables()
    {
        roomDefinitions.Clear();

        FTSMRoomInteractable[] interactables = FindObjectsByType<FTSMRoomInteractable>(FindObjectsSortMode.None);

        AddRoomDefinition(CreateMp1Definition(interactables));
        AddRoomDefinition(CreateBk1Definition(interactables));
        AddRoomDefinition(CreateStaffOfficeDefinition(interactables));
        AddRoomDefinition(CreateCorridorDefinition(interactables));

        UpdateActiveRoom();
    }

    private void AddRoomDefinition(RoomUiDefinition definition)
    {
        if (definition != null)
            roomDefinitions.Add(definition);
    }

    private RoomUiDefinition CreateMp1Definition(FTSMRoomInteractable[] interactables)
    {
        Transform root = FindRoomRoot("MP1_Room");
        if (root == null)
            return null;

        RoomUiDefinition definition = new RoomUiDefinition
        {
            Title = "MP1",
            Root = root,
            Bounds = CalculateRoomBounds(root),
            Objectives = new[]
            {
                CreateObjective("1: Shut down computers", null),
                CreateObjective("2: Shut down the doctor's computer", null),
                CreateObjective("3: Turn off the air conditioners", null),
                CreateObjective("4: Turn off the small fan", null),
                CreateObjective("5: Turn off the TV", null),
                CreateObjective("6: Turn off the lights", null)
            }
        };
        FinalizeRoomDefinition(definition);

        for (int i = 0; i < interactables.Length; i++)
        {
            FTSMRoomInteractable interactable = interactables[i];
            if (interactable == null || !interactable.IncludeInObjectiveUi || !BelongsToRoom(interactable, root))
                continue;

            string objectName = interactable.name.ToLowerInvariant();
            switch (interactable.InteractionType)
            {
                case FTSMRoomInteractionType.ToggleComputer:
                    if (objectName.Contains("drdesk"))
                        definition.Objectives[1].Targets.Add(interactable);
                    else if (objectName.Contains("wall_tv"))
                        definition.Objectives[4].Targets.Add(interactable);
                    else if (objectName.Contains("computer"))
                        definition.Objectives[0].Targets.Add(interactable);
                    break;

                case FTSMRoomInteractionType.ToggleAirConditioner:
                    if (objectName.Contains("air_conditioner"))
                        definition.Objectives[2].Targets.Add(interactable);
                    break;

                case FTSMRoomInteractionType.ToggleFan:
                    if (objectName.Contains("fan_small"))
                        definition.Objectives[3].Targets.Add(interactable);
                    break;

                case FTSMRoomInteractionType.ToggleLight:
                    definition.Objectives[5].Targets.Add(interactable);
                    break;
            }
        }

        return definition;
    }

    private RoomUiDefinition CreateBk1Definition(FTSMRoomInteractable[] interactables)
    {
        Transform root = FindRoomRoot("BK1_Room");
        if (root == null)
            return null;

        RoomUiDefinition definition = new RoomUiDefinition
        {
            Title = "BK1",
            Root = root,
            Bounds = CalculateRoomBounds(root),
            Objectives = new[]
            {
                CreateObjective("1: Turn off the air conditioner", null),
                CreateObjective("2: Shut down the computer", null),
                CreateObjective("3: Turn off the TV", null),
                CreateObjective("4: Turn off the lights", null)
            }
        };
        FinalizeRoomDefinition(definition);

        for (int i = 0; i < interactables.Length; i++)
        {
            FTSMRoomInteractable interactable = interactables[i];
            if (interactable == null || !interactable.IncludeInObjectiveUi || !BelongsToRoom(interactable, root))
                continue;

            string objectName = interactable.name.ToLowerInvariant();
            switch (interactable.InteractionType)
            {
                case FTSMRoomInteractionType.ToggleComputer:
                    if (objectName.Contains("wall_tv"))
                        definition.Objectives[2].Targets.Add(interactable);
                    else if (objectName.Contains("computer"))
                        definition.Objectives[1].Targets.Add(interactable);
                    break;

                case FTSMRoomInteractionType.ToggleAirConditioner:
                    if (objectName.Contains("air_conditioner"))
                        definition.Objectives[0].Targets.Add(interactable);
                    break;

                case FTSMRoomInteractionType.ToggleLight:
                    definition.Objectives[3].Targets.Add(interactable);
                    break;
            }
        }

        return definition;
    }

    private RoomUiDefinition CreateStaffOfficeDefinition(FTSMRoomInteractable[] interactables)
    {
        Transform root = FindRoomRoot("Staff_Office_Group");
        if (root == null)
            return null;

        RoomUiDefinition definition = new RoomUiDefinition
        {
            Title = "STAFF OFFICE",
            Root = root,
            Bounds = CalculateRoomBounds(root),
            Objectives = new[]
            {
                CreateObjective("1: Turn off the air conditioner", null),
                CreateObjective("2: Turn off the printer", null),
                CreateObjective("3: Shut down the computer", null),
                CreateObjective("4: Turn off the ceiling fan", null),
                CreateObjective("5: Turn off the desk fan", null),
                CreateObjective("6: Turn off the lights", null)
            }
        };
        FinalizeRoomDefinition(definition);

        for (int i = 0; i < interactables.Length; i++)
        {
            FTSMRoomInteractable interactable = interactables[i];
            if (interactable == null || !interactable.IncludeInObjectiveUi || !BelongsToRoom(interactable, root))
                continue;

            string objectName = interactable.name.ToLowerInvariant();
            switch (interactable.InteractionType)
            {
                case FTSMRoomInteractionType.ToggleComputer:
                    if (objectName.Contains("computer"))
                        definition.Objectives[2].Targets.Add(interactable);
                    break;

                case FTSMRoomInteractionType.ToggleAirConditioner:
                    if (objectName.Contains("printer"))
                        definition.Objectives[1].Targets.Add(interactable);
                    else if (objectName.Contains("air_conditioner"))
                        definition.Objectives[0].Targets.Add(interactable);
                    break;

                case FTSMRoomInteractionType.ToggleFan:
                    if (objectName.Contains("fan_small"))
                        definition.Objectives[4].Targets.Add(interactable);
                    else if (ContainsInPath(interactable.transform, "lightswitches02_right"))
                        definition.Objectives[3].Targets.Add(interactable);
                    break;

                case FTSMRoomInteractionType.ToggleLight:
                    if (objectName == "lightswitches01")
                        definition.Objectives[5].Targets.Add(interactable);
                    break;
            }
        }

        return definition;
    }

    private RoomUiDefinition CreateCorridorDefinition(FTSMRoomInteractable[] interactables)
    {
        Transform root = FindRoomRoot("corridor");
        if (root == null)
            return null;

        RoomUiDefinition definition = new RoomUiDefinition
        {
            Title = "CORRIDOR",
            Root = root,
            Bounds = CalculateRoomBounds(root),
            Objectives = new[]
            {
                CreateObjective("1: Turn off the coffee machine", null),
                CreateObjective("2: Turn off the printer", null),
                CreateObjective("3: Shut down the computer", null),
                CreateObjective("4: Turn off the microwave", null),
                CreateObjective("5: Turn off the desk fan", null),
                CreateObjective("6: Turn off the TV", null),
                CreateObjective("7: Turn off the fans", null),
                CreateObjective("8: Turn off the lights", null)
            }
        };
        FinalizeRoomDefinition(definition);

        for (int i = 0; i < interactables.Length; i++)
        {
            FTSMRoomInteractable interactable = interactables[i];
            if (interactable == null || !interactable.IncludeInObjectiveUi)
                continue;

            string objectName = interactable.name.ToLowerInvariant();
            bool inCorridor = BelongsToRoom(interactable, root) || IsCorridorLightWallSwitch(interactable) || IsCorridorTelevision(interactable);
            if (!inCorridor)
                continue;

            switch (interactable.InteractionType)
            {
                case FTSMRoomInteractionType.ToggleComputer:
                    if (objectName.Contains("wall_tv") || objectName == "tv")
                        definition.Objectives[5].Targets.Add(interactable);
                    else if (objectName.Contains("computer"))
                        definition.Objectives[2].Targets.Add(interactable);
                    break;

                case FTSMRoomInteractionType.ToggleAirConditioner:
                    if (objectName.Contains("coffee"))
                        definition.Objectives[0].Targets.Add(interactable);
                    else if (objectName.Contains("printer"))
                        definition.Objectives[1].Targets.Add(interactable);
                    else if (objectName.Contains("microwave"))
                        definition.Objectives[3].Targets.Add(interactable);
                    break;

                case FTSMRoomInteractionType.ToggleFan:
                    if (objectName.Contains("fan_small"))
                        definition.Objectives[4].Targets.Add(interactable);
                    else if (objectName.Contains("fan_alternative"))
                        definition.Objectives[6].Targets.Add(interactable);
                    break;

                case FTSMRoomInteractionType.ToggleLight:
                    if (BelongsToRoom(interactable, root) && ContainsInPath(interactable.transform, "corridor_ceiling_lights"))
                    {
                        definition.Objectives[7].Targets.Add(interactable);
                    }
                    break;
            }
        }

        return definition;
    }

    private void RefreshUi()
    {
        if (titleText == null || rows == null)
            return;

        if (activeRoom == null)
        {
            if (panelRoot != null)
                panelRoot.gameObject.SetActive(false);
            return;
        }

        if (panelRoot != null && !panelRoot.gameObject.activeSelf)
            panelRoot.gameObject.SetActive(true);

        titleText.text = activeRoom.Title;

        for (int i = 0; i < rows.Length; i++)
        {
            ObjectiveRowUi row = rows[i];
            if (row == null || row.rowObject == null)
                continue;

            bool visible = i < activeRoom.Objectives.Length;
            row.rowObject.SetActive(visible);
            if (!visible)
                continue;

            SetObjective(row, activeRoom.Objectives[i]);
        }

        if (panelRoot != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRoot);
    }

    private void SetObjective(ObjectiveRowUi row, ObjectiveState objective)
    {
        if (row.objectiveText == null || row.detailText == null || row.layoutElement == null || objective == null)
            return;

        int total = objective.Targets.Count;
        int done = CountTurnedOff(objective.Targets);
        bool completed = total > 0 && done >= total;
        bool hasDetail = !string.IsNullOrEmpty(objective.Detail);

        string countText = total > 0 ? $" ({done}/{total})" : string.Empty;
        string labelText = objective.Label + countText;
        row.objectiveText.text = completed ? labelText + " " + Checkmark : labelText;
        row.objectiveText.color = completed ? CompleteColor : ObjectiveColor;
        row.detailText.text = objective.Detail ?? string.Empty;
        row.detailText.enabled = hasDetail;
        row.layoutElement.preferredHeight = hasDetail ? rowHeightWithDetail : rowHeight;
        row.layoutElement.minHeight = hasDetail ? rowHeightWithDetail : rowHeight;

        if (row.rowRect != null)
            row.rowRect.sizeDelta = new Vector2(0f, hasDetail ? rowHeightWithDetail : rowHeight);
    }

    private bool UpdateActiveRoom()
    {
        RoomUiDefinition nextRoom = FindActiveRoom();
        if (ReferenceEquals(nextRoom, activeRoom))
            return false;

        activeRoom = nextRoom;
        return true;
    }

    private RoomUiDefinition FindActiveRoom()
    {
        if (roomDefinitions.Count == 0 || trackedPlayer == null)
            return null;

        Vector2 playerPosition = new Vector2(trackedPlayer.position.x, trackedPlayer.position.z);
        RoomUiDefinition bestContainedRoom = null;
        float bestContainedDistanceSqr = float.MaxValue;
        RoomUiDefinition bestFallbackRoom = null;
        float bestFallbackDistanceSqr = float.MaxValue;
        float maxDistanceSqr = roomSwitchDistance * roomSwitchDistance;

        for (int i = 0; i < roomDefinitions.Count; i++)
        {
            RoomUiDefinition candidate = roomDefinitions[i];
            if (candidate.Root == null)
                continue;

            Vector2 roomPosition = new Vector2(candidate.Center.x, candidate.Center.z);
            float distanceSqr = (playerPosition - roomPosition).sqrMagnitude;
            if (distanceSqr > maxDistanceSqr)
                continue;

            if (IsInsideFloorBounds(candidate.FloorBounds, playerPosition))
            {
                if (distanceSqr < bestContainedDistanceSqr)
                {
                    bestContainedDistanceSqr = distanceSqr;
                    bestContainedRoom = candidate;
                }

                continue;
            }

            if (distanceSqr < bestFallbackDistanceSqr)
            {
                bestFallbackDistanceSqr = distanceSqr;
                bestFallbackRoom = candidate;
            }
        }

        return bestContainedRoom ?? bestFallbackRoom;
    }

    private static ObjectiveState CreateObjective(string label, string detail)
    {
        return new ObjectiveState
        {
            Label = label,
            Detail = detail
        };
    }

    private static Transform FindRoomRoot(string roomName)
    {
        GameObject roomRoot = GameObject.Find(roomName);
        return roomRoot != null ? roomRoot.transform : null;
    }

    private static bool BelongsToRoom(FTSMRoomInteractable interactable, Transform roomRoot)
    {
        if (interactable == null || roomRoot == null)
            return false;

        return interactable.transform == roomRoot || interactable.transform.IsChildOf(roomRoot);
    }

    private static Bounds CalculateRoomBounds(Transform roomRoot)
    {
        Renderer[] renderers = roomRoot.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return new Bounds(roomRoot.position, Vector3.one * 1000f);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds;
    }

    private void FinalizeRoomDefinition(RoomUiDefinition definition)
    {
        definition.FloorBounds = GetFloorBounds(definition.Root, definition.Bounds);
        definition.Center = definition.FloorBounds.center;
        definition.ActivationDistance = Mathf.Max(definition.FloorBounds.extents.x, definition.FloorBounds.extents.z);
    }

    private static Bounds GetFloorBounds(Transform roomRoot, Bounds fallbackBounds)
    {
        if (roomRoot == null)
            return fallbackBounds;

        Transform floorTile = roomRoot.Find("floor_tile");
        if (floorTile == null)
            floorTile = roomRoot.Find("floor_tile_Corridor");
        if (floorTile != null)
        {
            Renderer floorRenderer = floorTile.GetComponent<Renderer>();
            if (floorRenderer != null)
                return floorRenderer.bounds;

            return new Bounds(floorTile.position, Vector3.one * 1000f);
        }

        return fallbackBounds;
    }

    private static bool IsInsideFloorBounds(Bounds bounds, Vector2 playerPosition)
    {
        return playerPosition.x >= bounds.min.x && playerPosition.x <= bounds.max.x &&
               playerPosition.y >= bounds.min.z && playerPosition.y <= bounds.max.z;
    }

    private static bool IsCorridorLightWallSwitch(FTSMRoomInteractable interactable)
    {
        return interactable != null && interactable.InteractionType == FTSMRoomInteractionType.ToggleLight && !HasRoomRootAncestor(interactable.transform) &&
               (ContainsInPath(interactable.transform, "lightswitches02_left") || ContainsInPath(interactable.transform, "lightswitches02_right"));
    }

    private static bool IsCorridorTelevision(FTSMRoomInteractable interactable)
    {
        return interactable != null && interactable.name.ToLowerInvariant().Contains("wall_tv") && !HasRoomRootAncestor(interactable.transform);
    }

    private static bool HasRoomRootAncestor(Transform target)
    {
        while (target != null)
        {
            string objectName = target.name;
            if (objectName == "MP1_Room" || objectName == "BK1_Room" || objectName == "Staff_Office_Group" || objectName == "corridor")
                return true;

            target = target.parent;
        }

        return false;
    }

    private static bool ContainsInPath(Transform target, string match)
    {
        while (target != null)
        {
            if (target.name.ToLowerInvariant().Contains(match))
                return true;
            target = target.parent;
        }

        return false;
    }

    private static int CountTurnedOff(List<FTSMRoomInteractable> interactables)
    {
        int count = 0;
        for (int i = 0; i < interactables.Count; i++)
        {
            FTSMRoomInteractable interactable = interactables[i];
            if (interactable != null && !interactable.IsOn)
                count++;
        }
        return count;
    }

    private static Text FindText(Transform root, string name)
    {
        if (root == null)
            return null;

        Transform target = root.Find(name);
        return target != null ? target.GetComponent<Text>() : null;
    }

    private static void SetTopStretchRect(RectTransform rect, float horizontalInset, float topInset, float height)
    {
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(horizontalInset, -topInset);
        rect.sizeDelta = new Vector2(-horizontalInset * 2f, height);
    }

    private static void ApplyGraphicEffect(Graphic graphic, bool outlineEnabled, Color outlineColor, Vector2 outlineDistance, bool shadowEnabled, Color shadowColor, Vector2 shadowDistance)
    {
        if (graphic == null)
            return;

        Outline outline = graphic.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = outlineEnabled;
            outline.effectColor = outlineColor;
            outline.effectDistance = outlineDistance;
        }

        Shadow shadow = null;
        Component[] components = graphic.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] is Shadow candidate && candidate is not Outline)
            {
                shadow = candidate;
                break;
            }
        }

        if (shadow != null)
        {
            shadow.enabled = shadowEnabled;
            shadow.effectColor = shadowColor;
            shadow.effectDistance = shadowDistance;
        }
    }
}
