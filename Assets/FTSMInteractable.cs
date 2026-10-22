using System;
using UnityEngine;

public enum FTSMDeviceType
{
    Computer,
    Laptop,
    Projector,
    AirConditioner,
    LightSwitch,
    FanSwitch,
    ExitDoor
}

public sealed class FTSMInteractable : MonoBehaviour
{
    public static event Action<FTSMInteractable> Interacted;

    [SerializeField] private FTSMDeviceType deviceType;
    [SerializeField] private int wattage = 100;
    [SerializeField] private Renderer[] renderersToDim;
    [SerializeField] private Light[] lightsToDisable;
    [SerializeField] private FTSMSpin[] fansToStop;
    [SerializeField] private GameObject[] activeMarkers;
    [SerializeField] private AudioSource audioSource;

    private bool isOn = true;
    private bool isLocked;
    [SerializeField] private FTSMPowerGame game;

    public FTSMDeviceType DeviceType => deviceType;
    public bool IsOn => isOn;
    public int Wattage => wattage;

    public void Initialize(FTSMPowerGame owner, FTSMDeviceType type, int watts, bool locked = false)
    {
        game = owner;
        deviceType = type;
        wattage = watts;
        isLocked = locked;
        if (type == FTSMDeviceType.ExitDoor)
            isOn = false;
    }

    public void SetVisualTargets(Renderer[] dimTargets, Light[] lightTargets, FTSMSpin[] fanTargets, GameObject[] markers)
    {
        renderersToDim = dimTargets;
        lightsToDisable = lightTargets;
        fansToStop = fanTargets;
        activeMarkers = markers;
    }

    public void SetAudio(AudioSource source)
    {
        audioSource = source;
    }

    public void Unlock()
    {
        isLocked = false;
        SetExitDoorVisual(true);
    }

    public void Interact()
    {
        if (isLocked)
        {
            game.ShowTemporaryMessage("先把非必要电器全部关闭，再打开安全出口。", 2f);
            return;
        }

        if (deviceType == FTSMDeviceType.ExitDoor)
        {
            Interacted?.Invoke(this);
            game.WinGame();
            return;
        }

        if (!isOn)
            return;

        isOn = false;
        Interacted?.Invoke(this);
        if (audioSource != null)
            audioSource.Play();

        for (int i = 0; i < renderersToDim.Length; i++)
        {
            if (renderersToDim[i] == null)
                continue;
            var mat = renderersToDim[i].material;
            mat.color = Color.black;
            mat.DisableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.black);
        }

        for (int i = 0; i < lightsToDisable.Length; i++)
        {
            if (lightsToDisable[i] != null)
                lightsToDisable[i].enabled = false;
        }

        for (int i = 0; i < fansToStop.Length; i++)
        {
            if (fansToStop[i] != null)
                fansToStop[i].StopSpinning();
        }

        for (int i = 0; i < activeMarkers.Length; i++)
        {
            if (activeMarkers[i] != null)
                activeMarkers[i].SetActive(false);
        }

        game.RegisterShutdown(this);
    }

    private void SetExitDoorVisual(bool unlocked)
    {
        for (int i = 0; i < renderersToDim.Length; i++)
        {
            if (renderersToDim[i] == null)
                continue;
            renderersToDim[i].material.color = unlocked ? new Color(0.1f, 0.8f, 0.2f) : new Color(0.25f, 0.25f, 0.25f);
        }
    }
}
