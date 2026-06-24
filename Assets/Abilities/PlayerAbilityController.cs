// PlayerAbilityController.cs
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Sits on the Player GameObject.
/// On Start, reads the selected character class from PlayerStats and
/// activates only the matching ability component.
/// All four ability components should be attached to the Player —
/// this controller enables only the correct one and disables the rest.
/// </summary>
public class PlayerAbilityController : MonoBehaviour
{
    [Header("Ability Components (attach all 4 to this GameObject)")]
    public Ability_Rage              rageAbility;
    public Ability_Shadowstep        shadowstepAbility;
    public Ability_DivineIntervention divineAbility;
    public Ability_ArcaneSurge       arcaneSurgeAbility;

    // UI event — passes (cooldownRatio 0→1, abilityName) for the HUD
    public UnityEvent<float, string> OnCooldownUpdated = new UnityEvent<float, string>();
    public UnityEvent<string>        OnAbilityActivated = new UnityEvent<string>();
    public UnityEvent<string>        OnAbilityEnded     = new UnityEvent<string>();

    private PlayerAbility activeAbility;
    private PlayerStats   playerStats;
    private PlayerMovement playerMovement;
    private PlayerWeaponController weaponController;

    private void Start()
    {
        playerStats      = GetComponent<PlayerStats>();
        playerMovement   = GetComponent<PlayerMovement>();
        weaponController = GetComponent<PlayerWeaponController>();

        // Disable all abilities first
        SetAllAbilitiesEnabled(false);

        if (playerStats == null || playerStats.characterData == null)
        {
            Debug.LogError("PlayerAbilityController: PlayerStats or characterData is null!");
            return;
        }

        // Pick the ability that matches the selected character class
        switch (playerStats.characterData.characterClass)
        {
            case CharacterClass.Barbarian:
                activeAbility = rageAbility;
                break;
            case CharacterClass.Rogue:
                activeAbility = shadowstepAbility;
                break;
            case CharacterClass.Cleric:
                activeAbility = divineAbility;
                break;
            case CharacterClass.Wizard:
                activeAbility = arcaneSurgeAbility;
                break;
            default:
                Debug.LogWarning($"PlayerAbilityController: No ability mapped for class " +
                                 $"{playerStats.characterData.characterClass}");
                return;
        }

        if (activeAbility == null)
        {
            Debug.LogWarning($"PlayerAbilityController: Ability component for " +
                             $"{playerStats.characterData.characterClass} is not assigned in the Inspector!");
            return;
        }

        // Enable and initialize the chosen ability
        activeAbility.enabled = true;
        activeAbility.Initialize(playerStats, playerMovement, weaponController);

        // Wire UI events
        activeAbility.OnCooldownTick.AddListener(ratio =>
            OnCooldownUpdated.Invoke(ratio, activeAbility.abilityName));

        activeAbility.OnAbilityActivated.AddListener(() =>
            OnAbilityActivated.Invoke(activeAbility.abilityName));

        activeAbility.OnAbilityEnded.AddListener(() =>
            OnAbilityEnded.Invoke(activeAbility.abilityName));

        Debug.Log($"[AbilityController] Equipped ability: {activeAbility.abilityName} " +
                  $"for {playerStats.characterData.characterClass}");
    }

    private void SetAllAbilitiesEnabled(bool enabled)
    {
        if (rageAbility       != null) rageAbility.enabled       = enabled;
        if (shadowstepAbility != null) shadowstepAbility.enabled = enabled;
        if (divineAbility     != null) divineAbility.enabled     = enabled;
        if (arcaneSurgeAbility != null) arcaneSurgeAbility.enabled = enabled;
    }

    // -------------------------------------------------------
    // Public getters (for UI)
    // -------------------------------------------------------

    public PlayerAbility GetActiveAbility()  => activeAbility;
    public bool          AbilityIsActive()   => activeAbility != null && activeAbility.IsActive;
    public bool          AbilityOnCooldown() => activeAbility != null && activeAbility.IsOnCooldown;
    public float         CooldownRatio()     => activeAbility != null ? activeAbility.CooldownRatio : 0f;
    public string        AbilityName()       => activeAbility != null ? activeAbility.abilityName : "";
}