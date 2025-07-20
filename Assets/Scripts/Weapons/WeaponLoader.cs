using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime utility that converts every <c>TextAsset</c> JSON file found under
/// <c>Resources/Weapons/</c> into a <see cref="WeaponDatabase"/> ScriptableObject.
/// <para>
/// The loader is designed for two reasons:
/// <list type="bullet">
///   <item>Make it easy for game designers to tweak weapon stats with any text editor.</item>
///   <item>Keep build‑time asset creation minimal – no need to hand‑create a ScriptableObject asset for every change.</item>
/// </list>
/// </para>
/// </summary>
public class WeaponLoader : MonoBehaviour
{
    /// <summary>
    /// Scans the <c>Resources/Weapons</c> folder, parses every <c>*.json</c> file into
    /// a <see cref="WeaponData"/> instance, and returns an in‑memory <see cref="WeaponDatabase"/>
    /// containing them.
    /// <para>
    /// The database is **not** saved as an asset – it exists only for the current session.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>Expected JSON schema (see <c>WeaponData</c>):</para>
    /// <example>
    /// {
    ///   "weaponName"     : "AK-47",
    ///   "damage"         : 35,
    ///   "fireRate"       : 600,
    ///   ... etc ...
    /// }
    /// </example>
    /// <para>
    /// If a file fails to parse, it is silently skipped (TODO: add extra logging).
    /// </para>
    /// </remarks>
    /// <returns>
    /// A freshly created <see cref="WeaponDatabase"/> populated with all valid JSON files.
    /// </returns>
    public WeaponDatabase LoadWeaponDatabase()
    {
        // Create a transient ScriptableObject so we can reuse editor tooling if desired.
        WeaponDatabase db = ScriptableObject.CreateInstance<WeaponDatabase>();
        db.weapons = new List<WeaponData>();

        // Load all text assets in Resources/Weapons (file names don't matter).
        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>("Weapons");

        foreach (TextAsset jsonFile in jsonFiles)
        {
            // Convert JSON WeaponData (strict field‑name matching).
            WeaponData data = JsonUtility.FromJson<WeaponData>(jsonFile.text);
            if (data != null)
            {
                db.weapons.Add(data);
                Debug.Log($"Loaded weapon JSON: {data.weaponName}");
            }
            else
            {
                Debug.LogWarning($"Failed to parse weapon JSON: {jsonFile.name}");
            }
        }
        return db;
    }
}
