// Ability_DivineIntervention.cs
using UnityEngine;

/// <summary>
/// Cleric — Divine Intervention
/// Instantly heals the player to full health and grants temporary invulnerability.
/// </summary>
public class Ability_DivineIntervention : PlayerAbility
{
    private void Reset()
    {
        abilityName   = "Divine Intervention";
        cooldown      = 25f;
        duration      = 4f;
        activationKey = KeyCode.E;
    }

    protected override bool CanActivate()
    {
        return true;
    }

    protected override void OnActivate()
    {
        playerStats.HealToFull();
        playerStats.SetInvulnerable(true);

        Debug.Log($"[Divine Intervention] ACTIVE — healed to full, invulnerable for {duration}s");
    }

    protected override void OnDeactivate()
    {
        playerStats.SetInvulnerable(false);
        playerStats.ResetModifiers();
        Debug.Log("[Divine Intervention] Ended — invulnerability removed.");
    }
}