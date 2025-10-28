using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement; // For loading different scene maps per round
using TMPro; // For updating TextMeshPro UI elements

// Manages the entire round progression system, anomaly spawning, and scene transitions
public class RoundManager : MonoBehaviour
{
    [Header("Round Configuration")]
    [SerializeField] private int totalRounds = 5;
    [SerializeField] private int currentRound = 1;

    [Header("Round 1: Easy")]
    [Tooltip("Number of anomalies to activate in Round 1")]
    [SerializeField] private int round1AnomalyCount = 2;
    [Tooltip("Seconds between each anomaly spawn")]
    [SerializeField] private float round1SpawnInterval = 60f;
    [Tooltip("Delay before first anomaly spawns (gives player time to prepare)")]
    [SerializeField] private float round1StartDelay = 30f;

    [Header("Round 2: Medium")]
    [SerializeField] private int round2AnomalyCount = 3;
    [SerializeField] private float round2SpawnInterval = 50f;
    [SerializeField] private float round2StartDelay = 20f;

    [Header("Round 3: Hard")]
    [SerializeField] private int round3AnomalyCount = 4;
    [SerializeField] private float round3SpawnInterval = 40f;
    [SerializeField] private float round3StartDelay = 15f;

    [Header("Round 4: Very Hard")]
    [SerializeField] private int round4AnomalyCount = 5;
    [SerializeField] private float round4SpawnInterval = 30f;
    [SerializeField] private float round4StartDelay = 10f;

    [Header("Round 5: Nightmare")]
    [SerializeField] private int round5AnomalyCount = 6;
    [SerializeField] private float round5SpawnInterval = 20f;
    [SerializeField] private float round5StartDelay = 5f;

    [Header("Scene References")]
    [SerializeField] private GameClockController gameClock;
    [SerializeField] private SegmentBattery battery;
    [SerializeField] private GameObject roundTransitionPanel;  // "ROUND 2 STARTING..." UI
    [SerializeField] private GameObject finalVictoryPanel;     // "YOU WON!" UI
    [SerializeField] private GameObject summaryReportPanel;
    [SerializeField] private GameObject TabletUIRoot;
    [SerializeField] private GameObject AnomalyTimerPanel;
    [SerializeField] private SummaryReportManager summaryManager;

    [Header("Scene Names")]
    [Tooltip("Leave empty to stay in current scene")]
    [SerializeField] private string round3SceneName = "Round 3 and 4 Scene";
    [Tooltip("Leave empty to stay in current scene")]
    [SerializeField] private string round5SceneName = "Round 5 Map";
    [Tooltip("Name of the main menu scene")]
    [SerializeField] private string mainMenuSceneName = "MainGame";

    [Header("Player Reset")]
    [Tooltip("Drag your player Transform here. If empty, will try to find it automatically.")]
    [SerializeField] private Transform playerTransform;
    [Tooltip("Should the player's rotation also reset between rounds?")]
    [SerializeField] private bool resetRotation = true;

    [Header("Disable During Transition")]
    [Tooltip("Drag scripts that should be disabled during round transition (e.g., FirstPersonMover, MouseLook, TabletPanelController)")]
    [SerializeField] private Behaviour[] disableDuringTransition;
    [Tooltip("Should the cursor be locked and hidden during transition?")]
    [SerializeField] private bool lockCursorDuringTransition = true;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    // Internal state tracking for round management
    private List<IAnomaly> allAnomalies = new List<IAnomaly>();
    private List<IAnomaly> activeAnomaliesThisRound = new List<IAnomaly>();
    private Coroutine spawnRoutine;
    private bool roundInProgress = false;
    private bool summaryShownThisRound = false;
    private bool isTransitioning = false; // Track if currently transitioning between rounds

    // Store the player's starting position and rotation for resetting between rounds
    private Vector3 playerStartPosition;
    private Quaternion playerStartRotation;

    // Store cursor state before transition so we can restore it later
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;

    // Called before Start() - used for finding all anomalies and setting up references
    void Awake()
    {
        // Find all anomalies in the scene (including disabled ones)
        var allBehaviours = FindObjectsOfType<MonoBehaviour>(true);
        allAnomalies.Clear();

        foreach (var mb in allBehaviours)
        {
            if (mb is IAnomaly a)
            {
                allAnomalies.Add(a);
            }
        }

        if (debugMode) Debug.Log($"[RoundManager] Found {allAnomalies.Count} anomalies in scene.");

        // Auto-find important references if they weren't set in the Inspector
        if (!gameClock) gameClock = FindObjectOfType<GameClockController>();
        if (!battery) battery = FindObjectOfType<SegmentBattery>();

        // Try to find the player GameObject automatically if not assigned
        if (!playerTransform)
        {
            // Try multiple common player names and tags
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (!player) player = GameObject.Find("FINALPLAYER");
            if (!player) player = GameObject.Find("Player");
            if (!player) player = GameObject.Find("FPSController");

            if (player)
            {
                playerTransform = player.transform;
                if (debugMode) Debug.Log($"[RoundManager] Auto-found player: {player.name}");
            }
            else
            {
                Debug.LogWarning("[RoundManager] Could not find player! Drag your player into the 'Player Transform' field.");
            }
        }

        // Save the player's starting position so we can teleport them back each round
        if (playerTransform)
        {
            playerStartPosition = playerTransform.position;
            playerStartRotation = playerTransform.rotation;
            if (debugMode) Debug.Log($"[RoundManager] Stored player start position: {playerStartPosition}");
        }

        // Make sure UI panels are hidden at the start
        if (roundTransitionPanel) roundTransitionPanel.SetActive(false);
        if (finalVictoryPanel) finalVictoryPanel.SetActive(false);
    }

    // Called after Awake() - kicks off the first round
    void Start()
    {
        // Start at the current round (respects Inspector setting for each scene)
        StartRound(currentRound);
    }

    /// <summary>
    /// Called by GameClockController when the in-game time reaches 6:00 AM.
    /// This handles round completion, anomaly cleanup, and progression to next round or victory.
    /// </summary>
    public void OnRoundTimeComplete()
    {
        if (debugMode) Debug.Log($"[RoundManager] Round {currentRound} time complete!");

        // Stop spawning any more anomalies for this round
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        // Clean up any active anomalies by reverting them back to normal
        foreach (var anomaly in activeAnomaliesThisRound)
        {
            if (anomaly.IsActive)
            {
                anomaly.Revert();
            }
        }
        activeAnomaliesThisRound.Clear();

        roundInProgress = false;

        // If this was the last round, show the victory screen instead of continuing
        if (currentRound >= totalRounds && !isTransitioning)
        {
            ShowFinalVictory();
            return;
        }

        // Hide the UI elements before showing the summary
        if (TabletUIRoot)
        {
            TabletUIRoot.SetActive(false);
        }

        if (AnomalyTimerPanel)
        {
            AnomalyTimerPanel.SetActive(false);
        }

        // Show summary panel
        if (summaryReportPanel)
        {
            summaryReportPanel.SetActive(true);

            // Make it cover everything (in case it�s not already)
            var canvas = summaryReportPanel.GetComponent<Canvas>();
            if (canvas)
            {
                canvas.sortingOrder = 999; // Force it above everything
            }

            // Block all raycasts beneath it (so old UI can�t be clicked)
            var group = summaryReportPanel.GetComponent<CanvasGroup>();
            if (!group) group = summaryReportPanel.AddComponent<CanvasGroup>();
            group.blocksRaycasts = true;
            group.interactable = true;
            group.alpha = 1f;

            // Pause the game & unlock cursor
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void OnContinueFromSummary()
    {
        if (summaryReportPanel) summaryReportPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentRound++;
        isTransitioning = true;
        StartCoroutine(TransitionToNextRound());
    }

    /// <summary>
    /// Initializes and starts a new round with the specified difficulty settings.
    /// Resets all game state, selects random anomalies, and begins spawning them.
    /// </summary>
    private void StartRound(int roundNumber)
    {
        summaryReportPanel.SetActive(false);

        AnomalyTimerPanel.SetActive(true);

        // Make sure the game isn't paused
        Time.timeScale = 1f;

        currentRound = roundNumber;
        roundInProgress = true;
        isTransitioning = false; // Round has started, we're no longer in transition

        // Reset the summary manager's anomaly counters
        if (summaryManager)
        {
            summaryManager.ResetCounts();
        }

        // Turn all gameplay controls back on
        EnableGameplayControls();

        // Teleport the player back to their starting position
        ResetPlayerPosition();

        // Get the difficulty settings for this specific round
        int anomalyCount;
        float spawnInterval;
        float startDelay;

        switch (currentRound)
        {
            case 1:
                anomalyCount = round1AnomalyCount;
                spawnInterval = round1SpawnInterval;
                startDelay = round1StartDelay;
                break;
            case 2:
                anomalyCount = round2AnomalyCount;
                spawnInterval = round2SpawnInterval;
                startDelay = round2StartDelay;
                break;
            case 3:
                anomalyCount = round3AnomalyCount;
                spawnInterval = round3SpawnInterval;
                startDelay = round3StartDelay;
                break;
            case 4:
                anomalyCount = round4AnomalyCount;
                spawnInterval = round4SpawnInterval;
                startDelay = round4StartDelay;
                break;
            case 5:
                anomalyCount = round5AnomalyCount;
                spawnInterval = round5SpawnInterval;
                startDelay = round5StartDelay;
                break;
            default:
                anomalyCount = round5AnomalyCount;
                spawnInterval = round5SpawnInterval;
                startDelay = round5StartDelay;
                break;
        }

        // Clear out the list of active anomalies from the previous round
        activeAnomaliesThisRound.Clear();

        if (debugMode) Debug.Log($"[RoundManager] Starting Round {currentRound} - {anomalyCount} anomalies, {spawnInterval}s interval.");

        // Give the player full battery to start the round
        if (battery)
        {
            battery.Refill(battery.Total);
        }

        // Reset the clock back to midnight (12:00 AM)
        if (gameClock)
        {
            gameClock.SetTime(0, 0);  // 12:00 AM
        }

        // Randomly select which anomalies will be active this round
        activeAnomaliesThisRound.Clear();
        var shuffledOrder = allAnomalies.OrderBy(x => Random.value).ToList();
        for (int i = 0; i < anomalyCount && i < shuffledOrder.Count; i++)
        {
            activeAnomaliesThisRound.Add(shuffledOrder[i]);
        }

        // Start the coroutine that spawns anomalies over time
        spawnRoutine = StartCoroutine(SpawnAnomaliesRoutine(spawnInterval, startDelay));
    }

    /// <summary>
    /// Teleports the player back to their starting position and rotation.
    /// Handles CharacterController components properly by disabling them first.
    /// </summary>
    private void ResetPlayerPosition()
    {
        if (!playerTransform)
        {
            if (debugMode) Debug.LogWarning("[RoundManager] Cannot reset player position - no player assigned!");
            return;
        }

        // CharacterControllers need to be disabled before teleporting, otherwise they fight the position change
        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc)
        {
            cc.enabled = false;
            playerTransform.position = playerStartPosition;
            if (resetRotation) playerTransform.rotation = playerStartRotation;
            cc.enabled = true;
        }
        else
        {
            // No CharacterController, so we can just set position directly
            playerTransform.position = playerStartPosition;
            if (resetRotation) playerTransform.rotation = playerStartRotation;
        }

        if (debugMode) Debug.Log($"[RoundManager] Reset player to starting position: {playerStartPosition}");
    }

    /// <summary>
    /// Disables player movement and interaction during round transitions.
    /// Stores the current cursor state so we can restore it later.
    /// </summary>
    private void DisableGameplayControls()
    {
        if (debugMode) Debug.Log("[RoundManager] Disabling gameplay controls during transition...");

        // Turn off all the gameplay scripts (movement, tablet controls, etc.)
        if (disableDuringTransition != null)
        {
            foreach (var b in disableDuringTransition)
            {
                if (b) b.enabled = false;
            }
        }

        // Save the current cursor settings and lock it during the transition
        if (lockCursorDuringTransition)
        {
            previousCursorLockMode = Cursor.lockState;
            previousCursorVisible = Cursor.visible;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// Re-enables all gameplay controls after a round transition completes.
    /// Restores the cursor to its previous state.
    /// </summary>
    private void EnableGameplayControls()
    {
        if (debugMode) Debug.Log("[RoundManager] Re-enabling gameplay controls...");

        // Turn all the gameplay scripts back on
        if (disableDuringTransition != null)
        {
            foreach (var b in disableDuringTransition)
            {
                if (b) b.enabled = true;
            }
        }

        // Restore the cursor to however it was before the transition
        if (lockCursorDuringTransition)
        {
            Cursor.lockState = previousCursorLockMode;
            Cursor.visible = previousCursorVisible;
        }
    }

    /// <summary>
    /// Coroutine that spawns anomalies at regular intervals throughout the round.
    /// Shuffles the order each cycle to keep things unpredictable.
    /// </summary>
    private IEnumerator SpawnAnomaliesRoutine(float interval, float startDelay)
    {
        // Give the player some time to get ready before the first anomaly appears
        if (startDelay > 0)
        {
            if (debugMode) Debug.Log($"[RoundManager] Waiting {startDelay}s before spawning first anomaly...");
            yield return new WaitForSeconds(startDelay);
        }

        if (debugMode) Debug.Log($"[RoundManager] Starting anomaly spawns now!");

        // Randomize the order so anomalies don't appear in the same sequence every time
        var shuffledOrder = activeAnomaliesThisRound.OrderBy(x => Random.value).ToList();

        // Trigger each anomaly once with delays between them
        foreach (var anomaly in shuffledOrder)
        {
            if (!roundInProgress) yield break; // Stop if round ended early

            anomaly.Trigger();

            if (debugMode) Debug.Log($"[RoundManager] Triggered {anomaly.Room} - {anomaly.Type}");

            // Wait before triggering the next anomaly
            yield return new WaitForSeconds(interval);
        }

        // After all anomalies have been triggered once, keep cycling through them
        while (roundInProgress)
        {
            // Re-shuffle for variety
            shuffledOrder = activeAnomaliesThisRound.OrderBy(x => Random.value).ToList();

            foreach (var anomaly in shuffledOrder)
            {
                if (!roundInProgress) yield break; // Stop if round ended

                // Only trigger if it's not already active (player might not have reported it yet)
                if (!anomaly.IsActive)
                {
                    anomaly.Trigger();
                    if (debugMode) Debug.Log($"[RoundManager] Re-triggered {anomaly.Room} - {anomaly.Type}");
                }

                // Wait before the next one
                yield return new WaitForSeconds(interval);
            }
        }
    }

    /// <summary>
    /// Updates the transition panel's text to show the current round number.
    /// Supports both TextMeshPro and legacy Unity Text components.
    /// </summary>
    private void UpdateTransitionText()
    {
        if (!roundTransitionPanel) return;

        // Try TextMeshPro first (newer UI system)
        var tmpText = roundTransitionPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText)
        {
            tmpText.text = $"ROUND {currentRound} STARTING...";
            return;
        }

        // Fall back to legacy Text component if TextMeshPro isn't found
        var legacyText = roundTransitionPanel.GetComponentInChildren<UnityEngine.UI.Text>();
        if (legacyText)
        {
            legacyText.text = $"ROUND {currentRound} STARTING...";
        }
    }

    /// <summary>
    /// Coroutine that handles the transition between rounds.
    /// Shows a transition screen, optionally loads a new scene, then starts the next round.
    /// </summary>
    private IEnumerator TransitionToNextRound()
    {
        if (debugMode) Debug.Log($"[RoundManager] Showing transition to Round {currentRound}...");

        // Disable player controls during the transition
        DisableGameplayControls();
        
        // CRITICAL: Hide victory panel during transition to prevent it showing when entering Round 5
        if (finalVictoryPanel) finalVictoryPanel.SetActive(false);

        // Check if we need to load a different scene for this round
        string targetScene = GetSceneForRound(currentRound);
        if (!string.IsNullOrEmpty(targetScene) && targetScene != SceneManager.GetActiveScene().name)
        {
            if (debugMode) Debug.Log($"[RoundManager] Loading scene: {targetScene}");
            
            // Update and show the transition text
            UpdateTransitionText();
            if (roundTransitionPanel) roundTransitionPanel.SetActive(true);

            // Wait 3 seconds (uses realtime so it works even if game is paused)
            yield return new WaitForSecondsRealtime(3f);

            // Load the new scene (this will reset everything)
            SceneManager.LoadScene(targetScene);
            yield break; // Exit coroutine since the scene is reloading
        }

        // If not changing scenes, just show the transition screen
        UpdateTransitionText();
        if (roundTransitionPanel) roundTransitionPanel.SetActive(true);

        // Wait 3 seconds for the player to see the transition
        yield return new WaitForSecondsRealtime(3f);

        // Hide the transition and start the round
        if (roundTransitionPanel) roundTransitionPanel.SetActive(false);

        if (debugMode) Debug.Log($"[RoundManager] Starting Round {currentRound} now!");

        // Start the actual round (this will re-enable controls)
        StartRound(currentRound);
    }

    /// <summary>
    /// Returns the scene name that should be loaded for a specific round number.
    /// Returns empty string if the round should stay in the current scene.
    /// </summary>
    private string GetSceneForRound(int round)
    {
        switch (round)
        {
            case 3:
                return round3SceneName;  // Rounds 3-4 use the same scene
            case 5:
                return round5SceneName;  // Round 5 has its own special map
            default:
                return string.Empty; // All other rounds stay in the current scene
        }
    }

    /// <summary>
    /// Shows the final victory screen after completing all rounds.
    /// Pauses the game and shows the cursor so player can click buttons.
    /// </summary>
    private void ShowFinalVictory()
    {
        if (debugMode) Debug.Log("[RoundManager] FINAL VICTORY!");

        // Make sure the transition panel doesn't show behind the victory screen
        if (roundTransitionPanel) roundTransitionPanel.SetActive(false);

        // Show the victory panel
        if (finalVictoryPanel) finalVictoryPanel.SetActive(true);

        // Disable gameplay controls so player can't move around
        DisableGameplayControls();

        // Freeze the game
        Time.timeScale = 0f;

        // Show cursor so player can click the Main Menu button
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// For debugging - manually advance to next round
    /// </summary>
    [ContextMenu("Skip to Next Round")]
    public void DebugSkipToNextRound()
    {
        OnRoundTimeComplete();
    }

    /// <summary>
    /// Called by the Main Menu button on the victory screen.
    /// Unpauses the game and loads the main menu scene.
    /// </summary>
    public void ReturnToMainMenu()
    {
        if (debugMode) Debug.Log("[RoundManager] Returning to main menu...");

        // Unpause the game before changing scenes
        Time.timeScale = 1f;

        // Load the main menu scene
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("[RoundManager] Main Menu Scene Name is not set! Please set it in the RoundManager Inspector.");
        }
    }
}