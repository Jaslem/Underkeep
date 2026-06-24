// Ability_ArcaneSurge.cs
using UnityEngine;

/// <summary>
/// Wizard — Arcane Surge
/// Greatly increases ranged fire rate and ranged damage output.
/// </summary>
public class Ability_ArcaneSurge : PlayerAbility
{
    [Header("Arcane Surge Settings")]
    [Tooltip("Ranged damage multiplier during Arcane Surge (e.g. 2.5 = 2.5x damage)")]
    public float rangedDamageMultiplier = 2.5f;

    [Tooltip("Fire rate multiplier — LOWER = faster (e.g. 0.3 = fire 3x as fast)")]
    public float fireRateMultiplier = 0.3f;

    private void Reset()
    {
        abilityName   = "Arcane Surge";
        cooldown      = 20f;
        duration      = 5f;
        activationKey = KeyCode.E;
    }

    protected override void OnActivate()
    {
        playerStats.SetRangedDamageMultiplier(rangedDamageMultiplier);
        playerStats.SetAttackSpeedMultiplier(fireRateMultiplier);

        Debug.Log($"[Arcane Surge] ACTIVE — {rangedDamageMultiplier}x ranged damage, " +
                  $"{fireRateMultiplier}x fire rate");
    }

    protected override void OnDeactivate()
    {
        playerStats.ResetModifiers();
        Debug.Log("[Arcane Surge] Ended — stats restored.");
    }
}