public static class SettingsKeys
{
    public const string Initialized = "settings_initialized";

    public const string MasterVolume = "audio_master";
    public const string BgmVolume = "audio_bgm";
    public const string SfxVolume = "audio_sfx";

    public const string ResolutionIndex = "video_resolution_index";
    public const string DisplayMode = "video_display_mode"; // 0=Fullscreen,1=Windowed,2=Borderless
    public const string FrameRateLimit = "video_frame_rate_limit"; // 0=Unlimited,30,60,120...
    public const string VSync = "video_vsync"; // 0/1
}