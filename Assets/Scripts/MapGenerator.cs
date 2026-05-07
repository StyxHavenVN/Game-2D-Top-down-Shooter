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

    // THÊM MỚI: Các biến để quản lý việc trồng cây (Vật thể)
    [Header("Cài đặt Vật cản (Cây cối)")]
    public GameObject treePrefab;             // Kéo Prefab cục Cây vào đây!
    [Range(0f, 1f)] public float treeDensity = 0.05f; // Tỷ lệ mọc cây (0.05 = 5%)
    private Transform treeContainer;          // Thùng chứa cây để cửa sổ Hierarchy không bị rác

    private Dictionary<Vector2Int, bool> generatedChunks = new Dictionary<Vector2Int, bool>();
    private Vector2Int currentPlayerChunk;

    void Start()
    {
        offsetX = Random.Range(-9999f, 9999f);
        offsetY = Random.Range(-9999f, 9999f);

        // Tạo ra một thư mục rỗng để chứa toàn bộ cây sinh ra, giúp game đỡ lộn xộn
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

                // Tọa độ của ô gạch hiện tại
                Vector3Int tilePosition = new Vector3Int(tileX, tileY, 0);

                // Lát gạch nền đất
                groundTilemap.SetTile(tilePosition, tileToSet);

                // Chỉ trồng cây cỏ lên trên lớp đất (grassTile)
                if (tileToSet == grassTile)
                {
                    // 1. Trồng cỏ trang trí (Tile)
                    if (Random.value < grassDensity && grassDetails != null && grassDetails.Length > 0)
                    {
                        TileBase randomGrass = grassDetails[Random.Range(0, grassDetails.Length)];
                        detailTilemap.SetTile(tilePosition, randomGrass);
                    }

                    // 2. THÊM MỚI: Trồng cây làm vật cản (GameObject)
                    // Random.value sẽ quay số từ 0.0 đến 1.0. Nếu nhỏ hơn 0.05 thì trúng giải trồng cây
                    if (treePrefab != null && Random.value < treeDensity)
                    {
                        // Lấy tọa độ trung tâm của ô gạch để đặt cây cho ngay ngắn
                        Vector3 worldPos = groundTilemap.GetCellCenterWorld(tilePosition);

                        // Đẻ ra cái cây và nhét nó vào TreeContainer
                        Instantiate(treePrefab, worldPos, Quaternion.identity, treeContainer);
                    }
                }
            }
        }
    }
}