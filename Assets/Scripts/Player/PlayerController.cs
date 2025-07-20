using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
   // Rigidbody of the player.
   private Rigidbody rb;
   private Collider[] ragdollColliders;
   private Rigidbody[] ragdollRigidbodies;

   [SerializeField] public RootMotionControlScript rootMotionControl;

   // Variable to keep track of current money balance.
   private int balance;
   // UI text component to display current balance
   public TextMeshProUGUI moneyText;

   // UI text component to display enemies killed
   [SerializeField] private TextMeshProUGUI enemiesKilledText;
   [SerializeField] public bool DebugMode;

   // Item-related
   public List<Item> items = new();

   // UI text component to display item pickup information to the player.
   public FadeAwayText itemText;
   public FadeAwayText itemDescText;
   [SerializeField] public ItemManager itemManager;
   private ItemSpawner itemSpawner;
   [SerializeField] private UIInventoryDisplay UIInventory;

   [SerializeField] private GameObject PauseMenu;
   [SerializeField] private GameObject GameOverMenu;
   [SerializeField] private TextMeshProUGUI GameOverText;

   [SerializeField] private Camera MainCamera;
   private Animator anim;
   private CharacterInputController characterInputController;
   public PlayerAudio playerAudio;
   // UI object to display winning text.
   public GameObject winTextObject;
   public GameObject winTextShopObject;

   [SerializeField] public GameObject killCountSkull;

   // UI Stat column
   public float speedStat = 1.0f;
   public float reloadSpeedStat = 1.0f;
   public float fireRateStat = 1.0f;
   public float damageStat = 1.0f;
   public float clipSizeStat = 1.0f;

   [SerializeField] public TextMeshProUGUI speedStatText;
   [SerializeField] public TextMeshProUGUI reloadSpeedStatText;
   [SerializeField] public TextMeshProUGUI fireRateStatText;
   [SerializeField] public TextMeshProUGUI damageStatText;
   [SerializeField] public TextMeshProUGUI clipSizeStatText;

   [SerializeField] public TextMeshProUGUI cyclesCompletedText;
   [SerializeField] public TextMeshProUGUI roomsProgressedText;

   private Collider shopItemLookAt;

   public List<GameObject> weaponObjects;
   private PlayerWeapon playerWeapon;
   private WeaponDatabase weaponDatabase;
   private GameObject currentWeaponModel;
   private int currentWeaponIndex = 0;
   private WeaponLoader weaponLoader;
   private Dictionary<string, int> weaponAmmoCounts = new();

   private bool isGrounded = true;

   // HealthBar
   public float maxHealth = 100f;
   public float currentHealth;
   public HealthBar healthBar;
   private bool isDead = false;

   static public int currentSceneNumber;

   static private int numScenes = 5;
   public int numScenesProgressed = 0;
   public int numCyclesCompleted = 0;
   // Prevent duplicate teleports (e.g. someone spams exit on the exit door in shop)
   private bool isTeleporting = false;

   // Reference to enemy spawns
   public EnemySpawnController spawner;

   public Vector3 maxBounds;
   public Vector3 minBounds;
   // Keep track of how many enemies to kill in each scene
   private int enemiesKilled = 0;
   private int[] enemiesToKill = { 5, 10, 15, 20, 25 };

   // Shop-related
   static private bool isInShop = false;
   // UI text component for the shop to purchase items
   [SerializeField] private TextMeshProUGUI purchaseItemText;
   private string purchasableShopItem = "";
   private Dictionary<string, int> shopPrices = new Dictionary<string, int>
   {
      { "DeepPockets", 300 },
      { "CoeurDeVie", 400 },
      { "Freeze", 200 },
      { "HealthRefill", 100 },
      { "WingsOfVoice", 1500 },
      { "AdrenalineShot", 700 },
      { "ProteinPowder", 1600 },
      { "FinalDestination", 1200 },
      { "SwiftExecution", 800 },
      { "ApollyonsPit", 1100 },
      { "PumpedUpKicks", 700 },
      { "DoubleTap", 2000 },
      { "CannonballSplash", 600 },
      { "GungHoGloves", 500 },
      { "AK-47", 0 },
      { "Bennelli M4", 0 },
      { "M249", 0 },
      { "Uzi", 0 },
      { "M48", 0 },
      { "M107", 0 }
   };


   public static PlayerController Instance { get; private set; }

   // Boss room variables
   public bool isInBossScene = false;
   private int currentBossPhase = 0;
   private Coroutine catwalkCoroutine;
   private bool isTouchingLava = false;
   private Coroutine lavaCoroutine;

   public T GetOrAddItemComponent<T>() where T : Item
   {
      T item = GetComponent<T>();
      if (item == null)
      {
         item = gameObject.AddComponent<T>();
         item.onPickup(); // Initialize if newly added
      }
      else
      {
         item.onPickup(); // Re-initialize if already exists
      }
      return item;
   }

   void Awake()
   {
      if (Instance != null && Instance != this) { Destroy(gameObject); return; }
      Instance = this;
      DontDestroyOnLoad(gameObject);

      Debug.LogWarning("Awake called!");

      itemSpawner = FindFirstObjectByType<ItemSpawner>();
      if (itemSpawner == null)
      {
         Debug.LogWarning("ItemSpawner not found in scene!");
      }

      spawner = FindFirstObjectByType<EnemySpawnController>();
      if (spawner == null)
      {
         Debug.LogWarning("EnemySpawnController not found in scene!");
      }
      SceneManager.sceneLoaded += OnSceneLoaded;

      weaponLoader = gameObject.AddComponent<WeaponLoader>();
      weaponDatabase = weaponLoader.LoadWeaponDatabase();
      if (weaponDatabase == null)
      {
         Debug.LogError("Failed to load weapon database.");
      }

   }

   void OnSceneLoaded(Scene scene, LoadSceneMode mode)
   {
      Debug.LogWarning("OnSceneLoaded called!");
      itemSpawner = FindFirstObjectByType<ItemSpawner>();
      if (itemSpawner == null)
      {
         Debug.LogWarning("ItemSpawner not found in scene!");
      }

      spawner = FindFirstObjectByType<EnemySpawnController>();
      if (spawner == null)
      {
         Debug.LogWarning("EnemySpawnController not found in scene!");
      }
      if (SceneManager.GetActiveScene().name == "BossScene")
      {
         isInBossScene = true;
         maxBounds = new Vector3(25, 25, 25);
         minBounds = new Vector3(-25, -40, -25);
      }

      if (enemiesKilledText != null && killCountSkull != null)
      {
         if (SceneManager.GetActiveScene().name == "ShopScene" || SceneManager.GetActiveScene().name == "BossScene")
         {
            enemiesKilledText.enabled = false;
            killCountSkull.SetActive(false);
         }
         else
         {
            enemiesKilledText.enabled = true;
            killCountSkull.SetActive(true);
         }
      }
   }

   // Start is called before the first frame update.
   void Start()
   {
      // Global Menu State - don't replay intro cinematic
      GlobalMenuState.introCinematicPlayed = true;
      PauseMenu.SetActive(false);
      GameOverMenu.SetActive(false);
      currentSceneNumber = 1;

      // Set components
      rb = GetComponent<Rigidbody>();
      ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
      ragdollRigidbodies = System.Array.FindAll(ragdollRigidbodies, r => r != rb);
      Collider[] mainColliders = GetComponents<Collider>();
      ragdollColliders = GetComponentsInChildren<Collider>();
      ragdollColliders = System.Array.FindAll(ragdollColliders, col => System.Array.IndexOf(mainColliders, col) < 0);
      anim = GetComponent<Animator>();

      // Start with pistol
      EquipWeapon("M1911");
      playerAudio = GetComponent<PlayerAudio>();
      characterInputController = GetComponent<CharacterInputController>();
      SetRagdollComponents(false);
      anim.SetBool("playingGame", true);

      // Initialize balance to zero if not in DebugMode
      balance = DebugMode ? 100000 : 0;
      // Update the balance display.
      SetCurrentBalance();

      // Update the enemies killed display.
      updateEnemiesKilledText();

      // Initially set the win text to be inactive.
      winTextObject.SetActive(false);
      winTextShopObject.SetActive(false);

      // Initially set purchase text to be not visible.
      purchaseItemText.enabled = false;

      // HealthBar
      currentHealth = maxHealth;
      healthBar.SetMaxHealth(maxHealth);
   }

   bool isOutsideBounds(Vector3 currentPlayerPosition)
   {
      if (currentPlayerPosition.x < minBounds.x || currentPlayerPosition.x > maxBounds.x ||
          currentPlayerPosition.y < minBounds.y || currentPlayerPosition.y > maxBounds.y ||
          currentPlayerPosition.z < minBounds.z || currentPlayerPosition.z > maxBounds.z)
      {
         return true;
      }
      else
      {
         return false;
      }
   }

   void Update()
   {
      // Check stat limits
      if (speedStat > 1.25f)
      {
         speedStat = 1.25f;
         rootMotionControl.animationSpeed = 1.5f * 1.25f;
         rootMotionControl.rootMovementSpeed = 1.5f * 1.25f;
         speedStatText.text = "<color=red>1.25x</color>";
      }
      if (fireRateStat > 100.00f)
      {
         fireRateStat = 100.0f;
         fireRateStatText.text = "<color=red>100.0x</color>";
      }


      if (isInBossScene == true)
      {
         if (enemiesKilled >= 10)
         {
            if (enemiesKilled >= 20)
            {
               currentBossPhase = 2;
            }
            else
            {
               currentBossPhase = 1;
            }
         }
         if (Boss.bossDead)
         {
            Boss.bossDead = false;
            if (!isTeleporting) StartCoroutine(teleportToNextRoom(15));
         }
      }
      if (DebugMode)
      {
         if (Input.GetKeyDown(KeyCode.T))
         {
            if (!isTeleporting) StartCoroutine(teleportToNextRoom(1));
         }
         if (Input.GetKeyDown(KeyCode.F7))
         {
            isInShop = true;

            SceneManager.LoadSceneAsync("ShopScene");
         }
         // Teleport to boss room
         if (Input.GetKeyDown(KeyCode.F5))
         {
            currentSceneNumber = numScenes;
            if (!isTeleporting) StartCoroutine(teleportToNextRoom(1));
         }
         if (!isInShop && itemSpawner)
         {
            if (Input.GetKeyDown(itemSpawner.DebugKeySpawn))
            {
               itemSpawner.getItemToSpawn();
            }
         }
         if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon("M1911");
         if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon("AK-47");
         if (Input.GetKeyDown(KeyCode.Alpha3)) EquipWeapon("M48");
         if (Input.GetKeyDown(KeyCode.Alpha4)) EquipWeapon("M107");
         if (Input.GetKeyDown(KeyCode.Alpha5)) EquipWeapon("M249");
         if (Input.GetKeyDown(KeyCode.Alpha6)) EquipWeapon("Bennelli M4");
         if (Input.GetKeyDown(KeyCode.Alpha7)) EquipWeapon("Uzi");
      }
      Vector3 currentPlayerPosition = GameObject.FindWithTag("Player").transform.position;
      if (isOutsideBounds(currentPlayerPosition))
      {
         Vector3 newInBoundsLocation = Vector3.Max(Vector3.Min(currentPlayerPosition, maxBounds - new Vector3(0.2f, 0, 0.2f)), minBounds + new Vector3(0.2f, 0, 0.2f));
         Debug.Log("New Vector: " + Vector3.Min(Vector3.Max(currentPlayerPosition, minBounds), maxBounds));
         GameObject.FindWithTag("Player").transform.position = newInBoundsLocation;
      }
      if (Input.GetKeyDown(KeyCode.Escape))
      {
         PauseMenu.SetActive(!PauseMenu.activeSelf);
         if (PauseMenu.activeSelf)
         {
            Time.timeScale = 0f;
         }
         else
         {
            Time.timeScale = 1;
         }
      }
      if (isInShop && Input.GetKeyDown(KeyCode.B) && !string.IsNullOrEmpty(purchasableShopItem) && shopPrices.ContainsKey(purchasableShopItem))
      {
         // First check if it can be purchased. If so, set new balance, play purchase sound, set player effects.
         if (balance >= shopPrices[purchasableShopItem])
         {
            // Set effects on player
            if (purchasableShopItem == "DeepPockets")
            {
               buyShopItem();
               GetOrAddItemComponent<DeepPockets>();
               UIInventory.OnPickupItem("DeepPockets");
            }
            else if (purchasableShopItem == "CoeurDeVie")
            {
               buyShopItem();
               GetOrAddItemComponent<CoeurDeVie>();
               UIInventory.OnPickupItem("CoeurDeVie");
            }
            else if (purchasableShopItem == "Freeze")
            {
               buyShopItem();
               GetOrAddItemComponent<Freeze>();
               UIInventory.OnPickupItem("Freeze");
            }
            else if (purchasableShopItem == "WingsOfVoice")
            {
               buyShopItem();
               GetOrAddItemComponent<WingsOfVoice>();
               UIInventory.OnPickupItem("WingsOfVoice");
               Destroy(shopItemLookAt.gameObject);
               Debug.Log("Destroyed " + shopItemLookAt.gameObject.name + ", tag: " + shopItemLookAt.gameObject.tag);

                itemText.SetText("Wings of Voice", 5.0f, 75);
                itemDescText.SetText("Press [L CTRL] to activate a short ranged dash!", 5.0f);
            }
            else if (purchasableShopItem == "AdrenalineShot")
            {
               buyShopItem();
               GetOrAddItemComponent<AdrenalineShot>();
               UIInventory.OnPickupItem("AdrenalineShot");
               Destroy(shopItemLookAt.gameObject);
               Debug.Log("Destroyed " + shopItemLookAt.gameObject.name + ", tag: " + shopItemLookAt.gameObject.tag);

                itemText.SetText("Adrenaline Shot", 5.0f, 75);
                itemDescText.SetText("Time to up the ante.\n20% Damage Increase", 5.0f);
            }
            else if (purchasableShopItem == "ProteinPowder")
            {
               buyShopItem();
               GetOrAddItemComponent<ProteinPowder>();
               UIInventory.OnPickupItem("ProteinPowder");
               Destroy(shopItemLookAt.gameObject);
               Debug.Log("Destroyed " + shopItemLookAt.gameObject.name + ", tag: " + shopItemLookAt.gameObject.tag);

                itemText.SetText("Protein Powder", 5.0f, 75);
                itemDescText.SetText("Everyone's doing it!\n40% Damage Increase", 5.0f);
            }
            else if (purchasableShopItem == "FinalDestination")
            {
               buyShopItem();
               GetOrAddItemComponent<FinalDestination>();
               UIInventory.OnPickupItem("FinalDestination");
               Destroy(shopItemLookAt.gameObject);
               Debug.Log("Destroyed " + shopItemLookAt.gameObject.name + ", tag: " + shopItemLookAt.gameObject.tag);

                itemText.SetText("Final Destination", 5.0f, 75);
                itemDescText.SetText("You can't run from Death.\n10% Crit Chance", 5.0f);
            }
            else if (purchasableShopItem == "SwiftExecution")
            {
               buyShopItem();
               GetOrAddItemComponent<SwiftExecution>();
               UIInventory.OnPickupItem("SwiftExecution");
               Destroy(shopItemLookAt.gameObject);
               Debug.Log("Destroyed " + shopItemLookAt.gameObject.name + ", tag: " + shopItemLookAt.gameObject.tag);

                itemText.SetText("Swift Execution", 5.0f, 75);
                itemDescText.SetText("Must've been the wind.\n+15% RPM", 5.0f);
            }
            else if (purchasableShopItem == "ApollyonsPit")
            {
               buyShopItem();
               GetOrAddItemComponent<ApollyonsPit>();
               UIInventory.OnPickupItem("ApollyonsPit");
               Destroy(shopItemLookAt.gameObject);
               Debug.Log("Destroyed " + shopItemLookAt.gameObject.name + ", tag: " + shopItemLookAt.gameObject.tag);

                itemText.SetText("Apollyon's Pit", 5.0f, 75);
                itemDescText.SetText("And unto them was given power.\n+100% RPM -35% DMG", 5.0f);
            }
            else if (purchasableShopItem == "PumpedUpKicks")
            {
               buyShopItem();
               GetOrAddItemComponent<PumpedUpKicks>();
               UIInventory.OnPickupItem("PumpedUpKicks");
               Destroy(shopItemLookAt.gameObject);
               Debug.Log("Destroyed " + shopItemLookAt.gameObject.name + ", tag: " + shopItemLookAt.gameObject.tag);

                itemText.SetText(itemManager.Grab<PumpedUpKicks>().toString(), 5.0f, 75);
                itemDescText.SetText("It was an exclusive drop sorry buddy.\n+5% Player Speed", 5.0f);
            }
            else if (purchasableShopItem == "DoubleTap")
            {
               buyShopItem();
               GetOrAddItemComponent<DoubleTap>();
               UIInventory.OnPickupItem("DoubleTap");
               Destroy(shopItemLookAt.gameObject);
               Debug.Log("Destroyed " + shopItemLookAt.gameObject.name + ", tag: " + shopItemLookAt.gameObject.tag);

                itemText.SetText(itemManager.Grab<DoubleTap>().toString(), 5.0f, 75);
                itemDescText.SetText("For legal reasons we couldn't do the root beer.\n2 Bullets for 1 Shot\n30% Damage Reduction", 5.0f);
            }
            else if (purchasableShopItem == "CannonballSplash")
            {
               buyShopItem();
               GetOrAddItemComponent<CannonballSplash>();
               //ItemManager.Instance.Grab<CannonballSplash>().onPickup();
               UIInventory.OnPickupItem("CannonballSplash");
               Destroy(shopItemLookAt.gameObject);
               Debug.Log("Destroyed " + shopItemLookAt.gameObject.name + ", tag: " + shopItemLookAt.gameObject.tag);

                itemText.SetText(itemManager.Grab<CannonballSplash>().toString(), 5.0f, 75);
                itemDescText.SetText("Oh you're shootin cannons now.\nBullet size up\n10% Damage Increase", 5.0f);
            }
            else if (purchasableShopItem == "GungHoGloves")
            {
               buyShopItem();
               GetOrAddItemComponent<GungHoGloves>();
               UIInventory.OnPickupItem("GungHoGloves");
               Destroy(shopItemLookAt.gameObject);
               Debug.Log("Destroyed " + shopItemLookAt.gameObject.name + ", tag: " + shopItemLookAt.gameObject.tag);

                itemText.SetText(itemManager.Grab<GungHoGloves>().toString(), 5.0f, 75);
                itemDescText.SetText("Fastest reload this side of the Mississippi.\n25% Reload Speed Increase", 5.0f);
            }
            else if (getCurrentWeaponName() != "AK-47" && purchasableShopItem == "AK-47")
            {
               buyShopItem();
               EquipWeapon("AK-47");
               itemText.SetText("AK-47 Equipped", 5.0f, 75);
            }
            else if (getCurrentWeaponName() != "Bennelli M4" && purchasableShopItem == "Bennelli M4")
            {
               buyShopItem();
               EquipWeapon("Bennelli M4");
               itemText.SetText("Bennelli M4 Equipped", 5.0f, 75);
            }
            else if (getCurrentWeaponName() != "M249" && purchasableShopItem == "M249")
            {
               buyShopItem();
               EquipWeapon("M249");
               itemText.SetText("M249 Equipped", 5.0f, 75);
            }
            else if (getCurrentWeaponName() != "Uzi" && purchasableShopItem == "Uzi")
            {
               buyShopItem();
               EquipWeapon("Uzi");
               itemText.SetText("Uzi Equipped", 5.0f, 75);
            }
            else if (getCurrentWeaponName() != "M48" && purchasableShopItem == "M48")
            {
               buyShopItem();
               EquipWeapon("M48");
               itemText.SetText("M48 Equipped", 5.0f, 75);
            }
            else if (getCurrentWeaponName() != "M107" && purchasableShopItem == "M107")
            {
               buyShopItem();
               EquipWeapon("M107");
               itemText.SetText("M107 Equipped", 5.0f, 75);
            }
            else if (purchasableShopItem == "HealthRefill" && currentHealth != maxHealth)
            {
               buyShopItem();
               currentHealth = maxHealth;
               healthBar.SetHealth(currentHealth);
            }
            else
            {
               playerAudio.PlayInsufficientFundsSound();
            }
         }
         shopItemLookAt = null;
         purchasableShopItem = "";
         purchaseItemText.enabled = false;
      }
      if (isInShop && Input.GetKeyDown(KeyCode.E) && !string.IsNullOrEmpty(purchasableShopItem) && purchasableShopItem == "EXIT")
      {
         if (!isTeleporting) StartCoroutine(teleportToNextRoom(5));
      }
      if (isInBossScene && Input.GetKeyDown(KeyCode.E) && !string.IsNullOrEmpty(purchasableShopItem) && purchasableShopItem == "Fire")
      {
         Debug.Log("Dropping cat");
         StartCoroutine(DropCatwalk());
      }
      if (isInBossScene && Input.GetKeyDown(KeyCode.E) && !string.IsNullOrEmpty(purchasableShopItem) && purchasableShopItem == "Lava")
      {
         StartCoroutine(LowerLava());
         StopBossColumns();
         RemoveEnemies();
      }
   }

   void StopBossColumns()
   {
      GameObject[] lavaDoors = GameObject.FindGameObjectsWithTag("BossColumns");
      foreach (GameObject door in lavaDoors)
      {
         door.SetActive(false);
      }
   }

   void OnTriggerEnter(Collider other)
   {
      // If about to collide with an obstacle, cancel dashing if active
      if (itemManager.Grab<WingsOfVoice>() != null && itemManager.Grab<WingsOfVoice>().hasItem())
      {
         itemManager.Grab<WingsOfVoice>().OnPlayerTriggerEnter(other);
      }
      if (other.gameObject.CompareTag("PickUp") && other.gameObject.activeInHierarchy)
      {
         playerAudio.PlayPickupSound();
         // Increment amount of money, update balance display
         balance += 150;
         SetCurrentBalance();
         other.gameObject.SetActive(false);
      }
      else if (other.gameObject.CompareTag("Item_WingsOfVoice"))
      {
         other.gameObject.GetComponent<Collider>().enabled = false;
         Transform pickupRoot = other.gameObject.transform.parent;
         other.gameObject.SetActive(false);
         Transform wings = pickupRoot.Find("airplane_wings");

         if (wings) wings.gameObject.SetActive(false);

         playerAudio.PlayPickupSound();
         Debug.Log("WINGS OF VOICE");

         if (GetComponent<WingsOfVoice>() == null)
         {
            WingsOfVoice ability = gameObject.AddComponent<WingsOfVoice>();
            ability.onPickup();                 // run item-specific init
         }

         Destroy(other.gameObject);              // remove ground pickup

         UIInventory.OnPickupItem("WingsOfVoice");
         itemSpawner.deactivateSpotlight();

         itemText.SetText("Wings of Voice", 5.0f, 75);
         itemDescText.SetText("Press [L CTRL] to activate a short ranged dash!", 5.0f);
      }
      else if (other.gameObject.CompareTag("Item_CoeurDeVie"))
      {
         playerAudio.PlayPickupSound();
         Debug.Log("COEUR DE VIE");
         GetOrAddItemComponent<CoeurDeVie>();
         UIInventory.OnPickupItem("CoeurDeVie");
         itemSpawner.deactivateSpotlight();

         other.gameObject.SetActive(false);
         itemText.SetText("Coeur De Vie", 5.0f, 75);
         itemDescText.SetText("A little bit of life.\n+25 HP", 5.0f);
      }
      else if (other.gameObject.CompareTag("Item_DeepPockets"))
      {
         playerAudio.PlayPickupSound();
         Debug.Log("DEEP POCKETS");
         GetOrAddItemComponent<DeepPockets>();
         UIInventory.OnPickupItem("DeepPockets");
         itemSpawner.deactivateSpotlight();

         other.gameObject.SetActive(false);
         itemText.SetText("Deep Pockets", 5.0f, 75);
         itemDescText.SetText("Some extra reserves.\n+10% Clip Size", 5.0f);
      }
      else if (other.gameObject.CompareTag("Item_AdrenalineShot"))
      {
         playerAudio.PlayPickupSound();
         Debug.Log("ADRENALINE SHOT");
         GetOrAddItemComponent<AdrenalineShot>();
         UIInventory.OnPickupItem("AdrenalineShot");
         itemSpawner.deactivateSpotlight();

         other.gameObject.SetActive(false);
         itemText.SetText("Adrenaline Shot", 5.0f, 75);
         itemDescText.SetText("Time to up the ante.\n20% Damage Increase", 5.0f);
      }
      else if (other.gameObject.CompareTag("Item_ProteinPowder"))
      {
         playerAudio.PlayPickupSound();
         Debug.Log("PROTEIN POWDER");
         GetOrAddItemComponent<ProteinPowder>();
         UIInventory.OnPickupItem("ProteinPowder");
         itemSpawner.deactivateSpotlight();

         other.gameObject.SetActive(false);
         itemText.SetText("Protein Powder", 5.0f, 75);
         itemDescText.SetText("Everyone's doing it!\n40% Damage Increase", 5.0f);
      }
      else if (other.gameObject.CompareTag("Item_FinalDestination"))
      {
         playerAudio.PlayPickupSound();
         Debug.Log("Final Destination");
         GetOrAddItemComponent<FinalDestination>();
         itemManager.Grab<FinalDestination>().onPickup();
         UIInventory.OnPickupItem("FinalDestination");
         itemSpawner.deactivateSpotlight();

         other.gameObject.SetActive(false);
         itemText.SetText("Final Destination", 5.0f, 75);
         itemDescText.SetText("You can't run from Death.\n10% Crit Chance", 5.0f);
      }
      else if (other.gameObject.CompareTag("Item_SwiftExecution"))
      {
         playerAudio.PlayPickupSound();
         Debug.Log("Swift Execution");
         GetOrAddItemComponent<SwiftExecution>();
         UIInventory.OnPickupItem("SwiftExecution");
         itemSpawner.deactivateSpotlight();

         other.gameObject.SetActive(false);
         itemText.SetText("Swift Execution", 5.0f, 75);
         itemDescText.SetText("Must've been the wind.\n+15% RPM", 5.0f);
      }
      else if (other.gameObject.CompareTag("Item_ApollyonsPit"))
      {
         playerAudio.PlayPickupSound();
         Debug.Log("Apollyon's Pit");
         GetOrAddItemComponent<ApollyonsPit>();
         UIInventory.OnPickupItem("ApollyonsPit");
         itemSpawner.deactivateSpotlight();

         other.gameObject.SetActive(false);
         itemText.SetText("Apollyon's Pit", 5.0f, 75);
         itemDescText.SetText("And unto them was given power.\n+100% RPM -35% DMG", 5.0f);
      }
      else if (other.gameObject.CompareTag("Item_GungHoGloving"))
      {
         playerAudio.PlayPickupSound();
         Debug.Log("Gung-Ho Gloves");
         GetOrAddItemComponent<GungHoGloves>();
         UIInventory.OnPickupItem("GungHoGloves");
         itemSpawner.deactivateSpotlight();

         other.gameObject.SetActive(false);
         itemText.SetText(itemManager.Grab<GungHoGloves>().toString(), 5.0f, 75);
         itemDescText.SetText("Fastest reload this side of the Mississippi.\n25% Reload Speed Increase", 5.0f);
      }
      else if (other.gameObject.CompareTag("Item_PumpedUpKicks"))
      {
         playerAudio.PlayPickupSound();
         Debug.Log("PumpedUpKicks");
         GetOrAddItemComponent<PumpedUpKicks>();
         UIInventory.OnPickupItem("PumpedUpKicks");
         itemSpawner.deactivateSpotlight();

         other.gameObject.SetActive(false);
         itemText.SetText(itemManager.Grab<PumpedUpKicks>().toString(), 5.0f, 75);
         itemDescText.SetText("It was an exclusive drop sorry buddy.\n+5% Player Speed", 5.0f);
      }
      else if (other.gameObject.CompareTag("Item_DoubleTap"))
      {
         playerAudio.PlayPickupSound();
         Debug.Log("DoubleTap");
         GetOrAddItemComponent<DoubleTap>();
         UIInventory.OnPickupItem("DoubleTap");
         itemSpawner.deactivateSpotlight();

         other.gameObject.SetActive(false);
         itemText.SetText(itemManager.Grab<DoubleTap>().toString(), 5.0f, 75);
         itemDescText.SetText("For legal reasons we couldn't do the root beer.\n2 Bullets for 1 Shot\n30% Damage Reduction", 5.0f);
      }
      else if (other.gameObject.CompareTag("Item_CannonballSplash"))
      {
         playerAudio.PlayPickupSound();
         Debug.Log("CannonballSplash");
         GetOrAddItemComponent<CannonballSplash>();
         itemManager.Grab<CannonballSplash>().onPickup();
         UIInventory.OnPickupItem("CannonballSplash");
         itemSpawner.deactivateSpotlight();

         other.gameObject.SetActive(false);
         itemText.SetText(itemManager.Grab<CannonballSplash>().toString(), 5.0f, 75);
         itemDescText.SetText("Oh you're shootin cannons now.\nBullet size up\n10% Damage Increase", 5.0f);
      }
      else if (isInShop)
      {
         ShopOnTriggerHelper(other);
      }
   }

   // Fix shop popups when moving between items
   private void OnTriggerStay(Collider other)
   {
      if (isInShop)
      {
         ShopOnTriggerHelper(other);
      }
      else if (isInBossScene && other.gameObject.CompareTag("FireColumnButton"))
      {
         purchasableShopItem = "Fire";
         purchaseItemText.enabled = true;
         purchaseItemText.text = "Press E to activate";
      }
      else if (isInBossScene && other.gameObject.CompareTag("DrainLavaButton"))
      {
         purchasableShopItem = "Lava";
         purchaseItemText.enabled = true;
         purchaseItemText.text = "Press E to activate";
      }
      else if (other.gameObject.CompareTag("Lava"))
      {
         isTouchingLava = true;
         if (lavaCoroutine == null)
         {
            lavaCoroutine = StartCoroutine(ApplyLavaDamageWhileTouching());
         }
      }
   }

   void OnTriggerExit(Collider other)
   {
      if (other.gameObject.CompareTag("Shop_DeepPockets")
         || other.gameObject.CompareTag("Shop_CoeurDeVie")
         || other.gameObject.CompareTag("ExitDoor")
         || other.gameObject.CompareTag("FireColumnButton")
         || other.gameObject.CompareTag("DrainLavaButton")
         || other.gameObject.CompareTag("Shop_HealthRefill")
         || other.gameObject.CompareTag("Shop_Freeze")
         || other.gameObject.CompareTag("Shop_WingsOfVoice")
         || other.gameObject.CompareTag("Shop_FinalDestination")
         || other.gameObject.CompareTag("Shop_DoubleTap")
         || other.gameObject.CompareTag("Shop_AdrenalineShot")
         || other.gameObject.CompareTag("Shop_ProteinPowder")
         || other.gameObject.CompareTag("Shop_SwiftExecution")
         || other.gameObject.CompareTag("Shop_ApollyonsPit")
         || other.gameObject.CompareTag("Shop_PumpedUpKicks")
         || other.gameObject.CompareTag("Shop_CannonballSplash")
         || other.gameObject.CompareTag("Shop_GungHoGloves")
         || other.gameObject.CompareTag("Shop_AK-47")
         || other.gameObject.CompareTag("Shop_Bennelli M4")
         || other.gameObject.CompareTag("Shop_M249")
         || other.gameObject.CompareTag("Shop_Uzi")
         || other.gameObject.CompareTag("Shop_M48")
         || other.gameObject.CompareTag("Shop_M107"))
      {
         purchasableShopItem = "";
         purchaseItemText.enabled = false;
      }
      else if (other.gameObject.CompareTag("Lava"))
      {
         isTouchingLava = false;
         if (lavaCoroutine != null)
         {
            StopCoroutine(lavaCoroutine);
            lavaCoroutine = null;
         }
      }
   }

   IEnumerator LowerLava()
   {
      GameObject lava = GameObject.FindGameObjectWithTag("Lava");

      float targetY = -45f;
      float moveSpeed = 6f; // units per second

      while (lava.transform.position.y > targetY)
      {
         Vector3 currentPosition = lava.transform.position;

         // Move downward smoothly
         currentPosition.y -= moveSpeed * Time.deltaTime;

         // Clamp to targetY if we go too far
         if (currentPosition.y < targetY)
            currentPosition.y = targetY;

         lava.transform.position = currentPosition;

         yield return null; // wait a frame
      }
   }


   IEnumerator ApplyLavaDamageWhileTouching()
   {
      while (isTouchingLava == true)
      {
         ApplyDamage(5f);
         yield return new WaitForSeconds(1f);
      }
   }

   IEnumerator DropCatwalk()
   {
      const float rotRate = 1.01f;

      GameObject[] fallawayCatwalks = GameObject.FindGameObjectsWithTag("FallingCatwalk");
      GameObject[] fallawayStairs = GameObject.FindGameObjectsWithTag("FallingStairs");

      float catwalkRot = 1f;   // Starting from 0 towards 90
      float stairRot = -89f;   // Starting just above -90, to allow exponential increase toward 0

      while (catwalkRot < 90f || stairRot < -0.5f)
      {
         // Rotate Catwalks
         foreach (GameObject catwalk in fallawayCatwalks)
         {
            Vector3 rotation = catwalk.transform.eulerAngles;
            rotation.x = catwalkRot;
            catwalk.transform.eulerAngles = rotation;
         }

         // Rotate Stairs
         foreach (GameObject stair in fallawayStairs)
         {
            Vector3 rotation = stair.transform.eulerAngles;
            rotation.x = stairRot;
            stair.transform.eulerAngles = rotation;
         }

         // Increment rotations
         catwalkRot *= rotRate;

         // For stairs, we want to move from -90 toward 0 exponentially
         // Since -90 * 1.01 = -90.9 (wrong direction), we need to use a different method.
         stairRot = Mathf.Lerp(stairRot, 0f, 1f - 1f / rotRate); // Moves toward 0 at similar exponential pace

         yield return null;
      }

      // Clamp both to final positions
      foreach (GameObject catwalk in fallawayCatwalks)
      {
         Vector3 rotation = catwalk.transform.eulerAngles;
         rotation.x = 90f;
         catwalk.transform.eulerAngles = rotation;
      }

      foreach (GameObject stair in fallawayStairs)
      {
         Vector3 rotation = stair.transform.eulerAngles;
         rotation.x = 0f;
         stair.transform.eulerAngles = rotation;
      }
   }



   // Function to update the displayed balance.
   void SetCurrentBalance()
   {
      // Update the balance text with the current balance.
      moneyText.text = "$" + balance.ToString();
   }

   // Function for Deep Pockets
   public void ApplyAmmoBonus(float AmmoBonus)
   {
      foreach (WeaponData weapon in weaponDatabase.weapons)
      {
         int add = Mathf.Max(1, Mathf.FloorToInt(weapon.magazineSize * AmmoBonus));

         weapon.magazineSize = weapon.magazineSize + add;

         Debug.Log("Increased " + weapon.weaponName + " magazine capacity by: " + AmmoBonus + "% to " + weapon.magazineSize);

      }
      playerWeapon.Reload();
      clipSizeStat *= (1 + AmmoBonus);
      Debug.Log("ClipSize: " + clipSizeStat);
      clipSizeStatText.text = Math.Round((decimal)clipSizeStat, 2) + "x";
   }

   // Function for Damage Modifier items
   public void ApplyDamageIncrease(float Multiplier)
   {
      foreach (WeaponData weapon in weaponDatabase.weapons)
      {
         weapon.damage *= Multiplier;
         Debug.Log("Increased " + weapon.weaponName + " damage by " + Multiplier + " to " + weapon.damage);
      }
   }

   // Function for RPM Modifier items
   public void ApplyFireRateIncrease(float Multiplier)
   {
      foreach (WeaponData weapon in weaponDatabase.weapons)
      {
         weapon.fireRate *= Multiplier;
         Debug.Log("Increased " + weapon.weaponName + " fire rate by " + Multiplier + " to " + weapon.fireRate);
      }
   }

   public void ApplySpeedIncrease(float Multiplier)
   {
      rootMotionControl.animationSpeed *= Multiplier;
      rootMotionControl.rootMovementSpeed *= Multiplier;
   }

   // Function for RPM Modifier items
   public void ApplyReloadSpeedDecrease(float Multiplier)
   {
      foreach (WeaponData weapon in weaponDatabase.weapons)
      {
         weapon.reloadTime *= Multiplier;
         Debug.Log("Decreased " + weapon.weaponName + " reload speed by " + Multiplier + " to " + weapon.reloadTime);
      }
   }

   // Function for Pellet Count Modifier items
   public void ModifyPelletCount()
   {
      foreach (WeaponData weapon in weaponDatabase.weapons)
      {
         if (weapon.pelletsPerShot == 0)
         {
            weapon.pelletsPerShot = 1;
         }
         weapon.pelletsPerShot *= 2;
         Debug.Log("Increased pellets per shot of " + weapon.weaponName + " to " + weapon.pelletsPerShot);
      }
   }

   private IEnumerator teleportToNextRoom(int secondsToTeleport)
   {
      isTeleporting = true;
      // Display the win text.
      winTextObject.SetActive(true);
      AsyncOperation asyncLoad = null;

      if (DebugMode) secondsToTeleport = 1;

      // Teleport back to start after killing boss
      if (isInBossScene)
      {
         Debug.Log("Teleport back to stage1...");
         itemSpawner.getItemToSpawn();
         itemDescText.SetText("The Goblin King is defeated!\nCollect your reward in the middle of the room", 15, 50);

         for (int seconds = secondsToTeleport; seconds >= 0; seconds--)
         {
            winTextObject.GetComponent<TextMeshProUGUI>().text = "Boss defeated! Teleporting to Room 1 in " + seconds + " seconds...";
            yield return new WaitForSeconds(1);
         }
         winTextObject.SetActive(false);
         isInBossScene = false;
         isTouchingLava = false;
         currentSceneNumber = 1;
         numScenesProgressed++;
         numCyclesCompleted++;
         cyclesCompletedText.text = "Cycles: " + numCyclesCompleted;
         roomsProgressedText.text = "Rooms: " + numScenesProgressed;

         // Ensure player doesn't spawn outside of map (was an issue with shop)
         rb.useGravity = false;
         transform.position = new Vector3(0, 1f, -5);
         asyncLoad = SceneManager.LoadSceneAsync("Room1Scene");
         // Back to scene 1 - refill player's health
         currentHealth = maxHealth;
         healthBar.SetHealth(currentHealth);
      }
      else if (isInShop == false && currentSceneNumber % 2 == 0)
      {
         Debug.Log("Teleport to shop...");
         for (int seconds = secondsToTeleport; seconds >= 0; seconds--)
         {
            winTextObject.GetComponent<TextMeshProUGUI>().text = "Teleporting to Shop in " + seconds + " seconds...";
            yield return new WaitForSeconds(1);
         }
         winTextObject.SetActive(false);
         // Ensure player doesn't spawn outside of map (was an issue with shop)
         rb.useGravity = false;
         transform.position = new Vector3(0, 1f, -5);
         asyncLoad = SceneManager.LoadSceneAsync("ShopScene");
         isInShop = true;
         // No enemies to kill in shop scene - make text invisible
         enemiesKilledText.enabled = false;
      }
      // Teleport to boss room if we've done everything else
      else if (currentSceneNumber == numScenes && isInBossScene == false)
      {
         Debug.Log("Teleport to boss...");
         for (int seconds = secondsToTeleport; seconds >= 0; seconds--)
         {
            winTextObject.GetComponent<TextMeshProUGUI>().text = "Teleporting to boss in " + seconds + " seconds...";
            yield return new WaitForSeconds(1);
         }
         winTextObject.SetActive(false);
         // Ensure player doesn't spawn outside of map (was an issue with shop)
         transform.position = new Vector3(0, 1f, -5);
         rb.useGravity = false;
         asyncLoad = SceneManager.LoadSceneAsync("BossScene");
         isInBossScene = true;
         numScenesProgressed++;
         roomsProgressedText.text = "Rooms: " + numScenesProgressed;
         enemiesKilledText.enabled = false;
      }
      else
      {
         isInBossScene = false;
         int nextSceneNumber = currentSceneNumber + 1;
         if (nextSceneNumber > numScenes)
         {
            // Going back to scene 1 - refill player's health
            currentHealth = maxHealth;
            healthBar.SetHealth(currentHealth);
            nextSceneNumber = 1;
         }

         Debug.Log("Teleport to room " + nextSceneNumber);

         // Different position object for Shop teleport message
         if (nextSceneNumber == 3 || nextSceneNumber == 5)
         {
            winTextObject.SetActive(false);
            itemText.DisableText();
            itemDescText.DisableText();
            winTextShopObject.SetActive(true);
            for (int seconds = secondsToTeleport; seconds >= 0; seconds--)
            {
               winTextShopObject.GetComponent<TextMeshProUGUI>().text = "Teleporting to Room " + nextSceneNumber + " in " + seconds + " seconds...";
               yield return new WaitForSeconds(1);
            }
            winTextShopObject.SetActive(false);
         }
         else
         {
            for (int seconds = secondsToTeleport; seconds >= 0; seconds--)
            {
               winTextObject.GetComponent<TextMeshProUGUI>().text = "Teleporting to Room " + nextSceneNumber + " in " + seconds + " seconds...";
               yield return new WaitForSeconds(1);
            }
            winTextObject.SetActive(false);
         }

         // Ensure player doesn't spawn outside of map (was an issue with shop)
         transform.position = new Vector3(0, 1f, -5);
         rb.useGravity = false;
         asyncLoad = SceneManager.LoadSceneAsync("Room" + nextSceneNumber + "Scene");
         currentSceneNumber = nextSceneNumber;
         isInShop = false;
         numScenesProgressed++;
         roomsProgressedText.text = "Rooms: " + numScenesProgressed;

         // Make sure enemies killed text is visible outside of shop scene
         enemiesKilledText.enabled = true;
      }

      enemiesKilled = 0;
      updateEnemiesKilledText();
      isTeleporting = false;
      while (!asyncLoad.isDone) yield return null;
      Physics.SyncTransforms();
      transform.position = new Vector3(0, 1f, -5);
      rb.useGravity = true;
   }

   public bool getIsInShop() { return isInShop; }

   private void OnCollisionStay(Collision collision)
   {
      if (collision.gameObject.CompareTag("Ground") || ((1 << collision.gameObject.layer) & LayerMask.GetMask("Obstacle")) != 0)
      {
         isGrounded = true;
      }
   }

   private void OnCollisionExit(Collision collision)
   {
      if (collision.gameObject.CompareTag("Ground") || ((1 << collision.gameObject.layer) & LayerMask.GetMask("Obstacle")) != 0)
      {
         isGrounded = false;
      }
      else if (collision.gameObject.CompareTag("Lava"))
      {
         isTouchingLava = false;
         if (lavaCoroutine != null)
         {
            StopCoroutine(lavaCoroutine);
            lavaCoroutine = null;
         }
      }
   }

   private void OnCollisionEnter(Collision collision)
   {
      // Consider grounded if hitting the floor or another surface
      if (collision.gameObject.CompareTag("Ground") || ((1 << collision.gameObject.layer) & LayerMask.GetMask("Obstacle")) != 0)
      {
         isGrounded = true;
      }
      else if (collision.gameObject.CompareTag("Lava"))
      {
         isTouchingLava = true;
         if (lavaCoroutine == null)
         {
            lavaCoroutine = StartCoroutine(ApplyLavaDamageWhileTouching());
         }
      }
   }

   public void ApplyDamage(float damage)
   {
      if (isDead) { return; }
      currentHealth -= damage;
      healthBar.SetHealth(currentHealth);
      playerAudio.PlayDamageSound();
      if (currentHealth <= 0)
      {
         isDead = true;
         anim.SetTrigger("dead");
         // Ragdoll
         SetRagdollComponents(true);
         anim.enabled = false;
         DisableEnemies();
         DisableWeapon();
         characterInputController.enabled = false;

         // Display game over menu
         GameOverMenu.SetActive(!GameOverMenu.activeSelf);
         if (numScenesProgressed == 1)
         {
            GameOverText.text = "YOU SURVIVED " + numScenesProgressed + " ROOM";
         }
         else
         {
            GameOverText.text = "YOU SURVIVED " + numScenesProgressed + " ROOMS";
         }
      }
   }

   private void DisableWeapon()
   {
      playerWeapon.setIsDisabled(true);
      foreach (GameObject weapon in weaponObjects)
      {
         // Disable colliders and renderers
         CapsuleCollider col = weapon.GetComponent<CapsuleCollider>();
         if (col != null)
         {
            col.enabled = false;
         }
         MeshRenderer renderer = weapon.GetComponent<MeshRenderer>();
         if (renderer != null)
         {
            renderer.enabled = false;
         }
         MeshRenderer[] meshRenderers = weapon.GetComponentsInChildren<MeshRenderer>();
         foreach (MeshRenderer mr in meshRenderers)
         {
            mr.enabled = false;
         }
      }
   }

   public void DisableEnemies()
   {
      spawner.Stop();

      GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
      foreach (GameObject enemy in enemies)
      {
         // Disable AI
         UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
         if (agent != null)
         {
            agent.enabled = false;
         }
      }
   }

   public void EnableEnemies()
   {
      spawner.StartSpawner();
      GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
      foreach (GameObject enemy in enemies)
      {
         // Enable AI
         UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
         if (agent != null)
         {
            agent.enabled = true;
         }
      }
   }

   private void RemoveEnemies()
   {
      spawner.Stop();

      GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
      foreach (GameObject enemy in enemies)
      {
         Destroy(enemy);
      }
   }

   // If ragdoll colliders are enabled, the player will move slightly forward with no input
   private void SetRagdollComponents(bool enable)
   {
      foreach (var r in ragdollRigidbodies)
      {
         r.isKinematic = !enable;
      }
      foreach (Collider col in ragdollColliders)
      {
         col.enabled = enable;
      }
   }

   public bool getIsGrounded()
   {
      return isGrounded;
   }

   private void updateEnemiesKilledText()
   {
      if (!isInBossScene)
      {
         int etk = enemiesToKill[currentSceneNumber - 1] + (5*numCyclesCompleted);
         enemiesKilledText.text = enemiesKilled.ToString() + "/" + etk.ToString();
         // Go to next scene if all enemies have been killed
         if (!isTeleporting && enemiesKilled >= etk)
         {
            // Stop Spawning Enemies
            spawner.Stop();
            // Spawn item if on level 3 or 5
            if (currentSceneNumber == 5 || currentSceneNumber == 3 || currentSceneNumber == 1 && isInBossScene == false)
            {
               itemSpawner.getItemToSpawn();
               itemDescText.SetText("Enemies Defeated!\nCollect your reward in the middle of the room", 15, 50);
               //winTextObject.SetActive(true);
               //winTextObject.GetComponent<TextMeshProUGUI>().text = "Enemies Defeated!\nCollect your reward in the middle of the room";
               StartCoroutine(teleportToNextRoom(15));
            }
            else if (currentSceneNumber != 5 || currentSceneNumber != 3)
            {
               StartCoroutine(teleportToNextRoom(5));
            }
         }
      }
   }

   public void incrementEnemiesKilled()
   {
      enemiesKilled++;
      updateEnemiesKilledText();
   }

   void EquipWeapon(string weaponToEquip)
   {
      Debug.Log("EquipWeapon called: " + weaponToEquip);
      // Check if current weapon is reloading, prevent switch if so
      if (playerWeapon != null && playerWeapon.isReloading)
      {
         Debug.Log("Cannot switch weapon while reloading!");
         return;
      }

      if (weaponObjects == null || weaponObjects.Count == 0)
      {
         Debug.LogError("weaponObjects list is null or empty");
         return;
      }

      int index = -1;
      for (int i = 0; i < weaponObjects.Count; i++)
      {
         if (weaponObjects[i].name == weaponToEquip) index = i;
      }

      if (playerWeapon != null)
      {
         string currentName = weaponDatabase.weapons[currentWeaponIndex].weaponName;
         weaponAmmoCounts[currentName] = playerWeapon.GetCurrentAmmo();
      }

      if (currentWeaponModel != null) // Switch weapon models
      {
         currentWeaponModel.SetActive(false);
      }
      currentWeaponIndex = index;
      currentWeaponModel = weaponObjects[currentWeaponIndex];
      currentWeaponModel.SetActive(true);

      playerWeapon = currentWeaponModel.GetComponent<PlayerWeapon>();
      if (playerWeapon == null)
      {
         Debug.LogError("PlayerWeapon component missing on: " + currentWeaponModel.name);
         return;
      }

      if (weaponDatabase == null || weaponDatabase.weapons == null || index >= weaponDatabase.weapons.Count)
      {
         Debug.LogError("weaponDatabase or its weapons list is invalid");
         return;
      }

      WeaponData weaponData = weaponDatabase.weapons[index];

      // Load saved ammo if it exists, otherwise default to full mag
      int ammoToLoad = weaponAmmoCounts.TryGetValue(weaponData.weaponName, out int savedAmmo)
          ? savedAmmo
          : weaponData.magazineSize;

      Debug.Log("Setting weapon data for: " + weaponData.weaponName);
      playerWeapon.SetWeaponData(weaponData, ammoToLoad);
   }

   public string getCurrentWeaponName()
   {
      return currentWeaponModel.name;
   }

   private void ShopOnTriggerHelper(Collider other)
   {
      if (other.gameObject.CompareTag("Shop_DeepPockets"))
      {
         purchasableShopItem = "DeepPockets";
         displayPurchaseText(other, "Press B to buy Deep Pockets\n+" + "10% bullet count" + "\n$" + shopPrices[purchasableShopItem]);
      }
      else if (other.gameObject.CompareTag("Shop_CoeurDeVie"))
      {
         purchasableShopItem = "CoeurDeVie";
         displayPurchaseText(other, "Press B to buy Coeur De Vie\nHealth Refill and +" + itemManager.Grab<CoeurDeVie>().getHealthBonus() + " Health" + "\n$" + shopPrices[purchasableShopItem]);
      }
      else if (other.gameObject.CompareTag("Shop_HealthRefill"))
      {
         purchasableShopItem = "HealthRefill";
         displayPurchaseText(other, "Press B to buy Health Refill\n$" + shopPrices[purchasableShopItem]);
      }
      else if (other.gameObject.CompareTag("Shop_Freeze"))
      {
         purchasableShopItem = "Freeze";
         displayPurchaseText(other, "Press B to buy Freeze\nFreeze enemies for 5 seconds\n(Single use)\n$" + shopPrices[purchasableShopItem]);
      }
      else if (other.gameObject.CompareTag("Shop_WingsOfVoice"))
      {
         purchasableShopItem = "WingsOfVoice";
         displayPurchaseText(other, "Press B to buy Wings Of Voice\nShort Ranged Dash\n$" + shopPrices[purchasableShopItem]);
      }
      else if (other.gameObject.CompareTag("Shop_AdrenalineShot"))
      {
         purchasableShopItem = "AdrenalineShot";
         displayPurchaseText(other, "Press B to buy Adrenaline Shot\n+20% DMG\n$" + shopPrices[purchasableShopItem]);
      }
      else if (other.gameObject.CompareTag("Shop_ProteinPowder"))
      {
         purchasableShopItem = "ProteinPowder";
         displayPurchaseText(other, "Press B to buy Protein Powder\n+40% DMG\n$" + shopPrices[purchasableShopItem]);
      }
      else if (other.gameObject.CompareTag("Shop_FinalDestination"))
      {
         purchasableShopItem = "FinalDestination";
         displayPurchaseText(other, "Press B to buy Final Destination\n10% Crit Chance\n$" + shopPrices[purchasableShopItem]);
      }
      else if (other.gameObject.CompareTag("Shop_SwiftExecution"))
      {
         purchasableShopItem = "SwiftExecution";
         displayPurchaseText(other, "Press B to buy Swift Execution\n+15% RPM\n$" + shopPrices[purchasableShopItem]);
      }
      else if (other.gameObject.CompareTag("Shop_ApollyonsPit"))
      {
         purchasableShopItem = "ApollyonsPit";
         displayPurchaseText(other, "Press B to buy Apollyon's Pit\n+100% RPM -35% DMG\n$" + shopPrices[purchasableShopItem]);
      }
      else if (other.gameObject.CompareTag("Shop_PumpedUpKicks"))
      {
         purchasableShopItem = "PumpedUpKicks";
         displayPurchaseText(other, "Press B to buy Pumped Up Kicks\n+5% Player Speed\n$" + shopPrices[purchasableShopItem]);
      }
      else if (other.gameObject.CompareTag("Shop_DoubleTap"))
      {
         purchasableShopItem = "DoubleTap";
         displayPurchaseText(other, "Press B to buy Double Tap\n2 Bullets for 1 Shot, -30% DMG\n$" + shopPrices[purchasableShopItem]);
      }
      else if (other.gameObject.CompareTag("Shop_CannonballSplash"))
      {
         purchasableShopItem = "CannonballSplash";
         displayPurchaseText(other, "Press B to buy Cannonball Splash\nIncreased Bullet Size, +10% DMG\n$" + shopPrices[purchasableShopItem]);
      }
      else if (other.gameObject.CompareTag("Shop_GungHoGloves"))
      {
         purchasableShopItem = "GungHoGloves";
         displayPurchaseText(other, "Press B to buy Gung-Ho Gloves\n25% Faster Reload Speed\n$" + shopPrices[purchasableShopItem]);
      }
      else if (other.gameObject.CompareTag("Shop_AK-47"))
      {
         purchasableShopItem = "AK-47";
         displayPurchaseText(other, "Press B to equip AK-47");
      }
      else if (other.gameObject.CompareTag("Shop_Bennelli M4"))
      {
         purchasableShopItem = "Bennelli M4";
         displayPurchaseText(other, "Press B to equip Bennelli M4");
      }
      else if (other.gameObject.CompareTag("Shop_M249"))
      {
         purchasableShopItem = "M249";
         displayPurchaseText(other, "Press B to equip M249");
      }
      else if (other.gameObject.CompareTag("Shop_Uzi"))
      {
         purchasableShopItem = "Uzi";
         displayPurchaseText(other, "Press B to equip Uzi");
      }
      else if (other.gameObject.CompareTag("Shop_M48"))
      {
         purchasableShopItem = "M48";
         displayPurchaseText(other, "Press B to equip M48");
      }
      else if (other.gameObject.CompareTag("Shop_M107"))
      {
         purchasableShopItem = "M107";
         displayPurchaseText(other, "Press B to equip M107");
      }
      else if (other.gameObject.CompareTag("ExitDoor"))
      {
         purchasableShopItem = "EXIT";
         displayPurchaseText(other, "Press E to go to next level");
      }
   }

   private void displayPurchaseText(Collider other, string purchaseItemTextStr)
   {
      shopItemLookAt = other;
      purchaseItemText.enabled = true;
      purchaseItemText.text = purchaseItemTextStr;
   }

   private void buyShopItem()
   {
      balance -= shopPrices[purchasableShopItem];
      SetCurrentBalance();
      playerAudio.PlayPurchaseSound();
   }
}