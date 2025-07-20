using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class ShopWeaponSpawner : MonoBehaviour
{
    [SerializeField] private GameObject shopWeaponPoolPrefab;
    [SerializeField] private GameObject saleSpot1;
    [SerializeField] private GameObject saleSpot2;
    [SerializeField] private GameObject saleSpot3;
    private PlayerController playerController;

    void Start()
    {
        if (saleSpot1 != null) Debug.Log("Found first sale location");
        if (saleSpot2 != null) Debug.Log("Found second sale location");
        if (saleSpot3 != null) Debug.Log("Found third sale location");

        playerController = GameObject.Find("Player").GetComponent<PlayerController>();

        // Choose 3 random weapons to add to shop
        GameObject weaponPool = Instantiate(shopWeaponPoolPrefab);
        List<GameObject> weaponPoolChildren = new List<GameObject>();
        for (int i = 0; i < weaponPool.transform.childCount; i++)
        {
            weaponPoolChildren.Add(weaponPool.transform.GetChild(i).gameObject);
        }

        // 1st weapon
        GameObject weaponPrefab1 = weaponPoolChildren[Random.Range(0, weaponPoolChildren.Count)];
        while (weaponPrefab1.name == playerController.getCurrentWeaponName())
        {
            weaponPrefab1 = weaponPoolChildren[Random.Range(0, weaponPoolChildren.Count)];
        }

        // 2nd weapon
        GameObject weaponPrefab2 = weaponPoolChildren[Random.Range(0, weaponPoolChildren.Count)];
        while (weaponPrefab2.name == playerController.getCurrentWeaponName() || weaponPrefab2.name == weaponPrefab1.name)
        {
            weaponPrefab2 = weaponPoolChildren[Random.Range(0, weaponPoolChildren.Count)];
        }

        // 3rd weapon
        GameObject weaponPrefab3 = weaponPoolChildren[Random.Range(0, weaponPoolChildren.Count)];
        while (weaponPrefab3.name == playerController.getCurrentWeaponName() || weaponPrefab3.name == weaponPrefab1.name || weaponPrefab3.name == weaponPrefab2.name)
        {
            weaponPrefab3 = weaponPoolChildren[Random.Range(0, weaponPoolChildren.Count)];
        }

        GameObject weaponGameObject1 = Instantiate(weaponPrefab1, saleSpot1.transform.position, weaponPrefab1.transform.rotation);
        weaponGameObject1.transform.SetParent(saleSpot1.transform);
        saleSpot1.tag = "Shop_" + weaponPrefab1.name;
        Debug.Log("Weapon1 tag: Shop_" + weaponPrefab1.name);

        GameObject weaponGameObject2 = Instantiate(weaponPrefab2, saleSpot2.transform.position, weaponPrefab2.transform.rotation);
        weaponGameObject2.transform.SetParent(saleSpot2.transform);
        saleSpot2.tag = "Shop_" + weaponPrefab2.name;
        Debug.Log("Weapon2 tag: Shop_" + weaponPrefab2.name);

        GameObject weaponGameObject3 = Instantiate(weaponPrefab3, saleSpot3.transform.position, weaponPrefab3.transform.rotation);
        weaponGameObject3.transform.SetParent(saleSpot3.transform);
        saleSpot3.tag = "Shop_" + weaponPrefab3.name;
        Debug.Log("Weapon3 tag: Shop_" + weaponPrefab3.name);

        Destroy(weaponPool);
    }
}
