// ChestWeaponPopupUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI popup that appears when a chest drops a weapon.
/// Shows the weapon sprite, name, and stats with swap/decline buttons.
/// </summary>
public class ChestWeaponPopupUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject popupPanel;

    [Header("Weapon Info")]
    public Image              weaponIcon;
    public TextMeshProUGUI    weaponNameText;
    public TextMeshProUGUI    weaponTypeText;
    public TextMeshProUGUI    weaponStatsText;

    [Header("Current Weapon Info")]
    public TextMeshProUGUI    currentWeaponNameText;
    public Image              currentWeaponIcon;

    [Header("Buttons")]
    public Button swapButton;
    public Button declineButton;

    // Internal state
    private WeaponDataSO          offeredWeapon;
    private PlayerWeaponController weaponController;

    private void Start()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        if (swapButton != null)
            swapButton.onClick.AddListener(OnSwapPressed);

        if (declineButton != null)
            declineButton.onClick.AddListener(OnDeclinePressed);
    }

    /// <summary>
    /// Show the popup with the offered weapon.
    /// </summary>
    public void Show(WeaponDataSO weapon, PlayerWeaponController controller)
    {
        if (weapon == null || controller == null) return;

        offeredWeapon    = weapon;
        weaponController = controller;

        // Pause time slightly so player can read
        Time.timeScale = 0.1f;

        // Populate offered weapon info
        if (weaponIcon != null && weapon.weaponSprite != null)
            weaponIcon.sprite = weapon.weaponSprite;

        if (weaponNameText != null)
            weaponNameText.text = weapon.weaponName;

        if (weaponTypeText != null)
            weaponTypeText.text = weapon.weaponType.ToString();

        if (weaponStatsText != null)
        {
            if (weapon.weaponType == WeaponType.Melee)
            {
                weaponStatsText.text =
                    $"Damage:    {weapon.meleeDamageMultiplier}x\n" +
                    $"Range:     {weapon.meleeRange}\n" +
                    $"Cooldown:  {weapon.meleeCooldown}s";
            }
            else
            {
                weaponStatsText.text =
                    $"Damage:    {weapon.rangedDamageMultiplier}x\n" +
                    $"Fire Rate: {weapon.fireRate}s\n" +
                    $"Mag Size:  {weapon.magazineSize}";
            }
        }

        // Populate current weapon info
        WeaponDataSO currentWeapon = weapon.weaponType == WeaponType.Melee
            ? controller.GetMeleeWeapon()?.weaponData
            : controller.GetRangedWeapon()?.weaponData;

        if (currentWeapon != null)
        {
            if (currentWeaponNameText != null)
                currentWeaponNameText.text = currentWeapon.weaponName;

            if (currentWeaponIcon != null && currentWeapon.weaponSprite != null)
                currentWeaponIcon.sprite = currentWeapon.weaponSprite;
        }
        else
        {
            if (currentWeaponNameText != null)
                currentWeaponNameText.text = "None";
        }

        if (popupPanel != null)
            popupPanel.SetActive(true);
    }

    // -------------------------------------------------------
    // Button Callbacks
    // -------------------------------------------------------

    private void OnSwapPressed()
    {
        if (offeredWeapon == null || weaponController == null) return;

        // Swap the correct slot based on weapon type
        // Melee swaps with melee, ranged swaps with ranged
        if (offeredWeapon.weaponType == WeaponType.Melee)
        {
            MeleeWeapon currentMelee = weaponController.GetMeleeWeapon();
            if (currentMelee != null && offeredWeapon.pickupPrefab != null)
                weaponController.PickUpWeapon(offeredWeapon, offeredWeapon.pickupPrefab);
        }
        else
        {
            RangedWeapon currentRanged = weaponController.GetRangedWeapon();
            if (currentRanged != null && offeredWeapon.pickupPrefab != null)
                weaponController.PickUpWeapon(offeredWeapon, offeredWeapon.pickupPrefab);
        }

        Hide();
    }

    private void OnDeclinePressed()
    {
        Hide();
    }

    private void Hide()
    {
        Time.timeScale = 1f;

        if (popupPanel != null)
            popupPanel.SetActive(false);

        offeredWeapon    = null;
        weaponController = null;
    }
}