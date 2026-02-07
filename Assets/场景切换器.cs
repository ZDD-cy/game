using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 挂在每个场景的「切换区」上
public class 场景切换器 : MonoBehaviour
{
    [Header("目标场景名字（和文件名一致）")]
    public string 目标场景名 = "第二关";
    public float 切换延迟 = 0f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("player"))
        {
            Invoke("加载场景", 切换延迟);
        }
    }

    void 加载场景()
    {
        // 直接按名字加载，不需要Build Settings
        SceneManager.LoadScene(目标场景名);
    }
}