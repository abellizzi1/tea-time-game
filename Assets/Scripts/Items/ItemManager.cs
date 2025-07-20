using UnityEngine;
using System.Collections.Generic;
using System;

public class ItemManager : MonoBehaviour
{
    public List<Item> items = new();
    readonly Dictionary<Type, List<Item>> byType = new();

    [SerializeField] GameObject itemStartPrefab;
    [SerializeField] GameObject itemRefillPrefab;
    [SerializeField] GameObject shopItemPoolPrefab;

    [SerializeField] GameObject shopItemPosition1Prefab;
    [SerializeField] GameObject shopItemPosition2Prefab;

    readonly List<Item> startBag = new();   // prefab assets only
    readonly List<Item> refillBag = new();
    readonly List<Item> shopBag = new();
    readonly List<Item> shopBag2 = new();

    public static ItemManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        AddChildrenToPool(itemStartPrefab.transform);
        FillBag(itemStartPrefab.transform, startBag);
        FillBag(itemRefillPrefab.transform, refillBag);
        FillBag(shopItemPosition1Prefab.transform, shopBag);
        FillBag(shopItemPosition2Prefab.transform, shopBag2);
    }

    void FillBag(Transform parent, List<Item> bag)
    {
        foreach (Transform child in parent)
        {
            Item prefab = child.GetComponent<Item>();
            if ((prefab is FinalDestination && PlayerController.Instance.GetComponent<FinalDestination>() != null)
                || (prefab is WingsOfVoice && PlayerController.Instance.GetComponent<WingsOfVoice>() != null)
                || (prefab is DoubleTap && PlayerController.Instance.GetComponent<DoubleTap>() != null)
                || (prefab is CannonballSplash && PlayerController.Instance.GetComponent<CannonballSplash>() != null))
            {
                continue;
            }
            if (prefab is PumpedUpKicks && PlayerController.Instance.speedStat >= 1.25f)
            {
                Debug.Log("Skipping PumpedUpKicks (speedStat already maxed)");
                continue;
            }
            if ((prefab is ApollyonsPit || prefab is SwiftExecution) && PlayerController.Instance.fireRateStat >= 100.00f)
            {
                Debug.Log("Skipping ApollyonsPit and  SwiftExecution(fireRateStat already maxed)");
                continue;
            }
            if (prefab) bag.Add(prefab);    // store the ASSET once
        }
    }

    void AddChildrenToPool(Transform parent)
    {
        if (parent == null) return;

        foreach (Transform child in parent)
        {
            Item prefab = child.GetComponent<Item>();
            if (prefab == null) continue;

            Item clone = Instantiate(prefab);
            clone.gameObject.SetActive(false);

            // master list
            items.Add(clone);
            Debug.Log("Adding " + clone.toString() + " to the ItemPool.");

            // type list
            Type t = clone.GetType();
            if (!byType.TryGetValue(t, out var list))
                byType[t] = list = new List<Item>();
            list.Add(clone);
        }
    }

    void Refill() => AddChildrenToPool(itemRefillPrefab.transform);

    public T Grab<T>() where T : Item
    {
        if (byType.TryGetValue(typeof(T), out var list) && list.Count > 0)
        {
            T pick = (T)list[0];
            return pick;
        }
        return null;
    }

    public Item GrabItemToSpawn()
    {
        // 1) take from the start bag until empty
        if (startBag.Count > 0)
            return PullRandom(startBag);

        // 2) then use refill prefabs forever
        if (refillBag.Count > 0)
        {
            return PullRandom(refillBag);
        }
        else
        {
            FillBag(itemRefillPrefab.transform, refillBag);
            return PullRandom(refillBag);
        }
      
        return null;                        // nothing left!
    }

    public Item GrabShopPosition1()
    {
        if (shopBag.Count > 0)
        {
            return PullRandom(shopBag);
        }
        else
        {
            FillBag(shopItemPosition1Prefab.transform, shopBag);
            return PullRandom(shopBag);
        }
    }

    public Item GrabShopPosition2()
    {
        if (shopBag2.Count > 0)
        {
            return PullRandom(shopBag2);
        }
        else
        {
            FillBag(shopItemPosition2Prefab.transform, shopBag2);
            return PullRandom(shopBag2);
        }
    }

    Item PullRandom(List<Item> bag)
    {
        while (true)
        {
            int idx = UnityEngine.Random.Range(0, bag.Count);
            Item prefab = bag[idx];
            if ((prefab is FinalDestination && PlayerController.Instance.GetComponent<FinalDestination>() != null)
                || (prefab is WingsOfVoice && PlayerController.Instance.GetComponent<WingsOfVoice>() != null)
                || (prefab is DoubleTap && PlayerController.Instance.GetComponent<DoubleTap>() != null)
                || (prefab is CannonballSplash && PlayerController.Instance.GetComponent<CannonballSplash>() != null)
                || (prefab is PumpedUpKicks && PlayerController.Instance.speedStat >= 1.25f))
            {
                bag.RemoveAt(idx);
                continue;
            }

            bag.RemoveAt(idx);

            Item instance = Instantiate(prefab);           // fresh scene clone
            instance.gameObject.SetActive(false);          // caller decides when to show
            return instance;
        }
    }
}
