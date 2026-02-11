using UnityEditor;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public Canvas PauseCanvas;
    public bool isPaused;
    void Start()
    {
        PauseCanvas.enabled = isPaused;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPaused && Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            PauseCanvas.enabled = isPaused;
            Time.timeScale = 0f;
        }
        else if (isPaused && Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            PauseCanvas.enabled = isPaused;
            Time.timeScale = 1f;
        }
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
