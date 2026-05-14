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

    [System.Serializable]
    public class ObstacleData
    {
        public string name;
        public GameObject prefab;

        [Range(0f, 0.2f)]
        public float density = 0.05f;
    }

    [Header("Cài đặt Vật cản")]
    public List<ObstacleData> obstacleList = new List<ObstacleData>();

    [Header("Kiểm tra va chạm vật cản")]
    public float obstacleCheckRadius = 0.8f;
    public LayerMask obstacleCheckLayer;

    private Transform treeContainer;
    private Dictionary<Vector2Int, bool> generatedChunks = new Dictionary<Vector2Int, bool>();
    private Vector2Int currentPlayerChunk;

    void Start()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
            else
            {
                Debug.LogError("[MapGenerator] Không tìm thấy Player. Hãy gắn Tag 'Player' cho nhân vật hoặc kéo Player vào Inspector.");
                enabled = false;
                return;
            }
        }

        if (groundTilemap == null)
        {
            Debug.LogError("[MapGenerator] Chưa gán Ground Tilemap.");
            enabled = false;
            return;
        }

        if (chunkSize <= 0)
        {
            Debug.LogError("[MapGenerator] chunkSize phải lớn hơn 0.");
            enabled = false;
            return;
        }

        if (scale <= 0)
        {
            Debug.LogWarning("[MapGenerator] scale đang <= 0, tự đặt lại thành 15.");
            scale = 15f;
        }

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
                Vector2Int chunkToGenerate = new Vector2Int(
                    currentPlayerChunk.x + xOffset,
                    currentPlayerChunk.y + yOffset
                );

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

                if (noiseValue < 0.35f)
                {
                    tileToSet = waterTile;
                }
                else if (noiseValue < 0.45f)
                {
                    tileToSet = sandTile;
                }

                Vector3Int tilePosition = new Vector3Int(tileX, tileY, 0);
                groundTilemap.SetTile(tilePosition, tileToSet);

                if (tileToSet == grassTile)
                {
                    SpawnGrassDetail(tilePosition);
                    SpawnObstacles(tilePosition);
                }
            }
        }
    }

    void SpawnGrassDetail(Vector3Int tilePosition)
    {
        if (detailTilemap == null) return;
        if (grassDetails == null || grassDetails.Length == 0) return;

        if (Random.value < grassDensity)
        {
            TileBase randomGrass = grassDetails[Random.Range(0, grassDetails.Length)];
            detailTilemap.SetTile(tilePosition, randomGrass);
        }
    }

    void SpawnObstacles(Vector3Int tilePosition)
    {
        if (obstacleList == null || obstacleList.Count == 0) return;

        float roll = Random.value;
        float cumulativeDensity = 0f;

        foreach (ObstacleData obs in obstacleList)
        {
            if (obs == null) continue;

            cumulativeDensity += obs.density;

            if (roll < cumulativeDensity)
            {
                if (obs.prefab == null)
                {
                    Debug.LogWarning("[MapGenerator] Có obstacle chưa gán prefab.");
                    break;
                }

                Vector3 worldPos = groundTilemap.GetCellCenterWorld(tilePosition);

                Collider2D hit;

                if (obstacleCheckLayer.value == 0)
                {
                    hit = Physics2D.OverlapCircle(worldPos, obstacleCheckRadius);
                }
                else
                {
                    hit = Physics2D.OverlapCircle(worldPos, obstacleCheckRadius, obstacleCheckLayer);
                }

                if (hit == null)
                {
                    Instantiate(obs.prefab, worldPos, Quaternion.identity, treeContainer);
                }

                break;
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
    }
#endif
}