// DungeonRoom.cs
using UnityEngine;
using System.Collections.Generic;

public enum RoomType { Start, Normal, Boss }

public class DungeonRoom
{
    public RectInt  Bounds;       // position and size in tile coordinates
    public RoomType Type;
    public List<DungeonRoom> ConnectedRooms = new List<DungeonRoom>();

    // Enemy spawning
    public List<Vector2Int> EnemySpawnPoints = new List<Vector2Int>();
    public bool EnemiesSpawned = false;

    // Runtime reference to the room's GameObject container
    public GameObject RoomObject;

    public DungeonRoom(RectInt bounds, RoomType type)
    {
        Bounds = bounds;
        Type   = type;
    }

    /// <summary>Center of the room in tile coordinates.</summary>
    public Vector2Int Center => new Vector2Int(
        Bounds.x + Bounds.width  / 2,
        Bounds.y + Bounds.height / 2
    );

    /// <summary>Returns true if this room overlaps another (with padding).</summary>
    public bool Overlaps(DungeonRoom other, int padding = 2)
    {
        RectInt a = new RectInt(
            Bounds.x      - padding,
            Bounds.y      - padding,
            Bounds.width  + padding * 2,
            Bounds.height + padding * 2
        );
        return a.Overlaps(other.Bounds);
    }
}