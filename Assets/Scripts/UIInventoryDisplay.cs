using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class UIInventoryDisplay : MonoBehaviour
{
    public Transform statusBar;              // Reference to the StatusBar container
    public GameObject itemIconPrefab;        // Prefab for the item icon
    [SerializeField] public Sprite wingsOfVoiceNormal;
    [SerializeField] public Sprite coeurDeVieNormal;
    [SerializeField] public Sprite deepPocketsNormal;
    [SerializeField] public Sprite freezeNormal;
    [SerializeField] public Sprite adrenalineShotNormal;
    [SerializeField] public Sprite proteinPowderNormal;
    [SerializeField] public Sprite finalDestinationNormal;
    [SerializeField] public Sprite swiftExecutionNormal;
    [SerializeField] public Sprite apollyonsPitNormal;
    [SerializeField] public Sprite gungHoGlovesNormal;
    [SerializeField] public Sprite pumpedUpKicksNormal;
    [SerializeField] public Sprite doubleTapNormal;
    [SerializeField] public Sprite cannonballSplashNormal;
    
    public static UIInventoryDisplay Instance { get; private set; }

    private Dictionary<string, GameObject> itemIcons = new Dictionary<string, GameObject>();

    private Dictionary<string, int> itemCounts = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void OnPickupItem(string itemName)
    {
        if (itemCounts.ContainsKey(itemName))
        {
            Debug.Log(itemName + " picked up, incrementing in Count Dictionary.");
            itemCounts[itemName]++;
        }
        else
        {
            Debug.Log("Duplicate " + itemName + " picked up, initializing in Count Dictionary.");
            itemCounts[itemName] = 1;
        }

        if (itemCounts[itemName] > 1)
        {
            Debug.Log("Duplicate " + itemName + " picked up, incrementing.");
            incrementDupeCount(itemName);
            return;
        }

        GameObject icon = Instantiate(itemIconPrefab, statusBar);

        GameObject iconObj = icon.transform.Find("IconImage").gameObject;
        GameObject circObj = icon.transform.Find("CooldownCircle").gameObject;
        GameObject dupeObj = icon.transform.Find("DuplicateCount").gameObject;

        Image image = iconObj.GetComponent<Image>();
        Image circle = circObj.GetComponent<Image>();
        TMPro.TextMeshProUGUI quantityText = dupeObj.GetComponent<TMPro.TextMeshProUGUI>();

        image.enabled = true;
        quantityText.enabled = false;

        circle.fillAmount = 0f;
        circle.enabled = true;

        switch (itemName)
        {
            case "WingsOfVoice":
                image.sprite = wingsOfVoiceNormal;
                break;
            case "CoeurDeVie":
                image.sprite = coeurDeVieNormal;
                break;
            case "DeepPockets":
                image.sprite = deepPocketsNormal;
                break;
            case "Freeze":
                image.sprite = freezeNormal;
                break;
            case "AdrenalineShot":
                image.sprite = adrenalineShotNormal;
                break;
            case "ProteinPowder":
                image.sprite = proteinPowderNormal;
                break;
            case "FinalDestination":
                image.sprite = finalDestinationNormal;
                break;
            case "SwiftExecution":
                image.sprite = swiftExecutionNormal;
                break;
            case "ApollyonsPit":
                image.sprite = apollyonsPitNormal;
                break;
            case "GungHoGloves":
                image.sprite = gungHoGlovesNormal;
                break;
            case "PumpedUpKicks":
                image.sprite = pumpedUpKicksNormal;
                break;
            case "DoubleTap":
                image.sprite = doubleTapNormal;
                break;
            case "CannonballSplash":
                image.sprite = cannonballSplashNormal;
                break;
        }

        itemIcons[itemName] = icon;
    }

    public Sprite grabImageSprite(string itemName)
    {
        switch (itemName)
        {
            case "WingsOfVoice":
                return wingsOfVoiceNormal;
            case "CoeurDeVie":
                return coeurDeVieNormal;
            case "DeepPockets":
                return deepPocketsNormal;
            case "Freeze":
                return freezeNormal;
            case "AdrenalineShot": 
                return adrenalineShotNormal;
            case "ProteinPowder":
                return proteinPowderNormal;
            case "FinalDestination":
                return finalDestinationNormal;
            case "SwiftExecution":
                return swiftExecutionNormal;
            case "ApollyonsPit":
                return apollyonsPitNormal;
            case "GungHoGloves":
                return gungHoGlovesNormal;
            case "PumpedUpKicks":
                return pumpedUpKicksNormal;
            case "DoubleTap":
                return doubleTapNormal;
            case "CannonballSplash":
                return cannonballSplashNormal;
            default:
                //TODO default image
                return wingsOfVoiceNormal;
        }

    }

    public void incrementDupeCount(string itemName)
    {
        if (!itemIcons[itemName].transform.Find("DuplicateCount").gameObject.GetComponent<TMPro.TextMeshProUGUI>().enabled)
        {
            itemIcons[itemName].transform.Find("DuplicateCount").gameObject.GetComponent<TMPro.TextMeshProUGUI>().enabled = true;
        }

        TMPro.TextMeshProUGUI dupeText = itemIcons[itemName].transform.Find("DuplicateCount").GetComponent<TMPro.TextMeshProUGUI>();
        Debug.Log("Enabled? " + dupeText.enabled);
        dupeText.text = "x" + itemCounts[itemName];
    }

    public void decrementItemCount(string itemName)
    {
        itemCounts[itemName]--;
        TMPro.TextMeshProUGUI dupeText = itemIcons[itemName].transform.Find("DuplicateCount").GetComponent<TMPro.TextMeshProUGUI>();
        dupeText.text = "x" + itemCounts[itemName];
        if (itemCounts[itemName] == 1)
        {
            itemIcons[itemName].transform.Find("DuplicateCount").gameObject.GetComponent<TMPro.TextMeshProUGUI>().enabled = false;
        }
        else if (itemCounts[itemName] == 0)
        {
            Destroy(itemIcons[itemName]);
        }
    }

    public void StartCooldown(float cooldownTime, string itemName)
    {
        StartCoroutine(RunCooldown(cooldownTime, itemName));
    }

    public void StartIndicator(float cooldownTime, string itemName)
    {
        StartCoroutine(RunCooldown(cooldownTime, itemName));
    }

    private IEnumerator RunCooldown(float cooldownTime, string itemName)
    {
        // No need to run the cooldown if there's none left
        if (itemCounts[itemName] > 0)
        {
            // Grab the correct cooldown circle
            Image cooldownOverlay = itemIcons[itemName].transform.Find("CooldownCircle").GetComponent<Image>();

            float timer = cooldownTime;
            cooldownOverlay.fillAmount = 1f;

            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                cooldownOverlay.fillAmount = timer / cooldownTime;
                yield return null;
            }

            cooldownOverlay.fillAmount = 0f;
        }
    }

    public Dictionary<string, int> getItemCounts()
    {
        return itemCounts;
    }

    private IEnumerator RunIndicator(float cooldownTime, string itemName)
    {
        // Grab the correct cooldown circle
        Image cooldownOverlay = itemIcons[itemName].transform.Find("CooldownCircle").GetComponent<Image>();

        float timer = cooldownTime;
        cooldownOverlay.fillAmount = 1f;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        cooldownOverlay.fillAmount = 0f;
    }
}
