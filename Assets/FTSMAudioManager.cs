using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class FTSMAudioManager : MonoBehaviour
{
    private const string MusicVolumeKey = "FTSM_MusicVolume";
    private const string EffectsVolumeKey = "FTSM_EffectsVolume";
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip acClip;
    [SerializeField] private AudioClip microwaveClip;
    [SerializeField] private AudioClip switchClip;
    [SerializeField] private AudioClip roomClip;
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip touchClip;
    [SerializeField] private float touchVolume = 0.45f;
    [SerializeField] private AudioClip successfulClip;
    [SerializeField] private AudioClip backgroundMusicClip;
    [SerializeField] private float backgroundMusicVolume = 0.5f;
    [SerializeField] private float defaultMusicVolume = 0.8f;
    [SerializeField] private float defaultEffectsVolume = 1f;
    [SerializeField] private float walkIntervalSeconds = 0.42f;
    [SerializeField] private float walkVolume = 0.35f;
    [SerializeField] private float walkSegmentStartSeconds = 0f;
    [SerializeField] private float walkSegmentDurationSeconds = 0.2f;
    [SerializeField] private float successFadeDelaySeconds = 5f;
    [SerializeField] private float successFadeDurationSeconds = 1.5f;

    private readonly Dictionary<string, List<FTSMRoomInteractable>> roomDevices = new Dictionary<string, List<FTSMRoomInteractable>>(4);
    private readonly HashSet<string> completedRooms = new HashSet<string>();

    private AudioSource effectSource;
    private AudioSource walkSource;
    private AudioSource successSource;
    private AudioSource musicSource;
    private float nextWalkTime;
    private float walkStopTime;
    private float successFadeStartTime;
    private float successFadeEndTime;
    private float successStartVolume = 1f;
    private float musicVolume = 1f;
    private float effectsVolume = 1f;
    private bool fadingSuccess;
    private bool initializedRooms;

    public static FTSMAudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureAudioSources();
        LoadVolumeSettings();
        ApplyVolumeSettings();
        BuildRoomState();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (walkSource != null && walkSource.isPlaying && Time.unscaledTime >= walkStopTime)
            walkSource.Stop();

        UpdateSuccessFade();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        FTSMRoomInteractable.StateChanged += HandleRoomInteractableStateChanged;
        if (!initializedRooms)
            BuildRoomState();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        if (Instance == this)
            FTSMRoomInteractable.StateChanged -= HandleRoomInteractableStateChanged;
    }

    public static void SetMusicVolume(float value)
    {
        float clampedValue = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicVolumeKey, clampedValue);
        PlayerPrefs.Save();

        if (Instance == null)
            return;

        Instance.musicVolume = clampedValue;
        Instance.ApplyVolumeSettings();
    }

    public static void SetEffectsVolume(float value)
    {
        float clampedValue = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(EffectsVolumeKey, clampedValue);
        PlayerPrefs.Save();

        if (Instance == null)
            return;

        Instance.effectsVolume = clampedValue;
        Instance.ApplyVolumeSettings();
    }

    public static float GetMusicVolume()
    {
        return Instance != null
            ? Instance.musicVolume
            : Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, 0.8f));
    }

    public static float GetEffectsVolume()
    {
        return Instance != null
            ? Instance.effectsVolume
            : Mathf.Clamp01(PlayerPrefs.GetFloat(EffectsVolumeKey, 1f));
    }

    public static void PlayMenuClick()
    {
        if (Instance != null)
            Instance.PlayClip(Instance.clickClip);
    }

    public static void PlayAc()
    {
        if (Instance != null)
            Instance.PlayClip(Instance.acClip);
    }

    public static void PlayMicrowave()
    {
        if (Instance != null)
            Instance.PlayClip(Instance.microwaveClip);
    }

    public static void PlaySuccessful()
    {
        if (Instance == null || Instance.successSource == null || Instance.successfulClip == null)
            return;

        Instance.successSource.Stop();
        Instance.successSource.ignoreListenerPause = true;
        Instance.successSource.clip = Instance.successfulClip;
        Instance.successSource.time = 0f;
        Instance.successSource.volume = Instance.effectsVolume;
        Instance.successStartVolume = Instance.successSource.volume;
        Instance.successFadeStartTime = Time.unscaledTime + Mathf.Max(0f, Instance.successFadeDelaySeconds);
        Instance.successFadeEndTime = Instance.successFadeStartTime + Mathf.Max(0.01f, Instance.successFadeDurationSeconds);
        Instance.fadingSuccess = true;
        Instance.successSource.Play();
    }

    public static void ReportWalk(bool moving)
    {
        if (Instance != null)
            Instance.HandleWalk(moving);
    }

    public static void StopWalk()
    {
        if (Instance != null && Instance.walkSource != null && Instance.walkSource.isPlaying)
            Instance.walkSource.Stop();
    }

    public static void PlayTouch()
    {
        if (Instance != null)
            Instance.PlayClip(Instance.touchClip, Instance.touchVolume);
    }

    public static void PlayRoomInteraction(FTSMRoomInteractable interactable)
    {
        if (Instance == null || interactable == null)
            return;

        string objectName = interactable.name.ToLowerInvariant();
        if (objectName.Contains("microwave"))
        {
            Instance.PlayClip(Instance.microwaveClip);
            return;
        }

        if (objectName.Contains("air_conditioner") || objectName.Contains("airconditioner") || objectName == "ac")
        {
            Instance.PlayClip(Instance.acClip);
            return;
        }

        if (objectName.Contains("lightswitch"))
        {
            Instance.PlayClip(Instance.switchClip);
            return;
        }

        Instance.PlayClip(Instance.touchClip, Instance.touchVolume);
    }

    private void HandleWalk(bool moving)
    {
        if (!moving)
        {
            StopWalk();
            return;
        }

        if (walkClip == null || walkSource == null || Time.unscaledTime < nextWalkTime)
            return;

        walkSource.clip = walkClip;
        walkSource.volume = walkVolume * effectsVolume;
        walkSource.time = Mathf.Clamp(walkSegmentStartSeconds, 0f, Mathf.Max(0f, walkClip.length - 0.01f));
        walkSource.Play();
        walkStopTime = Time.unscaledTime + Mathf.Max(0.05f, walkSegmentDurationSeconds);
        nextWalkTime = Time.unscaledTime + walkIntervalSeconds;
    }

    private void HandleRoomInteractableStateChanged(FTSMRoomInteractable interactable, bool isOn)
    {
        if (interactable == null || isOn)
            return;

        if (!initializedRooms)
            BuildRoomState();

        string roomKey = GetRoomKey(interactable.transform);
        if (string.IsNullOrEmpty(roomKey) || completedRooms.Contains(roomKey))
            return;

        if (!roomDevices.TryGetValue(roomKey, out List<FTSMRoomInteractable> devices) || devices.Count == 0)
            return;

        for (int i = 0; i < devices.Count; i++)
        {
            FTSMRoomInteractable device = devices[i];
            if (device != null && device.IsOn)
                return;
        }

        completedRooms.Add(roomKey);
        PlayClip(roomClip);
    }

    private void BuildRoomState()
    {
        roomDevices.Clear();
        completedRooms.Clear();

        FTSMRoomInteractable[] interactables = FindObjectsByType<FTSMRoomInteractable>(FindObjectsSortMode.None);
        for (int i = 0; i < interactables.Length; i++)
        {
            FTSMRoomInteractable interactable = interactables[i];
            if (!ShouldTrackRoomCompletion(interactable))
                continue;

            string roomKey = GetRoomKey(interactable.transform);
            if (string.IsNullOrEmpty(roomKey))
                continue;

            if (!roomDevices.TryGetValue(roomKey, out List<FTSMRoomInteractable> devices))
            {
                devices = new List<FTSMRoomInteractable>(16);
                roomDevices.Add(roomKey, devices);
            }

            devices.Add(interactable);
        }

        initializedRooms = true;
    }

    private static bool ShouldTrackRoomCompletion(FTSMRoomInteractable interactable)
    {
        return interactable != null
            && interactable.IncludeInObjectiveUi
            && interactable.InteractionType != FTSMRoomInteractionType.ToggleDoor
            && interactable.PowerWatts > 0;
    }

    private static string GetRoomKey(Transform target)
    {
        while (target != null)
        {
            switch (target.name)
            {
                case "MP1_Room":
                case "BK1_Room":
                case "Staff_Office_Group":
                case "corridor":
                    return target.name;
            }

            target = target.parent;
        }

        return null;
    }

    private void PlayClip(AudioClip clip)
    {
        PlayClip(clip, 1f);
    }

    private void UpdateSuccessFade()
    {
        if (!fadingSuccess || successSource == null)
            return;

        if (!successSource.isPlaying)
        {
            fadingSuccess = false;
            successSource.volume = successStartVolume;
            return;
        }

        float time = Time.unscaledTime;
        if (time < successFadeStartTime)
            return;

        float duration = Mathf.Max(0.01f, successFadeEndTime - successFadeStartTime);
        float t = Mathf.Clamp01((time - successFadeStartTime) / duration);
        successSource.volume = Mathf.Lerp(successStartVolume, 0f, t);

        if (t < 1f)
            return;

        successSource.Stop();
        successSource.volume = successStartVolume;
        fadingSuccess = false;
    }

    private void PlayClip(AudioClip clip, float volumeScale)
    {
        if (effectSource == null || clip == null)
            return;

        effectSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale) * effectsVolume);
    }

    private void EnsureAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        effectSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
        walkSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();
        successSource = sources.Length > 2 ? sources[2] : gameObject.AddComponent<AudioSource>();
        musicSource = sources.Length > 3 ? sources[3] : gameObject.AddComponent<AudioSource>();
        ConfigureSource(effectSource);
        ConfigureSource(walkSource);
        ConfigureSource(successSource);
        ConfigureSource(musicSource);
        walkSource.volume = walkVolume;
        successSource.ignoreListenerPause = true;
        musicSource.loop = true;
        musicSource.ignoreListenerPause = true;
        musicSource.clip = backgroundMusicClip;
        if (backgroundMusicClip != null && !musicSource.isPlaying)
            musicSource.Play();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        initializedRooms = false;
        BuildRoomState();
        ApplyVolumeSettings();
    }

    private void LoadVolumeSettings()
    {
        musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, defaultMusicVolume));
        effectsVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(EffectsVolumeKey, defaultEffectsVolume));
    }

    private void ApplyVolumeSettings()
    {
        if (walkSource != null)
            walkSource.volume = walkVolume * effectsVolume;

        if (successSource != null && !fadingSuccess)
            successSource.volume = effectsVolume;

        if (musicSource != null)
        {
            if (musicSource.clip != backgroundMusicClip)
                musicSource.clip = backgroundMusicClip;

            musicSource.volume = backgroundMusicVolume * musicVolume;
            if (backgroundMusicClip != null && !musicSource.isPlaying)
                musicSource.Play();
        }
    }

    private static void ConfigureSource(AudioSource source)
    {
        if (source == null)
            return;

        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
    }
}
