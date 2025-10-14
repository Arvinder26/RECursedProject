using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [Header("Round Configuration")]
    [SerializeField] private int totalRounds = 5;
    [SerializeField] private int currentRound = 1;

    [Header("Round 1: Easy")]
    [Tooltip("How many anomalies to activate in Round 1")]
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
    [SerializeField] private GameObject roundTransitionPanel; // "ROUND 2 STARTING..." UI
    [SerializeField] private GameObject finalVictoryPanel;    // "YOU WON!" UI
    
    [Header("Player Reset")]
    [Tooltip("Drag your player GameObject here. If empty, will try to find it automatically.")]
    [SerializeField] private Transform playerTransform;
    [Tooltip("Should the player's rotation also reset between rounds?")]
    [SerializeField] private bool resetRotation = true;

    [Header("Disable During Transition")]
    [Tooltip("Drag scripts here to disable during round transition (e.g., FirstPersonMover, MouseLook, TabletPanelController).")]
    [SerializeField] private Behaviour[] disableDuringTransition;
    [Tooltip("Should the cursor be locked and hidden during transition?")]
    [SerializeField] private bool lockCursorDuringTransition = true;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    // Internal state
    private List<IAnomaly> allAnomalies = new List<IAnomaly>();
    private List<IAnomaly> activeAnomaliesThisRound = new List<IAnomaly>();
    private Coroutine spawnRoutine;
    private bool roundInProgress = false;
    
    // Store the player's starting position and rotation
    private Vector3 playerStartPosition;
    private Quaternion playerStartRotation;
    
    // Store cursor state before transition
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;

    void Awake()
    {
        // Find all anomalies in the scene
        var allBehaviours = FindObjectsOfType<MonoBehaviour>(true);
        allAnomalies.Clear();
        
        foreach (var mb in allBehaviours)
        {
            if (mb is IAnomaly a)
                allAnomalies.Add(a);
        }

        if (debugMode) Debug.Log($"[RoundManager] Found {allAnomalies.Count} anomalies in scene.");

        // Auto-find references if not set
        if (!gameClock) gameClock = FindObjectOfType<GameClockController>();
        if (!battery) battery = FindObjectOfType<SegmentBattery>();

        // Auto-find player if not assigned
        if (!playerTransform)
        {
            // Try to find by common player names or tags
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

        // Store the player's initial position and rotation
        if (playerTransform)
        {
            playerStartPosition = playerTransform.position;
            playerStartRotation = playerTransform.rotation;
            if (debugMode) Debug.Log($"[RoundManager] Stored player start position: {playerStartPosition}");
        }

        // Hide panels
        if (roundTransitionPanel) roundTransitionPanel.SetActive(false);
        if (finalVictoryPanel) finalVictoryPanel.SetActive(false);
    }

    void Start()
    {
        // Start Round 1 automatically
        StartRound(1);
    }

    /// <summary>
    /// Called by GameClockController when time reaches 6:00 AM
    /// </summary>
    public void OnRoundTimeComplete()
    {
        if (debugMode) Debug.Log($"[RoundManager] Round {currentRound} time complete!");

        // Stop spawning anomalies
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        // Revert all active anomalies
        foreach (var anomaly in activeAnomaliesThisRound)
        {
            if (anomaly.IsActive)
                anomaly.Revert();
        }
        activeAnomaliesThisRound.Clear();

        roundInProgress = false;

        // Check if this was the final round
        if (currentRound >= totalRounds)
        {
            ShowFinalVictory();
        }
        else
        {
            // Move to next round
            currentRound++;
            StartCoroutine(TransitionToNextRound());
        }
    }

    private void StartRound(int roundNumber)
    {
        // Ensure game is unpaused
        Time.timeScale = 1f;
        
        currentRound = roundNumber;
        roundInProgress = true;

        // Re-enable all gameplay controls
        EnableGameplayControls();

        // Reset player position to starting position
        ResetPlayerPosition();

        // Get settings for this round
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

        // Clamp to available anomalies
        anomalyCount = Mathf.Min(anomalyCount, allAnomalies.Count);

        if (debugMode) Debug.Log($"[RoundManager] Starting Round {currentRound} - {anomalyCount} anomalies, {spawnInterval}s interval, {startDelay}s delay before first spawn");

        // Reset battery to full
        if (battery)
        {
            battery.Refill(battery.Total);
        }

        // Reset game clock to 12:00 AM
        if (gameClock)
        {
            gameClock.SetTime(0, 0); // 12:00 AM
        }

        // Pick random anomalies for this round
        activeAnomaliesThisRound.Clear();
        var shuffled = allAnomalies.OrderBy(x => Random.value).ToList();
        for (int i = 0; i < anomalyCount && i < shuffled.Count; i++)
        {
            activeAnomaliesThisRound.Add(shuffled[i]);
        }

        // Start spawning them over time (with initial delay)
        spawnRoutine = StartCoroutine(SpawnAnomaliesRoutine(spawnInterval, startDelay));
    }

    private void ResetPlayerPosition()
    {
        if (!playerTransform)
        {
            if (debugMode) Debug.LogWarning("[RoundManager] Cannot reset player position - no player assigned!");
            return;
        }

        // Check if player has a CharacterController (need to disable it to teleport)
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
            // No CharacterController, just set position directly
            playerTransform.position = playerStartPosition;
            if (resetRotation) playerTransform.rotation = playerStartRotation;
        }

        if (debugMode) Debug.Log($"[RoundManager] Reset player to starting position: {playerStartPosition}");
    }

    private void DisableGameplayControls()
    {
        if (debugMode) Debug.Log("[RoundManager] Disabling gameplay controls during transition...");

        // Disable all gameplay scripts
        if (disableDuringTransition != null)
        {
            foreach (var b in disableDuringTransition)
            {
                if (b) b.enabled = false;
            }
        }

        // Store current cursor state and lock it
        if (lockCursorDuringTransition)
        {
            previousCursorLockMode = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void EnableGameplayControls()
    {
        if (debugMode) Debug.Log("[RoundManager] Re-enabling gameplay controls...");

        // Re-enable all gameplay scripts
        if (disableDuringTransition != null)
        {
            foreach (var b in disableDuringTransition)
            {
                if (b) b.enabled = true;
            }
        }

        // Restore previous cursor state
        if (lockCursorDuringTransition)
        {
            Cursor.lockState = previousCursorLockMode;
            Cursor.visible = previousCursorVisible;
        }
    }

    private IEnumerator SpawnAnomaliesRoutine(float interval, float startDelay)
    {
        // Wait before spawning the first anomaly (gives player time to prepare)
        if (startDelay > 0)
        {
            if (debugMode) Debug.Log($"[RoundManager] Waiting {startDelay}s before spawning first anomaly...");
            yield return new WaitForSeconds(startDelay);
        }

        if (debugMode) Debug.Log($"[RoundManager] Starting anomaly spawns now!");

        // Shuffle the order
        var shuffledOrder = activeAnomaliesThisRound.OrderBy(x => Random.value).ToList();

        foreach (var anomaly in shuffledOrder)
        {
            if (!roundInProgress) yield break; // Stop if round ended

            // Trigger the anomaly
            anomaly.Trigger();

            if (debugMode) Debug.Log($"[RoundManager] Triggered {anomaly.Room} - {anomaly.Type}");

            // Wait before triggering the next one
            yield return new WaitForSeconds(interval);
        }

        // After all anomalies are triggered once, keep cycling
        while (roundInProgress)
        {
            // Re-shuffle and trigger again
            shuffledOrder = activeAnomaliesThisRound.OrderBy(x => Random.value).ToList();

            foreach (var anomaly in shuffledOrder)
            {
                if (!roundInProgress) yield break;

                // Only trigger if not already active
                if (!anomaly.IsActive)
                {
                    anomaly.Trigger();
                    if (debugMode) Debug.Log($"[RoundManager] Re-triggered {anomaly.Room} - {anomaly.Type}");
                }

                yield return new WaitForSeconds(interval);
            }
        }
    }

    private IEnumerator TransitionToNextRound()
    {
        if (debugMode) Debug.Log($"[RoundManager] Showing transition to Round {currentRound}...");

        // DISABLE ALL GAMEPLAY CONTROLS
        DisableGameplayControls();

        // Show transition screen
        if (roundTransitionPanel) roundTransitionPanel.SetActive(true);

        // Wait using realtime (works even if game is paused)
        yield return new WaitForSecondsRealtime(3f);

        // Hide transition
        if (roundTransitionPanel) roundTransitionPanel.SetActive(false);

        if (debugMode) Debug.Log($"[RoundManager] Starting Round {currentRound} now!");

        // Start next round (this will re-enable controls)
        StartRound(currentRound);
    }

    private void ShowFinalVictory()
    {
        if (debugMode) Debug.Log("[RoundManager] FINAL VICTORY!");

        if (finalVictoryPanel) finalVictoryPanel.SetActive(true);

        // Disable gameplay controls
        DisableGameplayControls();

        // Pause game
        Time.timeScale = 0f;
        
        // Show cursor for final victory screen
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
}