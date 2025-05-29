using System; // For DateTime
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for UI components like Slider
using TMPro; // Required for TextMeshPro components
using UnityEngine.InputSystem;
using System.Runtime.CompilerServices; // Added for Input System

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    public static PlayerController Instance;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator; // Make sure this is assigned in the Inspector

    [SerializeField] private float moveSpeed;
    public Vector3 playerMoveDirection;
    public float playerMaxHealth;
    public float playerHealth;

    public int experience;
    public int currentLevel;
    public int maxLevel;
    public List<int> playerLevels;

    private bool isImmune;
    [SerializeField] private float immunityDuration;
    [SerializeField] private float immunityTimer;

    public Weapon activeWeapon; // Assuming you have a Weapon script

    // Rewarded Ads Settings
    [Header("Rewarded Ads Settings")]
    [SerializeField] private int maxAdsPerDay = 10; // Maximum ads allowed per day
    private int currentAdsWatchedToday; // Ads watched today
    private DateTime lastAdWatchDate; // Last date an ad was watched

    public PlayerInput playerControl; // Ensure this is linked to your Input Actions Asset in Inspector
    private InputAction move;
    private InputAction pause;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy this GameObject if another instance already exists
        }
        else
        {
            Instance = this;
            // If PlayerController needs to persist across scenes (e.g., Main Menu -> Game)
            // Uncomment the line below:
            // DontDestroyOnLoad(gameObject);
        }
        playerControl = new PlayerInput(); // Initialize the PlayerInput asset
    }

    void OnEnable()
    {
        pause = playerControl.Player.Pause;
        pause.Enable();
        pause.performed += ctx => PauseGame();
        move = playerControl.Player.Move; // Get the 'Move' action from the 'Player' action map
        move.Enable(); // Enable the 'Move' action

        // Debug Log to check when PlayerController GameObject becomes active
        Debug.Log("PlayerController: OnEnable called. Subscribing to OnRewardedAdCompleted.");
        // Subscribe to the event from AdsInitializer when this GameObject is enabled
        AdsInitializer.OnRewardedAdCompleted += RewardPlayerForAd;
    }

    void OnDisable()
    {
        pause.Disable();
        move.Disable(); // Disable the 'Move' action when the GameObject is disabled

        // Debug Log to check when PlayerController GameObject becomes inactive
        Debug.Log("PlayerController: OnDisable called. Unsubscribing from OnRewardedAdCompleted.");
        // Unsubscribe from the event when this GameObject is disabled or destroyed
        AdsInitializer.OnRewardedAdCompleted -= RewardPlayerForAd;
    }

    void Start()
    {
        // Player level setup (if not already done)
        for (int i = playerLevels.Count; i < maxLevel; i++)
        {
            playerLevels.Add(Mathf.CeilToInt(playerLevels[playerLevels.Count - 1] * 1.1f + 15));
        }
        playerHealth = playerMaxHealth;
        UIController.Instance.UpdateHealthSlider();
        UIController.Instance.UpdateExperienceSlider();

        LoadAdWatchData(); // Load ad watch data when the game starts
        // Update the Ad UI immediately after loading data and when the game starts
        UIController.Instance.UpdateAdsRemainingText(currentAdsWatchedToday, maxAdsPerDay);
        UIController.Instance.SetRewardedAdButtonState(currentAdsWatchedToday < maxAdsPerDay);
    }

    void Update()
    {
        
        playerMoveDirection = move.ReadValue<Vector2>(); 

        
        animator.SetFloat("moveX", playerMoveDirection.x);
        animator.SetFloat("moveY", playerMoveDirection.y);

        
        if (playerMoveDirection == Vector3.zero)
        {
            animator.SetBool("moving", false);
        }
        else
        {
            animator.SetBool("moving", true);
        }

        // Immunity/Invincibility code
        if (immunityTimer > 0)
        {
            immunityTimer -= Time.deltaTime;
        }
        else
        {
            isImmune = false;
        }
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector3(playerMoveDirection.x * moveSpeed, playerMoveDirection.y * moveSpeed);
    }

    public void TakeDamage(float damage)
    {
        if (!isImmune)
        {
            isImmune = true;
            immunityTimer = immunityDuration;
            playerHealth -= damage;
            UIController.Instance.UpdateHealthSlider();
            if (playerHealth <= 0)
            {
                playerHealth = 0;
                UIController.Instance.UpdateHealthSlider(); // Update UI again to show 0 health

                

                GameManager.Instance.GameOver(); // Notify GameManager that game is over
            }
        }
    }

    public void GetExperience(int experienceToGet)
    {
        experience += experienceToGet;
        UIController.Instance.UpdateExperienceSlider();
        if (currentLevel < maxLevel && experience >= playerLevels[currentLevel - 1])
        {
            LevelUp();
        }
        else if (currentLevel >= maxLevel)
        {
            Debug.Log("Max level reached! Gaining extra gold or other rewards.");
        }
    }

    public void LevelUp()
    {
        experience -= playerLevels[currentLevel - 1];
        currentLevel++;
        UIController.Instance.UpdateExperienceSlider();
        if (UIController.Instance.levelUpButtons != null && UIController.Instance.levelUpButtons.Length > 0 && UIController.Instance.levelUpButtons[0] != null)
        {
            UIController.Instance.levelUpButtons[0].ActivateButton(activeWeapon);
        }
        else
        {
            Debug.LogWarning("LevelUpButton array is empty or element 0 is null in UIController. Cannot activate level up button.");
        }
        UIController.Instance.LevelUpPanelOpen();
    }

    // ===============================================
    // Rewarded Ads Logic (for PlayerController)
    // ===============================================

    // This method is linked to the "Watch Ad for Health" button in UI
    public void TryShowRewardedAdForHealth()
    {
        Debug.Log("PlayerController: TryShowRewardedAdForHealth called.");

        // Check if it's a new day (to reset ad count)
        if (lastAdWatchDate.Date < DateTime.Now.Date)
        {
            Debug.Log("PlayerController: New day detected. Resetting ad count.");
            currentAdsWatchedToday = 0; // Reset ad count if it's a new day
            lastAdWatchDate = DateTime.Now.Date; // Update last watched date to current date
            SaveAdWatchData(); // Save the reset count immediately
            // Update UI to reflect the reset count
            UIController.Instance.UpdateAdsRemainingText(currentAdsWatchedToday, maxAdsPerDay);
            UIController.Instance.SetRewardedAdButtonState(currentAdsWatchedToday < maxAdsPerDay);
        }

        // Check if ads can still be watched today (not exceeding daily limit)
        if (currentAdsWatchedToday < maxAdsPerDay)
        {
            Debug.Log($"PlayerController: Ads watched today: {currentAdsWatchedToday}/{maxAdsPerDay}. Attempting to show ad.");
            AdsInitializer adsInitializer = FindObjectOfType<AdsInitializer>(); // Find AdsInitializer in the scene
            if (adsInitializer != null)
            {
                adsInitializer.ShowRewardedAd(); // Order AdsInitializer to show the ad
                // UIController.Instance.SetRewardedAdButtonState(false); // Optionally disable button while ad is showing
            }
            else
            {
                Debug.LogError("PlayerController: AdsInitializer not found in the scene! Cannot show ad.");
                UIController.Instance.ShowAdLimitMessage(); // Show a message to the player
            }
        }
        else
        {
            Debug.Log("PlayerController: Reached daily ad limit for health reward. Showing ad limit message.");
            UIController.Instance.ShowAdLimitMessage(); // Show message when limit is reached
            UIController.Instance.SetRewardedAdButtonState(false); // Disable the button
        }
    }

    // This method is called by an Event from AdsInitializer when a Rewarded Ad completes (COMPLETED state)
    private void RewardPlayerForAd()
    {
        Debug.Log("RewardPlayerForAd method started. Giving reward now!");

        // Check health values before rewarding
        Debug.Log($"PlayerController: Current Health before reward: {playerHealth}");
        Debug.Log($"PlayerController: Max Health: {playerMaxHealth}");

        // Reward: Restore 50% of max health (added to current health)
        // Ensure playerMaxHealth is not 0 to avoid division by zero or calculation issues
        if (playerMaxHealth > 0)
        {
            // This line adds 50% of max health to current health, capped at max health
            playerHealth = Mathf.Min(playerMaxHealth, playerHealth + (playerMaxHealth * 0.5f));
        }
        else
        {
            // Fallback if playerMaxHealth is 0 (e.g., set to a default value)
            playerHealth = 100; // Example default health
            Debug.LogWarning("PlayerController: playerMaxHealth is 0, setting health to default 100.");
        }

        UIController.Instance.UpdateHealthSlider(); // Update health bar UI

        Debug.Log($"PlayerController: Health after reward: {playerHealth}");

        currentAdsWatchedToday++; // Increment ads watched today
        SaveAdWatchData(); // Save data

        Debug.Log($"PlayerController: Ads watched today: {currentAdsWatchedToday}");

        // Update Ad UI after rewarding
        UIController.Instance.UpdateAdsRemainingText(currentAdsWatchedToday, maxAdsPerDay);
        UIController.Instance.SetRewardedAdButtonState(currentAdsWatchedToday < maxAdsPerDay);

        // *** IMPORTANT: Call GameManager to revive the player ***
        GameManager.Instance.RevivePlayer();

        Debug.Log("RewardPlayerForAd method completed.");
    }

    // This method is called by GameManager when game is over, to update Ad UI on Game Over screen
    public void UpdateAdUIOnGameOver()
    {
        Debug.Log("PlayerController: UpdateAdUIOnGameOver called.");
        // Check and reset ad count if it's a new day
        if (lastAdWatchDate.Date < DateTime.Now.Date)
        {
            Debug.Log("PlayerController: New day detected in UpdateAdUIOnGameOver. Resetting ad count.");
            currentAdsWatchedToday = 0;
            lastAdWatchDate = DateTime.Now.Date;
            SaveAdWatchData();
        }
        UIController.Instance.UpdateAdsRemainingText(currentAdsWatchedToday, maxAdsPerDay);
        UIController.Instance.SetRewardedAdButtonState(currentAdsWatchedToday < maxAdsPerDay);
        // UIController.Instance.adLimitMessagePanel.SetActive(false); // This might not be needed here if handled by HideAdLimitMessageAfterDelay
    }

    // ===============================================
    // Save/Load Data (using PlayerPrefs)
    // ===============================================

    private void SaveAdWatchData()
    {
        PlayerPrefs.SetInt("AdsWatchedToday", currentAdsWatchedToday);
        // Save date in a readable format (yyyy-MM-dd)
        PlayerPrefs.SetString("LastAdWatchDate", lastAdWatchDate.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();
        Debug.Log($"PlayerController: Ad watch data saved: Count={currentAdsWatchedToday}, Date={lastAdWatchDate.ToShortDateString()}");
    }

    private void LoadAdWatchData()
    {
        currentAdsWatchedToday = PlayerPrefs.GetInt("AdsWatchedToday", 0);
        string dateString = PlayerPrefs.GetString("LastAdWatchDate", DateTime.MinValue.ToString("yyyy-MM-dd"));

        Debug.Log($"PlayerController: Raw loaded data - DateString='{dateString}', AdsWatchedToday={currentAdsWatchedToday}");

        // Try to parse the string back to DateTime
        if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out lastAdWatchDate))
        {
            // If the loaded date is older than today, reset ad count
            if (lastAdWatchDate.Date < DateTime.Now.Date)
            {
                Debug.Log("PlayerController: Loaded date is older than today. Resetting ad count.");
                currentAdsWatchedToday = 0;
                lastAdWatchDate = DateTime.Now.Date; // Update to current date
                SaveAdWatchData(); // Save the reset
            }
            Debug.Log($"PlayerController: Ad watch data loaded: Count={currentAdsWatchedToday}, Date={lastAdWatchDate.ToShortDateString()}");
        }
        else
        {
            // If date parsing fails (e.g., first play), initialize default values
            Debug.Log("PlayerController: Failed to parse last ad watch date or first play. Initializing data.");
            lastAdWatchDate = DateTime.Now.Date;
            currentAdsWatchedToday = 0;
            SaveAdWatchData();
        }
        
    }
    private void PauseGame()
    {
        gameManager.Pause();
    }
}