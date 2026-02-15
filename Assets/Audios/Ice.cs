// ============================================================================
// SFXEntry.cs — 音效配置数据（ScriptableObject）
//
// 在 Unity 中通过 Assets → Create → Audio → SFX Entry 创建
// 每个音效对应一个 SFXEntry 资产文件，可在 Inspector 中配置参数
// ============================================================================

using UnityEngine;

/// <summary>
/// 单个音效的配置信息。
/// 通过 ScriptableObject 实现数据驱动，方便在 Inspector 中调整参数。
/// </summary>
[CreateAssetMenu(fileName = "NewSFXEntry", menuName = "Audio/SFX Entry")]
public class SFXEntry : ScriptableObject
{
    // ===== 基本信息 =====

    [Header("基本信息")]
    [Tooltip("音效的唯一标识符，代码中通过这个 ID 调用播放")]
    public string sfxID;              // 如 "UI_Click"、"Explosion"

    [Tooltip("音频片段（可设置多个用于随机选取）")]
    public AudioClip[] clips;         // 支持多个变体，随机播放增加多样性

    // ===== 播放参数 =====

    [Header("播放参数")]
    [Tooltip("基础音量 (0~1)，最终音量 = 此值 × 全局 SFX 音量")]
    [Range(0f, 1f)]
    public float volume = 0.5f;

    [Tooltip("随机音高偏移范围 (0 = 无偏移)。设为 0.05 表示音高在 0.95~1.05 间随机")]
    [Range(0f, 0.3f)]
    public float pitchVariation = 0.02f;

    [Tooltip("空间混合 (0 = 纯2D, 1 = 纯3D)。2D 游戏设为 0，3D 游戏可按需调整")]
    [Range(0f, 1f)]
    public float spatialBlend = 0f;

    // ===== 防叠加 (Anti-Spam) =====

    [Header("防叠加 (Anti-Spam)")]
    [Tooltip("两次播放之间的最短间隔（秒）。防止同一音效连续播放造成噪音")]
    public float cooldown = 0.1f;

    [Tooltip("同时播放的最大实例数。如击打音设为 2~3 表示最多同时听到该数量的声音")]
    [Range(1, 8)]
    public int maxConcurrent = 1;

    // ===== 优先级 =====

    [Header("优先级 (1=最高, 5=最低)")]
    [Tooltip("当对象池满时，高优先级音效可以抢占低优先级的 AudioSource")]
    [Range(1, 5)]
    public int priority = 3;

    /// <summary>
    /// 从 clips 数组中随机获取一个 AudioClip。
    /// 如果只有一个则直接返回，多个则随机选取以增加音效多样性。
    /// </summary>
    /// <returns>随机选取的 AudioClip，如果数组为空则返回 null</returns>
    public AudioClip GetRandomClip()
    {
        // 空数组保护
        if (clips == null || clips.Length == 0) return null;
        // 单个直接返回，多个随机选取
        if (clips.Length == 1) return clips[0];
        return clips[Random.Range(0, clips.Length)];
    }
}
