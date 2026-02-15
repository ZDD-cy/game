using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public Canvas PauseCanvas;
    public Canvas SettingsCanvas;
    public Camera cam;
    public bool isPaused;
    public bool isMainMenu = true;

    public static PauseManager Instance { get; private set; }
    private void Awake()
    {
        
        UpdateCam();
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
        if (SettingsCanvas.enabled && Input.GetKeyDown(KeyCode.Escape))
        {
            if(!isMainMenu){DisplaySettings();}
            else{SettingsCanvas.enabled = !SettingsCanvas.enabled;}
        }else if (!isMainMenu && !isPaused && Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchPause(0f);
            AudioManager.Instance.PauseAll();
        }
        else if (!isMainMenu && isPaused && Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchPause(1f);
            AudioManager.Instance.ResumeAll();
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
        if(!isMainMenu)PauseCanvas.enabled = !PauseCanvas.enabled;
        SettingsCanvas.enabled = !SettingsCanvas.enabled;
    }

    public void UpdateCam()
    {
        GameObject RenderCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (RenderCamera != null)
        {
            cam = RenderCamera.GetComponent<Camera>();
        }
        else
        {
            Debug.LogError("No Main Camera!");
        }
        PauseCanvas.worldCamera = cam;
        SettingsCanvas.worldCamera = cam;
    }
    
    private void OnEnable()
    {
        // 注册场景加载完成事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    // 场景加载完成时的回调
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CustomAudio.PlaySoundOnSceneLoaded(scene.name);
        UpdateCam();
    }
}
