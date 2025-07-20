/// <summary>
/// Container that defines every tunable stat for a single weapon.
/// </summary>
/// <remarks>
/// <para>
/// Instances of <c>WeaponData</c> are created at runtime by <see cref="WeaponLoader"/>, which
/// deserialises JSON files from <c>Resources/Weapons</c>.  **Every public field name here must
/// match its JSON key exactly** or <see cref="JsonUtility"/> will ignore the value.
/// </para>
/// <para>
/// All values are expressed in "game units":
/// <list type="bullet">
///   <item><b>damage</b> ‑ raw HP removed per bullet.</item>
///   <item><b>fireRate</b> ‑ rounds per minute (RPM).</item>
///   <item><b>reloadTime</b> ‑ seconds to reload a full magazine.</item>
///   <item><b>bulletSpeed</b> ‑ units/second; used only for visual tracer velocity.</item>
///   <item><b>range</b> ‑ max effective distance (for AI or drop‑off).</item>
///   <item><b>spread</b> ‑ cone half‑angle in degrees applied per shot.</item>
///   <item><b>pelletsPerShot</b> ‑ >1 for shotguns; 0 or 1 for rifles/pistols.</item>
///   <item><b>recoil</b> ‑ arbitrary multiplier used by camera‑kick system.</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Example JSON file consumed by <see cref="WeaponLoader"/>:
/// <code>
/// {
///   "weaponName"     : "AK-47",
///   "damage"         : 35,
///   "fireRate"       : 600,
///   "reloadTime"     : 3.3,
///   "magazineSize"   : 30,
///   "bulletSpeed"    : 9999,
///   "range"          : 350,
///   "spread"         : 2.5,
///   "pelletsPerShot" : 0,
///   "recoil"         : 1.5,
///   "iconPath"       : "WeaponIcons/AK-47"
/// }
/// </code>
/// </example>
[System.Serializable]
public class WeaponData
{
    /// <summary>Human‑readable name; must be unique.</summary>
    public string weaponName;

    /// <summary>Base damage per projectile.</summary>
    public float damage;

    /// <summary>Fire‑rate in rounds per minute.</summary>
    public float fireRate;

    /// <summary>Seconds required to fully reload the magazine.</summary>
    public float reloadTime;

    /// <summary>Total rounds per magazine/clip.</summary>
    public int magazineSize;

    /// <summary>Projectile speed (visual only if hitscan).</summary>
    public float bulletSpeed;

    /// <summary>Effective range for AI / damage fall‑off.</summary>
    public float range;

    /// <summary>Horizontal spread (degrees) applied each shot.</summary>
    public float spread;

    /// <summary>Number of pellets per shot (shotguns > 1).</summary>
    public int pelletsPerShot;

    /// <summary>Recoil multiplier fed into camera‑kick algorithm.</summary>
    public float recoil;

    /// <summary>Relative Resources path to the weapon icon sprite.</summary>
    public string iconPath;

    // This class can be extended with more fields, e.g. headshotMultiplier, attachment slots, etc.
}
