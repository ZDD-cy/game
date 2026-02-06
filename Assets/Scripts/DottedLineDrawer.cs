using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AntLineDrawer : MonoBehaviour
{
    public Transform player;
    public float lineWidth = 0.08f;
    public float textureScrollSpeed = 2f;

    private LineRenderer lr;
    private Material lineMat;
    private float lineLen;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        lineMat = lr.material;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            lr.enabled = true;

            if (Camera.main != null)
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorld.z = 0f;

                lr.SetPosition(0, player.position);
                lr.SetPosition(1, mouseWorld);
            }

            // 蚂蚁线滚动效果
            float offset = Time.time * textureScrollSpeed;
            lineLen = (lr.GetPosition(1)-lr.GetPosition(0)).magnitude;
            lineMat.mainTextureScale = new Vector2(lineLen,1f);
            lineMat.mainTextureOffset = new Vector2(offset*-1, 0f);
        }
        else
        {
            lr.enabled = false;
        }
    }
}