using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// 挂在每个场景的「切换区」上
public class Sceneswitcher : MonoBehaviour
{
    private static readonly int Fadestart = Animator.StringToHash("Fadestart");
    public Animator transition;
    [Header("目标场景名字（和文件名一致）")]
    public string Targetscene;
    public float switchlatency;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("player"))
        {
            StartCoroutine(Loadlevel());
        }
    }
    
    public void MessageSwitch(string scenename)
    {
        Targetscene = scenename;
        StartCoroutine(Loadlevel());
    }
    
    IEnumerator Loadlevel()
    {
        transition.SetTrigger(Fadestart);
        yield return new WaitForSeconds(switchlatency);
        SceneManager.LoadScene(Targetscene);
    }

    public void QuitApplication()
    {
    #if UNITY_EDITOR
        EditorApplication.isPlaying = false; // 停止播放并退出编辑器(测试用，打包时可以删除if else只保留quit)
    #else
        Application.Quit();
    #endif
    }
}