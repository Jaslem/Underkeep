// RoomEnemySpawner.cs
using UnityEngine;
using System.Collections.Generic;

public class RoomEnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [Tooltip("List of enemy prefabs to randomly choose from")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Settings")]
    [Tooltip("Minimum enemies per room")]
    public int minEnemiesPerRoom = 2;

    [Tooltip("Maximum enemies per room")]
    public int maxEnemiesPerRoom = 5;

    [Tooltip("Minimum enemies in the boss room (in addition to the boss)")]
    public int minBossRoomEnemies = 3;

    [Tooltip("Maximum enemies in the boss room")]
    public int maxBossRoomEnemies = 6;

    [Tooltip("How close the player must be to a room center to trigger spawning")]
    public float roomEnterDistance = 8f;

    private DungeonData dungeonData;
    private Transform   playerTransform;
    private float       checkInterval = 0.5f;
    private float       checkTimer    = 0f;

    public void Initialize(DungeonData data)
    {
        dungeonData = data;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void Update()
    {
        if (dungeonData == null || playerTransform == null) return;

        // Check every 0.5 seconds instead of every frame for performance
        checkTimer -= Time.deltaTime;
        if (checkTimer > 0f) return;
        checkTimer = checkInterval;

        CheckRoomEntry();
    }

    private void CheckRoomEntry()
    {
        foreach (DungeonRoom room in dungeonData.Rooms)
        {
            if (room.EnemiesSpawned) continue;
            if (room.Type == RoomType.Start) continue;
            if (room.EnemySpawnPoints.Count == 0) continue;

            // Check if player is close enough to the room center
            Vector2 roomCenter = new Vector2(room.Center.x, room.Center.y);
            float dist = Vector2.Distance(playerTransform.position, roomCenter);

            if (dist <= roomEnterDistance)
                SpawnEnemiesInRoom(room);
        }
    }

    private void SpawnEnemiesInRoom(DungeonRoom room)
    {
        room.EnemiesSpawned = true;

        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("RoomEnemySpawner: No enemy prefabs assigned!");
            return;
        }

        int count = room.Type == RoomType.Boss
            ? Random.Range(minBossRoomEnemies, maxBossRoomEnemies + 1)
            : Random.Range(minEnemiesPerRoom,  maxEnemiesPerRoom  + 1);

        // Shuffle spawn points and pick the first 'count' of them
        List<Vector2Int> spawnPoints = new List<Vector2Int>(room.EnemySpawnPoints);
        ShuffleList(spawnPoints);

        int spawnCount = Mathf.Min(count, spawnPoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Vector3 spawnPos  = new Vector3(spawnPoints[i].x, spawnPoints[i].y, 0f);
            Instantiate(prefab, spawnPos, Quaternion.identity);
        }

        Debug.Log($"[RoomEnemySpawner] Spawned {spawnCount} enemies in {room.Type} room.");
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j   = Random.Range(0, i + 1);
            T temp  = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}