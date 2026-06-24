// PlayerHitEffect.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// Makes the player blink and grants iframes when hit.
/// Attach to the Player GameObject.
/// </summary>
public class PlayerHitEffect : MonoBehaviour
{
    [Header("Blink Settings")]
    [Tooltip("How long the player blinks after being hit (seconds)")]
    public float blinkDuration  = 1.5f;

    [Tooltip("How fast the player blinks — lower = faster")]
    public float blinkInterval  = 0.1f;

    [Header("References")]
    [Tooltip("Leave blank — found automatically")]
    public PlayerStats playerStats;

    private SpriteRenderer[] spriteRenderers;
    private Coroutine        blinkCoroutine;
    private bool             isBlinking = false;

    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        // Get all SpriteRenderers on player and children (includes weapon slots)
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (playerStats != null)
            playerStats.OnHealthChanged.AddListener(OnHealthChanged);
    }

    private void OnHealthChanged(float current, float max)
    {
        // Only trigger blink if health went down and player is alive
        if (!playerStats.IsAlive) return;
        if (isBlinking) return; // already blinking — don't restart

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(Blink());
    }

    private IEnumerator Blink()
    {
        isBlinking = true;

        // Grant iframes for the blink duration
        playerStats.SetInvulnerable(true);

        float timer = 0f;
        bool  visible = true;

        while (timer < blinkDuration)
        {
            // Toggle visibility
            visible = !visible;
            SetVisible(visible);

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        // Make sure player is visible when done
        SetVisible(true);

        // Remove iframes
        playerStats.SetInvulnerable(false);

        isBlinking     = false;
        blinkCoroutine = null;
    }

    private void SetVisible(bool visible)
    {
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr != null)
                sr.enabled = visible;
        }
    }

    private void OnDestroy()
    {
        if (playerStats != null)
            playerStats.OnHealthChanged.RemoveListener(OnHealthChanged);
    }
}