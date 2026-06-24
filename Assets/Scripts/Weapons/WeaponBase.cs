// WeaponBase.cs
using UnityEngine;
using System.Collections.Generic;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Data")]
    public WeaponDataSO weaponData;

    [Header("Aim Settings")]
    [Tooltip("Offset in degrees to correct for sprite direction.")]
    public float aimAngleOffset = 0f;

    protected PlayerStats          playerStats;
    protected SpriteRenderer       weaponSpriteRenderer;
    protected UnityEngine.Animator weaponAnimator;
    protected bool                 canAttack = true;

    private static readonly int HashIsAttacking = UnityEngine.Animator.StringToHash("IsAttacking");

    public virtual void Initialize(PlayerStats stats)
    {
        if (stats == null)
        {
            Debug.LogError($"{gameObject.name}: PlayerStats is null!", this);
            return;
        }

        if (weaponData == null)
        {
            Debug.LogError($"{gameObject.name}: WeaponData not assigned!", this);
            return;
        }

        playerStats          = stats;
        weaponSpriteRenderer = GetComponent<SpriteRenderer>();
        weaponAnimator       = GetComponent<UnityEngine.Animator>();

        ApplyHoldingSprite();
        ApplyCharacterWeaponAnimation();
    }

    // -------------------------------------------------------
    // Holding Sprite
    // -------------------------------------------------------

    protected void ApplyHoldingSprite()
    {
        if (weaponSpriteRenderer == null) return;
        if (playerStats == null || playerStats.characterData == null) return;

        Sprite holdingSprite =
            weaponData.GetHoldingSprite(playerStats.characterData.characterClass);

        if (holdingSprite != null)
            weaponSpriteRenderer.sprite = holdingSprite;
        else if (weaponData.weaponSprite != null)
            weaponSpriteRenderer.sprite = weaponData.weaponSprite;
    }

    // -------------------------------------------------------
    // Attack Animation
    // -------------------------------------------------------

    protected void ApplyCharacterWeaponAnimation()
    {
        if (weaponAnimator == null) return;
        if (playerStats == null || playerStats.characterData == null) return;

        AnimationClip attackClip =
            weaponData.GetAttackClip(playerStats.characterData.characterClass);

        if (attackClip == null)
        {
            Debug.LogWarning($"{weaponData.weaponName}: No attack clip for " +
                             $"{playerStats.characterData.characterClass}");
            return;
        }

        AnimatorOverrideController overrideController =
            new AnimatorOverrideController(weaponAnimator.runtimeAnimatorController);

        List<KeyValuePair<AnimationClip, AnimationClip>> overrides =
            new List<KeyValuePair<AnimationClip, AnimationClip>>();

        overrideController.GetOverrides(overrides);

        // Replace the first non-null slot regardless of name
        for (int i = 0; i < overrides.Count; i++)
        {
            if (overrides[i].Key == null) continue;

            overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(
                overrides[i].Key, attackClip);
            break;
        }

        overrideController.ApplyOverrides(overrides);
        weaponAnimator.runtimeAnimatorController = overrideController;

        Debug.Log($"[WeaponBase] Applied {attackClip.name} for " +
                  $"{playerStats.characterData.characterClass} on {weaponData.weaponName}");
    }

    // -------------------------------------------------------
    // Core
    // -------------------------------------------------------

    public abstract void HandleAttackInput();

    public virtual void OnEquip()
    {
        gameObject.SetActive(true);
        ApplyHoldingSprite();
        ApplyCharacterWeaponAnimation();
    }

    public virtual void OnUnequip()
    {
        gameObject.SetActive(false);
    }

    // -------------------------------------------------------
    // Attack Animation Trigger
    // -------------------------------------------------------

    protected void TriggerAttackAnimation(bool value)
    {
        if (weaponAnimator != null)
            weaponAnimator.SetBool(HashIsAttacking, value);

        // Restore holding sprite when attack ends
        if (!value)
            ApplyHoldingSprite();
    }

    // -------------------------------------------------------
    // Aim
    // -------------------------------------------------------

    public virtual void AimAtMouse()
    {
        if (weaponData == null) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector3 aimDirection = (mouseWorldPos - transform.position).normalized;
        float angle          = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        float correctedAngle = angle + aimAngleOffset;

        transform.rotation = Quaternion.Euler(0f, 0f, correctedAngle);

        if (weaponSpriteRenderer != null)
        {
            weaponSpriteRenderer.flipY = false;
            weaponSpriteRenderer.flipX = angle > 90f || angle < -90f;
        }
    }
}