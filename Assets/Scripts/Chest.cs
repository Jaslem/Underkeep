// Chest.cs
using System.Collections;
using UnityEngine;

public enum ChestState { Closed, Opening, Open, Mimic }

public class Chest : MonoBehaviour
{
    [Header("Chest Settings")]
    [Tooltip("Chance this chest is a mimic (0 = never, 1 = always)")]
    [Range(0f, 1f)]
    public float mimicChance = 0.2f;

    [Tooltip("How close the player must be to open the chest")]
    public float interactRange = 2f;

    [Header("Loot Settings")]
    [Tooltip("Possible weapons this chest can drop")]
    public WeaponDataSO[] possibleWeapons;

    [Tooltip("Prefab to spawn for each weapon that drops on the ground")]
    public GameObject weaponPickupPrefab;

    [Tooltip("Minimum currency dropped")]
    public int minCurrency = 10;

    [Tooltip("Maximum currency dropped")]
    public int maxCurrency = 50;

    [Tooltip("Chance to drop a weapon vs currency (0.5 = 50/50)")]
    [Range(0f, 1f)]
    public float weaponDropChance = 0.5f;

    [Header("Animations")]
    public AnimationClip chestOpenClip;
    public AnimationClip chestOpenToMimicClip;

    [Header("Mimic Settings")]
    [Tooltip("The MimicEnemy script on this GameObject")]
    public MimicEnemy mimicEnemy;

    [Header("References")]
    public ChestWeaponPopupUI weaponPopupUI;

    // State
    public ChestState State { get; private set; } = ChestState.Closed;

    private bool          playerInRange  = false;
    private Transform     playerTransform;
    private SpriteRenderer spriteRenderer;
    private Coroutine     openCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Determine if this chest is a mimic at spawn
        if (Random.value <= mimicChance)
            State = ChestState.Mimic;
        else
            State = ChestState.Closed;

        // Hide mimic component until revealed
        if (mimicEnemy != null)
            mimicEnemy.enabled = false;
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        // Find popup UI if not assigned
        if (weaponPopupUI == null)
            weaponPopupUI = FindObjectOfType<ChestWeaponPopupUI>();
    }

    private void Update()
    {
        if (State == ChestState.Open) return;
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        playerInRange = dist <= interactRange;

        // Show interact prompt when in range
        if (playerInRange && State == ChestState.Closed)
        {
            // Press F to open
            if (Input.GetKeyDown(KeyCode.F))
                Open();
        }
    }

    // -------------------------------------------------------
    // Opening
    // -------------------------------------------------------

    private void Open()
    {
        if (State == ChestState.Open || openCoroutine != null) return;
        openCoroutine = StartCoroutine(OpenCoroutine());
    }

    private IEnumerator OpenCoroutine()
    {
        State = ChestState.Opening;

        if (State == ChestState.Mimic)
        {
            // Play open → mimic transition animation
            yield return StartCoroutine(PlayClip(chestOpenToMimicClip));

            // Reveal mimic
            RevealMimic();
        }
        else
        {
            // Play open animation
            yield return StartCoroutine(PlayClip(chestOpenClip));

            // Drop loot
            State = ChestState.Open;
            DropLoot();
        }
    }

    private IEnumerator PlayClip(AnimationClip clip)
    {
        if (clip == null) yield break;

        float timer = 0f;
        while (timer < clip.length)
        {
            clip.SampleAnimation(gameObject, timer);
            timer += Time.deltaTime;
            yield return null;
        }
        clip.SampleAnimation(gameObject, clip.length);
    }

    // -------------------------------------------------------
    // Loot
    // -------------------------------------------------------

    private void DropLoot()
    {
        // Randomly drop weapon or currency
        if (Random.value <= weaponDropChance && possibleWeapons != null && possibleWeapons.Length > 0)
            DropWeapon();
        else
            DropCurrency();
    }

    private void DropWeapon()
    {
        // Pick a random weapon from the list
        WeaponDataSO weapon = possibleWeapons[Random.Range(0, possibleWeapons.Length)];
        if (weapon == null) return;

        // Show the weapon swap popup
        if (weaponPopupUI != null)
            weaponPopupUI.Show(weapon, playerTransform.GetComponent<PlayerWeaponController>());
        else
        {
            // Fallback — spawn pickup on ground if no popup UI
            SpawnWeaponPickup(weapon);
        }
    }

    private void SpawnWeaponPickup(WeaponDataSO weapon)
    {
        if (weaponPickupPrefab == null) return;

        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
        GameObject pickup = Instantiate(weaponPickupPrefab, spawnPos, Quaternion.identity);

        WeaponPickup pickupComponent = pickup.GetComponent<WeaponPickup>();
        if (pickupComponent != null)
        {
            pickupComponent.weaponData   = weapon;
            pickupComponent.weaponPrefab = weapon.pickupPrefab;
        }
    }

    private void DropCurrency()
    {
        int amount = Random.Range(minCurrency, maxCurrency + 1);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerCurrency currency = player.GetComponent<PlayerCurrency>();
            if (currency != null)
            {
                currency.AddCurrency(amount);
                Debug.Log($"[Chest] Dropped {amount} currency");
            }
        }
    }

    // -------------------------------------------------------
    // Mimic
    // -------------------------------------------------------

    private void RevealMimic()
    {
        State = ChestState.Mimic;

        if (mimicEnemy != null)
            mimicEnemy.enabled = true;

        Debug.Log("[Chest] Mimic revealed!");
    }

    // -------------------------------------------------------
    // Gizmos
    // -------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}