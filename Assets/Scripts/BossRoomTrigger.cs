// BossRoomTrigger.cs
using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    [Header("References")]
    public BossRoomManager bossRoomManager;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[BossRoomTrigger] Trigger entered by: {other.gameObject.name}");

        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        if (bossRoomManager != null)
            bossRoomManager.OnPlayerEntered();
        else
            Debug.LogWarning("[BossRoomTrigger] No BossRoomManager assigned!");
    }
}