// EnemyTypeData.cs
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemyWeaponAnimationEntry
{
    [Tooltip("The weapon this animation applies to")]
    public WeaponDataSO  weapon;

    [Tooltip("The attack animation clip this enemy type uses with that weapon")]
    public AnimationClip attackClip;

    [Tooltip("The sprite shown when this enemy is holding that weapon")]
    public Sprite        holdingSprite;
}

[CreateAssetMenu(fileName = "NewEnemyType", menuName = "Enemies/EnemyTypeData")]
public class EnemyTypeData : ScriptableObject
{
    [Header("Identity")]
    public string enemyTypeName;

    [Header("Weapon Animations")]
    public List<EnemyWeaponAnimationEntry> weaponAnimations = new List<EnemyWeaponAnimationEntry>();

    public AnimationClip GetAttackClip(WeaponDataSO weapon)
    {
        if (weapon == null) return null;

        foreach (EnemyWeaponAnimationEntry entry in weaponAnimations)
        {
            if (entry.weapon == weapon)
                return entry.attackClip;
        }

        return weapon.enemyAttackClip;
    }

    public Sprite GetHoldingSprite(WeaponDataSO weapon)
    {
        if (weapon == null) return null;

        foreach (EnemyWeaponAnimationEntry entry in weaponAnimations)
        {
            if (entry.weapon == weapon)
                return entry.holdingSprite != null ? entry.holdingSprite : weapon.enemyHoldingSprite;
        }

        return weapon.enemyHoldingSprite;
    }
}