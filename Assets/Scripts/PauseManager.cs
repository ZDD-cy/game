using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public Canvas PauseCanvas;
    public Canvas SettingsCanvas;
    public bool isPaused;
    public bool isMainMenu = true;

    public static PauseManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LeaveMainMenu()
    {
        isMainMenu = false;
    }
    void Start()
    {
        PauseCanvas.enabled = isPaused;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMainMenu && !isPaused && Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchPause(0f);
        }
        else if (!isMainMenu && isPaused && Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchPause(1f);
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

    public void SwitchPause(float timescale)
    {
        isPaused = !isPaused;
        PauseCanvas.enabled = isPaused;
        Time.timeScale = timescale;
    }
    
    public void DisplaySettings()
    {
        PauseCanvas.enabled = false;
        SettingsCanvas.enabled = true;
    }
}
