using System;
using UnityEngine;

public enum FTSMRoomInteractionType
{
    ToggleLight,
    ToggleFan,
    ToggleAirConditioner,
    ToggleComputer,
    ToggleDoor
}

public sealed class FTSMRoomInteractable : MonoBehaviour
{
    public static event Action<FTSMRoomInteractable, bool> StateChanged;

    [SerializeField] private FTSMRoomInteractionType interactionType;
    [SerializeField] private int powerWatts = -1;
    [SerializeField] private bool useSuggestedPower = true;
    [SerializeField] private bool startOn = true;
    [SerializeField] private Light[] lights;
    [SerializeField] private FTSMSpin[] fans;
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private FTSMRoomInteractable[] linkedInteractables;
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Vector3 openAxis = Vector3.up;
    [SerializeField] private float openAngle = 95f;
    [SerializeField] private float doorSpeed = 180f;
    [SerializeField] private Transform animatedTransform;
    [SerializeField] private Vector3 toggleAxis = Vector3.right;
    [SerializeField] private float offAngle = 0f;
    [SerializeField] private float onAngle = -12f;
    [SerializeField] private float toggleSpeed = 240f;
    [SerializeField] private int screenMaterialIndex = -1;
    [SerializeField] private Material offMaterialOverride;
    [SerializeField] private bool dimRenderersOnToggle = true;
    [SerializeField] private float lightOffRendererMultiplier = 0.45f;
    [SerializeField] private float lightOnRendererMultiplier = 1.15f;
    [SerializeField] private bool includeInObjectiveUi = true;

    private bool isOn;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Quaternion targetRotation;
    private Quaternion toggleBaseRotation;
    private Quaternion toggleOffRotation;
    private Quaternion toggleOnRotation;
    private Material[] cachedMaterials;
    private Material[] cachedScreenOnMaterials;
    private Color[] cachedColors;
    private Color[] cachedEmissionColors;
    private bool[] cachedEmissionEnabled;

    public FTSMRoomInteractionType InteractionType => interactionType;
    public bool IsOn => isOn;
    public int PowerWatts => useSuggestedPower ? GetSuggestedPowerWatts() : Mathf.Max(0, powerWatts);
    public bool IncludeInObjectiveUi => includeInObjectiveUi;

    private void Reset()
    {
        if (powerWatts <= 0)
            powerWatts = GetSuggestedPowerWatts();
    }

    private void OnValidate()
    {
        if (useSuggestedPower)
            powerWatts = GetSuggestedPowerWatts();
        else if (powerWatts < 0)
            powerWatts = 0;
    }

    private void Awake()
    {
        isOn = startOn;
        CacheRendererState();

        if (doorTransform == null)
            doorTransform = transform;

        closedRotation = doorTransform.localRotation;
        openRotation = closedRotation * Quaternion.AngleAxis(openAngle, openAxis.normalized);
        targetRotation = isOn ? openRotation : closedRotation;

        if (animatedTransform != null)
        {
            Quaternion currentRotation = animatedTransform.localRotation;
            float currentAngle = isOn ? onAngle : offAngle;
            toggleBaseRotation = currentRotation * Quaternion.AngleAxis(-currentAngle, toggleAxis.normalized);
            toggleOffRotation = toggleBaseRotation * Quaternion.AngleAxis(offAngle, toggleAxis.normalized);
            toggleOnRotation = toggleBaseRotation * Quaternion.AngleAxis(onAngle, toggleAxis.normalized);
        }

        ApplyState(true);
    }

    private void Update()
    {
        if (interactionType == FTSMRoomInteractionType.ToggleDoor && doorTransform != null)
            doorTransform.localRotation = Quaternion.RotateTowards(doorTransform.localRotation, targetRotation, doorSpeed * Time.deltaTime);

        if (animatedTransform != null && interactionType != FTSMRoomInteractionType.ToggleDoor)
        {
            Quaternion targetToggleRotation = isOn ? toggleOnRotation : toggleOffRotation;
            animatedTransform.localRotation = Quaternion.RotateTowards(animatedTransform.localRotation, targetToggleRotation, toggleSpeed * Time.deltaTime);
        }
    }

    public void Interact()
    {
        SetState(!isOn, true);
    }

    public void SetState(bool state)
    {
        SetState(state, true);
    }

    private void CacheRendererState()
    {
        cachedMaterials = new Material[renderers.Length];
        cachedScreenOnMaterials = new Material[renderers.Length];
        cachedColors = new Color[renderers.Length];
        cachedEmissionColors = new Color[renderers.Length];
        cachedEmissionEnabled = new bool[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            Material material;
            if (screenMaterialIndex >= 0)
            {
                Material[] materials = renderers[i].materials;
                if (screenMaterialIndex >= materials.Length)
                    continue;
                material = materials[screenMaterialIndex];
                cachedScreenOnMaterials[i] = material;
            }
            else
            {
                material = renderers[i].material;
            }

            cachedMaterials[i] = material;
            cachedColors[i] = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : material.color;
            cachedEmissionEnabled[i] = material.IsKeywordEnabled("_EMISSION");
            if (material.HasProperty("_EmissionColor"))
                cachedEmissionColors[i] = material.GetColor("_EmissionColor");
        }
    }

    private void ApplyState(bool immediate)
    {
        switch (interactionType)
        {
            case FTSMRoomInteractionType.ToggleLight:
                for (int i = 0; i < lights.Length; i++)
                {
                    if (lights[i] != null)
                        lights[i].enabled = isOn;
                }
                if (dimRenderersOnToggle)
                    ApplyRendererPowerState(isOn, lightOffRendererMultiplier, lightOnRendererMultiplier);
                break;

            case FTSMRoomInteractionType.ToggleFan:
                for (int i = 0; i < fans.Length; i++)
                {
                    if (fans[i] != null)
                        fans[i].SetSpinning(isOn);
                }
                break;

            case FTSMRoomInteractionType.ToggleAirConditioner:
                for (int i = 0; i < lights.Length; i++)
                {
                    if (lights[i] != null)
                        lights[i].enabled = isOn;
                }
                ApplyRendererPowerState(isOn, 0f, 1f);
                break;

            case FTSMRoomInteractionType.ToggleComputer:
                ApplyRendererPowerState(isOn, 0f, 1f);
                break;

            case FTSMRoomInteractionType.ToggleDoor:
                targetRotation = isOn ? openRotation : closedRotation;
                if (immediate && doorTransform != null)
                    doorTransform.localRotation = targetRotation;
                break;
        }

        if (immediate && animatedTransform != null && interactionType != FTSMRoomInteractionType.ToggleDoor)
            animatedTransform.localRotation = isOn ? toggleOnRotation : toggleOffRotation;
    }

    private void SetState(bool state, bool propagateToLinked)
    {
        if (isOn == state && !propagateToLinked)
            return;

        isOn = state;
        ApplyState(false);
        StateChanged?.Invoke(this, isOn);

        if (!propagateToLinked || linkedInteractables == null)
            return;

        for (int i = 0; i < linkedInteractables.Length; i++)
        {
            FTSMRoomInteractable linked = linkedInteractables[i];
            if (linked != null)
                linked.SetState(state, false);
        }
    }

    private int GetSuggestedPowerWatts()
    {
        string objectName = name.ToLowerInvariant();

        switch (interactionType)
        {
            case FTSMRoomInteractionType.ToggleLight:
                return 180;

            case FTSMRoomInteractionType.ToggleFan:
                return objectName.Contains("small") ? 35 : 75;

            case FTSMRoomInteractionType.ToggleAirConditioner:
                if (objectName.Contains("printer"))
                    return 80;
                if (objectName.Contains("lightswitch"))
                    return 0;
                return 1500;

            case FTSMRoomInteractionType.ToggleComputer:
                if (objectName.Contains("tv"))
                    return 120;
                return 180;

            default:
                return 0;
        }
    }

    private void ApplyRendererPowerState(bool state, float offMultiplier, float onMultiplier)
    {
        for (int i = 0; i < cachedMaterials.Length; i++)
        {
            Material material = cachedMaterials[i];
            if (material == null)
                continue;

            if (screenMaterialIndex >= 0 && offMaterialOverride != null && renderers[i] != null)
            {
                Material[] materials = renderers[i].materials;
                if (screenMaterialIndex < materials.Length)
                {
                    materials[screenMaterialIndex] = state ? cachedScreenOnMaterials[i] : offMaterialOverride;
                    renderers[i].materials = materials;
                }
                continue;
            }

            Color baseColor = cachedColors[i];
            float multiplier = state ? onMultiplier : offMultiplier;
            Color targetColor = baseColor * multiplier;
            targetColor.a = baseColor.a;

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", targetColor);
            material.color = targetColor;

            if (!material.HasProperty("_EmissionColor"))
                continue;

            if (state && cachedEmissionEnabled[i])
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", cachedEmissionColors[i] * onMultiplier);
            }
            else if (state)
            {
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
            }
        }
    }
}
