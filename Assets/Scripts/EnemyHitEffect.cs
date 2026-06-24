// EnemyHitEffect.cs
using System.Collections;
using UnityEngine;

public class EnemyHitEffect : MonoBehaviour
{
    [Header("Flash Settings")]
    public float flashDuration = 0.1f;

    private SpriteRenderer[] spriteRenderers;
    private Color[]          originalColors;
    private Coroutine        flashCoroutine;
    private EnemyStats       enemyStats;

    private void Awake()
    {
        enemyStats      = GetComponent<EnemyStats>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                originalColors[i] = spriteRenderers[i].color;
        }
    }

    private void Start()
    {
        if (enemyStats != null)
            enemyStats.OnHealthChanged.AddListener(OnHealthChanged);
    }

    private void OnHealthChanged(float current, float max)
    {
        if (!enemyStats.IsAlive) return;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        // Flash red on hit
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                spriteRenderers[i].color = Color.red;
        }

        yield return new WaitForSeconds(flashDuration);

        // Restore original colors
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                spriteRenderers[i].color = originalColors[i];
        }

        flashCoroutine = null;
    }

    private void OnDestroy()
    {
        if (enemyStats != null)
            enemyStats.OnHealthChanged.RemoveListener(OnHealthChanged);
    }
}