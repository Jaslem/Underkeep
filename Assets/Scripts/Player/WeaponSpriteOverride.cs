// WeaponSpriteOverride.cs
using UnityEngine;

[System.Serializable]
public class WeaponSpriteOverride
{
    [Tooltip("The weapon this sprite applies to")]
    public WeaponDataSO weapon;

    [Tooltip("The sprite this character uses when holding that weapon")]
    public Sprite sprite;
}