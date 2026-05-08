using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Cài đặt Thế giới vô tận")]
    public Transform player;
    public int chunkSize = 16;
    public int renderDistance = 2;

    [Header("Thuật toán Địa hình")]
    public float scale = 15f;
    private float offsetX;
    private float offsetY;

    [Header("Khay Gạch Nền")]
    public Tilemap groundTilemap;
    public TileBase waterTile;
    public TileBase sandTile;
    public TileBase grassTile;

    [Header("Cài đặt Trang trí (Cỏ, Hoa...)")]
    public Tilemap detailTilemap;
    public TileBase[] grassDetails;
    [Range(0f, 1f)] public float grassDensity = 0.2f;

    // --- PHẦN NÂNG CẤP: DANH SÁCH VẬT CẢN ĐA DẠNG ---
    [System.Serializable]
    public class ObstacleData
    {
        public string name;           // Tên gợi nhớ (ví dụ: Cây thông, Đá cuội)
        public GameObject prefab;     // Prefab của vật thể
        [Range(0f, 0.2f)]
        public float density = 0.05f; // Tỷ lệ mọc riêng của loại này
    }

    [Header("Cài đặt Vật cản (Đa dạng)")]
    public List<ObstacleData> obstacleList = new List<ObstacleData>();
    private Transform treeContainer;

    private Dictionary<Vector2Int, bool> generatedChunks = new Dictionary<Vector2Int, bool>();
    private Vector2Int currentPlayerChunk;

    void Start()
    {
        offsetX = Random.Range(-9999f, 9999f);
        offsetY = Random.Range(-9999f, 9999f);
        treeContainer = new GameObject("TreeContainer").transform;
        currentPlayerChunk = GetChunkPosition(player.position);
        UpdateChunks();
    }

    void Update()
    {
        if (player == null) return;
        Vector2Int currentChunk = GetChunkPosition(player.position);
        if (currentChunk != currentPlayerChunk)
        {
            currentPlayerChunk = currentChunk;
            UpdateChunks();
        }
    }

    Vector2Int GetChunkPosition(Vector3 playerPos)
    {
        if (groundTilemap == null) return Vector2Int.zero;
        Vector3Int cellPosition = groundTilemap.WorldToCell(playerPos);
        return new Vector2Int(
            Mathf.FloorToInt((float)cellPosition.x / chunkSize),
            Mathf.FloorToInt((float)cellPosition.y / chunkSize)
        );
    }

    void UpdateChunks()
    {
        for (int xOffset = -renderDistance; xOffset <= renderDistance; xOffset++)
        {
            for (int yOffset = -renderDistance; yOffset <= renderDistance; yOffset++)
            {
                Vector2Int chunkToGenerate = new Vector2Int(currentPlayerChunk.x + xOffset, currentPlayerChunk.y + yOffset);
                if (!generatedChunks.ContainsKey(chunkToGenerate))
                {
                    GenerateChunk(chunkToGenerate);
                    generatedChunks.Add(chunkToGenerate, true);
                }
            }
        }
    }

    void GenerateChunk(Vector2Int chunkCoords)
    {
        int startX = chunkCoords.x * chunkSize;
        int startY = chunkCoords.y * chunkSize;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                int tileX = startX + x;
                int tileY = startY + y;

                float xCoord = (float)tileX / scale + offsetX;
                float yCoord = (float)tileY / scale + offsetY;
                float noiseValue = Mathf.PerlinNoise(xCoord, yCoord);

                TileBase tileToSet = grassTile;
                if (noiseValue < 0.35f) tileToSet = waterTile;
                else if (noiseValue < 0.45f) tileToSet = sandTile;

                Vector3Int tilePosition = new Vector3Int(tileX, tileY, 0);
                groundTilemap.SetTile(tilePosition, tileToSet);

                if (tileToSet == grassTile)
                {
                    // 1. Trồng cỏ trang trí (Tile)
                    if (Random.value < grassDensity && grassDetails != null && grassDetails.Length > 0)
                    {
                        TileBase randomGrass = grassDetails[Random.Range(0, grassDetails.Length)];
                        detailTilemap.SetTile(tilePosition, randomGrass);
                    }

                    // 2. PHẦN NÂNG CẤP: Duyệt danh sách vật cản để spawn
                    SpawnObstacles(tilePosition);
                }
            }
        }
    }

    void SpawnObstacles(Vector3Int tilePosition)
    {
        foreach (ObstacleData obs in obstacleList)
        {
            if (obs.prefab != null && Random.value < obs.density)
            {
                Vector3 worldPos = groundTilemap.GetCellCenterWorld(tilePosition);
                Instantiate(obs.prefab, worldPos, Quaternion.identity, treeContainer);

                // break giúp đảm bảo mỗi ô gạch chỉ mọc tối đa 1 vật thể, 
                // tránh tình trạng Đá mọc đè lên Cây.
                break;
            }
        }
    }
}