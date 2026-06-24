// PlayerStats.cs
using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
    [Header("Character Data")]
    [Tooltip("Fallback only — overridden at runtime by GameManager.selectedCharacter")]
    public CharacterDataSO characterData;

    // -------------------------------------------------------
    // Base stats (set once from CharacterDataSO)
    // -------------------------------------------------------
    public float MoveSpeed          { get; private set; }
    public float MobilityMultiplier { get; private set; }
    public float MaxHealth          { get; private set; }
    public float CurrentHealth      { get; private set; }
    public float BaseMeleeDamage    { get; private set; }
    public float BaseRangedDamage   { get; private set; }

    // -------------------------------------------------------
    // Temporary modifiers (set by abilities, reset on deactivate)
    // -------------------------------------------------------
    public float MeleeDamageMultiplier   { get; private set; } = 1f;
    public float RangedDamageMultiplier  { get; private set; } = 1f;
    public float AttackSpeedMultiplier   { get; private set; } = 1f; // <1 = faster, >1 = slower
    public float DamageResistance        { get; private set; } = 0f; // 0-1, portion of damage blocked
    public bool  IsInvulnerable          { get; private set; } = false;
    public float DodgeIFrameMultiplier   { get; private set; } = 1f;

    public bool IsAlive => CurrentHealth > 0;

    // -------------------------------------------------------
    // Events
    // -------------------------------------------------------
    public UnityEvent<float, float> OnHealthChanged = new UnityEvent<float, float>();
    public UnityEvent               OnPlayerDied    = new UnityEvent();

    // -------------------------------------------------------
    // Init
    // -------------------------------------------------------

    private void Awake()
    {
        if (GameManager.Instance != null && GameManager.Instance.selectedCharacter != null)
            characterData = GameManager.Instance.selectedCharacter;

        if (characterData == null)
        {
            Debug.LogError("PlayerStats: No CharacterData assigned and none found in GameManager!");
            return;
        }

        InitializeStats();
    }

    private void InitializeStats()
    {
        MoveSpeed          = characterData.moveSpeed;
        MobilityMultiplier = characterData.mobilityMultiplier;
        MaxHealth          = characterData.maxHealth;
        CurrentHealth      = characterData.maxHealth;
        BaseMeleeDamage    = characterData.baseMeleeDamage;
        BaseRangedDamage   = characterData.baseRangedDamage;

        ResetModifiers();
    }

    public void SetCharacterData(CharacterDataSO data)
    {
        characterData = data;
        InitializeStats();
    }

    // -------------------------------------------------------
    // Modifier API (used by abilities)
    // -------------------------------------------------------

    public void ResetModifiers()
    {
        MeleeDamageMultiplier  = 1f;
        RangedDamageMultiplier = 1f;
        AttackSpeedMultiplier  = 1f;
        DamageResistance       = 0f;
        IsInvulnerable         = false;
        DodgeIFrameMultiplier  = 1f;
    }

    public void SetMeleeDamageMultiplier(float value)   => MeleeDamageMultiplier  = value;
    public void SetRangedDamageMultiplier(float value)  => RangedDamageMultiplier = value;
    public void SetAttackSpeedMultiplier(float value)   => AttackSpeedMultiplier  = value;
    public void SetDamageResistance(float value)        => DamageResistance       = Mathf.Clamp01(value);
    public void SetInvulnerable(bool value)             => IsInvulnerable         = value;
    public void SetDodgeIFrameMultiplier(float value)   => DodgeIFrameMultiplier  = value;

    // -------------------------------------------------------
    // Health
    // -------------------------------------------------------

    public void TakeDamage(float amount)
    {
        if (!IsAlive || IsInvulnerable) return;

        float mitigated = amount * (1f - DamageResistance);
        CurrentHealth = Mathf.Clamp(CurrentHealth - mitigated, 0f, MaxHealth);
        OnHealthChanged.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0f, MaxHealth);
        OnHealthChanged.Invoke(CurrentHealth, MaxHealth);
    }

    public void HealToFull()
    {
        CurrentHealth = MaxHealth;
        OnHealthChanged.Invoke(CurrentHealth, MaxHealth);
    }

    // -------------------------------------------------------
    // Damage helpers (weapons call these)
    // -------------------------------------------------------

    public float GetFinalMeleeDamage(float weaponMeleeMultiplier)
        => BaseMeleeDamage * weaponMeleeMultiplier * MeleeDamageMultiplier;

    public float GetFinalRangedDamage(float weaponRangedMultiplier)
        => BaseRangedDamage * weaponRangedMultiplier * RangedDamageMultiplier;

    // -------------------------------------------------------
    // Death
    // -------------------------------------------------------

    private void Die()
    {
        Debug.Log($"{characterData.characterName} has died.");
        OnPlayerDied.Invoke();
    }
}