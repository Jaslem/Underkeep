// BossRoomManager.cs
using UnityEngine;

public class BossRoomManager : MonoBehaviour
{
    [Header("Boss Prefabs")]
    [Tooltip("Drag both boss prefabs here — one will be picked randomly")]
    public GameObject[] bossPrefabs;

    [Header("Spawn Point")]
    public Transform bossSpawnPoint;

    [Header("Room Sealing")]
    public GameObject[] sealingWalls;

    [Header("References")]
    public GameOverUI gameOverUI;

    private GameObject spawnedBoss;
    private EnemyStats bossStats;
    private bool       bossDefeated = false;
    private bool       triggered    = false;

    private void Start()
    {
        if (sealingWalls != null)
        {
            foreach (GameObject wall in sealingWalls)
                if (wall != null) wall.SetActive(false);
        }
    }

    public void OnPlayerEntered()
    {
        Debug.Log($"[BossRoomManager] OnPlayerEntered called — " +
                  $"triggered: {triggered}, " +
                  $"bossPrefabs null: {bossPrefabs == null}, " +
                  $"bossPrefabs length: {bossPrefabs?.Length}");

        if (triggered) return;
        triggered = true;

        if (sealingWalls != null)
        {
            foreach (GameObject wall in sealingWalls)
                if (wall != null) wall.SetActive(true);
        }

        SpawnBoss();
    }

    private void SpawnBoss()
    {
        Debug.Log($"[BossRoomManager] SpawnBoss called — " +
                  $"bossPrefabs null: {bossPrefabs == null}, " +
                  $"length: {bossPrefabs?.Length}");

        if (bossPrefabs == null || bossPrefabs.Length == 0)
        {
            Debug.LogWarning("[BossRoomManager] No boss prefabs assigned!");
            return;
        }

        // Filter out null entries
        System.Collections.Generic.List<GameObject> validBosses =
            new System.Collections.Generic.List<GameObject>();

        foreach (GameObject boss in bossPrefabs)
            if (boss != null) validBosses.Add(boss);

        if (validBosses.Count == 0)
        {
            Debug.LogWarning("[BossRoomManager] All boss prefab slots are null!");
            return;
        }

        // Pick a random boss
        GameObject bossPrefab = validBosses[Random.Range(0, validBosses.Count)];
        Debug.Log($"[BossRoomManager] Picked boss prefab: {bossPrefab.name}");

        Vector3 spawnPos = bossSpawnPoint != null
            ? bossSpawnPoint.position
            : transform.position;

        spawnedBoss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

        // Subscribe to boss death
        bossStats = spawnedBoss.GetComponent<EnemyStats>();
        if (bossStats != null)
            bossStats.OnDied.AddListener(OnBossDefeated);
        else
            Debug.LogWarning($"[BossRoomManager] Boss {bossPrefab.name} has no EnemyStats!");

        Debug.Log($"[BossRoomManager] Spawned boss: {spawnedBoss.name} at {spawnPos}");
    }

    private void OnBossDefeated()
    {
        if (bossDefeated) return;
        bossDefeated = true;

        if (sealingWalls != null)
        {
            foreach (GameObject wall in sealingWalls)
                if (wall != null) wall.SetActive(false);
        }

        if (gameOverUI != null)
            gameOverUI.ShowVictory();
        else
            Debug.LogWarning("[BossRoomManager] No GameOverUI assigned!");

        Debug.Log("[BossRoomManager] Boss defeated — Victory!");
    }

    private void OnDestroy()
    {
        if (bossStats != null)
            bossStats.OnDied.RemoveListener(OnBossDefeated);
    }
}