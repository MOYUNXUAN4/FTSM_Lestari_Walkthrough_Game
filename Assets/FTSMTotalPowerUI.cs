using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class FTSMTotalPowerUI : MonoBehaviour
{
    [SerializeField] private Font uiFont;
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Text titleText;
    [SerializeField] private Text totalPowerText;
    [SerializeField] private Text deltaText;
    [SerializeField] private Texture2D successTexture;
    [SerializeField] private Vector2 successImageAnchoredPosition = new Vector2(0f, 40f);
    [SerializeField] private Vector2 successImageMaxSize = new Vector2(1280f, 640f);
    [SerializeField] private Vector2 successButtonSize = new Vector2(240f, 72f);
    [SerializeField] private Vector2 retryButtonAnchoredPosition = new Vector2(-160f, -210f);
    [SerializeField] private Vector2 exitButtonAnchoredPosition = new Vector2(160f, -210f);
    [SerializeField] private Vector2 panelSize = new Vector2(320f, 140f);
    [SerializeField] private Vector2 panelOffset = new Vector2(-18f, -8f);
    [SerializeField] private Color backgroundColor = new Color(0.03f, 0.06f, 0.12f, 0.58f);
    [SerializeField] private Color totalPowerColor = Color.white;
    [SerializeField] private Color zeroPowerColor = new Color(0.45f, 1f, 0.5f, 1f);
    [SerializeField] private Color powerReductionColor = new Color(0.45f, 1f, 0.5f, 1f);
    [SerializeField] private float deltaFadeInSeconds = 0.18f;
    [SerializeField] private float deltaHoldSeconds = 1f;
    [SerializeField] private float deltaFadeOutSeconds = 0.32f;
    [SerializeField] private bool textOutlineEnabled = true;
    [SerializeField] private Color textOutlineColor = new Color(0f, 0f, 0f, 0.38f);
    [SerializeField] private Vector2 textOutlineDistance = new Vector2(1f, -1f);
    [SerializeField] private bool backgroundShadowEnabled = true;
    [SerializeField] private Color backgroundShadowColor = new Color(0f, 0f, 0f, 0.22f);
    [SerializeField] private Vector2 backgroundShadowDistance = new Vector2(2f, -2f);

    private readonly List<FTSMRoomInteractable> trackedDevices = new List<FTSMRoomInteractable>(64);

    private Canvas parentCanvas;
    private GameObject successOverlay;
    private Button retryButton;
    private Button exitButton;
    private int currentTotalWatts;
    private float deltaMessageStartedAt;
    private Color deltaBaseColor;
    private bool initialized;
    private bool subscribed;
    private bool successShown;

    private void Reset()
    {
        AutoBindReferences();
        ApplyVisuals();
    }

    private void OnValidate()
    {
        AutoBindReferences();
        ApplyVisuals();
        EnsureSuccessOverlay();
        ApplySuccessOverlayLayout();
    }

    private void Awake()
    {
        AutoBindReferences();
        ApplyVisuals();
        EnsureSuccessOverlay();
        ApplySuccessOverlayLayout();
    }

    private void Start()
    {
        RebuildTrackedDevices();
        initialized = true;
        SetSuccessOverlayVisible(false);
        RefreshTotalPower();
    }

    private void Update()
    {
        if (!initialized || deltaText == null || !deltaText.enabled)
            return;

        float elapsed = Time.unscaledTime - deltaMessageStartedAt;
        float totalDuration = deltaFadeInSeconds + deltaHoldSeconds + deltaFadeOutSeconds;
        if (elapsed >= totalDuration)
        {
            deltaText.enabled = false;
            return;
        }

        float alpha;
        if (elapsed < deltaFadeInSeconds)
            alpha = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, deltaFadeInSeconds));
        else if (elapsed < deltaFadeInSeconds + deltaHoldSeconds)
            alpha = 1f;
        else
        {
            float fadeOutElapsed = elapsed - deltaFadeInSeconds - deltaHoldSeconds;
            alpha = 1f - Mathf.Clamp01(fadeOutElapsed / Mathf.Max(0.0001f, deltaFadeOutSeconds));
        }

        Color color = deltaBaseColor;
        color.a *= alpha;
        deltaText.color = color;
    }

    private void OnEnable()
    {
        if (subscribed)
            return;

        FTSMRoomInteractable.StateChanged += HandleStateChanged;
        subscribed = true;

        if (initialized)
            RefreshTotalPower();
    }

    private void OnDisable()
    {
        if (!subscribed)
            return;

        FTSMRoomInteractable.StateChanged -= HandleStateChanged;
        subscribed = false;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    private void HandleStateChanged(FTSMRoomInteractable interactable, bool isOn)
    {
        if (!initialized || interactable == null || !ShouldTrack(interactable))
            return;

        RefreshTotalPower();
        if (!isOn)
            ShowReduction(interactable.PowerWatts);
    }

    private void RebuildTrackedDevices()
    {
        trackedDevices.Clear();

        FTSMRoomInteractable[] interactables = FindObjectsByType<FTSMRoomInteractable>(FindObjectsSortMode.None);
        for (int i = 0; i < interactables.Length; i++)
        {
            FTSMRoomInteractable interactable = interactables[i];
            if (ShouldTrack(interactable))
                trackedDevices.Add(interactable);
        }
    }

    private void RefreshTotalPower()
    {
        currentTotalWatts = 0;
        for (int i = 0; i < trackedDevices.Count; i++)
        {
            FTSMRoomInteractable device = trackedDevices[i];
            if (device != null && device.IsOn)
                currentTotalWatts += device.PowerWatts;
        }

        UpdateText();
        if (!successShown && currentTotalWatts == 0)
            ShowSuccessOverlay();
    }

    private void UpdateText()
    {
        if (totalPowerText != null)
        {
            totalPowerText.text = $"{currentTotalWatts} W";
            totalPowerText.color = currentTotalWatts == 0 ? zeroPowerColor : totalPowerColor;
        }
    }

    private void ShowReduction(int watts)
    {
        if (deltaText == null || watts <= 0)
            return;

        deltaText.text = $"-{watts} W";
        deltaBaseColor = powerReductionColor;
        Color color = deltaBaseColor;
        color.a = 0f;
        deltaText.color = color;
        deltaText.enabled = true;
        deltaMessageStartedAt = Time.unscaledTime;
    }

    private static bool ShouldTrack(FTSMRoomInteractable interactable)
    {
        if (interactable == null || !interactable.IncludeInObjectiveUi || interactable.InteractionType == FTSMRoomInteractionType.ToggleDoor || interactable.PowerWatts <= 0)
            return false;

        if (IsHiddenCorridorCeilingLight(interactable) || IsHiddenCorridorFanWallSwitch(interactable))
            return false;

        return true;
    }

    private static bool IsHiddenCorridorCeilingLight(FTSMRoomInteractable interactable)
    {
        return interactable.InteractionType == FTSMRoomInteractionType.ToggleLight && ContainsInPath(interactable.transform, "corridor_ceiling_lights");
    }

    private static bool IsHiddenCorridorFanWallSwitch(FTSMRoomInteractable interactable)
    {
        return interactable.InteractionType == FTSMRoomInteractionType.ToggleFan && ContainsInPath(interactable.transform, "lightswitches02 (1)");
    }

    private void AutoBindReferences()
    {
        if (panelRoot == null)
            panelRoot = GetComponent<RectTransform>();
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        if (titleText == null)
            titleText = FindText(transform, "Title");
        if (totalPowerText == null)
            totalPowerText = FindText(transform, "Total Power");
        if (deltaText == null)
            deltaText = FindText(transform, "Delta");
        if (uiFont == null && titleText != null)
            uiFont = titleText.font;
        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();
    }

    private void ApplyVisuals()
    {
        if (panelRoot != null)
        {
            panelRoot.anchorMin = new Vector2(1f, 1f);
            panelRoot.anchorMax = new Vector2(1f, 1f);
            panelRoot.pivot = new Vector2(1f, 1f);
            panelRoot.anchoredPosition = panelOffset;
            panelRoot.sizeDelta = panelSize;
        }

        if (backgroundImage != null)
            backgroundImage.color = backgroundColor;

        ApplyGraphicEffect(backgroundImage, false, Color.black, Vector2.zero, backgroundShadowEnabled, backgroundShadowColor, backgroundShadowDistance);

        if (titleText != null)
        {
            titleText.font = uiFont != null ? uiFont : titleText.font;
            titleText.fontSize = 24;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.UpperRight;
            SetRect(titleText.rectTransform, new Vector2(16f, -12f), new Vector2(-16f, -44f));
        }

        if (totalPowerText != null)
        {
            totalPowerText.font = uiFont != null ? uiFont : totalPowerText.font;
            totalPowerText.fontSize = 40;
            totalPowerText.color = totalPowerColor;
            totalPowerText.alignment = TextAnchor.UpperRight;
            SetRect(totalPowerText.rectTransform, new Vector2(16f, -46f), new Vector2(-16f, -92f));
        }

        if (deltaText != null)
        {
            deltaText.font = uiFont != null ? uiFont : deltaText.font;
            deltaText.fontSize = 26;
            deltaText.color = powerReductionColor;
            deltaText.alignment = TextAnchor.UpperRight;
            deltaText.enabled = false;
            SetRect(deltaText.rectTransform, new Vector2(16f, -96f), new Vector2(-16f, -126f));
        }

        ApplyGraphicEffect(titleText, textOutlineEnabled, textOutlineColor, textOutlineDistance, false, Color.black, Vector2.zero);
        ApplyGraphicEffect(totalPowerText, textOutlineEnabled, textOutlineColor, textOutlineDistance, false, Color.black, Vector2.zero);
        ApplyGraphicEffect(deltaText, textOutlineEnabled, textOutlineColor, textOutlineDistance, false, Color.black, Vector2.zero);
    }

    private void EnsureSuccessOverlay()
    {
        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
            return;

        EnsureEventSystem();

        Transform existingOverlay = parentCanvas.transform.Find("Success Overlay");
        if (existingOverlay != null)
        {
            successOverlay = existingOverlay.gameObject;
            BindExistingSuccessOverlay(existingOverlay);
            return;
        }

        successOverlay = new GameObject("Success Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        successOverlay.transform.SetParent(parentCanvas.transform, false);

        RectTransform overlayRect = successOverlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = successOverlay.GetComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.82f);
        overlayImage.raycastTarget = true;

        CreateSuccessImage(successOverlay.transform);
        retryButton = CreateButton(successOverlay.transform, "Retry Button", "Restart Game", retryButtonAnchoredPosition, RestartScene);
        exitButton = CreateButton(successOverlay.transform, "Exit Button", "Back to Menu", exitButtonAnchoredPosition, ExitGame);
        successOverlay.SetActive(false);
    }

    private void BindExistingSuccessOverlay(Transform overlayRoot)
    {
        retryButton = FindButton(overlayRoot, "Retry Button", "Restart Game", RestartScene);
        exitButton = FindButton(overlayRoot, "Exit Button", "Back to Menu", ExitGame);
        ApplySuccessOverlayLayout();
    }

    private void CreateSuccessImage(Transform parent)
    {
        GameObject imageObject = new GameObject("Successful Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = successImageAnchoredPosition;
        rect.sizeDelta = successImageMaxSize;

        Image image = imageObject.GetComponent<Image>();
        image.preserveAspect = true;
        image.raycastTarget = false;
        RefreshSuccessImage(parent);
    }

    private Button CreateButton(Transform parent, string objectName, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = successButtonSize;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.08f, 0.18f, 0.32f, 0.96f);

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.08f, 0.18f, 0.32f, 0.96f);
        colors.highlightedColor = new Color(0.12f, 0.28f, 0.46f, 1f);
        colors.pressedColor = new Color(0.05f, 0.14f, 0.24f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.15f, 0.15f, 0.15f, 0.7f);
        button.colors = colors;
        button.onClick.AddListener(onClick);

        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text text = textObject.GetComponent<Text>();
        text.text = label;
        text.font = uiFont;
        text.fontSize = 30;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;

        Outline outline = textObject.GetComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.38f);
        outline.effectDistance = new Vector2(1f, -1f);

        return button;
    }

    private static void EnsureEventSystem()
    {
        UnityEngine.EventSystems.EventSystem eventSystem = UnityEngine.EventSystems.EventSystem.current;
        if (eventSystem != null)
        {
            EnsureInputSystemModule(eventSystem.gameObject);
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
        EnsureInputSystemModule(eventSystemObject);
    }

    private static void EnsureInputSystemModule(GameObject eventSystemObject)
    {
        if (eventSystemObject == null)
            return;

        UnityEngine.EventSystems.StandaloneInputModule standaloneInputModule = eventSystemObject.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        if (standaloneInputModule != null)
            DestroyImmediate(standaloneInputModule);

        if (eventSystemObject.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
            eventSystemObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
    }

    private Button FindButton(Transform parent, string objectName, string label, UnityEngine.Events.UnityAction onClick)
    {
        Transform child = parent.Find(objectName);
        if (child == null)
            return null;

        Button button = child.GetComponent<Button>();
        if (button == null)
            return null;

        button.onClick.RemoveListener(onClick);
        button.onClick.AddListener(onClick);

        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = objectName == "Retry Button" ? retryButtonAnchoredPosition : exitButtonAnchoredPosition;
            rect.sizeDelta = successButtonSize;
        }

        Transform labelTransform = child.Find("Label");
        if (labelTransform != null)
        {
            Text text = labelTransform.GetComponent<Text>();
            if (text != null)
            {
                text.text = label;
                if (uiFont != null)
                    text.font = uiFont;
            }
        }

        return button;
    }

    private void RefreshSuccessImage(Transform overlayRoot)
    {
#if UNITY_EDITOR
        if (successTexture == null)
            successTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Poster/successful.png");
#endif
        if (successTexture == null)
            return;

        Transform imageTransform = overlayRoot.Find("Successful Image");
        if (imageTransform == null)
            return;

        Image image = imageTransform.GetComponent<Image>();
        if (image == null)
            return;

        image.sprite = Sprite.Create(successTexture, new Rect(0f, 0f, successTexture.width, successTexture.height), new Vector2(0.5f, 0.5f));
        image.preserveAspect = true;
        image.raycastTarget = false;

        RectTransform rect = image.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = successImageAnchoredPosition;
        Vector2 size = new Vector2(successTexture.width, successTexture.height);
        float scale = Mathf.Min(successImageMaxSize.x / Mathf.Max(1f, size.x), successImageMaxSize.y / Mathf.Max(1f, size.y));
        scale = Mathf.Max(0.0001f, scale);
        rect.sizeDelta = size * scale;
    }

    private void ApplySuccessOverlayLayout()
    {
        if (successOverlay == null)
            return;

        Transform overlayRoot = successOverlay.transform;
        RefreshSuccessImage(overlayRoot);

        Transform retryTransform = overlayRoot.Find("Retry Button");
        if (retryTransform != null)
        {
            RectTransform rect = retryTransform.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = retryButtonAnchoredPosition;
                rect.sizeDelta = successButtonSize;
            }
        }

        Transform exitTransform = overlayRoot.Find("Exit Button");
        if (exitTransform != null)
        {
            RectTransform rect = exitTransform.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = exitButtonAnchoredPosition;
                rect.sizeDelta = successButtonSize;
            }
        }
    }

    private void ShowSuccessOverlay()
    {
        successShown = true;
        FTSMAudioManager.PlaySuccessful();
        SetSuccessOverlayVisible(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    private void SetSuccessOverlayVisible(bool visible)
    {
        if (successOverlay != null)
            successOverlay.SetActive(visible);
    }

    private void RestartScene()
    {
        StartCoroutine(RestartSceneAfterClick());
    }

    private System.Collections.IEnumerator RestartSceneAfterClick()
    {
        Time.timeScale = 1f;
        FTSMAudioManager.PlayMenuClick();
        FTSMStartMenu.RequestGameRestart();
        yield return new WaitForSecondsRealtime(0.2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ExitGame()
    {
        StartCoroutine(ReturnToMenuAfterClick());
    }

    private System.Collections.IEnumerator ReturnToMenuAfterClick()
    {
        Time.timeScale = 1f;
        FTSMAudioManager.PlayMenuClick();
        FTSMStartMenu.RequestReturnToMenu();
        yield return new WaitForSecondsRealtime(0.2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static Text FindText(Transform root, string name)
    {
        if (root == null)
            return null;

        Transform child = root.Find(name);
        return child != null ? child.GetComponent<Text>() : null;
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

    private static void SetRect(RectTransform rect, Vector2 topLeft, Vector2 bottomRight)
    {
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.offsetMin = new Vector2(topLeft.x, bottomRight.y);
        rect.offsetMax = new Vector2(bottomRight.x, topLeft.y);
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
