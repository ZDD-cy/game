using System.Collections.Generic;
using UnityEngine;

public class LinkManager : MonoBehaviour
{
    public static LinkManager Instance; // 单例，方便全局访问

    [Header("预制体")]
    public GameObject linkVisualPrefab; // LinkVisual的预制体（可选）

    private Dictionary<int, LinkVisual> activeLinks = new Dictionary<int, LinkVisual>();
    private int nextLinkId = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 创建一条新的连接线（返回一个唯一ID，用于后续操作）
    public int CreateLink(Vector3 start, Vector3 end, Transform parent = null)
    {
        GameObject linkObj;
        if (linkVisualPrefab != null)
        {
            linkObj = Instantiate(linkVisualPrefab, parent);
        }
        else
        {
            linkObj = new GameObject("DynamicLink");
            if (parent != null) linkObj.transform.SetParent(parent);
            linkObj.AddComponent<LinkVisual>();
        }

        LinkVisual visual = linkObj.GetComponent<LinkVisual>();
        visual.Initialize(start, end, parent);

        int linkId = nextLinkId++;
        activeLinks.Add(linkId, visual);

        Debug.Log($"创建连接线，ID: {linkId}");
        return linkId;
    }

    // 根据ID更新连接线位置
    public void UpdateLinkPositions(int linkId, Vector3 newStart, Vector3 newEnd)
    {
        if (activeLinks.TryGetValue(linkId, out LinkVisual visual))
        {
            visual.UpdatePositions(newStart, newEnd);
        }
    }

    // 根据ID设置连接线状态（攻击系统会调用这个！）
    public void SetLinkState(int linkId, LinkState state)
    {
        if (activeLinks.TryGetValue(linkId, out LinkVisual visual))
        {
            visual.SetState(state);
        }
    }

    // 根据ID销毁连接线
    public void BreakLink(int linkId)
    {
        if (activeLinks.TryGetValue(linkId, out LinkVisual visual))
        {
            visual.Break();
            activeLinks.Remove(linkId);
        }
    }

    // 获取所有活跃连接线（DOT系统可能用来计算伤害）
    public Dictionary<int, LinkVisual> GetAllActiveLinks()
    {
        return new Dictionary<int, LinkVisual>(activeLinks);
    }
}