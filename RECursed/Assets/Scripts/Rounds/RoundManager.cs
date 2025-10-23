using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Scene Management")]
    [Tooltip("Name of the scene to load when Round 3 starts (e.g., 'Round3Map')")]
    [SerializeField] private string round3SceneName = "Round3And4Scene";
    [Tooltip("Name of the scene to load when Round 5 starts (e.g., 'Round5Map')")]
    [SerializeField] private string round5SceneName = "Round5Map";

    [Header("Scene References")]
    [SerializeField] private GameClockController gameClock;
    [SerializeField] private SegmentBattery battery;
    [SerializeField] private GameObject roundTransitionPanel;
    [SerializeField] private TMPro.TextMeshProUGUI roundTransitionText; // Text component to update
    [SerializeField] private GameObject finalVictoryPanel;
    
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

    private List<IAnomaly> allAnomalies = new List<IAnomaly>();
    private List<IAnomaly> activeAnomaliesThisRound = new List<IAnomaly>();
    private Coroutine spawnRoutine;
    private bool roundInProgress = false;
    
    private Vector3 playerStartPosition;
    private Quaternion playerStartRotation;
    
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;

    void Awake()
    {
        // OPTIMIZED: Search for specific anomaly types instead of ALL MonoBehaviours
        allAnomalies.Clear();
        
        if (debugMode) Debug.Log("[RoundManager] Searching for anomalies...");
        
        var movedObjects = FindObjectsOfType<MovedObject>(true);
        var disappearedObjects = FindObjectsOfType<DisappearedObject>(true);
        var extraObjects = FindObjectsOfType<ExtraObject>(true);
        var lightFlickers = FindObjectsOfType<LightFlickerAnomaly>(true);
        
        foreach (var a in movedObjects) allAnomalies.Add(a);
        foreach (var a in disappearedObjects) allAnomalies.Add(a);
        foreach (var a in extraObjects) allAnomalies.Add(a);
        foreach (var a in lightFlickers) allAnomalies.Add(a);

        if (debugMode) Debug.Log($"[RoundManager] Found {allAnomalies.Count} anomalies in scene.");

        if (!gameClock) gameClock = FindObjectOfType<GameClockController>();
        if (!battery) battery = FindObjectOfType<SegmentBattery>();

        if (!playerTransform)
        {
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

        if (playerTransform)
        {
            playerStartPosition = playerTransform.position;
            playerStartRotation = playerTransform.rotation;
            if (debugMode) Debug.Log($"[RoundManager] Stored player start position: {playerStartPosition}");
        }

        if (roundTransitionPanel) roundTransitionPanel.SetActive(false);
        if (finalVictoryPanel) finalVictoryPanel.SetActive(false);
    }

    void Start()
    {
        // Start Round 1 automatically
        StartRound(1);
    }

    public void OnRoundTimeComplete()
    {
        if (debugMode) Debug.Log($"[RoundManager] Round {currentRound} time complete!");

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        foreach (var anomaly in activeAnomaliesThisRound)
        {
            if (anomaly.IsActive)
                anomaly.Revert();
        }
        activeAnomaliesThisRound.Clear();

        roundInProgress = false;

        // Move to next round first
        currentRound++;
        
        // FIXED: Check for scene transitions BEFORE checking for final victory!
        if (currentRound == 3)
        {
            Debug.Log("[RoundManager] Transitioning to Round 3 scene!");
            StartCoroutine(TransitionToNextRound());
            return;
        }
        
        if (currentRound == 5)
        {
            Debug.Log("[RoundManager] Transitioning to Round 5 scene!");
            StartCoroutine(TransitionToNextRound());
            return;
        }

        // NOW check if this was the final round
        if (currentRound > totalRounds)
        {
            ShowFinalVictory();
        }
        else
        {
            // Normal transition within same scene
            StartCoroutine(TransitionToNextRound());
        }
    }

    // ===== HEAVILY LOGGED VERSION FOR DEBUGGING =====
    private void StartRound(int roundNumber)
    {
        Debug.Log("=== [1] StartRound CALLED ===");
        
        Time.timeScale = 1f;
        
        Debug.Log("=== [2] Time scale set ===");
        
        currentRound = roundNumber;
        roundInProgress = true;

        Debug.Log("=== [3] Round variables set ===");

        EnableGameplayControls();
        
        Debug.Log("=== [4] Controls enabled ===");

        ResetPlayerPosition();
        
        Debug.Log("=== [5] Player reset ===");

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

        Debug.Log("=== [6] Round settings retrieved ===");

        anomalyCount = Mathf.Min(anomalyCount, allAnomalies.Count);

        Debug.Log($"=== [7] Anomaly count clamped to {anomalyCount} ===");

        if (debugMode) Debug.Log($"[RoundManager] Starting Round {currentRound} - {anomalyCount} anomalies, {spawnInterval}s interval, {startDelay}s delay before first spawn");

        Debug.Log("=== [8] About to reset battery ===");
        
        if (battery)
        {
            battery.Refill(battery.Total);
        }

        Debug.Log("=== [9] Battery reset, about to reset clock ===");

        if (gameClock)
        {
            gameClock.SetTime(0, 0);
        }

        Debug.Log("=== [10] Clock reset, about to shuffle anomalies ===");

        activeAnomaliesThisRound.Clear();
        
        Debug.Log("=== [11] Cleared active anomalies list ===");
        
        var shuffled = allAnomalies.OrderBy(x => Random.value).ToList();
        
        Debug.Log("=== [12] Anomalies shuffled ===");
        
        for (int i = 0; i < anomalyCount && i < shuffled.Count; i++)
        {
            activeAnomaliesThisRound.Add(shuffled[i]);
        }

        Debug.Log("=== [13] Active anomalies selected, about to start spawn routine ===");

        spawnRoutine = StartCoroutine(SpawnAnomaliesRoutine(spawnInterval, startDelay));
        
        Debug.Log("=== [14] StartRound FINISHED ===");
    }

    private void ResetPlayerPosition()
    {
        if (!playerTransform)
        {
            if (debugMode) Debug.LogWarning("[RoundManager] Cannot reset player position - no player assigned!");
            return;
        }

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
            playerTransform.position = playerStartPosition;
            if (resetRotation) playerTransform.rotation = playerStartRotation;
        }

        if (debugMode) Debug.Log($"[RoundManager] Reset player to starting position: {playerStartPosition}");
    }

    private void DisableGameplayControls()
    {
        if (debugMode) Debug.Log("[RoundManager] Disabling gameplay controls during transition...");

        if (disableDuringTransition != null)
        {
            foreach (var b in disableDuringTransition)
            {
                if (b) b.enabled = false;
            }
        }

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

        if (disableDuringTransition != null)
        {
            foreach (var b in disableDuringTransition)
            {
                if (b) b.enabled = true;
            }
        }

        if (lockCursorDuringTransition)
        {
            Cursor.lockState = previousCursorLockMode;
            Cursor.visible = previousCursorVisible;
        }
    }

    private IEnumerator SpawnAnomaliesRoutine(float interval, float startDelay)
    {
        if (startDelay > 0)
        {
            if (debugMode) Debug.Log($"[RoundManager] Waiting {startDelay}s before spawning first anomaly...");
            yield return new WaitForSeconds(startDelay);
        }

        if (debugMode) Debug.Log($"[RoundManager] Starting anomaly spawns now!");

        var shuffledOrder = activeAnomaliesThisRound.OrderBy(x => Random.value).ToList();

        foreach (var anomaly in shuffledOrder)
        {
            if (!roundInProgress) yield break;

            anomaly.Trigger();

            if (debugMode) Debug.Log($"[RoundManager] Triggered {anomaly.Room} - {anomaly.Type}");

            yield return new WaitForSeconds(interval);
        }

        while (roundInProgress)
        {
            shuffledOrder = activeAnomaliesThisRound.OrderBy(x => Random.value).ToList();

            foreach (var anomaly in shuffledOrder)
            {
                if (!roundInProgress) yield break;

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

        DisableGameplayControls();

        // Show transition screen with UPDATED TEXT
        if (roundTransitionPanel)
        {
            roundTransitionPanel.SetActive(true);
            
            // UPDATE THE TEXT DYNAMICALLY based on current round
            if (roundTransitionText)
            {
                if (currentRound == 3 || currentRound == 5)
                {
                    roundTransitionText.text = $"ROUND {currentRound}\nLOADING NEW MAP...";
                }
                else
                {
                    roundTransitionText.text = $"ROUND {currentRound}\nSTARTING...";
                }
            }
        }

        if (currentRound == 3)
        {
            if (debugMode) Debug.Log($"[RoundManager] Round 3 detected! Loading scene: {round3SceneName}");
            
            yield return new WaitForSecondsRealtime(2f);
            
            if (!string.IsNullOrEmpty(round3SceneName))
            {
                if (Application.CanStreamedLevelBeLoaded(round3SceneName))
                {
                    SceneManager.LoadScene(round3SceneName);
                }
                else
                {
                    Debug.LogError($"[RoundManager] Scene '{round3SceneName}' not found in Build Settings!");
                }
            }
            else
            {
                Debug.LogError("[RoundManager] Round3SceneName is empty!");
            }
            
            yield break;
        }

        if (currentRound == 5)
        {
            if (debugMode) Debug.Log($"[RoundManager] Round 5 detected! Loading nightmare scene: {round5SceneName}");
            
            yield return new WaitForSecondsRealtime(2f);
            
            if (!string.IsNullOrEmpty(round5SceneName))
            {
                if (Application.CanStreamedLevelBeLoaded(round5SceneName))
                {
                    SceneManager.LoadScene(round5SceneName);
                }
                else
                {
                    Debug.LogError($"[RoundManager] Scene '{round5SceneName}' not found in Build Settings!");
                }
            }
            else
            {
                Debug.LogError("[RoundManager] Round5SceneName is empty!");
            }
            
            yield break;
        }

        yield return new WaitForSecondsRealtime(3f);

        if (roundTransitionPanel) roundTransitionPanel.SetActive(false);

        if (debugMode) Debug.Log($"[RoundManager] Starting Round {currentRound} now!");

        StartRound(currentRound);
    }

    private void ShowFinalVictory()
    {
        if (debugMode) Debug.Log("[RoundManager] FINAL VICTORY!");

        if (finalVictoryPanel) finalVictoryPanel.SetActive(true);

        DisableGameplayControls();

        Time.timeScale = 0f;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    [ContextMenu("Skip to Next Round")]
    public void DebugSkipToNextRound()
    {
        OnRoundTimeComplete();
    }
}