// ChestSpawner.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns chests randomly in dungeon rooms after generation.
/// Attach to the DungeonGenerator GameObject.
/// </summary>
public class ChestSpawner : MonoBehaviour
{
    [Header("Chest Settings")]
    [Tooltip("The chest prefab to spawn")]
    public GameObject chestPrefab;

    [Tooltip("Chance a room gets a chest (0 = never, 1 = always)")]
    [Range(0f, 1f)]
    public float chestSpawnChance = 0.4f;

    [Tooltip("Maximum chests per room")]
    public int maxChestsPerRoom = 1;

    [Tooltip("Don't spawn chests in the start room or boss room")]
    public bool skipStartAndBossRooms = true;

    // Called by DungeonGenerator after rooms are placed
    public void SpawnChests(DungeonData dungeonData)
    {
        if (chestPrefab == null)
        {
            Debug.LogWarning("[ChestSpawner] No chest prefab assigned!");
            return;
        }

        if (dungeonData == null || dungeonData.Rooms == null) return;

        foreach (DungeonRoom room in dungeonData.Rooms)
        {
            if (skipStartAndBossRooms &&
                (room.Type == RoomType.Start || room.Type == RoomType.Boss))
                continue;

            // Random chance to spawn a chest in this room
            if (Random.value > chestSpawnChance) continue;

            int chestsToSpawn = Random.Range(1, maxChestsPerRoom + 1);

            for (int i = 0; i < chestsToSpawn; i++)
                SpawnChestInRoom(room);
        }
    }

    private void SpawnChestInRoom(DungeonRoom room)
    {
        // Pick a random position inside the room avoiding edges
        int margin = 2;
        int x = Random.Range(room.Bounds.x + margin, room.Bounds.xMax - margin);
        int y = Random.Range(room.Bounds.y + margin, room.Bounds.yMax - margin);

        Vector3 spawnPos = new Vector3(x, y, 0f);

        Instantiate(chestPrefab, spawnPos, Quaternion.identity);
    }
}