using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{

    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown displayModeDropdown; // 0 Fullscreen,1 Windowed,2 Borderless
    public TMP_Dropdown frameRateDropdown;   // 0 Unlimited,1=30,2=60,3=120...
    public Toggle vsyncToggle;

    void Start()
    {
        var mgr = SettingsManager.Instance;

        InitResolutionDropdown();

        displayModeDropdown.value = PlayerPrefs.GetInt(SettingsKeys.DisplayMode, 0);

        int fps = PlayerPrefs.GetInt(SettingsKeys.FrameRateLimit, 60);
        frameRateDropdown.value = fps switch
        {
            0 => 0, 30 => 1, 60 => 2, 120 => 3, 240 =>4 ,_ => 2
        };

        vsyncToggle.isOn = PlayerPrefs.GetInt(SettingsKeys.VSync, 1) == 1;

        resolutionDropdown.onValueChanged.AddListener(mgr.SetResolutionIndex);
        displayModeDropdown.onValueChanged.AddListener(mgr.SetDisplayMode);

        frameRateDropdown.onValueChanged.AddListener(OnFrameRateChanged);
        vsyncToggle.onValueChanged.AddListener(mgr.SetVSync);
    }

    void InitResolutionDropdown()
    {
        var mgr = SettingsManager.Instance;
        Resolution[] res = mgr.GetResolutions();

        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        for (int i = res.Length - 1; i >= 0 ; i--)
        {
            Resolution r = res[i];
            string label = $"{r.width} x {r.height} @ {r.refreshRateRatio.value:F0}Hz";
            options.Add(label);
        }

        resolutionDropdown.AddOptions(options);

        int savedIndex = PlayerPrefs.GetInt(SettingsKeys.ResolutionIndex, 0);
        resolutionDropdown.value = Mathf.Clamp(savedIndex, 0, res.Length - 1);
        resolutionDropdown.RefreshShownValue();
    }

    void OnFrameRateChanged(int index)
    {
        int fps = index switch { 0 => 0, 1 => 30, 2 => 60, 3 => 120, 4 => 240, _ => 60 };
        SettingsManager.Instance.SetFrameRateLimit(fps);
    }
}