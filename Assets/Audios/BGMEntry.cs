// ============================================================================
// BGMEntry.cs — BGM 配置数据（ScriptableObject）
//
// 在 Unity 中通过 Assets → Create → Audio → BGM Entry 创建
// 每首 BGM 对应一个 BGMEntry 资产文件，支持任意数量的背景音乐轨道
// ============================================================================

using UnityEngine;

/// <summary>
/// 单首背景音乐的配置信息。
/// 通过 ScriptableObject 实现数据驱动，不再硬编码 BGM 状态枚举。
/// </summary>
[CreateAssetMenu(fileName = "NewBGMEntry", menuName = "Audio/BGM Entry")]
public class BGMEntry : ScriptableObject
{
    [Header("基本信息")]
    [Tooltip("BGM 的唯一标识符，代码中通过这个 ID 调用播放")]
    public string bgmID;              // 如 "MainMenu"、"Business"、"Boss"

    [Tooltip("BGM 音频片段")]
    public AudioClip clip;

    [Header("播放参数")]
    [Tooltip("基础音量 (0~1)，最终音量 = 此值 × 全局 BGM 音量")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Tooltip("是否循环播放（绝大多数 BGM 应设为 true）")]
    public bool loop = true;

    [Tooltip("自定义淡入淡出时长（秒）。设为 -1 则使用 AudioManager 的全局默认值")]
    public float customFadeDuration = -1f;
}
