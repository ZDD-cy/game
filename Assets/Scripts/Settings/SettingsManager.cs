using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [Header("Defaults")]

    public int defaultResolutionIndex = 0;
    public int defaultDisplayMode = 0; // 0 Fullscreen,1 Windowed,2 Borderless
    public int defaultFrameRateLimit = 60;
    public int defaultVSync = 1;

    public static SettingsManager Instance { get; private set; }

    Resolution[] resolutions;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        resolutions = Screen.resolutions;

        InitDefaultsIfNeeded();
        ApplyVideo();
    }

    void InitDefaultsIfNeeded()
    {
        if (PlayerPrefs.GetInt(SettingsKeys.Initialized, 0) == 1) return;

        PlayerPrefs.SetInt(SettingsKeys.ResolutionIndex, Mathf.Clamp(defaultResolutionIndex, 0, resolutions.Length - 1));
        PlayerPrefs.SetInt(SettingsKeys.DisplayMode, defaultDisplayMode);
        PlayerPrefs.SetInt(SettingsKeys.FrameRateLimit, defaultFrameRateLimit);
        PlayerPrefs.SetInt(SettingsKeys.VSync, defaultVSync);

        PlayerPrefs.SetInt(SettingsKeys.Initialized, 1);
        PlayerPrefs.Save();
    }

    public void ApplyVideo()
    {
        int resIndex = PlayerPrefs.GetInt(SettingsKeys.ResolutionIndex, 0);
        resIndex = Mathf.Clamp(resIndex, 0, resolutions.Length - 1);
        Resolution r = resolutions[resIndex];

        int mode = PlayerPrefs.GetInt(SettingsKeys.DisplayMode, 0);
        FullScreenMode fsMode = mode switch
        {
            0 => FullScreenMode.ExclusiveFullScreen,
            1 => FullScreenMode.Windowed,
            2 => FullScreenMode.FullScreenWindow,
            _ => FullScreenMode.ExclusiveFullScreen
        };

        Screen.SetResolution(r.width, r.height, fsMode, r.refreshRateRatio);

        int vsync = PlayerPrefs.GetInt(SettingsKeys.VSync, 1);
        QualitySettings.vSyncCount = vsync;

        int limit = PlayerPrefs.GetInt(SettingsKeys.FrameRateLimit, 60);
        Application.targetFrameRate = (limit <= 0) ? -1 : limit;
    }

    public Resolution[] GetResolutions() => resolutions;
    
    public void SetResolutionIndex(int index) { PlayerPrefs.SetInt(SettingsKeys.ResolutionIndex, index); PlayerPrefs.Save(); ApplyVideo(); }
    public void SetDisplayMode(int mode) { PlayerPrefs.SetInt(SettingsKeys.DisplayMode, mode); PlayerPrefs.Save(); ApplyVideo(); }
    public void SetFrameRateLimit(int fps) { PlayerPrefs.SetInt(SettingsKeys.FrameRateLimit, fps); PlayerPrefs.Save(); ApplyVideo(); }
    public void SetVSync(bool on) { PlayerPrefs.SetInt(SettingsKeys.VSync, on ? 1 : 0); PlayerPrefs.Save(); ApplyVideo(); }
}