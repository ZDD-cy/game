using UnityEngine;

public class InfiniteTilemapLoop : MonoBehaviour
{
    [Header("References")]
    public Transform cam;
    public Transform[] chunks; // 15个块，手动拖入

    [Header("Chunk Settings")]
    public int chunkSizeInTiles = 6;   // 单块 6x6
    public float tileSize = 1f;        // Tile 单位大小 1x1

    private float chunkWorldSize;

    void Start()
    {
        chunkWorldSize = chunkSizeInTiles * tileSize;
    }

    void LateUpdate()
    {
        if (!cam || chunks == null || chunks.Length == 0) return;

        Vector3 camPos = cam.position;

        foreach (var chunk in chunks)
        {
            Vector3 pos = chunk.position;

            float dx = camPos.x - pos.x;
            float dy = camPos.y - pos.y;

            if (dx > chunkWorldSize * 0.5f) pos.x += chunkWorldSize * 1f;
            else if (dx < -chunkWorldSize * 0.5f) pos.x -= chunkWorldSize * 1f;

            if (dy > chunkWorldSize * 0.5f) pos.y += chunkWorldSize * 1f;
            else if (dy < -chunkWorldSize * 0.5f) pos.y -= chunkWorldSize * 1f;

            chunk.position = pos;
        }
    }
}