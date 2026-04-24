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

    // THÊM MỚI: Các biến để quản lý việc trồng cỏ
    [Header("Cài đặt Trang trí (Cỏ, Hoa...)")]
    public Tilemap detailTilemap;             // Kéo lớp LopCo vào đây
    public TileBase[] grassDetails;           // Danh sách các loại cỏ trang trí
    [Range(0f, 1f)] public float grassDensity = 0.2f; // Tỷ lệ mọc cỏ (0.2 = 20%)

    private Dictionary<Vector2Int, bool> generatedChunks = new Dictionary<Vector2Int, bool>();
    private Vector2Int currentPlayerChunk;

    void Start()
    {
        offsetX = Random.Range(-9999f, 9999f);
        offsetY = Random.Range(-9999f, 9999f);

        // MỚI: Định vị ngay vị trí ô gạch của nhân vật lúc mới vào game
        currentPlayerChunk = GetChunkPosition(player.position);
        UpdateChunks();
    }

    void Update()
    {
        if (player == null) return;

        // MỚI: Liên tục kiểm tra xem nhân vật đang dẫm lên ô gạch nào
        Vector2Int currentChunk = GetChunkPosition(player.position);

        if (currentChunk != currentPlayerChunk)
        {
            currentPlayerChunk = currentChunk;
            UpdateChunks();
        }
    }

    // MỚI: Hàm chuyên dụng để dịch tọa độ thực tế sang tọa độ ô lưới (Cell)
    Vector2Int GetChunkPosition(Vector3 playerPos)
    {
        if (groundTilemap == null) return Vector2Int.zero;

        // WorldToCell là "chìa khóa vàng" giúp map sinh ra cực chuẩn xác
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

                // Lát gạch nền đất
                groundTilemap.SetTile(new Vector3Int(tileX, tileY, 0), tileToSet);

                // THÊM MỚI: Tự động trồng cỏ lên trên lớp đất
                // Chỉ trồng nếu ô đất vừa lát là ô cỏ (grassTile)
                if (tileToSet == grassTile)
                {
                    // Kiểm tra tỷ lệ phần trăm (grassDensity) và đảm bảo có cỏ trong danh sách
                    if (Random.value < grassDensity && grassDetails != null && grassDetails.Length > 0)
                    {
                        // Chọn ngẫu nhiên 1 cọng cỏ trong mảng grassDetails
                        TileBase randomGrass = grassDetails[Random.Range(0, grassDetails.Length)];

                        // Đặt cọng cỏ đó lên lớp detailTilemap (LopCo)
                        detailTilemap.SetTile(new Vector3Int(tileX, tileY, 0), randomGrass);
                    }
                }
            }
        }
    }
}