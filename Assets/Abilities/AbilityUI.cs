// AbilityUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityUI : MonoBehaviour
{
    [Header("References")]
    public PlayerAbilityController abilityController;

    [Header("UI Elements")]
    public Image cooldownFillImage;       // Radial fill image
    public TextMeshProUGUI abilityNameText;
    public TextMeshProUGUI cooldownTimerText; // optional, shows "4.2s"
    public GameObject activeIndicator;       // optional glow/highlight when active

    private void Start()
    {
        if (abilityController == null)
        {
            Debug.LogWarning("AbilityUI: No PlayerAbilityController assigned.");
            return;
        }

        // Wire up events
        abilityController.OnCooldownUpdated.AddListener(UpdateCooldown);
        abilityController.OnAbilityActivated.AddListener(OnActivated);
        abilityController.OnAbilityEnded.AddListener(OnEnded);

        // Set ability name on start
        if (abilityNameText != null)
            abilityNameText.text = abilityController.AbilityName();
    }

    private void Update()
    {
        // Keep timer text updated every frame
        if (cooldownTimerText != null && abilityController != null)
        {
            float timer = abilityController.GetActiveAbility() != null
                ? abilityController.GetActiveAbility().CooldownTimer
                : 0f;

            cooldownTimerText.text = timer > 0f ? $"{timer:F1}s" : "Ready";
        }
    }

    private void UpdateCooldown(float ratio, string name)
    {
        if (cooldownFillImage != null)
            cooldownFillImage.fillAmount = 1f - ratio; // 0 = empty (on cooldown), 1 = full (ready)

        if (abilityNameText != null)
            abilityNameText.text = name;
    }

    private void OnActivated(string name)
    {
        if (activeIndicator != null)
            activeIndicator.SetActive(true);
    }

    private void OnEnded(string name)
    {
        if (activeIndicator != null)
            activeIndicator.SetActive(false);
    }

    private void OnDestroy()
    {
        if (abilityController == null) return;
        abilityController.OnCooldownUpdated.RemoveListener(UpdateCooldown);
        abilityController.OnAbilityActivated.RemoveListener(OnActivated);
        abilityController.OnAbilityEnded.RemoveListener(OnEnded);
    }
}