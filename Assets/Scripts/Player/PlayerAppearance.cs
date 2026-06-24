// PlayerAppearance.cs
using UnityEngine;

public class PlayerAppearance : MonoBehaviour
{
    [Header("Sprite Renderer")]
    public SpriteRenderer playerSpriteRenderer;

    [Header("Character Animator Overrides")]
    public AnimatorOverrideController wizardAnimator;
    public AnimatorOverrideController clericAnimator;
    public AnimatorOverrideController rogueAnimator;
    public AnimatorOverrideController barbarianAnimator;

    private void Start()
    {
        PlayerStats stats = GetComponent<PlayerStats>();

        if (stats == null || stats.characterData == null)
        {
            Debug.LogWarning("PlayerAppearance: No PlayerStats or characterData found.");
            return;
        }

        if (playerSpriteRenderer == null)
            playerSpriteRenderer = GetComponent<SpriteRenderer>();

        if (playerSpriteRenderer != null && stats.characterData.characterSprite != null)
            playerSpriteRenderer.sprite = stats.characterData.characterSprite;

        UnityEngine.Animator bodyAnimator = GetComponent<UnityEngine.Animator>();
        AnimatorOverrideController characterOverride = null;

        switch (stats.characterData.characterClass)
        {
            case CharacterClass.Wizard:    characterOverride = wizardAnimator;    break;
            case CharacterClass.Cleric:    characterOverride = clericAnimator;    break;
            case CharacterClass.Rogue:     characterOverride = rogueAnimator;     break;
            case CharacterClass.Barbarian: characterOverride = barbarianAnimator; break;
        }

        if (bodyAnimator != null && characterOverride != null)
            bodyAnimator.runtimeAnimatorController = characterOverride;

        Debug.Log($"[Appearance] Set up {stats.characterData.characterName}");
    }
}