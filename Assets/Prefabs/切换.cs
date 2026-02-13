using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGate : MonoBehaviour
{
    [Header("关卡设置")]
    [Tooltip("要加载的目标关卡场景名")]
    public string targetLevelName = "Level2";

    [Header("过渡设置")]
    [Tooltip("是否显示加载过渡动画")]
    public bool useFadeTransition = true;
    [Tooltip("过渡动画持续时间（秒）")]
    public float fadeDuration = 1f;

    [Header("调试")]
    [Tooltip("是否只在玩家进入时触发一次")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;
    private Animator fadeAnimator; // 用于淡入淡出的动画器

    private void Start()
    {
        // 尝试获取场景中的淡入淡出控制器
        if (useFadeTransition)
        {
            fadeAnimator = FindObjectOfType<Animator>();
            if (fadeAnimator == null)
            {
                Debug.LogWarning("未找到淡入淡出动画器，将直接加载关卡");
                useFadeTransition = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 防重复触发
        if (hasTriggered && triggerOnce) return;

        // 只对玩家触发
        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(LoadNextLevel());
        }
    }

    private IEnumerator LoadNextLevel()
    {
        if (useFadeTransition && fadeAnimator != null)
        {
            // 播放淡出动画
            fadeAnimator.SetTrigger("FadeOut");
            // 等待动画完成
            yield return new WaitForSeconds(fadeDuration);
        }

        // 异步加载目标关卡
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetLevelName);

        // 等待加载完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}

