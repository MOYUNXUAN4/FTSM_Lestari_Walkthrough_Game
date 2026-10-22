using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class FTSMStartMenu : MonoBehaviour
{
    private const string NoVrModePreferenceKey = "FTSM.NoVRMode";
    private static bool startImmediatelyOnLoad;

    [SerializeField] private Camera startCamera;
    [SerializeField] private GameObject playerRoot;
    [SerializeField] private GameObject noVrPlayerRoot;
    [SerializeField] private GameObject vrPlayerRoot;
    [SerializeField] private GameObject vrSimulatorRoot;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Canvas crosshairCanvas;
    [SerializeField] private Canvas vrCrosshairCanvas;
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider effectsSlider;
    [SerializeField] private Toggle noVrModeToggle;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Slider pauseMusicSlider;
    [SerializeField] private Slider pauseEffectsSlider;
    [SerializeField] private Button backToMenuButton;

    private readonly List<Button> managedButtons = new List<Button>(4);
    private bool started;
    private bool isPauseOpen;

    public static FTSMStartMenu Instance { get; private set; }
    public bool IsPauseOpen => isPauseOpen;

    public static void RequestGameRestart()
    {
        startImmediatelyOnLoad = true;
    }

    public static void RequestReturnToMenu()
    {
        startImmediatelyOnLoad = false;
    }

    public static bool TryTogglePause()
    {
        if (Instance == null || !Instance.IsGameplayActive())
            return false;

        if (Instance.isPauseOpen)
            Instance.ClosePauseMenu();
        else
            Instance.OpenPauseMenu();

        return true;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            return;

        TryTogglePause();
    }

    private void Awake()
    {
        Instance = this;
        started = false;
        isPauseOpen = false;

        BindButton(playButton, HandlePlayClicked);
        BindButton(settingsButton, HandleSettingsClicked);
        BindButton(exitButton, HandleExitClicked);
        BindButton(backToMenuButton, HandleBackToMenuClicked);

        BindMusicSlider(musicSlider);
        BindMusicSlider(pauseMusicSlider);
        BindEffectsSlider(effectsSlider);
        BindEffectsSlider(pauseEffectsSlider);

        if (noVrModeToggle != null)
        {
            noVrModeToggle.onValueChanged.RemoveListener(HandleNoVrModeToggleChanged);
            noVrModeToggle.onValueChanged.AddListener(HandleNoVrModeToggleChanged);
            noVrModeToggle.SetIsOnWithoutNotify(IsNoVrModeEnabled());
        }

        SyncVolumeSliders();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        ShowMenu();

        if (startImmediatelyOnLoad)
        {
            startImmediatelyOnLoad = false;
            HandlePlayClicked();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        for (int i = 0; i < managedButtons.Count; i++)
        {
            Button button = managedButtons[i];
            if (button == null)
                continue;

            if (button == playButton)
                button.onClick.RemoveListener(HandlePlayClicked);
            else if (button == settingsButton)
                button.onClick.RemoveListener(HandleSettingsClicked);
            else if (button == exitButton)
                button.onClick.RemoveListener(HandleExitClicked);
            else if (button == backToMenuButton)
                button.onClick.RemoveListener(HandleBackToMenuClicked);
        }

        UnbindSlider(musicSlider);
        UnbindSlider(pauseMusicSlider);
        UnbindSlider(effectsSlider);
        UnbindSlider(pauseEffectsSlider);

        if (noVrModeToggle != null)
            noVrModeToggle.onValueChanged.RemoveListener(HandleNoVrModeToggleChanged);
    }

    private void ShowMenu()
    {
        started = false;
        isPauseOpen = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameObject noVrRoot = GetNoVrPlayerRoot();
        if (noVrRoot != null)
            noVrRoot.SetActive(false);

        if (vrPlayerRoot != null)
            vrPlayerRoot.SetActive(false);

        if (vrSimulatorRoot != null)
            vrSimulatorRoot.SetActive(false);

        if (crosshairCanvas != null)
            crosshairCanvas.gameObject.SetActive(false);

        if (vrCrosshairCanvas != null)
            vrCrosshairCanvas.gameObject.SetActive(false);

        SetMainMenuVisualsVisible(true);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        SetCameraState(playerCamera, false);
        SetCameraState(GetVrCamera(), false);
        SetCameraState(startCamera, true);

        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(true);
    }

    private void HandlePlayClicked()
    {
        if (started)
            return;

        started = true;
        FTSMAudioManager.PlayMenuClick();
        StartCoroutine(StartGameAfterClick());
    }

    private void HandleSettingsClicked()
    {
        if (settingsPanel == null)
            return;

        FTSMAudioManager.PlayMenuClick();
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    private void HandleExitClicked()
    {
        FTSMAudioManager.PlayMenuClick();
        Application.Quit();
    }

    private void HandleBackToMenuClicked()
    {
        ReturnToMenuFromPause();
    }

    private void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
        managedButtons.Add(button);
    }

    private void BindMusicSlider(Slider slider)
    {
        if (slider == null)
            return;

        slider.onValueChanged.RemoveListener(HandleMusicSliderChanged);
        slider.onValueChanged.AddListener(HandleMusicSliderChanged);
    }

    private void BindEffectsSlider(Slider slider)
    {
        if (slider == null)
            return;

        slider.onValueChanged.RemoveListener(HandleEffectsSliderChanged);
        slider.onValueChanged.AddListener(HandleEffectsSliderChanged);
    }

    private void UnbindSlider(Slider slider)
    {
        if (slider == null)
            return;

        slider.onValueChanged.RemoveListener(HandleMusicSliderChanged);
        slider.onValueChanged.RemoveListener(HandleEffectsSliderChanged);
    }

    private void SyncVolumeSliders()
    {
        float musicVolume = FTSMAudioManager.GetMusicVolume();
        float effectsVolume = FTSMAudioManager.GetEffectsVolume();

        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(musicVolume);
        if (pauseMusicSlider != null)
            pauseMusicSlider.SetValueWithoutNotify(musicVolume);
        if (effectsSlider != null)
            effectsSlider.SetValueWithoutNotify(effectsVolume);
        if (pauseEffectsSlider != null)
            pauseEffectsSlider.SetValueWithoutNotify(effectsVolume);
    }

    private void HandleMusicSliderChanged(float value)
    {
        FTSMAudioManager.SetMusicVolume(value);
        SyncVolumeSliders();
    }

    private void HandleEffectsSliderChanged(float value)
    {
        FTSMAudioManager.SetEffectsVolume(value);
        SyncVolumeSliders();
    }

    private void HandleNoVrModeToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt(NoVrModePreferenceKey, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private bool IsNoVrModeEnabled()
    {
        return PlayerPrefs.GetInt(NoVrModePreferenceKey, 1) != 0;
    }

    private GameObject GetNoVrPlayerRoot()
    {
        return noVrPlayerRoot != null ? noVrPlayerRoot : playerRoot;
    }

    private bool IsGameplayActive()
    {
        GameObject noVrRoot = GetNoVrPlayerRoot();
        if (noVrRoot != null && noVrRoot.activeInHierarchy)
            return true;

        if (vrPlayerRoot != null && vrPlayerRoot.activeInHierarchy)
            return true;

        if (playerCamera != null && playerCamera.enabled && playerCamera.gameObject.activeInHierarchy)
            return true;

        Camera vrCamera = GetVrCamera();
        return vrCamera != null && vrCamera.enabled && vrCamera.gameObject.activeInHierarchy;
    }

    private void ActivateSelectedPlayerRig()
    {
        bool useNoVrMode = IsNoVrModeEnabled();
        GameObject noVrRoot = GetNoVrPlayerRoot();
        Camera vrCamera = GetVrCamera();

        if (useNoVrMode)
        {
            if (noVrRoot != null)
                noVrRoot.SetActive(true);

            if (vrPlayerRoot != null)
                vrPlayerRoot.SetActive(false);

            if (vrSimulatorRoot != null)
                vrSimulatorRoot.SetActive(false);

            SetCameraState(playerCamera, true);
            SetCameraState(vrCamera, false);

            if (crosshairCanvas != null)
                crosshairCanvas.gameObject.SetActive(true);

            if (vrCrosshairCanvas != null)
                vrCrosshairCanvas.gameObject.SetActive(false);
            return;
        }

        if (noVrRoot != null)
            noVrRoot.SetActive(false);

        if (crosshairCanvas != null)
            crosshairCanvas.gameObject.SetActive(false);

        SetCameraState(playerCamera, false);

        if (vrPlayerRoot != null)
        {
            if (noVrRoot != null)
                vrPlayerRoot.transform.SetPositionAndRotation(noVrRoot.transform.position, noVrRoot.transform.rotation);

            CharacterController vrCharacterController = vrPlayerRoot.GetComponent<CharacterController>();
            if (vrCharacterController != null)
                vrCharacterController.enabled = false;

            vrPlayerRoot.SetActive(true);

            if (vrCharacterController != null)
                vrCharacterController.enabled = true;
        }

        if (vrSimulatorRoot != null)
            vrSimulatorRoot.SetActive(true);

        SetCameraState(vrCamera, true);

        if (vrCrosshairCanvas != null)
            vrCrosshairCanvas.gameObject.SetActive(true);
    }

    private IEnumerator StartGameAfterClick()
    {
        yield return new WaitForSecondsRealtime(0.2f);

        bool useNoVrMode = IsNoVrModeEnabled();
        if (!useNoVrMode)
            yield return null;

        ActivateSelectedPlayerRig();
        ClosePauseMenuImmediate();

        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (startCamera != null)
            SetCameraState(startCamera, false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OpenPauseMenu()
    {
        isPauseOpen = true;
        Time.timeScale = 0f;
        FTSMAudioManager.StopWalk();
        SyncVolumeSliders();

        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(true);

        SetMainMenuVisualsVisible(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (pausePanel != null)
            pausePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ClosePauseMenu()
    {
        ClosePauseMenuImmediate();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ClosePauseMenuImmediate()
    {
        isPauseOpen = false;
        Time.timeScale = 1f;
        SetMainMenuVisualsVisible(true);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (menuCanvas != null && started)
            menuCanvas.gameObject.SetActive(false);
    }

    private void SetMainMenuVisualsVisible(bool visible)
    {
        Transform panelRoot = playButton != null ? playButton.transform.parent : null;
        if (panelRoot == null)
            return;

        for (int i = 0; i < panelRoot.childCount; i++)
        {
            Transform child = panelRoot.GetChild(i);
            if (pausePanel != null && child == pausePanel.transform)
                continue;
            if (settingsPanel != null && child == settingsPanel.transform)
                continue;

            child.gameObject.SetActive(visible);
        }
    }

    private void ReturnToMenuFromPause()
    {
        ClosePauseMenuImmediate();
        FTSMAudioManager.PlayMenuClick();
        ShowMenu();
    }

    private IEnumerator ReturnToMenuAfterClick()
    {
        ClosePauseMenuImmediate();
        FTSMAudioManager.PlayMenuClick();
        RequestReturnToMenu();
        yield return new WaitForSecondsRealtime(0.2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private Camera GetVrCamera()
    {
        return vrPlayerRoot != null ? vrPlayerRoot.GetComponentInChildren<Camera>(true) : null;
    }

    private static void SetCameraState(Camera cameraComponent, bool enabled)
    {
        if (cameraComponent == null)
            return;

        cameraComponent.enabled = enabled;

        AudioListener audioListener = cameraComponent.GetComponent<AudioListener>();
        if (audioListener != null)
            audioListener.enabled = enabled;
    }
}
