// PlayerAbility.cs
using UnityEngine;
using UnityEngine.Events;

public abstract class PlayerAbility : MonoBehaviour
{
    [Header("Ability Settings")]
    public string abilityName = "Ability";
    public float cooldown = 10f;
    public float duration = 5f;
    public KeyCode activationKey = KeyCode.E;

    // State
    public bool IsActive       { get; private set; }
    public bool IsOnCooldown   => cooldownTimer > 0f;
    public float CooldownTimer => cooldownTimer;
    public float CooldownRatio => cooldownTimer / cooldown; // 0 = ready, 1 = just used

    protected PlayerStats playerStats;
    protected PlayerMovement playerMovement;
    protected PlayerWeaponController weaponController;

    private float cooldownTimer = 0f;
    private float durationTimer = 0f;

    // Events for UI
    public UnityEvent OnAbilityActivated  = new UnityEvent();
    public UnityEvent OnAbilityEnded      = new UnityEvent();
    public UnityEvent<float> OnCooldownTick = new UnityEvent<float>(); // passes CooldownRatio 0→1

    // -------------------------------------------------------
    // Setup
    // -------------------------------------------------------

    public virtual void Initialize(PlayerStats stats, PlayerMovement movement, PlayerWeaponController controller)
    {
        playerStats      = stats;
        playerMovement   = movement;
        weaponController = controller;
    }

    // -------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer < 0f) cooldownTimer = 0f;
            OnCooldownTick.Invoke(CooldownRatio);
        }

        if (IsActive)
        {
            durationTimer -= Time.deltaTime;
            OnAbilityTick(durationTimer / duration); // passes 1→0 progress to subclass

            if (durationTimer <= 0f)
                EndAbility();
        }

        if (Input.GetKeyDown(activationKey) && !IsOnCooldown && !IsActive && playerStats.IsAlive)
            TryActivate();
    }

    // -------------------------------------------------------
    // Activation
    // -------------------------------------------------------

    private void TryActivate()
    {
        if (!CanActivate()) return;

        IsActive      = true;
        durationTimer = duration;
        cooldownTimer = cooldown;

        OnActivate();
        OnAbilityActivated.Invoke();
        Debug.Log($"[Ability] {abilityName} activated!");
    }

    private void EndAbility()
    {
        IsActive = false;
        OnDeactivate();
        OnAbilityEnded.Invoke();
        Debug.Log($"[Ability] {abilityName} ended.");
    }

    // -------------------------------------------------------
    // Overrideable hooks
    // -------------------------------------------------------

    /// <summary>Return false to block activation (e.g. already at full health).</summary>
    protected virtual bool CanActivate() => true;

    /// <summary>Called once when the ability starts.</summary>
    protected abstract void OnActivate();

    /// <summary>Called once when the ability ends.</summary>
    protected abstract void OnDeactivate();

    /// <summary>Called every frame while ability is active. progress goes 1→0.</summary>
    protected virtual void OnAbilityTick(float progress) { }
}