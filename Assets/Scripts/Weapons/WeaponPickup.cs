// WeaponPickup.cs
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon To Give")]
    [Tooltip("The WeaponDataSO that defines this weapon's stats")]
    public WeaponDataSO weaponData;

    [Tooltip("The weapon prefab (with MeleeWeapon or RangedWeapon component) to instantiate on the player")]
    public GameObject weaponPrefab;

    [Header("Visuals (optional)")]
    [Tooltip("Assign a SpriteRenderer on this pickup object")]
    public SpriteRenderer pickupSprite;

    [Header("Pickup Settings")]
    public bool destroyOnPickup = true;

    private void Start()
    {
        // Auto-assign sprite from weapon data if available
        if (pickupSprite != null && weaponData != null && weaponData.weaponSprite != null)
        {
            pickupSprite.sprite = weaponData.weaponSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerWeaponController weaponController = other.GetComponent<PlayerWeaponController>();

        if (weaponController == null)
        {
            Debug.LogWarning("WeaponPickup: Player does not have a PlayerWeaponController!");
            return;
        }

        if (weaponData == null || weaponPrefab == null)
        {
            Debug.LogWarning("WeaponPickup: weaponData or weaponPrefab not assigned!");
            return;
        }

        // Hand the weapon off to the controller
        weaponController.PickUpWeapon(weaponData, weaponPrefab);

        if (destroyOnPickup)
            Destroy(gameObject);
    }

    // Draw a label in the scene view for easy identification
    private void OnDrawGizmos()
    {
        if (weaponData == null) return;
#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.5f,
            weaponData.weaponName
        );
#endif
    }
}
