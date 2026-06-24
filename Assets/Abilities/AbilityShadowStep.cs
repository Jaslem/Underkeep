// Ability_Shadowstep.cs
using UnityEngine;

/// <summary>
/// Rogue — Shadowstep
/// Widens the dodge i-frame window and greatly boosts damage output,
/// but slows attack speed as a tradeoff.
/// </summary>
public class Ability_Shadowstep : PlayerAbility
{
    [Header("Shadowstep Settings")]
    [Tooltip("Multiplier applied to the dodge i-frame window (e.g. 2.5 = 2.5x longer invulnerability on roll)")]
    public float dodgeIFrameMultiplier = 2.5f;

    [Tooltip("Damage multiplier for both melee and ranged during Shadowstep")]
    public float damageMultiplier = 2.2f;

    [Tooltip("Attack speed penalty — HIGHER = slower (e.g. 1.6 = 60% slower)")]
    public float attackSpeedPenalty = 1.6f;

    private void Reset()
    {
        abilityName   = "Shadowstep";
        cooldown      = 14f;
        duration      = 5f;
        activationKey = KeyCode.E;
    }

    protected override void OnActivate()
    {
        playerStats.SetDodgeIFrameMultiplier(dodgeIFrameMultiplier);
        playerStats.SetMeleeDamageMultiplier(damageMultiplier);
        playerStats.SetRangedDamageMultiplier(damageMultiplier);
        playerStats.SetAttackSpeedMultiplier(attackSpeedPenalty);

        Debug.Log($"[Shadowstep] ACTIVE — {damageMultiplier}x damage, " +
                  $"{dodgeIFrameMultiplier}x i-frames, {attackSpeedPenalty}x attack speed (slower)");
    }

    protected override void OnDeactivate()
    {
        playerStats.ResetModifiers();
        Debug.Log("[Shadowstep] Ended — stats restored.");
    }
}