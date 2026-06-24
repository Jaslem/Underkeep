// WeaponDataSO.cs
using UnityEngine;
using System.Collections.Generic;

public enum WeaponType { Melee, Ranged }

[System.Serializable]
public class WeaponAnimationEntry
{
    public CharacterClass characterClass;
    public AnimationClip  attackClip;
    [Tooltip("The sprite shown when this character is holding this weapon (idle pose)")]
    public Sprite         holdingSprite;
}

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/WeaponData")]
public class WeaponDataSO : ScriptableObject
{
    [Header("General")]
    public string     weaponName   = "Unnamed Weapon";
    public WeaponType weaponType   = WeaponType.Melee;
    public Sprite     weaponSprite;

    [Header("Character Animations and Sprites")]
    [Tooltip("Assign one entry per character class for this weapon")]
    public List<WeaponAnimationEntry> characterAttackAnimations = new List<WeaponAnimationEntry>();

    [Header("Enemy Animations")]
    [Tooltip("The animation clip enemies use when attacking with this weapon")]
    public AnimationClip enemyAttackClip;

    [Tooltip("The sprite shown when an enemy is holding this weapon")]
    public Sprite        enemyHoldingSprite;

    [Header("Damage Multipliers")]
    public float meleeDamageMultiplier  = 1f;
    public float rangedDamageMultiplier = 1f;

    [Header("Melee Settings")]
    public float meleeRange          = 1.5f;
    public float meleeAttackDuration = 0.3f;
    public float meleeCooldown       = 0.5f;
    public float meleeKnockback      = 3f;

    [Header("Ranged Settings")]
    public GameObject projectilePrefab;
    public float      fireRate        = 0.25f;
    public float      projectileSpeed = 15f;
    public int        magazineSize    = 10;
    public float      reloadTime      = 1.5f;
    public bool       isAutomatic     = false;

    [Header("Pickup")]
    public GameObject pickupPrefab;

    public AnimationClip GetAttackClip(CharacterClass characterClass)
    {
        foreach (WeaponAnimationEntry entry in characterAttackAnimations)
        {
            if (entry.characterClass == characterClass)
                return entry.attackClip;
        }
        Debug.LogWarning($"{weaponName}: No attack clip found for {characterClass}");
        return null;
    }

    public Sprite GetHoldingSprite(CharacterClass characterClass)
    {
        foreach (WeaponAnimationEntry entry in characterAttackAnimations)
        {
            if (entry.characterClass == characterClass)
                return entry.holdingSprite;
        }
        // Fall back to default weapon sprite
        return weaponSprite;
    }
}