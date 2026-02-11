// ============================================================================
// AudioManager.cs — 通用音频管理器
//
// 功能：
//   1. 单例模式，跨场景持久（DontDestroyOnLoad）
//   2. SFX 对象池：预创建固定数量 AudioSource，零 GC 播放音效
//   3. 防叠加系统：每个音效可配置冷却时间 + 最大并发数
//   4. BGM 系统：支持任意数量 BGM 轨道，协程驱动交叉淡化
//   5. 音量管理：通过 AudioMixer 统一控制，PlayerPrefs 持久化
//   6. 全局暂停/恢复、3D 音效支持、事件回调
//
// 使用方法：
//   AudioManager.Instance.PlaySFX("Explosion");              // 播放音效
//   AudioManager.Instance.PlaySFX("Footstep", worldPos);     // 播放 3D 音效
//   AudioManager.Instance.PlayBGM("Battle");                  // 切换 BGM
//   AudioManager.Instance.StopBGM();                          // 停止 BGM
//   AudioManager.Instance.SetMasterVolume(0.8f);              // 设置音量
//
// 快速上手：
//   1. 在场景中创建空物体，挂载此脚本
//   2. （可选）拖入 AudioMixer 资产（需 Expose：MasterVol / BGMVol / SFXVol）
//   3. 创建 SFXEntry / BGMEntry ScriptableObject 并拖入对应列表
//   4. 通过 AudioManager.Instance 调用各种方法
// 建议目录位置：
// ./Assets/Scripts/Audio/
// ============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 通用全局音频管理器（单例）。
/// 在任意场景中挂载到空物体上即可使用，自动标记 DontDestroyOnLoad。
/// 负责 SFX 播放（对象池 + 防叠加）和 BGM 交叉淡化控制。
/// </summary>
public class AudioManager : MonoBehaviour
{
    // ================================================================
    //  单例
    // ================================================================

    /// <summary>
    /// 全局唯一实例。通过 AudioManager.Instance 访问。
    /// </summary>
    public static AudioManager Instance { get; private set; }

    // ================================================================
    //  事件（供外部系统监听）
    // ================================================================

    /// <summary>当 BGM 切换时触发，参数为新的 bgmID（停止时为 null）</summary>
    public event Action<string> OnBGMChanged;

    /// <summary>当音量设置变化时触发，参数为 (通道名, 新音量值)</summary>
    public event Action<string, float> OnVolumeChanged;

    // ================================================================
    //  Inspector 配置
    // ================================================================

    [Header("=== Audio Mixer（可选） ===")]
    [Tooltip("Unity AudioMixer 资产。需要 Expose 三个参数：MasterVol / BGMVol / SFXVol。不设置也可正常工作。")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("=== SFX 配置 ===")]
    [Tooltip("所有音效的配置列表。将 SFXEntry ScriptableObject 拖入此列表")]
    [SerializeField] private List<SFXEntry> sfxEntries = new List<SFXEntry>();

    [Tooltip("SFX 对象池大小。小游戏 8 即可，大型游戏建议 16~20")]
    [Range(4, 32)]
    [SerializeField] private int sfxPoolSize = 12;

    [Header("=== BGM 配置 ===")]
    [Tooltip("所有 BGM 的配置列表。将 BGMEntry ScriptableObject 拖入此列表")]
    [SerializeField] private List<BGMEntry> bgmEntries = new List<BGMEntry>();

    [Tooltip("BGM 默认交叉淡化时长（秒）。单个 BGMEntry 可覆盖此值")]
    [SerializeField] private float defaultBGMFadeDuration = 1.5f;

    [Header("=== 音量默认值 ===")]
    [Tooltip("主音量默认值")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultMasterVolume = 1f;

    [Tooltip("BGM 音量默认值")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultBGMVolume = 0.7f;

    [Tooltip("SFX 音量默认值")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultSFXVolume = 0.8f;

    [Header("=== PlayerPrefs 键名前缀 ===")]
    [Tooltip("音量存储的键名前缀，不同项目可设不同前缀避免冲突")]
    [SerializeField] private string prefsPrefix = "Audio";

    // ================================================================
    //  内部状态 —— SFX
    // ================================================================

    // SFX 对象池：预创建的 AudioSource 数组
    private AudioSource[] _sfxPool;

    // SFX 配置字典：通过 sfxID 快速查找对应的 SFXEntry
    private Dictionary<string, SFXEntry> _sfxDict;

    // 防叠加追踪：记录每个 sfxID 的上次播放时间
    private Dictionary<string, float> _lastPlayTime;

    // 防叠加追踪：记录每个 sfxID 当前正在播放的实例数
    private Dictionary<string, int> _activeCounts;

    // ================================================================
    //  内部状态 —— BGM
    // ================================================================

    // BGM 配置字典：通过 bgmID 快速查找对应的 BGMEntry
    private Dictionary<string, BGMEntry> _bgmDict;

    // BGM 交叉淡化用的两个 AudioSource
    private AudioSource _bgmSourceA;
    private AudioSource _bgmSourceB;

    // 当前播放的 BGM ID（null 表示无 BGM）
    private string _currentBGMID;

    // 当前正在执行的淡化协程引用
    private Coroutine _bgmFadeCoroutine;

    // ================================================================
    //  内部状态 —— 音量
    // ================================================================

    // 缓存当前音量值（线性 0~1）
    private float _masterVolume;
    private float _bgmVolume;
    private float _sfxVolume;

    // 全局暂停标记
    private bool _isPaused;

    // ================================================================
    //  生命周期
    // ================================================================

    private void Awake()
    {
        // --- 单例初始化 ---
        // 如果已有实例则销毁自身，保证全局唯一
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // --- 初始化各子系统 ---
        InitSFXPool();
        InitSFXDictionary();
        InitBGMDictionary();
        InitBGMSources();
        LoadVolumeSettings();
    }

    private void OnDestroy()
    {
        // 单例销毁时清理引用，防止空引用
        if (Instance == this)
            Instance = null;
    }

    // ================================================================
    //  SFX 系统 —— 初始化
    // ================================================================

    /// <summary>
    /// 初始化 SFX 对象池。
    /// 创建 sfxPoolSize 个子物体，每个挂一个 AudioSource。
    /// 使用对象池避免运行时频繁创建/销毁 AudioSource 产生 GC。
    /// </summary>
    private void InitSFXPool()
    {
        _sfxPool = new AudioSource[sfxPoolSize];

        for (int i = 0; i < sfxPoolSize; i++)
        {
            // 创建子物体并添加 AudioSource 组件
            GameObject child = new GameObject($"SFXSource_{i}");
            child.transform.SetParent(transform);
            AudioSource source = child.AddComponent<AudioSource>();

            // 基本设置
            source.playOnAwake = false;   // 不自动播放
            source.spatialBlend = 0f;     // 默认 2D（播放时会根据 SFXEntry 覆盖）
            source.loop = false;          // SFX 不循环

            // 如果配置了 AudioMixer，将 SFX 分配到 SFX Group
            AssignMixerGroup(source, "SFX");

            _sfxPool[i] = source;
        }
    }

    /// <summary>
    /// 将 SFXEntry 列表转为字典，方便通过 ID 快速 O(1) 查找。
    /// 同时初始化防叠加追踪字典。
    /// </summary>
    private void InitSFXDictionary()
    {
        _sfxDict = new Dictionary<string, SFXEntry>();
        _lastPlayTime = new Dictionary<string, float>();
        _activeCounts = new Dictionary<string, int>();

        foreach (SFXEntry entry in sfxEntries)
        {
            // 跳过空配置或无 ID 的条目
            if (entry == null || string.IsNullOrEmpty(entry.sfxID))
                continue;

            // 重复 ID 警告
            if (_sfxDict.ContainsKey(entry.sfxID))
            {
                Debug.LogWarning($"[AudioManager] 重复的 SFX ID: {entry.sfxID}，后者将覆盖前者");
            }

            _sfxDict[entry.sfxID] = entry;
            _lastPlayTime[entry.sfxID] = -999f;   // 初始化为很久以前，确保第一次一定能播放
            _activeCounts[entry.sfxID] = 0;
        }
    }

    // ================================================================
    //  SFX 系统 —— 播放
    // ================================================================

    /// <summary>
    /// 播放一个 2D 音效。这是外部调用的主要接口。
    /// 自动处理：对象池分配、冷却检查、并发限制、音高随机化、多变体选取。
    /// </summary>
    /// <param name="sfxID">音效 ID，对应 SFXEntry.sfxID</param>
    /// <returns>是否成功播放（可能因冷却/并发限制而被拒绝）</returns>
    public bool PlaySFX(string sfxID)
    {
        return PlaySFXInternal(sfxID, Vector3.zero, false);
    }

    /// <summary>
    /// 在指定世界坐标播放一个 3D 音效。
    /// 音效的 spatialBlend 由 SFXEntry 配置决定。
    /// </summary>
    /// <param name="sfxID">音效 ID</param>
    /// <param name="worldPosition">3D 世界坐标</param>
    /// <returns>是否成功播放</returns>
    public bool PlaySFX(string sfxID, Vector3 worldPosition)
    {
        return PlaySFXInternal(sfxID, worldPosition, true);
    }

    /// <summary>
    /// SFX 播放的内部实现。
    /// 统一处理 2D/3D 播放逻辑，避免代码重复。
    /// </summary>
    /// <param name="sfxID">音效 ID</param>
    /// <param name="position">播放位置（仅 3D 模式使用）</param>
    /// <param name="usePosition">是否使用 3D 定位</param>
    /// <returns>是否成功播放</returns>
    private bool PlaySFXInternal(string sfxID, Vector3 position, bool usePosition)
    {
        // --- 全局暂停检查 ---
        if (_isPaused) return false;

        // --- 查找配置 ---
        if (!_sfxDict.TryGetValue(sfxID, out SFXEntry entry))
        {
            Debug.LogWarning($"[AudioManager] 未找到 SFX: {sfxID}");
            return false;
        }

        // 从配置中随机获取一个 AudioClip（支持多变体）
        AudioClip clip = entry.GetRandomClip();
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] SFX '{sfxID}' 的 AudioClip 为空");
            return false;
        }

        // --- 冷却检查：距上次播放是否已过冷却时间 ---
        float timeSinceLastPlay = Time.unscaledTime - _lastPlayTime[sfxID];
        if (timeSinceLastPlay < entry.cooldown)
        {
            return false; // 静默拒绝（正常的防叠加行为，不需要报错）
        }

        // --- 并发检查：当前是否已达最大并发数 ---
        if (_activeCounts[sfxID] >= entry.maxConcurrent)
        {
            return false; // 已达上限，静默拒绝
        }

        // --- 从对象池获取空闲的 AudioSource ---
        AudioSource source = GetAvailableSource(entry.priority);
        if (source == null)
        {
            return false; // 对象池全满且无法抢占
        }

        // --- 配置 AudioSource 参数 ---
        source.clip = clip;
        source.volume = entry.volume * _sfxVolume;
        source.spatialBlend = entry.spatialBlend;

        // 音高随机化：每次播放略有不同，增加自然感
        source.pitch = 1f + UnityEngine.Random.Range(-entry.pitchVariation, entry.pitchVariation);

        // 3D 定位：将 AudioSource 移动到指定世界坐标
        if (usePosition)
        {
            source.transform.position = position;
        }

        source.Play();

        // --- 更新防叠加追踪数据 ---
        _lastPlayTime[sfxID] = Time.unscaledTime;
        _activeCounts[sfxID]++;

        // 启动协程：音效播完后自动减少并发计数
        float actualDuration = clip.length / Mathf.Abs(source.pitch);
        StartCoroutine(TrackSFXCompletion(sfxID, actualDuration));

        return true;
    }

    /// <summary>
    /// 立即停止所有正在播放的音效。
    /// 适用于场景切换、游戏暂停等场景。
    /// </summary>
    public void StopAllSFX()
    {
        foreach (AudioSource source in _sfxPool)
        {
            if (source.isPlaying)
                source.Stop();
        }

        // 重置所有并发计数
        var keys = new List<string>(_activeCounts.Keys);
        foreach (string key in keys)
        {
            _activeCounts[key] = 0;
        }
    }

    /// <summary>
    /// 停止指定 ID 的所有正在播放的音效实例。
    /// </summary>
    /// <param name="sfxID">要停止的音效 ID</param>
    public void StopSFX(string sfxID)
    {
        if (!_sfxDict.TryGetValue(sfxID, out SFXEntry entry)) return;

        // 遍历对象池，找到匹配 clip 的 source 并停止
        foreach (AudioSource source in _sfxPool)
        {
            if (source.isPlaying && source.clip != null)
            {
                // 检查是否属于该 SFXEntry 的任何一个变体 clip
                foreach (AudioClip entryClip in entry.clips)
                {
                    if (source.clip == entryClip)
                    {
                        source.Stop();
                        break;
                    }
                }
            }
        }

        // 重置该音效的并发计数
        if (_activeCounts.ContainsKey(sfxID))
            _activeCounts[sfxID] = 0;
    }

    // ================================================================
    //  SFX 系统 —— 内部辅助
    // ================================================================

    /// <summary>
    /// 音效播放完毕后的清理协程。
    /// 等待音频播完后将该 sfxID 的并发计数 -1。
    /// </summary>
    /// <param name="sfxID">音效 ID</param>
    /// <param name="duration">等待时长（秒）</param>
    private IEnumerator TrackSFXCompletion(string sfxID, float duration)
    {
        // 使用 WaitForSecondsRealtime：即使 TimeScale=0（游戏暂停）也能正确计时
        yield return new WaitForSecondsRealtime(duration);

        if (_activeCounts.ContainsKey(sfxID))
        {
            _activeCounts[sfxID] = Mathf.Max(0, _activeCounts[sfxID] - 1);
        }
    }

    /// <summary>
    /// 从对象池中获取一个空闲的 AudioSource。
    /// 如果全部在使用中，尝试抢占优先级最低的那个。
    /// </summary>
    /// <param name="requestPriority">请求播放的音效优先级（1=最高）</param>
    /// <returns>可用的 AudioSource，如果无法获取则返回 null</returns>
    private AudioSource GetAvailableSource(int requestPriority)
    {
        // 第一轮：寻找空闲的 AudioSource（没在播放的）
        foreach (AudioSource source in _sfxPool)
        {
            if (!source.isPlaying)
                return source;
        }

        // 第二轮：对象池全满，尝试抢占优先级最低的 Source
        // 只有当请求的优先级高于（数字小于）某个正在播放的音效时才能抢占
        AudioSource lowestPriSource = null;
        int lowestPriority = requestPriority; // 只抢占比自己低优先级的

        foreach (AudioSource source in _sfxPool)
        {
            int sourcePri = GetSourcePriority(source);
            if (sourcePri > lowestPriority) // 数字越大 = 优先级越低
            {
                lowestPriority = sourcePri;
                lowestPriSource = source;
            }
        }

        // 找到了可以抢占的，停止它并返回
        if (lowestPriSource != null)
        {
            lowestPriSource.Stop();
            return lowestPriSource;
        }

        // 无法抢占：所有正在播放的音效优先级都 >= 请求的
        return null;
    }

    /// <summary>
    /// 查找某个 AudioSource 当前播放的音效的优先级。
    /// 通过匹配 clip 反查 SFXEntry。
    /// </summary>
    /// <param name="source">要查询的 AudioSource</param>
    /// <returns>优先级数值（1=最高, 5=最低, 99=空闲）</returns>
    private int GetSourcePriority(AudioSource source)
    {
        if (source.clip == null) return 99; // 空闲 source 优先级最低，便于被选中

        foreach (var entry in _sfxDict.Values)
        {
            // 检查所有变体 clip
            foreach (AudioClip clip in entry.clips)
            {
                if (clip == source.clip)
                    return entry.priority;
            }
        }
        return 3; // 找不到对应配置，返回中间默认值
    }

    // ================================================================
    //  BGM 系统 —— 初始化
    // ================================================================

    /// <summary>
    /// 将 BGMEntry 列表转为字典，方便通过 ID 快速查找。
    /// </summary>
    private void InitBGMDictionary()
    {
        _bgmDict = new Dictionary<string, BGMEntry>();

        foreach (BGMEntry entry in bgmEntries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.bgmID))
                continue;

            if (_bgmDict.ContainsKey(entry.bgmID))
            {
                Debug.LogWarning($"[AudioManager] 重复的 BGM ID: {entry.bgmID}，后者将覆盖前者");
            }

            _bgmDict[entry.bgmID] = entry;
        }
    }

    /// <summary>
    /// 初始化 BGM 用的两个 AudioSource（用于交叉淡化 A/B 切换）。
    /// </summary>
    private void InitBGMSources()
    {
        _bgmSourceA = CreateBGMSource("BGMSource_A");
        _bgmSourceB = CreateBGMSource("BGMSource_B");

        // 初始状态：都静音
        _bgmSourceA.volume = 0f;
        _bgmSourceB.volume = 0f;
    }

    /// <summary>
    /// 创建一个配置好的 BGM AudioSource 子物体。
    /// </summary>
    /// <param name="sourceName">子物体名称</param>
    /// <returns>配置好的 AudioSource</returns>
    private AudioSource CreateBGMSource(string sourceName)
    {
        GameObject obj = new GameObject(sourceName);
        obj.transform.SetParent(transform);
        AudioSource source = obj.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.spatialBlend = 0f;   // BGM 始终是 2D
        source.loop = true;         // BGM 默认循环
        source.priority = 0;        // Unity AudioSource 的最高优先级

        // 分配到 BGM Mixer Group
        AssignMixerGroup(source, "BGM");

        return source;
    }

    // ================================================================
    //  BGM 系统 —— 播放控制
    // ================================================================

    /// <summary>
    /// 播放指定 ID 的 BGM，自动交叉淡化切换。
    /// 如果已经在播放相同 BGM，则不重复切换。
    /// </summary>
    /// <param name="bgmID">BGM ID，对应 BGMEntry.bgmID</param>
    public void PlayBGM(string bgmID)
    {
        // 重复检查：已在播放相同 BGM 则忽略
        if (bgmID == _currentBGMID) return;

        // 查找配置
        if (!_bgmDict.TryGetValue(bgmID, out BGMEntry entry))
        {
            Debug.LogWarning($"[AudioManager] 未找到 BGM: {bgmID}");
            return;
        }

        if (entry.clip == null)
        {
            Debug.LogWarning($"[AudioManager] BGM '{bgmID}' 的 AudioClip 为空");
            return;
        }

        _currentBGMID = bgmID;

        // 停止上一次的淡化协程（如果还在执行）
        if (_bgmFadeCoroutine != null)
            StopCoroutine(_bgmFadeCoroutine);

        // 计算淡化时长：优先使用 BGMEntry 的自定义值，否则用全局默认值
        float fadeDuration = entry.customFadeDuration >= 0f
            ? entry.customFadeDuration
            : defaultBGMFadeDuration;

        // 启动交叉淡化
        _bgmFadeCoroutine = StartCoroutine(CrossFadeBGM(entry, fadeDuration));

        // 触发事件
        OnBGMChanged?.Invoke(bgmID);
    }

    /// <summary>
    /// 停止当前 BGM（淡出到静音）。
    /// </summary>
    /// <param name="fadeDuration">淡出时长（秒），-1 使用默认值</param>
    public void StopBGM(float fadeDuration = -1f)
    {
        if (_currentBGMID == null) return;

        _currentBGMID = null;

        if (_bgmFadeCoroutine != null)
            StopCoroutine(_bgmFadeCoroutine);

        float duration = fadeDuration >= 0f ? fadeDuration : defaultBGMFadeDuration;
        _bgmFadeCoroutine = StartCoroutine(FadeOutBGM(duration));

        OnBGMChanged?.Invoke(null);
    }

    /// <summary>
    /// 获取当前正在播放的 BGM ID。
    /// </summary>
    /// <returns>当前 BGM ID，无 BGM 播放时返回 null</returns>
    public string GetCurrentBGMID()
    {
        return _currentBGMID;
    }

    // ================================================================
    //  BGM 系统 —— 交叉淡化
    // ================================================================

    /// <summary>
    /// BGM 交叉淡化协程。
    /// 将当前播放的 BGM 渐出，将目标 BGM 渐入。
    /// 使用两个 AudioSource 交替实现无缝切换。
    /// </summary>
    /// <param name="targetEntry">目标 BGM 配置</param>
    /// <param name="fadeDuration">淡化时长（秒）</param>
    private IEnumerator CrossFadeBGM(BGMEntry targetEntry, float fadeDuration)
    {
        // 确定目标音量 = BGMEntry 自身音量 × 全局 BGM 音量
        float targetVolume = targetEntry.volume * _bgmVolume;

        // 确定哪个 Source 当前活跃、哪个空闲
        AudioSource currentSource = _bgmSourceA.isPlaying ? _bgmSourceA : _bgmSourceB;
        AudioSource nextSource = (currentSource == _bgmSourceA) ? _bgmSourceB : _bgmSourceA;

        // 配置下一个 Source
        nextSource.clip = targetEntry.clip;
        nextSource.loop = targetEntry.loop;
        nextSource.volume = 0f;
        nextSource.Play();

        // 执行线性渐变
        float elapsed = 0f;
        float startVolumeOut = currentSource.volume;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            // 当前 Source 渐出
            currentSource.volume = Mathf.Lerp(startVolumeOut, 0f, t);
            // 下一个 Source 渐入
            nextSource.volume = Mathf.Lerp(0f, targetVolume, t);

            yield return null;
        }

        // 渐变完成：停止旧 Source，确保新 Source 音量到位
        currentSource.Stop();
        currentSource.volume = 0f;
        nextSource.volume = targetVolume;
    }

    /// <summary>
    /// BGM 淡出协程（用于 StopBGM）。
    /// 将所有正在播放的 BGM Source 渐出到静音。
    /// </summary>
    /// <param name="fadeDuration">淡出时长（秒）</param>
    private IEnumerator FadeOutBGM(float fadeDuration)
    {
        // 记录两个 Source 的起始音量
        float startA = _bgmSourceA.volume;
        float startB = _bgmSourceB.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            // 两个 Source 同时渐出
            if (_bgmSourceA.isPlaying)
                _bgmSourceA.volume = Mathf.Lerp(startA, 0f, t);
            if (_bgmSourceB.isPlaying)
                _bgmSourceB.volume = Mathf.Lerp(startB, 0f, t);

            yield return null;
        }

        // 完全停止
        _bgmSourceA.Stop();
        _bgmSourceB.Stop();
        _bgmSourceA.volume = 0f;
        _bgmSourceB.volume = 0f;
    }

    // ================================================================
    //  全局暂停/恢复
    // ================================================================

    /// <summary>
    /// 暂停所有音频（SFX + BGM）。
    /// 适用于游戏暂停菜单等场景。
    /// </summary>
    public void PauseAll()
    {
        _isPaused = true;

        // 暂停所有 SFX
        foreach (AudioSource source in _sfxPool)
        {
            if (source.isPlaying) source.Pause();
        }

        // 暂停 BGM
        if (_bgmSourceA.isPlaying) _bgmSourceA.Pause();
        if (_bgmSourceB.isPlaying) _bgmSourceB.Pause();
    }

    /// <summary>
    /// 恢复所有被暂停的音频。
    /// </summary>
    public void ResumeAll()
    {
        _isPaused = false;

        // 恢复所有 SFX
        foreach (AudioSource source in _sfxPool)
        {
            source.UnPause();
        }

        // 恢复 BGM
        _bgmSourceA.UnPause();
        _bgmSourceB.UnPause();
    }

    /// <summary>
    /// 当前是否处于暂停状态。
    /// </summary>
    public bool IsPaused => _isPaused;

    // ================================================================
    //  音量控制
    // ================================================================

    /// <summary>
    /// 设置主音量 (0~1)。影响所有音频。
    /// 自动持久化到 PlayerPrefs。
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
        ApplyMixerVolume("MasterVol", _masterVolume);
        PlayerPrefs.SetFloat($"{prefsPrefix}_MasterVol", _masterVolume);
        OnVolumeChanged?.Invoke("Master", _masterVolume);
    }

    /// <summary>
    /// 设置 BGM 音量 (0~1)。仅影响背景音乐。
    /// 同步更新当前正在播放的 BGM Source 的实际音量。
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        _bgmVolume = Mathf.Clamp01(volume);
        ApplyMixerVolume("BGMVol", _bgmVolume);
        PlayerPrefs.SetFloat($"{prefsPrefix}_BGMVol", _bgmVolume);

        // 同步更新当前正在播放的 BGM Source 的音量
        // 需要乘以当前 BGMEntry 的自身音量
        UpdateActiveBGMVolume();

        OnVolumeChanged?.Invoke("BGM", _bgmVolume);
    }

    /// <summary>
    /// 设置 SFX 音量 (0~1)。仅影响音效。
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        ApplyMixerVolume("SFXVol", _sfxVolume);
        PlayerPrefs.SetFloat($"{prefsPrefix}_SFXVol", _sfxVolume);
        OnVolumeChanged?.Invoke("SFX", _sfxVolume);
    }

    // --- 音量 Getter（供 UI 滑动条读取当前值） ---

    /// <summary>获取当前主音量 (0~1)</summary>
    public float GetMasterVolume() => _masterVolume;

    /// <summary>获取当前 BGM 音量 (0~1)</summary>
    public float GetBGMVolume() => _bgmVolume;

    /// <summary>获取当前 SFX 音量 (0~1)</summary>
    public float GetSFXVolume() => _sfxVolume;

    /// <summary>
    /// 静音/取消静音 切换。
    /// 将主音量设为 0 或恢复到之前的值。
    /// </summary>
    /// <param name="mute">true=静音, false=取消静音</param>
    public void SetMute(bool mute)
    {
        if (mute)
        {
            // 静音：将 Mixer 主音量设为 -80dB
            ApplyMixerVolume("MasterVol", 0f);
        }
        else
        {
            // 取消静音：恢复缓存的音量值
            ApplyMixerVolume("MasterVol", _masterVolume);
        }
    }

    // ================================================================
    //  音量控制 —— 内部辅助
    // ================================================================

    /// <summary>
    /// 将 0~1 的线性音量值转为 AudioMixer 使用的对数分贝值并应用。
    /// AudioMixer 的音量参数范围是 -80dB（静音） ~ 0dB（最大）。
    /// </summary>
    /// <param name="paramName">AudioMixer 中 Exposed 的参数名</param>
    /// <param name="linearVolume">线性音量值 (0~1)</param>
    private void ApplyMixerVolume(string paramName, float linearVolume)
    {
        if (mainMixer == null) return;

        // 线性 → 分贝转换
        // 当音量为 0 时设为 -80dB（静音），避免 Log10(0) 的数学错误
        float dB = linearVolume > 0.0001f
            ? Mathf.Log10(linearVolume) * 20f
            : -80f;

        mainMixer.SetFloat(paramName, dB);
    }

    /// <summary>
    /// 从 PlayerPrefs 加载用户保存的音量设置。
    /// 在 Awake 中调用，确保音量设置跨会话持久。
    /// </summary>
    private void LoadVolumeSettings()
    {
        _masterVolume = PlayerPrefs.GetFloat($"{prefsPrefix}_MasterVol", defaultMasterVolume);
        _bgmVolume    = PlayerPrefs.GetFloat($"{prefsPrefix}_BGMVol", defaultBGMVolume);
        _sfxVolume    = PlayerPrefs.GetFloat($"{prefsPrefix}_SFXVol", defaultSFXVolume);

        // 应用到 Mixer
        ApplyMixerVolume("MasterVol", _masterVolume);
        ApplyMixerVolume("BGMVol", _bgmVolume);
        ApplyMixerVolume("SFXVol", _sfxVolume);
    }

    /// <summary>
    /// 更新当前正在播放的 BGM AudioSource 的实际音量。
    /// 用于音量滑动条实时变更时同步 BGM 音量。
    /// </summary>
    private void UpdateActiveBGMVolume()
    {
        // 获取当前 BGM 的自身音量系数
        float entryVolume = 1f;
        if (_currentBGMID != null && _bgmDict.TryGetValue(_currentBGMID, out BGMEntry entry))
        {
            entryVolume = entry.volume;
        }

        // 更新正在播放的 Source
        if (_bgmSourceA.isPlaying) _bgmSourceA.volume = _bgmVolume * entryVolume;
        if (_bgmSourceB.isPlaying) _bgmSourceB.volume = _bgmVolume * entryVolume;
    }

    // ================================================================
    //  通用辅助方法
    // ================================================================

    /// <summary>
    /// 将 AudioSource 分配到指定的 AudioMixer Group。
    /// 如果没有配置 AudioMixer 则跳过（系统仍然可以正常工作）。
    /// </summary>
    /// <param name="source">要分配的 AudioSource</param>
    /// <param name="groupName">Mixer Group 名称（如 "SFX"、"BGM"）</param>
    private void AssignMixerGroup(AudioSource source, string groupName)
    {
        if (mainMixer == null) return;

        AudioMixerGroup[] groups = mainMixer.FindMatchingGroups(groupName);
        if (groups.Length > 0)
            source.outputAudioMixerGroup = groups[0];
    }

    /// <summary>
    /// 检查指定的 SFX ID 是否已注册。
    /// 用于运行时动态检查音效是否可用。
    /// </summary>
    /// <param name="sfxID">要检查的音效 ID</param>
    /// <returns>是否已注册</returns>
    public bool HasSFX(string sfxID)
    {
        return _sfxDict != null && _sfxDict.ContainsKey(sfxID);
    }

    /// <summary>
    /// 检查指定的 BGM ID 是否已注册。
    /// </summary>
    /// <param name="bgmID">要检查的 BGM ID</param>
    /// <returns>是否已注册</returns>
    public bool HasBGM(string bgmID)
    {
        return _bgmDict != null && _bgmDict.ContainsKey(bgmID);
    }

    // ================================================================
    //  运行时动态注册（可选功能）
    // ================================================================

    /// <summary>
    /// 在运行时动态注册一个新的 SFXEntry。
    /// 适用于 DLC、模组、或运行时加载的音效资源。
    /// </summary>
    /// <param name="entry">要注册的 SFXEntry</param>
    public void RegisterSFX(SFXEntry entry)
    {
        if (entry == null || string.IsNullOrEmpty(entry.sfxID)) return;

        _sfxDict[entry.sfxID] = entry;
        if (!_lastPlayTime.ContainsKey(entry.sfxID))
            _lastPlayTime[entry.sfxID] = -999f;
        if (!_activeCounts.ContainsKey(entry.sfxID))
            _activeCounts[entry.sfxID] = 0;
    }

    /// <summary>
    /// 在运行时动态注册一个新的 BGMEntry。
    /// </summary>
    /// <param name="entry">要注册的 BGMEntry</param>
    public void RegisterBGM(BGMEntry entry)
    {
        if (entry == null || string.IsNullOrEmpty(entry.bgmID)) return;

        _bgmDict[entry.bgmID] = entry;
    }
}
