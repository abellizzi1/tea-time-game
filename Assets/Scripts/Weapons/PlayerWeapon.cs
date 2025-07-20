using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Runtime controller for the player's currently equipped firearm.
/// <para>
/// • Receives a <see cref="WeaponData"/> struct from <c>PlayerController</c> when the
///   weapon is equipped (<see cref="SetWeaponData"/>).
/// </para>
/// <para>
/// • Handles: aiming, firing, reloading, ammo tracking, UI updates, sound, and tracer
///   spawn.  The script is intentionally self‑contained so any weapon prefab just needs
///   this component plus a muzzle <c>bulletSpawn</c> transform.
/// </para>
/// <para>
/// • Uses an object‑pool (<c>ObjectPool.SharedInstance</c>) instead of <c>Instantiate</c>
///   to avoid GC churn when firing rapidly.
/// </para>
/// </summary>
public class PlayerWeapon : MonoBehaviour
{
    #region Weapon data & state

    private WeaponData currentWeapon; // Stats injected at equip‑time

    // Ammo / cooldown
    private float nextAllowedFireTime = 0f; // Fire‑rate gate (seconds)
    public bool isReloading = false; // True while reload anim plays
    private int currentAmmoInMagazine; // Remaining rounds in clip
    private Coroutine reloadCoroutine; // Track the active reload coroutine 

    #endregion

    #region Inspector references

    // Bullet prefab & muzzle transform (assigned in prefab)
    [Header("Projectile")]
    public GameObject bulletPrefab;
    public Transform bulletSpawn;

    [Header("Player root (for rotation)")]
    [SerializeField] private Transform playerRoot; // Whole character that should rotate

    // Audio
    [Header("Audio")]
    public AudioClip fireSound;
    [SerializeField] private AudioClip reloadSound;
    private AudioSource audioSource;

    // UI
    [Header("UI")]
    [SerializeField] public GameObject WeaponUI;   // Canvas panel shared by all guns
    public Transform WeaponIcon;           // Not actually used but kept for legacy
    [SerializeField] public TextMeshProUGUI weaponNameText;
    private TextMeshProUGUI WeaponAmmoCount;

    // Managers
    [Header("Managers")]
    [SerializeField] private ItemManager itemManager;

    // Animation
    private Animator anim;

    // Cannot shoot when disabled (e.g. when the player dies)
    private bool isDisabled;

    #endregion

    #region Initialisation

    /// <summary>
    /// Initializes the weapon with new data and optionally sets the current ammo count.
    /// </summary>
    /// <param name="data">
    /// The <see cref="WeaponData"/> object containing all stats and settings for the weapon,
    /// such as magazine size, fire rate, damage, spread, etc.
    /// </param>
    /// <param name="currentAmmo">
    /// (Optional) The amount of ammo to initialize the weapon with. If this is set to a value
    /// less than 0 (default), the magazine will be filled to its maximum capacity.
    /// </param>
    public void SetWeaponData(WeaponData data, int currentAmmo = -1)
    {
        currentWeapon = data;
        currentAmmoInMagazine = (currentAmmo >= 0) ? currentAmmo : currentWeapon.magazineSize;

        LoadWeaponIcon();
        WeaponAmmoCount ??= WeaponUI.transform.Find("AmmoCounter").GetComponent<TextMeshProUGUI>();
        UpdateAmmoUI();

        Debug.Log($"Switched to weapon: {data.weaponName}");
        weaponNameText.text = data.weaponName;
        PrintWeaponStats();
    }

    /// <summary>
    /// Unity Start – cache audio & animator.
    /// </summary>
    private void Start()
    {
        Debug.Log($"{name} PlayerWeapon script is running.");
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.volume = .5f;
        }
        anim = GetComponentInParent<Animator>();

        isDisabled = false;
    }

    #endregion

    #region UI helpers

    /// <summary>
    /// Loads the correct weapon icon sprite into the shared HUD panel.
    /// Safe‑guards against missing <c>Image</c> component or bad path.
    /// </summary>
    private void LoadWeaponIcon()
    {
        Transform iconTf = WeaponUI.transform.Find("WeaponIcon");
        if (iconTf == null)
        {
            Debug.LogWarning("WeaponIcon child not found under WeaponUI.");
            return;
        }

        if (!iconTf.TryGetComponent(out Image iconImg))
        {
            Debug.LogWarning("WeaponIcon GameObject has no Image component.");
            return;
        }

        Sprite icon = Resources.Load<Sprite>(currentWeapon.iconPath);
        if (icon != null)
        {
            iconImg.sprite = icon;
            iconImg.enabled = true;
            Debug.Log($"Loaded Weapon Icon at Resources/{currentWeapon.iconPath}.png");
        }
        else
        {
            Debug.LogWarning($"Could not load icon at Resources/{currentWeapon.iconPath}.png");
            iconImg.enabled = false;
        }
    }

    /// <summary>
    /// Refreshes the "x / magSize" ammo counter on HUD.
    /// </summary>
    private void UpdateAmmoUI()
    {
        if (WeaponAmmoCount != null)
        {
            WeaponAmmoCount.SetText($"{currentAmmoInMagazine}/{currentWeapon.magazineSize}");
        }
    }

    #endregion

    #region Main loop

    /// <summary>
    /// Handles aiming, firing input, and reload input every frame.
    /// </summary>
    private void Update()
    {
        // Guard against being called before <see cref="SetWeaponData"/>
        if (currentWeapon == null || bulletPrefab == null || bulletSpawn == null || Time.timeScale == 0 || isDisabled)
            return;

        AimGunAtMouse();

        /* ------------------------
         * Reload key ("R")
         * ----------------------*/
        if (!isReloading && currentAmmoInMagazine < currentWeapon.magazineSize && Input.GetKeyDown(KeyCode.R))
        {
            isReloading = true;
            audioSource.PlayOneShot(reloadSound);
            anim.SetTrigger("reload");
            reloadCoroutine = StartCoroutine(WaitForAnimationAndReload());
        }

        /* ------------------------
         * Fire input (LMB)
         * ----------------------*/
        if (!isReloading && Input.GetMouseButton(0) && Time.time >= nextAllowedFireTime)
        {
            if (currentAmmoInMagazine > 0)
            {
                Fire();

                // Convert RPM to seconds between shots ⇒ 60 / rpm
                float cooldown = 60f / currentWeapon.fireRate;
                nextAllowedFireTime = Time.time + cooldown;

                currentAmmoInMagazine--;
                UpdateAmmoUI();
                Debug.Log($"Fired! Ammo left: {currentAmmoInMagazine}/{currentWeapon.magazineSize}. Next fire at: {nextAllowedFireTime:F3}");
            }
            else
            {
                Debug.Log("Out of ammo! Press R to reload.");
            }
        }

    }

    #endregion

    #region Core mechanics

    /// <summary>
    /// Spawns (or fetches from pool) one or more bullets, sets their trajectory & damage, and
    /// plays muzzle audio. Handles multi‑pellet weapons like shotguns.
    /// </summary>
    private void Fire()
    {
        for (int i = 0; i < Mathf.Max(1, currentWeapon.pelletsPerShot); i++)
        {
            GameObject bullet = ObjectPool.SharedInstance.GetPooledObject();
            if (bullet == null)
            {
                Debug.LogWarning("No available bullet in pool.");
                return;
            }

            if (PlayerController.Instance.TryGetComponent<CannonballSplash>(out var cannonballSplash) &&
                        cannonballSplash.getItemAcquired())
            {
                if (bullet.TryGetComponent(out LineRenderer lr))
                {
                    lr.startColor = Color.red;
                    lr.endColor = Color.red;
                }
                bullet.transform.localScale = cannonballSplash.newBulletSize;
            }


            // Position the bullet slightly in front of the muzzle to avoid clipping into gun.
            //Vector3 newScale = new Vector3(0.05f, 0.05f, 0.05f);
            //bullet.transform.localScale = newScale;
            bullet.transform.position = bulletSpawn.position + bulletSpawn.forward * 0.2f;
            bullet.transform.rotation = Quaternion.identity;
            bullet.SetActive(true);

            // Ignore self‑collision (player ↔ bullet)
            if (bullet.TryGetComponent(out Collider bulletCol) && bulletSpawn.root.TryGetComponent(out Collider playerCol))
            {
                Physics.IgnoreCollision(bulletCol, playerCol);
            }

            /* ---------------------------------------------
             * Calculate aim direction based on mouse world‑hit
             * -------------------------------------------*/
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, bulletSpawn.position);

            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 targetPoint = ray.GetPoint(enter);
                Vector3 flatDirection = targetPoint - bulletSpawn.position; flatDirection.y = 0f;

                // Add weapon spread & convert to velocity
                Vector3 direction = ApplySpread(flatDirection.normalized, currentWeapon.spread);
                Vector3 velocity = direction * currentWeapon.bulletSpeed;

                bullet.transform.rotation = Quaternion.LookRotation(direction);

                if (bullet.TryGetComponent(out Bullet bulletScript))
                {
                    bulletScript.SetItemManager(itemManager);
                    bulletScript.velocity = velocity;
                    bulletScript.SetWeaponData(currentWeapon);
                }
                else
                {
                    Debug.LogWarning("Bullet prefab is missing the Bullet script.");
                    bullet.SetActive(false);
                    continue;
                }

                Debug.DrawRay(bulletSpawn.position, direction * 10f, Color.red, 2f);
            }
        }

        // Play fire SFX once per shot (not per pellet)
        if (fireSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(fireSound);
        }
    }

    /// <summary>
    /// Rotates the entire player so that <paramref name="playerRoot"/> faces the mouse
    /// cursor projected on a ground plane.
    /// </summary>
    private void AimGunAtMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, transform.position); // y‑plane through player

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 targetPoint = ray.GetPoint(enter);
            Vector3 flatDirection = targetPoint - playerRoot.position; flatDirection.y = 0f;

            if (flatDirection != Vector3.zero)
            {
                playerRoot.rotation = Quaternion.LookRotation(flatDirection.normalized);
            }
        }
    }

    /// <summary>
    /// Adds random horizontal spread to the given direction vector.
    /// Spread is specified in degrees in <see cref="WeaponData.spread"/>.
    /// </summary>
    private Vector3 ApplySpread(Vector3 direction, float spreadAngle)
    {
        if (spreadAngle <= 0f) return direction;

        float spreadRad = spreadAngle * Mathf.Deg2Rad;
        direction.x += Random.Range(-spreadRad, spreadRad);
        direction.z += Random.Range(-spreadRad, spreadRad);
        return direction.normalized;
    }

    /// <summary>
    /// Waits for both the reload animation and the weapon's reload time to complete,
    /// then refills the magazine and ends the reload state.
    /// </summary>
    private IEnumerator WaitForAnimationAndReload()
    {
        int reloadLayerIndex = anim.GetLayerIndex("ReloadLayer");

        // Wait until the reload animation starts playing
        while (!anim.GetCurrentAnimatorStateInfo(reloadLayerIndex).IsName("Rifle Reload"))
            yield return null;

        float animLength = anim.GetCurrentAnimatorStateInfo(reloadLayerIndex).length;
        float weaponReloadTime = currentWeapon.reloadTime;

        // Wait for the longer of animation or reloadTime
        float waitTime = Mathf.Max(animLength, weaponReloadTime);
        yield return new WaitForSeconds(waitTime);

        Reload();
    }

    /// <summary>
    /// Refills magazine and updates HUD – called from <see cref="WaitForAnimationAndReload"/>.
    /// </summary>
    public void Reload()
    {
        currentAmmoInMagazine = currentWeapon.magazineSize;
        UpdateAmmoUI();
        isReloading = false;
        Debug.Log($"Reloaded! Ammo refilled: {currentAmmoInMagazine}/{currentWeapon.magazineSize}");
    }

    /// <summary>
    /// Returns the current ammo in the magazine
    /// </summary>
    /// <returns></returns>
    public int GetCurrentAmmo()
    {
        return currentAmmoInMagazine;
    }

    /// <summary>
    /// Sets the isDisabled boolean
    /// </summary>
    public void setIsDisabled(bool disable)
    {
        isDisabled = disable;
    }

    #endregion

    #region Debug helpers

    /// <summary>
    /// Logs the full stat block of the currently equipped weapon to the console.
    /// Handy while balancing.
    /// </summary>
    public void PrintWeaponStats()
    {
        if (currentWeapon == null)
        {
            Debug.LogWarning("No weapon data loaded.");
            return;
        }

        Debug.Log($"--- Weapon Stats for {currentWeapon.weaponName} ---\n" +
                  $"Damage: {currentWeapon.damage}\n" +
                  $"Fire Rate: {currentWeapon.fireRate} rounds/min\n" +
                  $"Reload Time: {currentWeapon.reloadTime} s\n" +
                  $"Magazine Size: {currentWeapon.magazineSize}\n" +
                  $"Bullet Speed: {currentWeapon.bulletSpeed}\n" +
                  $"Range: {currentWeapon.range}\n" +
                  $"Spread: {currentWeapon.spread}\n" +
                  $"Pellets Per Shot: {currentWeapon.pelletsPerShot}\n" +
                  $"Recoil: {currentWeapon.recoil}\n" +
                  $"Icon Path: {currentWeapon.iconPath}\n" +
                  $"-----------------------------------");
    }

    #endregion
}
