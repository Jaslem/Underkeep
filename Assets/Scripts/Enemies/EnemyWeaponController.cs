// EnemyWeaponController.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyWeaponController : MonoBehaviour
{
    [Header("Possible Weapons")]
    public WeaponDataSO[] possibleWeapons;

    [Header("Enemy Type")]
    public EnemyTypeData enemyTypeData;

    [Header("Character Type (optional)")]
    public CharacterDataSO characterData;

    [Header("Weapon Holder")]
    public Transform weaponHolder;

    public WeaponDataSO EquippedWeapon { get; private set; }
    public bool IsMelee  => EquippedWeapon != null && EquippedWeapon.weaponType == WeaponType.Melee;
    public bool IsRanged => EquippedWeapon != null && EquippedWeapon.weaponType == WeaponType.Ranged;

    private EnemyStats     enemyStats;
    private SpriteRenderer weaponSpriteRenderer;
    private AnimationClip  storedAttackClip;
    private Coroutine      attackAnimCoroutine;

    private void Awake()
    {
        enemyStats = GetComponent<EnemyStats>();

        if (enemyStats == null)
            Debug.LogError($"{gameObject.name}: EnemyWeaponController can't find EnemyStats!", this);

        if (weaponHolder != null)
        {
            weaponSpriteRenderer = weaponHolder.GetComponent<SpriteRenderer>();
        }
        else
        {
            Debug.LogError($"{gameObject.name}: WeaponHolder is not assigned!", this);
        }
    }

    private void Start()
    {
        if (possibleWeapons == null || possibleWeapons.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name}: No weapons assigned!");
            return;
        }

        // Filter out null entries
        List<WeaponDataSO> validWeapons = new List<WeaponDataSO>();
        foreach (WeaponDataSO w in possibleWeapons)
            if (w != null) validWeapons.Add(w);

        if (validWeapons.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name}: All weapon slots are null!");
            return;
        }

        EquippedWeapon = validWeapons[Random.Range(0, validWeapons.Count)];

        ApplyHoldingSprite();
        StoreAttackClip();

        Debug.Log($"{gameObject.name} equipped: {EquippedWeapon.weaponName}");
    }

    // -------------------------------------------------------
    // Holding Sprite
    // -------------------------------------------------------

    private void ApplyHoldingSprite()
    {
        if (weaponSpriteRenderer == null || EquippedWeapon == null) return;

        Sprite spriteToUse = null;

        if (enemyTypeData != null)
            spriteToUse = enemyTypeData.GetHoldingSprite(EquippedWeapon);

        if (spriteToUse == null)
            spriteToUse = EquippedWeapon.enemyHoldingSprite;

        if (spriteToUse == null)
            spriteToUse = EquippedWeapon.weaponSprite;

        if (spriteToUse != null)
            weaponSpriteRenderer.sprite = spriteToUse;
    }

    // -------------------------------------------------------
    // Store Attack Clip
    // -------------------------------------------------------

    private void StoreAttackClip()
    {
        if (EquippedWeapon == null) return;

        AnimationClip attackClip = enemyTypeData != null
            ? enemyTypeData.GetAttackClip(EquippedWeapon)
            : EquippedWeapon.enemyAttackClip;

        if (attackClip == null)
        {
            Debug.LogWarning($"{gameObject.name}: No attack clip for {EquippedWeapon.weaponName}");
            return;
        }

        storedAttackClip = attackClip;
        Debug.Log($"[EnemyWeaponController] Stored attack clip: {attackClip.name}");
    }

    // -------------------------------------------------------
    // Attack Animation
    // -------------------------------------------------------

    public void TriggerAttackAnimation(bool value)
    {
        if (value && storedAttackClip != null)
        {
            if (attackAnimCoroutine != null)
                StopCoroutine(attackAnimCoroutine);
            attackAnimCoroutine = StartCoroutine(PlayAttackClip(storedAttackClip));
        }
        else
        {
            if (attackAnimCoroutine != null)
            {
                StopCoroutine(attackAnimCoroutine);
                attackAnimCoroutine = null;
            }
            ApplyHoldingSprite();
        }
    }

    private IEnumerator PlayAttackClip(AnimationClip clip)
{
    float timer = 0f;

    // Sample on the root enemy GameObject since that's
    // what the clips were recorded on
    GameObject targetObject = transform.root.gameObject;

    Debug.Log($"[EnemyWeaponController] Sampling on: {targetObject.name}");

    while (timer < clip.length)
    {
        clip.SampleAnimation(targetObject, timer);
        timer += Time.deltaTime;
        yield return null;
    }

    clip.SampleAnimation(targetObject, clip.length);
    ApplyHoldingSprite();
    attackAnimCoroutine = null;

    Debug.Log($"[EnemyWeaponController] Finished playing {clip.name}");
}
    // -------------------------------------------------------
    // Aim
    // -------------------------------------------------------

    public void AimAtTarget(Vector3 targetPos)
    {
        if (weaponHolder == null) return;

        Vector3 dir = (targetPos - weaponHolder.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        weaponHolder.rotation = Quaternion.Euler(0f, 0f, angle);

        if (weaponSpriteRenderer != null)
            weaponSpriteRenderer.flipY = angle > 90f || angle < -90f;
    }

    // -------------------------------------------------------
    // Damage
    // -------------------------------------------------------

    public float GetFinalMeleeDamage()
    {
        if (enemyStats == null || EquippedWeapon == null) return 0f;
        return enemyStats.meleeDamage * EquippedWeapon.meleeDamageMultiplier;
    }

    public float GetFinalRangedDamage()
    {
        if (enemyStats == null || EquippedWeapon == null) return 0f;
        return enemyStats.rangedDamage * EquippedWeapon.rangedDamageMultiplier;
    }
}