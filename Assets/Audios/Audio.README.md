# 🔊 AudioManager — Unity 通用音频管理系统

> 一套开箱即用的 Unity 音频解决方案，覆盖 SFX 音效与 BGM 背景音乐的完整生命周期管理。  
> 适用于 2D / 3D、单机 / 联机、小型独立游戏到中大型商业项目。

---

## 📁 文件清单

| 文件 | 用途 | 说明 |
|------|------|------|
| `AudioManager.cs` | 主管理器（单例） | 挂载到场景空物体上，跨场景持久 |
| `SFXEntry.cs` | 音效配置 | ScriptableObject，每个音效一份资产 |
| `BGMEntry.cs` | BGM 配置 | ScriptableObject，每首 BGM 一份资产 |

---

## 🚀 快速上手（5 分钟接入）

### 第一步：导入文件

将 `AudioManager.cs`、`SFXEntry.cs`、`BGMEntry.cs` 放入项目的 `Assets/Scripts/Audio/` 目录。

### 第二步：场景配置

1. 在启动场景中创建空物体，命名为 `AudioManager`
2. 挂载 `AudioManager` 组件
3. 在 Inspector 中配置：
   - **Audio Mixer**（可选）：拖入你的 AudioMixer 资产
   - **SFX Pool Size**：对象池大小（小游戏 8，大游戏 16~20）
   - **BGM Fade Duration**：默认淡化时长
   - **Prefs Prefix**：PlayerPrefs 键名前缀（不同项目设不同值避免冲突）

### 第三步：创建音效配置

在 Project 窗口中：

```
右键 → Create → Audio → SFX Entry
```

填写字段后拖入 AudioManager 的 `sfxEntries` 列表。

### 第四步：创建 BGM 配置

```
右键 → Create → Audio → BGM Entry
```

填写字段后拖入 AudioManager 的 `bgmEntries` 列表。

### 第五步：代码调用

```csharp
// 播放音效
AudioManager.Instance.PlaySFX("UI_Click");

// 播放 BGM
AudioManager.Instance.PlayBGM("MainMenu");
```

**搞定！** 🎉

---

## 📖 全部 API 接口一览

### SFX 音效

| 方法签名 | 返回值 | 说明 |
|----------|--------|------|
| `PlaySFX(string sfxID)` | `bool` | 播放 2D 音效 |
| `PlaySFX(string sfxID, Vector3 worldPosition)` | `bool` | 在世界坐标播放 3D 音效 |
| `StopSFX(string sfxID)` | `void` | 停止指定音效的所有实例 |
| `StopAllSFX()` | `void` | 停止所有正在播放的音效 |
| `HasSFX(string sfxID)` | `bool` | 检查音效 ID 是否已注册 |
| `RegisterSFX(SFXEntry entry)` | `void` | 运行时动态注册新音效 |

> `PlaySFX` 返回 `false` 的可能原因：ID 不存在、Clip 为空、冷却中、并发已满、对象池已满且无法抢占、全局暂停中。

### BGM 背景音乐

| 方法签名 | 返回值 | 说明 |
|----------|--------|------|
| `PlayBGM(string bgmID)` | `void` | 播放 BGM（自动交叉淡化） |
| `StopBGM(float fadeDuration = -1f)` | `void` | 淡出停止 BGM，-1 用默认时长 |
| `GetCurrentBGMID()` | `string` | 获取当前 BGM ID，无则 null |
| `HasBGM(string bgmID)` | `bool` | 检查 BGM ID 是否已注册 |
| `RegisterBGM(BGMEntry entry)` | `void` | 运行时动态注册新 BGM |

### 音量控制

| 方法签名 | 返回值 | 说明 |
|----------|--------|------|
| `SetMasterVolume(float volume)` | `void` | 设置主音量 (0~1) |
| `SetBGMVolume(float volume)` | `void` | 设置 BGM 音量 (0~1) |
| `SetSFXVolume(float volume)` | `void` | 设置 SFX 音量 (0~1) |
| `GetMasterVolume()` | `float` | 获取当前主音量 |
| `GetBGMVolume()` | `float` | 获取当前 BGM 音量 |
| `GetSFXVolume()` | `float` | 获取当前 SFX 音量 |
| `SetMute(bool mute)` | `void` | 静音 / 取消静音 |

> 所有音量设置自动持久化到 PlayerPrefs，下次启动自动恢复。

### 全局控制

| 方法签名 / 属性 | 返回值 | 说明 |
|-----------------|--------|------|
| `PauseAll()` | `void` | 暂停所有音频（SFX + BGM） |
| `ResumeAll()` | `void` | 恢复所有被暂停的音频 |
| `IsPaused` | `bool` | 当前是否处于暂停状态 |

### 事件回调

| 事件 | 参数 | 触发时机 |
|------|------|----------|
| `OnBGMChanged` | `string bgmID` | BGM 切换时（停止时为 null） |
| `OnVolumeChanged` | `string channel, float volume` | 任意音量通道变化时 |

```csharp
// 事件订阅示例
AudioManager.Instance.OnBGMChanged += (bgmID) => {
    Debug.Log($"BGM 切换到: {bgmID ?? "无"}");
};

AudioManager.Instance.OnVolumeChanged += (channel, vol) => {
    Debug.Log($"{channel} 音量: {vol:P0}");
};
```

---

## 🧩 ScriptableObject 配置字段

### SFXEntry 字段

| 字段 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `sfxID` | `string` | — | 唯一标识符（如 `"UI_Click"`） |
| `clips` | `AudioClip[]` | — | 音频片段数组（多个则随机选取） |
| `volume` | `float` | 0.5 | 基础音量 (0~1) |
| `pitchVariation` | `float` | 0.02 | 音高随机偏移范围 |
| `spatialBlend` | `float` | 0 | 空间混合（0=2D, 1=3D） |
| `cooldown` | `float` | 0.1s | 两次播放最短间隔 |
| `maxConcurrent` | `int` | 1 | 最大同时播放实例数 |
| `priority` | `int` | 3 | 优先级（1=最高, 5=最低） |

### BGMEntry 字段

| 字段 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `bgmID` | `string` | — | 唯一标识符（如 `"Battle"`） |
| `clip` | `AudioClip` | — | BGM 音频片段 |
| `volume` | `float` | 1.0 | 基础音量 (0~1) |
| `loop` | `bool` | true | 是否循环播放 |
| `customFadeDuration` | `float` | -1 | 自定义淡化时长，-1 用全局默认 |

---

## 💡 常见使用场景

### 场景一：UI 音效

```csharp
// 按钮点击（可在 Inspector 的 OnClick 事件中直接绑定调用）
public void OnButtonClick()
{
    AudioManager.Instance.PlaySFX("UI_Click");
}

// 错误提示
public void ShowError(string msg)
{
    AudioManager.Instance.PlaySFX("UI_Error");
    errorText.text = msg;
}
```

### 场景二：战斗音效（防叠加）

```
SFXEntry 配置建议：
├── sfxID:          "Combat_Hit"
├── clips:          [Hit_01.wav, Hit_02.wav, Hit_03.wav]  ← 多变体随机
├── volume:         0.6
├── pitchVariation: 0.08                                   ← 较大的随机幅度
├── cooldown:       0.05                                   ← 短冷却，允许快速连击
├── maxConcurrent:  3                                      ← 最多同时 3 个
└── priority:       2                                      ← 较高优先级
```

### 场景三：3D 空间音效 

```csharp
// 在爆炸位置播放 3D 音效
public void Explode(Vector3 position)
{
    AudioManager.Instance.PlaySFX("Explosion", position);
    // SFXEntry 的 spatialBlend 设为 1.0 即可
}
```

### 场景四：BGM 场景切换

```csharp
// 场景加载时切换 BGM
public class LevelManager : MonoBehaviour
{
    void OnSceneLoaded(string sceneName)
    {
        switch (sceneName)
        {
            case "MainMenu":  AudioManager.Instance.PlayBGM("MainMenu");  break;
            case "Village":   AudioManager.Instance.PlayBGM("Peace");     break;
            case "Dungeon":   AudioManager.Instance.PlayBGM("Tension");   break;
            case "BossFight": AudioManager.Instance.PlayBGM("Boss");      break;
        }
    }
}
```

### 场景五：暂停菜单

```csharp
public void TogglePauseMenu()
{
    if (AudioManager.Instance.IsPaused)
    {
        AudioManager.Instance.ResumeAll();
        Time.timeScale = 1f;
    }
    else
    {
        AudioManager.Instance.PauseAll();
        Time.timeScale = 0f;
    }
}
```

### 场景六：音量设置 UI

```csharp
// 绑定到 Slider 的 OnValueChanged
public void OnMasterSliderChanged(float value)
{
    AudioManager.Instance.SetMasterVolume(value);
}

// 初始化 Slider 显示
void Start()
{
    masterSlider.value = AudioManager.Instance.GetMasterVolume();
    bgmSlider.value    = AudioManager.Instance.GetBGMVolume();
    sfxSlider.value    = AudioManager.Instance.GetSFXVolume();
}
```

### 场景七：运行时动态加载（Addressables / DLC）

```csharp
// 从 Addressables 加载并注册
async void LoadDLCAudio()
{
    var sfx = await Addressables.LoadAssetAsync<SFXEntry>("DLC_Explosion");
    AudioManager.Instance.RegisterSFX(sfx);

    var bgm = await Addressables.LoadAssetAsync<BGMEntry>("DLC_BossTheme");
    AudioManager.Instance.RegisterBGM(bgm);
}
```

---

## 🎛️ AudioMixer 配置指南（可选）

AudioMixer 不是必须的——不配置时系统也能正常工作。但配置后可以获得更精细的音频控制（EQ、压缩、混响等）

#### 混音的情绪和主题

混音器可以有效地用于在游戏中营造情绪，游戏可以轻松转换其情绪并使玩家感受到设计师所期望的感受，这对于游戏设计（程序把任务甩回给策划）是非常有帮助的

#### 全局混音

混音器用于控制游戏中所有声音的总体混音。这些混音器将控制全局混音，可视为路由声音实例的静态单声道混音。快照可以捕获混音器的状态，并随着游戏的进行在这些不同的状态之间转换。要定义混音的情绪或主题，并随着玩家在游戏中的进展而改变这些情绪

### 推荐的 Mixer 结构

```
Master (Exposed: "MasterVol")
├── BGM (Exposed: "BGMVol")
└── SFX (Exposed: "SFXVol")
```

### Expose 参数步骤

1. 打开 AudioMixer 窗口
2. 选中 Master Group → Inspector 中找到 Volume
3. 右键 Volume → `Expose 'Volume (of Master)' to script`
4. 在 AudioMixer 窗口右上角 `Exposed Parameters` 中重命名为 `MasterVol`
5. 对 BGM 和 SFX Group 重复操作，分别命名为 `BGMVol` 和 `SFXVol`

---

## ⚙️ 系统架构

```
AudioManager (GameObject, DontDestroyOnLoad)
│
├── 🎵 BGMSource_A          ← BGM 交叉淡化 Source A
├── 🎵 BGMSource_B          ← BGM 交叉淡化 Source B
│
├── 🔈 SFXSource_0          ← SFX 对象池
├── 🔈 SFXSource_1
├── 🔈 SFXSource_2
├── 🔈 ...
└── 🔈 SFXSource_N
```

### 核心机制

| 机制 | 说明 |
|------|------|
| **单例模式** | `DontDestroyOnLoad`，全局唯一，跨场景持久 |
| **SFX 对象池** | 预创建固定数量 AudioSource，零运行时 GC |
| **防叠加** | 冷却时间 + 最大并发数，防止同一音效疯狂叠加 |
| **优先级抢占** | 对象池满时，高优先级音效可抢占低优先级的 Source |
| **BGM 交叉淡化** | 双 Source A/B 切换，协程驱动线性渐变 |
| **音量持久化** | PlayerPrefs 自动保存/加载，支持自定义键名前缀 |
| **Mixer 可选** | 有 AudioMixer 用 Mixer 控制，没有也能正常工作 |

---

## 🔧 参数调优建议

### SFX 对象池大小

| 游戏类型 | 推荐值 | 说明 |
|----------|--------|------|
| 休闲 / 解谜 | 6~8 | 同时音效少 |
| RPG / 冒险 | 10~14 | 中等音效密度 |
| ACT / FPS / RTS | 16~24 | 大量并发音效 |

### 防叠加参数参考

| 音效类型 | cooldown | maxConcurrent | 说明 |
|----------|----------|---------------|------|
| UI 点击 | 0.1s | 1 | 严格防连点 |
| 脚步声 | 0.2s | 1 | 有节奏感 |
| 击打音 | 0.05s | 2~3 | 允许快速连击但不爆音 |
| 爆炸 | 0.3s | 2 | 多次爆炸可叠加 |
| 环境音 | 1.0s | 1 | 避免重复触发 |

### 优先级分配建议

| 优先级 | 适合的音效类型 |
|--------|---------------|
| 1（最高） | 关键 UI 反馈、重要剧情语音 |
| 2 | 战斗核心音效（技能释放、Boss 攻击） |
| 3（默认） | 一般战斗音效（命中、受击） |
| 4 | 环境音效（风声、水流） |
| 5（最低） | 背景装饰音（鸟叫、虫鸣） |

---

## ❓ FAQ

**Q：不使用 AudioMixer 可以吗？**  
A：完全可以。不拖入 Mixer 资产时，系统会跳过所有 Mixer 相关逻辑，音量通过 AudioSource.volume 直接控制。

**Q：同一个场景放了两个 AudioManager 怎么办？**  
A：单例模式自动处理——后创建的会自动销毁自身，只保留第一个。

**Q：如何实现"只暂停 SFX 不暂停 BGM"？**  
A：目前 `PauseAll` 是全部暂停。如果有此需求，可以单独调用 `StopAllSFX()` 停止音效，BGM 保持不动。

**Q：BGM 切换时会不会中断？**  
A：不会。系统使用双 Source 交叉淡化，旧 BGM 渐出的同时新 BGM 渐入，过渡平滑。

**Q：支持 WebGL 吗？**  
A：支持。所有 API 都是标准 Unity AudioSource，没有使用平台特定功能。

**Q：如何和 Addressables 配合？**  
A：用 `RegisterSFX()` / `RegisterBGM()` 在异步加载完成后动态注册即可。

---

## 📜 版本历史

| 版本 | 日期 | 说明 |
|------|------|------|
| v2.0 | 2026 | 通用模板重构：新增3D 音效、暂停恢复、动态注册 |
| v1.0 | 2025 | 尝试泛化为2D游戏通用音频管理器 |
| v0.1 | 2025 | 给某个项目的特供版本 |
