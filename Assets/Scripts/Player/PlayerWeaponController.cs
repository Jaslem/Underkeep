// PlayerWeaponController.cs
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Weapon Slots")]
    public MeleeWeapon meleeWeapon;
    public RangedWeapon rangedWeapon;

    [Header("References")]
    public PlayerStats playerStats;

    private WeaponBase activeWeapon;
    private bool isMeleeActive = false;
    private PlayerMovement playerMovement;

    // Track whether weapon was hidden due to dodge
    private bool weaponHiddenForDodge = false;

    private void Start()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        playerMovement = GetComponent<PlayerMovement>();

        if (meleeWeapon != null)
        {
            meleeWeapon.Initialize(playerStats);
            meleeWeapon.OnUnequip();
        }

        if (rangedWeapon != null)
        {
            rangedWeapon.Initialize(playerStats);
            rangedWeapon.OnUnequip();
        }

        if (rangedWeapon != null)
            SwitchToRanged();
        else if (meleeWeapon != null)
            SwitchToMelee();
    }

    private void Update()
    {
        HandleSwitchInput();

        if (activeWeapon != null)
        {
            // Hide weapon during dodge, show when done
            HandleWeaponVisibilityDuringDodge();

            // Only handle attack input when weapon is visible
            if (!weaponHiddenForDodge)
                activeWeapon.HandleAttackInput();
        }
    }

    // -------------------------------------------------------
    // Dodge Visibility
    // -------------------------------------------------------

    private void HandleWeaponVisibilityDuringDodge()
{
    if (playerMovement == null) return;

    // Use IsDodgeAnimating instead of IsRolling so the weapon
    // stays hidden until the full dodge animation finishes
    bool isDodging = playerMovement.IsDodgeAnimating;

    if (isDodging && !weaponHiddenForDodge)
    {
        weaponHiddenForDodge = true;
        SetActiveWeaponVisible(false);
    }
    else if (!isDodging && weaponHiddenForDodge)
    {
        weaponHiddenForDodge = false;
        SetActiveWeaponVisible(true);
    }
}

    private void SetActiveWeaponVisible(bool visible)
    {
        if (activeWeapon == null) return;

        SpriteRenderer sr = activeWeapon.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = visible;
    }

    // -------------------------------------------------------
    // Input Handling
    // -------------------------------------------------------

    private void HandleSwitchInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            ToggleWeapon();

        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll != 0f)
            ToggleWeapon();
    }

    private void ToggleWeapon()
    {
        if (isMeleeActive)
            SwitchToRanged();
        else
            SwitchToMelee();
    }

    // -------------------------------------------------------
    // Switching
    // -------------------------------------------------------

    public void SwitchToMelee()
    {
        if (meleeWeapon == null)
        {
            Debug.LogWarning("PlayerWeaponController: No melee weapon in slot.");
            return;
        }

        if (activeWeapon != null)
            activeWeapon.OnUnequip();

        activeWeapon = meleeWeapon;
        meleeWeapon.OnEquip();
        isMeleeActive = true;

        Debug.Log($"Switched to melee: {meleeWeapon.weaponData.weaponName}");
    }

    public void SwitchToRanged()
    {
        if (rangedWeapon == null)
        {
            Debug.LogWarning("PlayerWeaponController: No ranged weapon in slot.");
            return;
        }

        if (activeWeapon != null)
            activeWeapon.OnUnequip();

        activeWeapon = rangedWeapon;
        rangedWeapon.OnEquip();
        isMeleeActive = false;

        Debug.Log($"Switched to ranged: {rangedWeapon.weaponData.weaponName}");
    }

    // -------------------------------------------------------
    // Pickup System
    // -------------------------------------------------------

    public void PickUpWeapon(WeaponDataSO newWeaponData, GameObject weaponPrefab)
    {
        if (newWeaponData.weaponType == WeaponType.Melee)
            PickUpMeleeWeapon(newWeaponData, weaponPrefab);
        else
            PickUpRangedWeapon(newWeaponData, weaponPrefab);
    }

    private void PickUpMeleeWeapon(WeaponDataSO newWeaponData, GameObject weaponPrefab)
    {
        if (meleeWeapon != null)
        {
            if (activeWeapon == meleeWeapon)
                activeWeapon = null;

            Destroy(meleeWeapon.gameObject);
        }

        GameObject newGO = Instantiate(weaponPrefab, transform);
        MeleeWeapon newMelee = newGO.GetComponent<MeleeWeapon>();

        if (newMelee == null)
        {
            Debug.LogError("PickUpMeleeWeapon: Prefab does not have a MeleeWeapon component!");
            Destroy(newGO);
            return;
        }

        newMelee.weaponData = newWeaponData;
        newMelee.Initialize(playerStats);
        meleeWeapon = newMelee;

        if (isMeleeActive || activeWeapon == null)
            SwitchToMelee();
        else
            meleeWeapon.OnUnequip();

        Debug.Log($"Picked up melee weapon: {newWeaponData.weaponName}");
    }

    private void PickUpRangedWeapon(WeaponDataSO newWeaponData, GameObject weaponPrefab)
    {
        if (rangedWeapon != null)
        {
            if (activeWeapon == rangedWeapon)
                activeWeapon = null;

            Destroy(rangedWeapon.gameObject);
        }

        GameObject newGO = Instantiate(weaponPrefab, transform);
        RangedWeapon newRanged = newGO.GetComponent<RangedWeapon>();

        if (newRanged == null)
        {
            Debug.LogError("PickUpRangedWeapon: Prefab does not have a RangedWeapon component!");
            Destroy(newGO);
            return;
        }

        newRanged.weaponData = newWeaponData;
        newRanged.Initialize(playerStats);
        rangedWeapon = newRanged;

        if (!isMeleeActive || activeWeapon == null)
            SwitchToRanged();
        else
            rangedWeapon.OnUnequip();

        Debug.Log($"Picked up ranged weapon: {newWeaponData.weaponName}");
    }

    // -------------------------------------------------------
    // Public Getters
    // -------------------------------------------------------

    public WeaponBase GetActiveWeapon()  => activeWeapon;
    public MeleeWeapon GetMeleeWeapon()  => meleeWeapon;
    public RangedWeapon GetRangedWeapon() => rangedWeapon;
    public bool IsMeleeActive()          => isMeleeActive;
}