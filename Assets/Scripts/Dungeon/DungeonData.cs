// DungeonData.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Holds all data about the generated dungeon — rooms, hallways, and the boss room.
/// Passed between generator systems so they all work from the same layout.
/// </summary>
public class DungeonData
{
    public List<DungeonRoom> Rooms     = new List<DungeonRoom>();
    public DungeonRoom       StartRoom;
    public DungeonRoom       BossRoom;
    public List<RectInt>     Hallways  = new List<RectInt>();
}