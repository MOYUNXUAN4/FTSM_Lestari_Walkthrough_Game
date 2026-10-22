using UnityEngine;
using UnityEngine.UI;

public sealed class FTSMPowerGame : MonoBehaviour
{
    [SerializeField] private Text powerText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text messageText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Text resultText;
    [SerializeField] private FTSMInteractable exitDoor;
    [SerializeField] private int targetSafeWatts = 100;
    [SerializeField] private float timeLimitSeconds = 180f;

    private int initialWatts;
    private int currentWatts;
    private float remainingSeconds;
    private bool ended;
    private bool exitUnlocked;
    private float messageUntil;
    private bool subscribed;

    public int CurrentWatts => currentWatts;

    public void Setup(Text power, Text timer, Text message, GameObject panel, Text result, FTSMInteractable door, int target, float seconds)
    {
        powerText = power;
        timerText = timer;
        messageText = message;
        resultPanel = panel;
        resultText = result;
        exitDoor = door;
        targetSafeWatts = target;
        timeLimitSeconds = seconds;
    }

    public void RegisterInitialLoad(int watts)
    {
        initialWatts += watts;
        currentWatts += watts;
        remainingSeconds = timeLimitSeconds;
        UpdateHud();
    }

    private void Start()
    {
        remainingSeconds = timeLimitSeconds;
        RecalculateInitialLoad();
        if (resultPanel != null)
            resultPanel.SetActive(false);
        UpdateHud();
        ShowTemporaryMessage("任务：关闭所有房间非必要电器，把耗电降到100W。", 4f);
    }

    private void OnEnable()
    {
        if (subscribed)
            return;

        FTSMInteractable.Interacted += HandleDeviceInteracted;
        subscribed = true;
    }

    private void OnDisable()
    {
        if (!subscribed)
            return;

        FTSMInteractable.Interacted -= HandleDeviceInteracted;
        subscribed = false;
    }

    private void RecalculateInitialLoad()
    {
        initialWatts = 0;
        currentWatts = 0;
        FTSMInteractable[] devices = FindObjectsByType<FTSMInteractable>(FindObjectsSortMode.None);
        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].DeviceType == FTSMDeviceType.ExitDoor || !devices[i].IsOn)
                continue;

            initialWatts += devices[i].Wattage;
            currentWatts += devices[i].Wattage;
        }
    }

    private void Update()
    {
        if (ended)
            return;

        remainingSeconds -= Time.deltaTime;
        if (remainingSeconds <= 0f)
        {
            remainingSeconds = 0f;
            if (currentWatts > targetSafeWatts)
                FailGame();
        }

        if (messageText != null && Time.time > messageUntil)
            messageText.text = string.Empty;

        UpdateHud();
    }

    private void HandleDeviceInteracted(FTSMInteractable device)
    {
        if (device == null)
            return;

        string objectName = device.name.ToLowerInvariant();
        if (objectName.Contains("microwave"))
            FTSMAudioManager.PlayMicrowave();
    }

    public void RegisterShutdown(FTSMInteractable device)
    {
        currentWatts -= device.Wattage;
        if (currentWatts < 0)
            currentWatts = 0;

        ShowTemporaryMessage($"已关闭：{device.name}  -{device.Wattage}W", 1.5f);

        if (!exitUnlocked && currentWatts <= targetSafeWatts)
        {
            exitUnlocked = true;
            if (exitDoor != null)
                exitDoor.Unlock();
            ShowTemporaryMessage("安全出口已解锁！前往走廊尽头点击绿色大门。", 5f);
        }

        UpdateHud();
    }

    public void ShowTemporaryMessage(string message, float seconds)
    {
        if (messageText == null)
            return;
        messageText.text = message;
        messageUntil = Time.time + seconds;
    }

    public void WinGame()
    {
        if (ended)
            return;
        ended = true;
        FTSMAudioManager.PlaySuccessful();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (resultPanel != null)
            resultPanel.SetActive(true);
        resultText.text = $"FTSM 巡检完成！\n你成功为学校节省了 {initialWatts - currentWatts}W 电力！\n绿色大使奖章：UKM ECO CHAMPION";
    }

    private void FailGame()
    {
        ended = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (resultPanel != null)
            resultPanel.SetActive(true);
        resultText.text = "电力过载，巡检失败。\n请重开场景并更快关闭设备。";
    }

    private void UpdateHud()
    {
        if (powerText != null)
        {
            powerText.text = $"当前耗电量：{currentWatts}W / 目标安全值：{targetSafeWatts}W";
            powerText.color = currentWatts <= targetSafeWatts ? new Color(0.1f, 0.8f, 0.2f) : Color.red;
        }

        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(remainingSeconds);
            timerText.text = $"倒计时：{seconds / 60:00}:{seconds % 60:00}";
        }
    }
}
