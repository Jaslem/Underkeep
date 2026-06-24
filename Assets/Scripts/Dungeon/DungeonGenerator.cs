/// DungeonGenerator.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Room Count")]
    public int minRooms = 10;
    public int maxRooms = 15;

    [Header("Room Size")]
    public int minRoomWidth  = 8;
    public int maxRoomWidth  = 16;
    public int minRoomHeight = 8;
    public int maxRoomHeight = 16;

    [Header("Boss Room Size")]
    public int bossRoomWidth  = 20;
    public int bossRoomHeight = 20;

    [Header("Generation Settings")]
    public int placementRadius      = 40;
    public int maxPlacementAttempts = 100;

    [Header("References")]
    public TileSpawner      tileSpawner;
    public RoomEnemySpawner enemySpawner;
    public ChestSpawner     chestSpawner;

    [Header("Boss Room")]
    public GameOverUI   gameOverUI;
    public GameObject[] bossPrefabs;

    public DungeonData Data { get; private set; }

    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        Data = new DungeonData();

        PlaceRooms();
        ConnectRooms();
        GenerateHallways();
        SpawnTiles();
        SpawnPlayer();
        SpawnChests();
        SetupBossRoom();

        Debug.Log($"[DungeonGenerator] Generated {Data.Rooms.Count} rooms " +
                  $"and {Data.Hallways.Count} hallways.");
    }

    // -------------------------------------------------------
    // Room Placement
    // -------------------------------------------------------

    private void PlaceRooms()
    {
        int roomCount = Random.Range(minRooms, maxRooms + 1);

        DungeonRoom startRoom = CreateRoom(Vector2Int.zero, RoomType.Start);
        Data.Rooms.Add(startRoom);
        Data.StartRoom = startRoom;

        for (int i = 0; i < roomCount; i++)
        {
            DungeonRoom room = TryPlaceRoom(RoomType.Normal);
            if (room != null)
                Data.Rooms.Add(room);
        }

        DungeonRoom bossRoom = PlaceBossRoom();
        Data.Rooms.Add(bossRoom);
        Data.BossRoom = bossRoom;
    }

    private DungeonRoom TryPlaceRoom(RoomType type)
    {
        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            int w = Random.Range(minRoomWidth,  maxRoomWidth  + 1);
            int h = Random.Range(minRoomHeight, maxRoomHeight + 1);
            int x = Random.Range(-placementRadius, placementRadius);
            int y = Random.Range(-placementRadius, placementRadius);

            DungeonRoom candidate = new DungeonRoom(new RectInt(x, y, w, h), type);

            bool overlaps = Data.Rooms.Any(r => r.Overlaps(candidate));
            if (!overlaps)
                return candidate;
        }

        Debug.LogWarning("[DungeonGenerator] Could not place room after max attempts.");
        return null;
    }

    private DungeonRoom CreateRoom(Vector2Int pos, RoomType type)
    {
        int w = type == RoomType.Boss ? bossRoomWidth  : Random.Range(minRoomWidth,  maxRoomWidth  + 1);
        int h = type == RoomType.Boss ? bossRoomHeight : Random.Range(minRoomHeight, maxRoomHeight + 1);
        return new DungeonRoom(new RectInt(pos.x, pos.y, w, h), type);
    }

    private DungeonRoom PlaceBossRoom()
    {
        DungeonRoom furthest = Data.Rooms
            .Where(r => r.Type == RoomType.Normal)
            .OrderByDescending(r => Vector2Int.Distance(r.Center, Data.StartRoom.Center))
            .FirstOrDefault();

        Vector2Int bossPos = furthest != null
            ? furthest.Center + new Vector2Int(furthest.Bounds.width + 5, 0)
            : new Vector2Int(placementRadius + 10, 0);

        DungeonRoom bossRoom = new DungeonRoom(
            new RectInt(bossPos.x, bossPos.y, bossRoomWidth, bossRoomHeight),
            RoomType.Boss
        );

        int pushAttempts = 0;
        while (Data.Rooms.Any(r => r.Overlaps(bossRoom)) && pushAttempts < 50)
        {
            bossPos  += new Vector2Int(2, 0);
            bossRoom  = new DungeonRoom(
                new RectInt(bossPos.x, bossPos.y, bossRoomWidth, bossRoomHeight),
                RoomType.Boss
            );
            pushAttempts++;
        }

        return bossRoom;
    }

    // -------------------------------------------------------
    // Room Connection
    // -------------------------------------------------------

    private void ConnectRooms()
    {
        List<DungeonRoom> connected   = new List<DungeonRoom> { Data.StartRoom };
        List<DungeonRoom> unconnected = new List<DungeonRoom>(Data.Rooms);
        unconnected.Remove(Data.StartRoom);
        unconnected.Remove(Data.BossRoom);

        while (unconnected.Count > 0)
        {
            DungeonRoom bestA    = null;
            DungeonRoom bestB    = null;
            float       bestDist = float.MaxValue;

            foreach (DungeonRoom c in connected)
            {
                foreach (DungeonRoom u in unconnected)
                {
                    float dist = Vector2Int.Distance(c.Center, u.Center);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestA    = c;
                        bestB    = u;
                    }
                }
            }

            if (bestA != null && bestB != null)
            {
                bestA.ConnectedRooms.Add(bestB);
                bestB.ConnectedRooms.Add(bestA);
                connected.Add(bestB);
                unconnected.Remove(bestB);
            }
        }

        DungeonRoom bossConnector = Data.Rooms
            .Where(r => r.Type == RoomType.Normal)
            .OrderByDescending(r => Vector2Int.Distance(r.Center, Data.StartRoom.Center))
            .FirstOrDefault();

        if (bossConnector != null)
        {
            bossConnector.ConnectedRooms.Add(Data.BossRoom);
            Data.BossRoom.ConnectedRooms.Add(bossConnector);
        }
    }

    // -------------------------------------------------------
    // Hallways
    // -------------------------------------------------------

    private void GenerateHallways()
    {
        HashSet<string> generated = new HashSet<string>();

        foreach (DungeonRoom room in Data.Rooms)
        {
            foreach (DungeonRoom connected in room.ConnectedRooms)
            {
                string key = GetConnectionKey(room, connected);
                if (generated.Contains(key)) continue;
                generated.Add(key);
                GenerateLHallway(room.Center, connected.Center);
            }
        }
    }

    private void GenerateLHallway(Vector2Int a, Vector2Int b)
    {
        int hallwayWidth = 3;

        if (Random.value > 0.5f)
        {
            Data.Hallways.Add(GetHorizontalSegment(a, new Vector2Int(b.x, a.y), hallwayWidth));
            Data.Hallways.Add(GetVerticalSegment(new Vector2Int(b.x, a.y), b, hallwayWidth));
        }
        else
        {
            Data.Hallways.Add(GetVerticalSegment(a, new Vector2Int(a.x, b.y), hallwayWidth));
            Data.Hallways.Add(GetHorizontalSegment(new Vector2Int(a.x, b.y), b, hallwayWidth));
        }
    }

    private RectInt GetHorizontalSegment(Vector2Int from, Vector2Int to, int width)
    {
        int x = Mathf.Min(from.x, to.x);
        int y = from.y - width / 2;
        int w = Mathf.Abs(to.x - from.x) + width;
        int h = width;
        return new RectInt(x, y, w, h);
    }

    private RectInt GetVerticalSegment(Vector2Int from, Vector2Int to, int width)
    {
        int x = from.x - width / 2;
        int y = Mathf.Min(from.y, to.y);
        int w = width;
        int h = Mathf.Abs(to.y - from.y) + width;
        return new RectInt(x, y, w, h);
    }

    private string GetConnectionKey(DungeonRoom a, DungeonRoom b)
    {
        int idA = Data.Rooms.IndexOf(a);
        int idB = Data.Rooms.IndexOf(b);
        return idA < idB ? $"{idA}-{idB}" : $"{idB}-{idA}";
    }

    // -------------------------------------------------------
    // Spawn Tiles
    // -------------------------------------------------------

    private void SpawnTiles()
    {
        if (tileSpawner != null)
            tileSpawner.SpawnAll(Data);
        else
            Debug.LogWarning("[DungeonGenerator] No TileSpawner assigned!");

        if (enemySpawner != null)
            enemySpawner.Initialize(Data);
        else
            Debug.LogWarning("[DungeonGenerator] No RoomEnemySpawner assigned!");
    }

    // -------------------------------------------------------
    // Spawn Player
    // -------------------------------------------------------

    private void SpawnPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3 startPos = new Vector3(
            Data.StartRoom.Center.x,
            Data.StartRoom.Center.y,
            0f
        );
        player.transform.position = startPos;
    }

    // -------------------------------------------------------
    // Spawn Chests
    // -------------------------------------------------------

    private void SpawnChests()
    {
        if (chestSpawner != null)
            chestSpawner.SpawnChests(Data);
        else
            Debug.LogWarning("[DungeonGenerator] No ChestSpawner assigned!");
    }

    // -------------------------------------------------------
    // Setup Boss Room
    // -------------------------------------------------------

    private void SetupBossRoom()
{
    Debug.Log($"[DungeonGenerator] SetupBossRoom called — " +
              $"BossRoom null: {Data.BossRoom == null}");

    if (Data.BossRoom == null)
    {
        Debug.LogWarning("[DungeonGenerator] No boss room generated!");
        return;
    }

    Vector3 bossCenter = new Vector3(
        Data.BossRoom.Center.x,
        Data.BossRoom.Center.y,
        0f
    );

    Debug.Log($"[DungeonGenerator] Boss room center: {bossCenter}");

    // Create boss room manager GameObject
    GameObject bossRoomGO         = new GameObject("BossRoomSetup");
    bossRoomGO.transform.position = bossCenter;

    // Add BossRoomManager
    BossRoomManager manager = bossRoomGO.AddComponent<BossRoomManager>();
    manager.bossPrefabs     = bossPrefabs;
    manager.gameOverUI      = gameOverUI;

    // Create spawn point — set position BEFORE assigning to manager
    GameObject spawnPoint         = new GameObject("BossSpawnPoint");
    spawnPoint.transform.parent   = bossRoomGO.transform;
    spawnPoint.transform.position = bossCenter; // explicit world position
    manager.bossSpawnPoint        = spawnPoint.transform;

    Debug.Log($"[DungeonGenerator] Spawn point set to: {spawnPoint.transform.position}");

    // Create trigger at bottom edge of boss room
    GameObject triggerGO          = new GameObject("BossRoomTrigger");
    triggerGO.transform.parent    = bossRoomGO.transform;
    triggerGO.transform.position  = new Vector3(
        Data.BossRoom.Center.x,
        Data.BossRoom.Bounds.y,
        0f
    );

    BoxCollider2D triggerCol  = triggerGO.AddComponent<BoxCollider2D>();
    triggerCol.isTrigger      = true;
    triggerCol.size           = new Vector2(Data.BossRoom.Bounds.width, 2f);

    BossRoomTrigger trigger   = triggerGO.AddComponent<BossRoomTrigger>();
    trigger.bossRoomManager   = manager;

    Debug.Log($"[DungeonGenerator] Boss room setup complete at {bossCenter}");
}
}