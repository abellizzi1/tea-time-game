using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject container that holds a list of all weapon definitions used in the game.
/// </summary>
/// <remarks>
/// This database is populated at runtime by the <see cref="WeaponLoader"/> class,
/// which reads weapon JSON files from the Resources/Weapons folder.
///
/// The database allows centralized access to all available weapons.
/// </remarks>
[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Game/Weapon Database", order = 1)]
public class WeaponDatabase : ScriptableObject
{
    /// <summary>
    /// The list of all available weapon data loaded into the game.
    /// </summary>
    public List<WeaponData> weapons;
}
