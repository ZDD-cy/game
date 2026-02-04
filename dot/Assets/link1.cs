using UnityEngine;

public class SimpleLinkDemo : MonoBehaviour
{
    private LineRenderer lineRenderer;

    void Start()
    {
        // 1. 为当前物体动态添加一个LineRenderer组件（这支“笔”）
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        // 2. 对这支“笔”进行基本设置
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // 使用一个简单的默认着色器
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2; // 声明这条线由2个点构成（一条线段）
    }

    void Update()
    {
        // 3. 每帧确定线的起点（本物体位置）和终点（鼠标世界坐标）
        Vector3 startPos = transform.position;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // 如果是2D游戏，确保Z轴一致

        // 4. 告诉LineRenderer这两个点的位置，线就自动画出来了
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, mousePos);

        // 5. 【额外效果】动态改变宽度，产生脉冲感
        float pulse = Mathf.Sin(Time.time * 5f) * 0.03f; // 5是脉冲速度，0.03是强度
        lineRenderer.startWidth = 0.1f + pulse;
        lineRenderer.endWidth = 0.1f + pulse;
    }
}