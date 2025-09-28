using System.Collections.Generic;
using UnityEngine;

public class BossArea : MonoBehaviour
{
    public static BossArea instance;

    [Header("Debug")]
    public bool showGizmos = true;

    [Header("Area Settings")]
    public int innerRadius = 10;
    public int outerRadius = 15;

    [Header("Boundary Effect")]
    public int dmgBound = 5;
    public float slowSpeed = 0.5f;

    [Header("Border Visual")]
    public Color borderColor = Color.red;
    public float borderWidth = 0.1f;
    private List<LineRenderer> borderLines = new List<LineRenderer>();

    private BoxCollider2D limitCollider;
    private Bounds bossInnerBounds;
    private Bounds bossOuterBounds;

    void Awake()
    {
        instance = this;
    }

    public void CreateBossArea(Vector3 playerPosition)
    {
        int playerGroup = GameController.instance.GetSpatialGroup(playerPosition.x, playerPosition.y);

        // Tính vùng trong
        List<int> innerGroups = Helper.GetExpandedSpatialGroupsV2(playerGroup, innerRadius);
        bossInnerBounds = CalculateBounds(innerGroups);

        // Tính vùng ngoài
        List<int> outerGroups = Helper.GetExpandedSpatialGroupsV2(playerGroup, outerRadius);
        bossOuterBounds = CalculateBounds(outerGroups);

        // Gắn collider trigger
        if (limitCollider == null)
        {
            limitCollider = gameObject.AddComponent<BoxCollider2D>();
            limitCollider.isTrigger = true;
        }

        transform.position = bossOuterBounds.center;
        limitCollider.size = bossOuterBounds.size;

        Debug.Log($"[BossArea] Created boss area at {bossOuterBounds.center} | Size = {bossOuterBounds.size}");

        DrawBorder();  // 👈 Vẽ viền tự động
    }

    private Bounds CalculateBounds(List<int> groups)
    {
        float cellSizeX = GameController.instance.SpatialGroupWidth / Mathf.Sqrt(GameController.instance.NumberOfPartitions);
        float cellSizeY = GameController.instance.SpatialGroupHeight / Mathf.Sqrt(GameController.instance.NumberOfPartitions);
        int cellPerRow = (int)Mathf.Sqrt(GameController.instance.NumberOfPartitions);

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (int group in groups)
        {
            int x = group % cellPerRow;
            int y = group / cellPerRow;

            float worldX = x * cellSizeX - GameController.instance.SpatialGroupWidth / 2f;
            float worldY = y * cellSizeY - GameController.instance.SpatialGroupHeight / 2f;

            minX = Mathf.Min(minX, worldX);
            maxX = Mathf.Max(maxX, worldX + cellSizeX);
            minY = Mathf.Min(minY, worldY);
            maxY = Mathf.Max(maxY, worldY + cellSizeY);
        }

        Vector2 center = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);
        Vector2 size = new Vector2(maxX - minX, maxY - minY);

        return new Bounds(center, size);
    }

    private void DrawBorder()
    {
        // Xóa viền cũ nếu có
        foreach (var line in borderLines)
        {
            if (line != null) Destroy(line.gameObject);
        }
        borderLines.Clear();

        // Tạo 4 cạnh viền
        Vector2 min = bossOuterBounds.min;
        Vector2 max = bossOuterBounds.max;

        Vector3[] corners = new Vector3[4];
        corners[0] = new Vector3(min.x, min.y, 0); // bottom left
        corners[1] = new Vector3(min.x, max.y, 0); // top left
        corners[2] = new Vector3(max.x, max.y, 0); // top right
        corners[3] = new Vector3(max.x, min.y, 0); // bottom right

        // Tạo 4 LineRenderer tương ứng 4 cạnh
        for (int i = 0; i < 4; i++)
        {
            GameObject lineObj = new GameObject("BorderLine_" + i);
            lineObj.transform.SetParent(transform);
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.SetPosition(0, corners[i]);
            line.SetPosition(1, corners[(i + 1) % 4]);
            line.startWidth = borderWidth;
            line.endWidth = borderWidth;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = borderColor;
            line.endColor = borderColor;
            line.sortingOrder = 100; // vẽ lên trên cùng

            borderLines.Add(line);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("player"))
        {
            Vector2 playerPos = other.transform.position;
            Vector2 min = bossOuterBounds.min;
            Vector2 max = bossOuterBounds.max;

            float distToLeft = Mathf.Abs(playerPos.x - min.x);
            float distToRight = Mathf.Abs(max.x - playerPos.x);
            float distToBottom = Mathf.Abs(playerPos.y - min.y);
            float distToTop = Mathf.Abs(max.y - playerPos.y);

            float minDist = Mathf.Min(distToLeft, distToRight, distToBottom, distToTop);

            if (minDist <= 1f)
            {
                Character.instance.ModifyHealth(dmgBound);
                Character.instance._speedMultiplier = slowSpeed;
            }
            else
            {
                Character.instance._speedMultiplier = 1f;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (bossInnerBounds.size != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(bossInnerBounds.center, bossInnerBounds.size);
        }

        if (bossOuterBounds.size != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(bossOuterBounds.center, bossOuterBounds.size);
        }
    }
}
