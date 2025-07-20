using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class ShopItemSpawner : MonoBehaviour
{
    ItemManager itemManager;

    [SerializeField] private GameObject saleSpot1;
    [SerializeField] private GameObject saleSpot2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (saleSpot1 != null) Debug.Log("Found first sale location");
        if (saleSpot2 != null) Debug.Log("Found second sale location");
    }

    void Awake()
    {
        itemManager = ItemManager.Instance;
        if (itemManager == null) Debug.LogWarning("ItemManager not found in scene!");

        getItemsToSpawn();
    }

    public void getItemsToSpawn()
    {
        Item ItemToSpawn = ItemManager.Instance.GrabShopPosition1();
        if (ItemToSpawn)
        {
            Debug.Log("Shop1 getItemToSpawn(): " + ItemToSpawn.toString());

            ItemToSpawn.gameObject.transform.position = saleSpot1.transform.position;
            ItemToSpawn.transform.SetParent(saleSpot1.transform);
            saleSpot1.tag = "Shop_" + ItemToSpawn.tagString();
            ItemToSpawn.gameObject.SetActive(true);
        }

        Item ItemToSpawn2 = ItemToSpawn;

        while (ItemToSpawn2.toString() == ItemToSpawn.toString())
        {
            ItemToSpawn2 = ItemManager.Instance.GrabShopPosition2();
        }
        if (ItemToSpawn2)
        {
            Debug.Log("Shop2 getItemToSpawn(): " + ItemToSpawn2.toString());

            ItemToSpawn2.gameObject.transform.position = saleSpot2.transform.position;
            ItemToSpawn2.transform.SetParent(saleSpot2.transform);
            saleSpot2.tag = "Shop_" + ItemToSpawn2.tagString();
            ItemToSpawn2.gameObject.SetActive(true);
        }
    }
}
