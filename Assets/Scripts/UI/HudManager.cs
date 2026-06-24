// HUDManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("References")]
    public PlayerStats            playerStats;
    public PlayerWeaponController weaponController;
    public PlayerAbilityController abilityController;
    public PlayerCurrency         playerCurrency;

    // -------------------------------------------------------
    // Health Bar
    // -------------------------------------------------------
    [Header("Health Bar")]
    public Slider          healthSlider;
    public Image           healthFillImage;
    public TextMeshProUGUI healthText;        // e.g. "85 / 100"
    public Color           healthFullColor   = new Color(0.2f, 0.8f, 0.2f); // green
    public Color           healthLowColor    = new Color(0.8f, 0.1f, 0.1f); // red
    [Range(0f, 1f)]
    public float           lowHealthThreshold = 0.3f;

    // -------------------------------------------------------
    // Currency
    // -------------------------------------------------------
    [Header("Currency")]
    public TextMeshProUGUI currencyText;      // e.g. "✦ 120"

    // -------------------------------------------------------
    // Ammo Counter
    // -------------------------------------------------------
    [Header("Ammo Counter")]
    public TextMeshProUGUI ammoText;          // e.g. "10 / 10"
    public TextMeshProUGUI reloadingText;     // "RELOADING..."
    public GameObject      ammoPanel;         // hide when melee is active

    // -------------------------------------------------------
    // Ability Indicator
    // -------------------------------------------------------
    [Header("Ability Indicator")]
    public Image           abilityFillImage;  // radial fill — 0=cooldown, 1=ready
    public TextMeshProUGUI abilityNameText;   // e.g. "RAGE"
    public TextMeshProUGUI abilityCooldownText; // e.g. "4.2s" or "READY"
    public GameObject      abilityReadyGlow;  // optional glow object shown when ready

    // -------------------------------------------------------
    // Internal tracking
    // -------------------------------------------------------
    private RangedWeapon trackedRanged;

    // -------------------------------------------------------
    // Init
    // -------------------------------------------------------

    private void Start()
    {
        // Auto-find components on Player if not assigned
        if (playerStats == null || weaponController == null ||
            abilityController == null || playerCurrency == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                if (playerStats       == null) playerStats       = player.GetComponent<PlayerStats>();
                if (weaponController  == null) weaponController  = player.GetComponent<PlayerWeaponController>();
                if (abilityController == null) abilityController = player.GetComponent<PlayerAbilityController>();
                if (playerCurrency    == null) playerCurrency    = player.GetComponent<PlayerCurrency>();
            }
        }

        // Subscribe to events
        if (playerStats != null)
        {
            playerStats.OnHealthChanged.AddListener(UpdateHealth);
            UpdateHealth(playerStats.CurrentHealth, playerStats.MaxHealth);
        }

        if (playerCurrency != null)
        {
            playerCurrency.OnCurrencyChanged.AddListener(UpdateCurrency);
            UpdateCurrency(playerCurrency.CurrentCurrency);
        }

        // Subscribe to ranged weapon ammo events
        SubscribeToRanged(weaponController != null ? weaponController.GetRangedWeapon() : null);

        // Set ability name
        if (abilityNameText != null && abilityController != null)
            abilityNameText.text = abilityController.AbilityName().ToUpper();

        // Hide glow initially
        if (abilityReadyGlow != null)
            abilityReadyGlow.SetActive(false);
    }

    private void Update()
    {
        // Re-subscribe if ranged weapon changed (after pickup)
        if (weaponController != null)
        {
            RangedWeapon current = weaponController.GetRangedWeapon();
            if (current != trackedRanged)
            {
                UnsubscribeFromRanged(trackedRanged);
                SubscribeToRanged(current);
            }

            // Show/hide ammo panel based on active weapon
            if (ammoPanel != null)
                ammoPanel.SetActive(!weaponController.IsMeleeActive());
        }

        // Update ability indicator every frame
        UpdateAbilityIndicator();
    }

    // -------------------------------------------------------
    // Health
    // -------------------------------------------------------

    private void UpdateHealth(float current, float max)
    {
        float ratio = max > 0 ? current / max : 0f;

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = max;
            healthSlider.value    = current;
        }

        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";

        if (healthFillImage != null)
            healthFillImage.color = ratio <= lowHealthThreshold ? healthLowColor : healthFullColor;
    }

    // -------------------------------------------------------
    // Currency
    // -------------------------------------------------------

    private void UpdateCurrency(int amount)
    {
        if (currencyText != null)
            currencyText.text = $"$ {amount}";
    }

    // -------------------------------------------------------
    // Ammo
    // -------------------------------------------------------

    private void SubscribeToRanged(RangedWeapon weapon)
    {
        if (weapon == null) return;
        trackedRanged = weapon;
        weapon.OnAmmoChanged.AddListener(UpdateAmmo);
        weapon.OnReloadStart.AddListener(ShowReloading);
        weapon.OnReloadEnd.AddListener(HideReloading);
        UpdateAmmo(weapon.CurrentAmmo, weapon.weaponData.magazineSize);
    }

    private void UnsubscribeFromRanged(RangedWeapon weapon)
    {
        if (weapon == null) return;
        weapon.OnAmmoChanged.RemoveListener(UpdateAmmo);
        weapon.OnReloadStart.RemoveListener(ShowReloading);
        weapon.OnReloadEnd.RemoveListener(HideReloading);
        trackedRanged = null;
    }

    private void UpdateAmmo(int current, int max)
    {
        if (ammoText != null)
            ammoText.text = $"{current} / {max}";
    }

    private void ShowReloading()
    {
        if (reloadingText != null) reloadingText.gameObject.SetActive(true);
        if (ammoText      != null) ammoText.gameObject.SetActive(false);
    }

    private void HideReloading()
    {
        if (reloadingText != null) reloadingText.gameObject.SetActive(false);
        if (ammoText      != null) ammoText.gameObject.SetActive(true);
    }

    // -------------------------------------------------------
    // Ability Indicator
    // -------------------------------------------------------

    private void UpdateAbilityIndicator()
    {
        if (abilityController == null) return;

        PlayerAbility ability = abilityController.GetActiveAbility();
        if (ability == null) return;

        bool  isReady      = !ability.IsOnCooldown && !ability.IsActive;
        float cooldownRatio = ability.CooldownRatio; // 0 = ready, 1 = just used

        // Fill goes from 0 (on cooldown) to 1 (ready)
        if (abilityFillImage != null)
            abilityFillImage.fillAmount = 1f - cooldownRatio;

        // Cooldown timer text
        if (abilityCooldownText != null)
        {
            if (ability.IsActive)
                abilityCooldownText.text = "ACTIVE";
            else if (isReady)
                abilityCooldownText.text = "READY";
            else
                abilityCooldownText.text = $"{ability.CooldownTimer:F1}s";
        }

        // Show glow when ready
        if (abilityReadyGlow != null)
            abilityReadyGlow.SetActive(isReady);
    }

    // -------------------------------------------------------
    // Cleanup
    // -------------------------------------------------------

    private void OnDestroy()
    {
        if (playerStats    != null) playerStats.OnHealthChanged.RemoveListener(UpdateHealth);
        if (playerCurrency != null) playerCurrency.OnCurrencyChanged.RemoveListener(UpdateCurrency);
        UnsubscribeFromRanged(trackedRanged);
    }
}