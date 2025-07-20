using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] public KeyCode DebugKeySpawn;

    ItemManager itemManager;

    private GameObject spawnCube;
    public  GameObject spotLight;
    private GameObject endItemPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnCube = transform.Find("Start").gameObject;
        if (spawnCube != null)
        {
            Debug.Log("Found Start");

            spotLight = spawnCube.transform.Find("Spot Light").gameObject;

            if (spotLight != null)
            {
                Debug.Log("Found Spot Light");
                spotLight.SetActive(false);
            }

        }
        endItemPos = transform.Find("End").gameObject;
        if (endItemPos != null)
        {
            Debug.Log("Found End");
        }
    }

    void Awake()
    {
        itemManager = FindFirstObjectByType<ItemManager>();
        if (itemManager == null)
        {
            Debug.LogWarning("ItemManager not found in scene!");
        }

        spotLight = GameObject.Find("Item_Spotlight");
        if (spotLight == null)
        {
            Debug.LogWarning("Item Spotlight not found in scene!");
        }

    }

    void Update()
    {
        // DISABLING FOR ALPHA
        // Debug Mode, need a room clear condition defined        
        //if (Input.GetKeyDown(DebugKeySpawn))
        //{
        //    if (itemManager.items.Count > 0)
        //    {
        //        getItemToSpawn();
        //    }
        //}
    }


    public void getItemToSpawn()
    {
        Item ItemToSpawn = ItemManager.Instance.GrabItemToSpawn();
        if (ItemToSpawn)
        {
            Debug.Log("getItemToSpawn(): " + ItemToSpawn.toString());
            Debug.Log("ItemToSpawn GameObject name: " + ItemToSpawn.gameObject.name);
            Debug.Log("ItemToSpawn type: " + ItemToSpawn.GetType());

            ItemToSpawn.gameObject.transform.position = spawnCube.transform.position;
            ItemToSpawn.gameObject.SetActive(true);

            StartCoroutine(ItemSpawnCoroutine(ItemToSpawn));
        }
    }

    public void deactivateSpotlight()
    {
        Debug.Log("Deactivating Spotlight");
        if (spotLight)
        {
            spotLight.SetActive(false);
        }
    }


    private IEnumerator ItemSpawnCoroutine(Item SpawningItem)
    {
        spotLight.SetActive(true);

        Vector3 startPos = spawnCube.transform.position;
        Vector3 endPos   = endItemPos.transform.position;

        float duration = 6.0f; // Duration of drop
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Smooth movement (ease out)
            SpawningItem.transform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, t));

            yield return null;
        }

        SpawningItem.transform.position = endPos;
    }

}
