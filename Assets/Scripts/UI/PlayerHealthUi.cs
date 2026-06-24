// PlayerHealthUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;

    [Header("UI Elements")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText; // e.g. "85 / 100"
    public Image fillImage;            // optional color change on low health

    [Header("Low Health Settings")]
    public float lowHealthThreshold = 0.25f; // 25% health
    public Color normalColor = Color.green;
    public Color lowHealthColor = Color.red;

    private void Start()
    {
        if (playerStats == null)
        {
            Debug.LogWarning("PlayerHealthUI: No PlayerStats assigned.");
            return;
        }

        // Subscribe to health change event
        playerStats.OnHealthChanged.AddListener(UpdateHealthUI);

        // Initialize display
        UpdateHealthUI(playerStats.CurrentHealth, playerStats.MaxHealth);
    }

    private void UpdateHealthUI(float current, float max)
    {
        float ratio = current / max;

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";

        if (fillImage != null)
            fillImage.color = ratio <= lowHealthThreshold ? lowHealthColor : normalColor;
    }

    private void OnDestroy()
    {
        if (playerStats != null)
            playerStats.OnHealthChanged.RemoveListener(UpdateHealthUI);
    }
}
