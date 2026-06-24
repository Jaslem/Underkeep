/// CharacterDataSO.cs
using UnityEngine;
using System.Collections.Generic;

public enum CharacterClass
{
    Wizard,
    Cleric,
    Rogue,
    Barbarian
}

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Characters/CharacterData")]
public class CharacterDataSO : ScriptableObject
{
    [Header("Identity")]
    public CharacterClass characterClass;
    public string characterName;
    public Sprite characterSprite;
    [TextArea] public string characterDescription;

    [Header("Stats")]
    [Tooltip("Movement speed in units per second")]
    public float moveSpeed = 5f;

    [Tooltip("Dodge roll speed multiplier")]
    public float mobilityMultiplier = 1f;

    [Tooltip("Maximum health points")]
    public float maxHealth = 100f;

    [Tooltip("Base melee damage before weapon multiplier")]
    public float baseMeleeDamage = 20f;

    [Tooltip("Base ranged damage before weapon multiplier")]
    public float baseRangedDamage = 15f;

    [Header("Weapon Sprite Overrides")]
    [Tooltip("For each weapon this character can hold, assign the sprite they use for it")]
    public List<WeaponSpriteOverride> weaponSpriteOverrides = new List<WeaponSpriteOverride>();

    /// <summary>
    /// Returns the character-specific sprite for a given weapon.
    /// Falls back to the weapon's default sprite if no override exists.
    /// </summary>
    public Sprite GetWeaponSprite(WeaponDataSO weapon)
    {
        if (weapon == null) return null;

        foreach (WeaponSpriteOverride entry in weaponSpriteOverrides)
        {
            if (entry.weapon == weapon)
                return entry.sprite;
        }

        // No override found — use the weapon's default sprite
        return weapon.weaponSprite;
    }
}