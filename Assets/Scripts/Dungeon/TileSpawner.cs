// TileSpawner.cs
using UnityEngine;
using System.Collections.Generic;

public class TileSpawner : MonoBehaviour
{
    [Header("Floor Sprites")]
    public Sprite[] floorSprites;           // Floor1-18

    [Header("Floor Edge Sprites")]
    public Sprite[] topFloorSprites;        // TopFloor1-6
    public Sprite[] bottomFloorSprites;     // BottomFloor1-2
    public Sprite   leftFloorSprite;        // LeftFloor1
    public Sprite   rightFloorSprite;       // RightFloor1
    public Sprite   topLeftFloorSprite;     // TopLeftFloor1
    public Sprite   topRightFloorSprite;    // TopRightFloor1
    public Sprite   bottomLeftFloorSprite;  // BottomLeftFloor1
    public Sprite   bottomRightFloorSprite; // BottomRightFloor1

    [Header("Wall Sprites")]
    public Sprite[] topWallSprites;         // TopWall1-4
    public Sprite[] bottomWallSprites;      // BottomWall1-4
    public Sprite[] leftWallSprites;        // LeftWall1-3
    public Sprite[] rightWallSprites;       // RightWall1-3
    public Sprite   topLeftWallSprite;      // TopLeftWall1
    public Sprite   topRightWallSprite;     // TopRightWall1
    public Sprite   bottomLeftWallSprite;   // BottomLeftWall1
    public Sprite   bottomRightWallSprite;  // BottomRightWall1

    [Header("Empty/Void Sprite")]
    [Tooltip("Sprite used for empty space outside rooms and hallways")]
    public Sprite emptySprite;

    [Tooltip("How many extra tiles of empty space to place around the dungeon border")]
    public int emptyBorderSize = 5;

    [Header("Tile Settings")]
    public float tileSize = 1f;

    [Header("Sorting Orders")]
    public int emptySortingOrder = -1;
    public int floorSortingOrder =  0;
    public int wallSortingOrder  =  1;

    private HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> wallPositions  = new HashSet<Vector2Int>();

    private GameObject tilesParent;

    public void SpawnAll(DungeonData data)
    {
        if (tilesParent != null)
            Destroy(tilesParent);

        tilesParent = new GameObject("DungeonTiles");
        floorPositions.Clear();
        wallPositions.Clear();

        CollectFloorPositions(data);
        CollectWallPositions();
        SpawnAllTiles();
    }

    // -------------------------------------------------------
    // Position Collection
    // -------------------------------------------------------

    private void CollectFloorPositions(DungeonData data)
    {
        foreach (DungeonRoom room in data.Rooms)
        {
            for (int x = room.Bounds.x; x < room.Bounds.xMax; x++)
                for (int y = room.Bounds.y; y < room.Bounds.yMax; y++)
                    floorPositions.Add(new Vector2Int(x, y));

            GenerateSpawnPoints(room);
        }

        foreach (RectInt hallway in data.Hallways)
        {
            for (int x = hallway.x; x < hallway.xMax; x++)
                for (int y = hallway.y; y < hallway.yMax; y++)
                    floorPositions.Add(new Vector2Int(x, y));
        }
    }

    private void CollectWallPositions()
    {
        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new Vector2Int( 1,  1), new Vector2Int(-1,  1),
            new Vector2Int( 1, -1), new Vector2Int(-1, -1)
        };

        foreach (Vector2Int pos in floorPositions)
        {
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbour = pos + dir;
                if (!floorPositions.Contains(neighbour))
                    wallPositions.Add(neighbour);
            }
        }
    }

    private void GenerateSpawnPoints(DungeonRoom room)
    {
        if (room.Type == RoomType.Start) return;

        int margin = 2;
        for (int x = room.Bounds.x + margin; x < room.Bounds.xMax - margin; x += 3)
            for (int y = room.Bounds.y + margin; y < room.Bounds.yMax - margin; y += 3)
                room.EnemySpawnPoints.Add(new Vector2Int(x, y));
    }

    // -------------------------------------------------------
    // Tile Spawning
    // -------------------------------------------------------

    private void SpawnAllTiles()
    {
        // Calculate bounds of the entire dungeon
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (Vector2Int pos in floorPositions)
        {
            if (pos.x < minX) minX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y > maxY) maxY = pos.y;
        }

        foreach (Vector2Int pos in wallPositions)
        {
            if (pos.x < minX) minX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y > maxY) maxY = pos.y;
        }

        // Expand bounds by border size for empty tiles
        minX -= emptyBorderSize;
        minY -= emptyBorderSize;
        maxX += emptyBorderSize;
        maxY += emptyBorderSize;

        // Spawn empty tiles first (bottom layer)
        if (emptySprite != null)
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (!floorPositions.Contains(pos) && !wallPositions.Contains(pos))
                        SpawnTile(emptySprite, pos, emptySortingOrder, false);
                }
            }
        }

        // Spawn floor tiles
        foreach (Vector2Int pos in floorPositions)
        {
            Sprite sprite = GetFloorSprite(pos);
            SpawnTile(sprite, pos, floorSortingOrder, false);
        }

        // Spawn wall tiles
        foreach (Vector2Int pos in wallPositions)
        {
            Sprite sprite = GetWallSprite(pos);
            if (sprite != null)
                SpawnTile(sprite, pos, wallSortingOrder, true);
        }
    }

    private void SpawnTile(Sprite sprite, Vector2Int gridPos, int sortingOrder, bool addCollider)
    {
        if (sprite == null) return;

        Vector3 worldPos        = new Vector3(gridPos.x * tileSize, gridPos.y * tileSize, 0f);
        GameObject tile         = new GameObject($"Tile_{gridPos.x}_{gridPos.y}");
        tile.transform.SetParent(tilesParent.transform);
        tile.transform.position = worldPos;

        SpriteRenderer sr   = tile.AddComponent<SpriteRenderer>();
        sr.sprite           = sprite;
        sr.sortingOrder     = sortingOrder;

        if (addCollider)
        {
            BoxCollider2D col = tile.AddComponent<BoxCollider2D>();
            col.size          = Vector2.one * tileSize;
            tile.tag          = "Wall";
        }
    }

    // -------------------------------------------------------
    // Floor Sprite Selection
    // -------------------------------------------------------

    private Sprite GetFloorSprite(Vector2Int pos)
    {
        bool hasFloorUp    = floorPositions.Contains(pos + Vector2Int.up);
        bool hasFloorDown  = floorPositions.Contains(pos + Vector2Int.down);
        bool hasFloorLeft  = floorPositions.Contains(pos + Vector2Int.left);
        bool hasFloorRight = floorPositions.Contains(pos + Vector2Int.right);

        // Corner floor edges
        if (!hasFloorUp   && !hasFloorLeft)  return topLeftFloorSprite;
        if (!hasFloorUp   && !hasFloorRight) return topRightFloorSprite;
        if (!hasFloorDown && !hasFloorLeft)  return bottomLeftFloorSprite;
        if (!hasFloorDown && !hasFloorRight) return bottomRightFloorSprite;

        // Single edge floor tiles
        if (!hasFloorUp)    return RandomSprite(topFloorSprites);
        if (!hasFloorDown)  return RandomSprite(bottomFloorSprites);
        if (!hasFloorLeft)  return leftFloorSprite;
        if (!hasFloorRight) return rightFloorSprite;

        // Interior floor
        return RandomSprite(floorSprites);
    }

    // -------------------------------------------------------
    // Wall Sprite Selection
    // -------------------------------------------------------

    private Sprite GetWallSprite(Vector2Int pos)
    {
        bool hasFloorUp    = floorPositions.Contains(pos + Vector2Int.up);
        bool hasFloorDown  = floorPositions.Contains(pos + Vector2Int.down);
        bool hasFloorLeft  = floorPositions.Contains(pos + Vector2Int.left);
        bool hasFloorRight = floorPositions.Contains(pos + Vector2Int.right);

        bool hasFloorUpLeft    = floorPositions.Contains(pos + new Vector2Int(-1,  1));
        bool hasFloorUpRight   = floorPositions.Contains(pos + new Vector2Int( 1,  1));
        bool hasFloorDownLeft  = floorPositions.Contains(pos + new Vector2Int(-1, -1));
        bool hasFloorDownRight = floorPositions.Contains(pos + new Vector2Int( 1, -1));

        // Corner walls
        if (hasFloorDownRight && !hasFloorDown && !hasFloorRight) return topLeftWallSprite;
        if (hasFloorDownLeft  && !hasFloorDown && !hasFloorLeft)  return topRightWallSprite;
        if (hasFloorUpRight   && !hasFloorUp   && !hasFloorRight) return bottomLeftWallSprite;
        if (hasFloorUpLeft    && !hasFloorUp   && !hasFloorLeft)  return bottomRightWallSprite;

        // Straight walls
        if (hasFloorDown)  return RandomSprite(topWallSprites);
        if (hasFloorUp)    return RandomSprite(bottomWallSprites);
        if (hasFloorRight) return RandomSprite(leftWallSprites);
        if (hasFloorLeft)  return RandomSprite(rightWallSprites);

        // Outer corners
        if (hasFloorDownRight) return topLeftWallSprite;
        if (hasFloorDownLeft)  return topRightWallSprite;
        if (hasFloorUpRight)   return bottomLeftWallSprite;
        if (hasFloorUpLeft)    return bottomRightWallSprite;

        return null;
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    private Sprite RandomSprite(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0) return null;
        return sprites[Random.Range(0, sprites.Length)];
    }
}