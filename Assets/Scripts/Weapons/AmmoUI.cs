// AmmoUI.cs
using UnityEngine;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    [Header("References")]
    public PlayerWeaponController weaponController;

    [Header("UI Elements")]
    public TextMeshProUGUI ammoText;
    public GameObject reloadingText; // optional "RELOADING..." label

    private RangedWeapon trackedRanged;

    private void Start()
    {
        if (weaponController == null)
        {
            Debug.LogWarning("AmmoUI: No PlayerWeaponController assigned.");
            return;
        }

        SubscribeToRanged(weaponController.GetRangedWeapon());
    }

    private void Update()
    {
        // Re-subscribe if the ranged weapon slot changed (after a pickup)
        RangedWeapon current = weaponController.GetRangedWeapon();
        if (current != trackedRanged)
        {
            UnsubscribeFromRanged(trackedRanged);
            SubscribeToRanged(current);
        }
    }

    private void SubscribeToRanged(RangedWeapon weapon)
    {
        if (weapon == null) return;
        trackedRanged = weapon;
        weapon.OnAmmoChanged.AddListener(UpdateAmmoDisplay);
        weapon.OnReloadStart.AddListener(ShowReloading);
        weapon.OnReloadEnd.AddListener(HideReloading);

        // Force an immediate UI refresh
        UpdateAmmoDisplay(weapon.CurrentAmmo, weapon.weaponData.magazineSize);
    }

    private void UnsubscribeFromRanged(RangedWeapon weapon)
    {
        if (weapon == null) return;
        weapon.OnAmmoChanged.RemoveListener(UpdateAmmoDisplay);
        weapon.OnReloadStart.RemoveListener(ShowReloading);
        weapon.OnReloadEnd.RemoveListener(HideReloading);
        trackedRanged = null;
    }

    private void UpdateAmmoDisplay(int current, int max)
    {
        if (ammoText != null)
            ammoText.text = $"{current} / {max}";
    }

    private void ShowReloading()
    {
        if (reloadingText != null)
            reloadingText.SetActive(true);
    }

    private void HideReloading()
    {
        if (reloadingText != null)
            reloadingText.SetActive(false);
    }

    private void OnDestroy()
    {
        UnsubscribeFromRanged(trackedRanged);
    }
}
