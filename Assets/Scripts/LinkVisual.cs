using UnityEngine;

public enum LinkState
{
    Neutral,    // 默认，连接中
    Attacking,  // 正在传输攻击/DOT
    Dangerous,  // 受到干扰，但未断开
    Broken      // 已断开
}

[RequireComponent(typeof(LineRenderer))]
public class LinkVisual : MonoBehaviour
{
    [Header("线条配置")]
    public float pulseAmplitude = 0.02f;  // 脉冲幅度（调小这个值！）
    public float pulseSpeed = 3.0f;       // 脉冲速度
    public float dashLength = 0.2f;       // 虚线每段长度
    public float dashSpeed = 1.0f;        // 虚线流动速度
    
    [Header("颜色配置")]
    public Color neutralColor = new Color(0, 1, 1, 0.8f);     // 青色，半透明
    public Color attackingColor = new Color(1, 0.5f, 0, 0.9f); // 橙色
    public Color brokenColor = new Color(1, 0, 0, 0.5f);       // 红色

    private LineRenderer lineRenderer;
    private Material lineMaterial;
    private float pulsePhase = 0f;
    private float dashOffset = 0f;
    private LinkState currentState = LinkState.Neutral;

    void Awake()
    {
        // 获取或添加LineRenderer组件
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        // 初始化LineRenderer参数（关键！）
        InitializeLineRenderer();
    }

    void InitializeLineRenderer()
    {
        // ====== 1. 基本设置 ======
        lineRenderer.positionCount = 2;  // 只有起点和终点
        
        // ====== 2. 宽度设置 ======
        lineRenderer.startWidth = 0.08f;  // 起始宽度（调小）
        lineRenderer.endWidth = 0.08f;    // 结束宽度（调小，保持一致）
        lineRenderer.widthMultiplier = 1f;
        
// ====== 3. 材质设置 ======
// 创建或获取材质
if (lineMaterial == null)
{
    // 优先使用UI/Default，它几乎在所有项目中都可用
    Shader lineShader = Shader.Find("UI/Default");
    
    // 如果找不到，尝试其他可能的名字
    if (lineShader == null)
    {
        lineShader = Shader.Find("Sprites/Default");
    }
    if (lineShader == null)
    {
        lineShader = Shader.Find("Unlit/Color");
    }
    if (lineShader == null)
    {
        // 最后尝试Standard
        lineShader = Shader.Find("Standard");
        Debug.LogWarning("使用Standard着色器作为后备方案");
    }
    
    if (lineShader != null)
    {
        lineMaterial = new Material(lineShader);
        lineMaterial.name = "DynamicDashMaterial";
    }
    else
    {
        // 如果真的找不到任何Shader，创建一个空材质并记录错误
        Debug.LogError("无法找到任何可用的着色器！线条可能无法正确显示");
        lineMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
    }
}
        
        // ====== 4. 其他优化设置 ======
        lineRenderer.useWorldSpace = true;      // 使用世界坐标
        lineRenderer.alignment = LineAlignment.View; // 面向视角
        lineRenderer.textureMode = LineTextureMode.Tile; // 纹理平铺模式
        lineRenderer.numCapVertices = 4;        // 端点圆滑度
        lineRenderer.numCornerVertices = 4;     // 拐角圆滑度
        
        // ====== 5. 初始颜色 ======
        SetState(LinkState.Neutral);
    }

    void ConfigureDashMaterial()
    {
        if (lineMaterial == null) return;
        
        // 设置虚线纹理（使用内置的粒子点纹理）
        Texture2D dashTex = CreateDashTexture();
        lineMaterial.mainTexture = dashTex;
        
        // 关键：设置平铺参数
        // 这个值控制虚线段的密度，值越大虚线越密
        lineMaterial.mainTextureScale = new Vector2(10f, 1f);
        
        // 启用透明度混合
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        lineMaterial.SetInt("_ZWrite", 0);
        lineMaterial.DisableKeyword("_ALPHATEST_ON");
        lineMaterial.EnableKeyword("_ALPHABLEND_ON");
        lineMaterial.renderQueue = 3000;
    }

    Texture2D CreateDashTexture()
    {
        // 创建一个简单的1x4纹理：透明-不透明-透明-不透明
        Texture2D tex = new Texture2D(4, 1, TextureFormat.RGBA32, false);
        
        // 像素0: 透明
        tex.SetPixel(0, 0, new Color(1, 1, 1, 0));
        // 像素1: 不透明
        tex.SetPixel(1, 0, new Color(1, 1, 1, 1));
        // 像素2: 透明
        tex.SetPixel(2, 0, new Color(1, 1, 1, 0));
        // 像素3: 不透明
        tex.SetPixel(3, 0, new Color(1, 1, 1, 1));
        
        tex.Apply();
        return tex;
    }

    void Update()
    {
        if (lineRenderer == null) return;
        
        // 1. 更新脉冲相位（幅度已减小）
        pulsePhase += Time.deltaTime * pulseSpeed;
        float pulse = Mathf.Sin(pulsePhase) * pulseAmplitude;
        
        // 设置宽度（幅度很小）
        lineRenderer.startWidth = 0.08f + pulse;
        lineRenderer.endWidth = 0.08f + pulse;
        
        // 2. 更新虚线流动效果
        UpdateDashFlow();
        
        // 3. 更新颜色脉冲效果（可选）
        UpdateColorPulse();
    }

    void UpdateDashFlow()
    {
        if (lineMaterial == null) return;
        
        // 使虚线纹理向左滚动，产生流动效果
        dashOffset -= Time.deltaTime * dashSpeed;
        lineMaterial.mainTextureOffset = new Vector2(dashOffset, 0);
    }

    void UpdateColorPulse()
    {
        // 轻微的亮度脉冲
        float brightnessPulse = 0.1f * Mathf.Sin(pulsePhase * 0.5f);
        Color currentColor = GetBaseColorForState(currentState);
        
        // 增加亮度但不改变透明度
        Color pulsedColor = new Color(
            Mathf.Clamp01(currentColor.r + brightnessPulse),
            Mathf.Clamp01(currentColor.g + brightnessPulse),
            Mathf.Clamp01(currentColor.b + brightnessPulse),
            currentColor.a
        );
        
        lineRenderer.startColor = pulsedColor;
        lineRenderer.endColor = pulsedColor;
    }

    Color GetBaseColorForState(LinkState state)
    {
        switch (state)
        {
            case LinkState.Neutral: return neutralColor;
            case LinkState.Attacking: return attackingColor;
            case LinkState.Dangerous: return brokenColor;
            case LinkState.Broken: return brokenColor;
            default: return neutralColor;
        }
    }

    // 公开方法：由LinkManager调用
    public void Initialize(Vector3 start, Vector3 end, Transform parent = null)
    {
        transform.SetParent(parent);
        UpdatePositions(start, end);
    }

    public void UpdatePositions(Vector3 start, Vector3 end)
    {
        if (lineRenderer == null) return;
        
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    public void SetState(LinkState state)
    {
        currentState = state;
        
        if (lineRenderer == null) return;
        
        Color baseColor = GetBaseColorForState(state);
        lineRenderer.startColor = baseColor;
        lineRenderer.endColor = baseColor;
        
        // 根据不同状态调整参数
        switch (state)
        {
            case LinkState.Attacking:
                pulseSpeed = 4.0f;      // 攻击状态脉冲更快
                dashSpeed = 2.0f;       // 虚线流动更快
                break;
            case LinkState.Broken:
                pulseSpeed = 1.0f;      // 断开状态脉冲慢
                dashSpeed = 0f;         // 停止流动
                break;
            default:
                pulseSpeed = 2.0f;
                dashSpeed = 1.0f;
                break;
        }
    }

    public void Break()
    {
        SetState(LinkState.Broken);
        Destroy(gameObject, 0.3f);  // 延迟销毁，可以看到断开效果
    }

    // 在编辑器中预览效果
    void OnDrawGizmos()
    {
        if (lineRenderer != null && lineRenderer.positionCount >= 2)
        {
            Gizmos.color = lineRenderer.startColor;
            Gizmos.DrawLine(lineRenderer.GetPosition(0), lineRenderer.GetPosition(1));
        }
    }
}