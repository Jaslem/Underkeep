// Ability_Rage.cs
using UnityEngine;

/// <summary>
/// Barbarian — Rage
/// Temporarily boosts melee damage, attack speed, and damage resistance.
/// </summary>
public class Ability_Rage : PlayerAbility
{
    [Header("Rage Settings")]
    [Tooltip("Melee damage multiplier during Rage (e.g. 2.0 = double damage)")]
    public float meleeDamageMultiplier = 2f;

    [Tooltip("Attack speed multiplier during Rage — LOWER = faster (e.g. 0.5 = twice as fast)")]
    public float attackSpeedMultiplier = 0.5f;

    [Tooltip("Portion of incoming damage blocked (0–1, e.g. 0.4 = 40% reduction)")]
    public float damageResistance = 0.4f;

    private void Reset()
    {
        // Sensible defaults shown in Inspector when first added
        abilityName  = "Rage";
        cooldown     = 18f;
        duration     = 6f;
        activationKey = KeyCode.E;
    }

    protected override void OnActivate()
    {
        playerStats.SetMeleeDamageMultiplier(meleeDamageMultiplier);
        playerStats.SetAttackSpeedMultiplier(attackSpeedMultiplier);
        playerStats.SetDamageResistance(damageResistance);

        Debug.Log($"[Rage] ACTIVE — {meleeDamageMultiplier}x melee, " +
                  $"{attackSpeedMultiplier}x attack speed, {damageResistance * 100f}% resistance");
    }

    protected override void OnDeactivate()
    {
        playerStats.ResetModifiers();
        Debug.Log("[Rage] Ended — stats restored.");
    }
}