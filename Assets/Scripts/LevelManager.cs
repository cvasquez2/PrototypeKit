using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// - Handles simple level progression (optionally score-based).
/// - Can optionally auto-load scenes per level.
/// - Exposes generic UnityEvents so different projects can plug in their own logic.
/// - Singleton that can persist across scenes.
/// </summary>
public class LevelManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static LevelManager Instance { get; private set; }

    [System.Serializable]
    public class IntEvent : UnityEvent<int> { }

    [Header("Progression")]
    [Tooltip("If true, levels advance based on score thresholds via NotifyScoreChanged().")]
    [SerializeField] private bool useScoreBasedProgression = true;

    [Tooltip("Score required to advance one level when using score-based progression.")]
    [SerializeField] private int scorePerLevel = 1000;

    [Tooltip("Starting level when the game begins or is reset.")]
    [SerializeField] private int startLevel = 1;

    [Header("Scenes (optional)")]
    [Tooltip("If true, LevelManager will automatically load a scene for the current level.")]
    [SerializeField] private bool autoLoadScenes = false;

    [SerializeField] private int firstLevelSceneIndex = 0;
    [SerializeField] private string[] levelSceneNames; // Alternative: use scene names instead of indices

    [Header("UI (optional)")]
    [Tooltip("Text that shows current level (e.g. CENTER HUD). If null, it will be auto-found.")]
    [SerializeField] private Text levelText;

    [SerializeField] private float levelTextDuration = 3f;

    [Header("Events")]
    [Tooltip("Invoked whenever the current level value changes (new level int).")]
    public IntEvent OnLevelChanged;

    [Tooltip("Invoked when a level starts (after scene load if autoLoadScenes is enabled).")]
    public IntEvent OnLevelStarted;

    [Tooltip("Invoked when the current level is completed, before advancing to the next level.")]
    public IntEvent OnLevelCompleted;

    /// <summary>
    /// The current level number (1-based).
    /// </summary>
    public int CurrentLevel { get; private set; } = 1;

    /// <summary>
    /// Last score threshold that was reached (used only when score-based).
    /// </summary>
    private int lastScoreThreshold = 0;

    /// <summary>
    /// Initialize singleton pattern and (optionally) persist across scenes.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Initialize the level system on start.
    /// </summary>
    private void Start()
    {
        Initialize(startLevel);
    }

    /// <summary>
    /// Subscribe to scene loaded events if we are handling scenes.
    /// </summary>
    private void OnEnable()
    {
        if (autoLoadScenes)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    /// <summary>
    /// Unsubscribe from scene loaded events.
    /// </summary>
    private void OnDisable()
    {
        if (autoLoadScenes)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    /// <summary>
    /// Initializes the level manager to a specific level.
    /// </summary>
    /// <param name="level">Level to initialize to (1-based).</param>
    public void Initialize(int level = 1)
    {
        CurrentLevel = Mathf.Max(1, level);
        lastScoreThreshold = (CurrentLevel - 1) * scorePerLevel;

        FindLevelTextIfNeeded();
        ShowLevelText();

        OnLevelChanged?.Invoke(CurrentLevel);

        if (autoLoadScenes)
        {
            LoadSceneForCurrentLevel();
        }
        else
        {
            OnLevelStarted?.Invoke(CurrentLevel);
        }
    }

    /// <summary>
    /// Handle scene load - re-find UI and fire level started event.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindLevelTextIfNeeded();
        ShowLevelText();

        OnLevelStarted?.Invoke(CurrentLevel);
    }

    /// <summary>
    /// Public API for your score system to call.
    /// Any project can hook its own score manager into this.
    /// </summary>
    /// <param name="newScore">The latest score value.</param>
    public void NotifyScoreChanged(int newScore)
    {
        if (!useScoreBasedProgression)
            return;

        int nextLevelThreshold = lastScoreThreshold + scorePerLevel;
        if (newScore >= nextLevelThreshold)
        {
            LevelUp();
        }
    }

    /// <summary>
    /// Manually force a level up (e.g. after a boss kill, waves cleared, etc.).
    /// </summary>
    public void LevelUp()
    {
        OnLevelCompleted?.Invoke(CurrentLevel);

        CurrentLevel++;
        lastScoreThreshold += scorePerLevel;

        OnLevelChanged?.Invoke(CurrentLevel);

        if (autoLoadScenes)
        {
            LoadSceneForCurrentLevel();
        }
        else
        {
            ShowLevelText();
            OnLevelStarted?.Invoke(CurrentLevel);
        }
    }

    /// <summary>
    /// Jump directly to a specific level.
    /// Useful for testing or different game modes.
    /// </summary>
    public void GoToLevel(int targetLevel)
    {
        CurrentLevel = Mathf.Max(1, targetLevel);
        lastScoreThreshold = (CurrentLevel - 1) * scorePerLevel;

        OnLevelChanged?.Invoke(CurrentLevel);

        if (autoLoadScenes)
        {
            LoadSceneForCurrentLevel();
        }
        else
        {
            ShowLevelText();
            OnLevelStarted?.Invoke(CurrentLevel);
        }
    }

    /// <summary>
    /// Reset game to level 1.  
    /// Note: Your score system should reset itself separately.
    /// </summary>
    public void ResetToLevelOne()
    {
        Initialize(1);
    }

    /// <summary>
    /// Reload the current scene (if autoLoadScenes is enabled).
    /// </summary>
    public void ReloadCurrentScene()
    {
        if (!autoLoadScenes)
            return;

        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    /// <summary>
    /// Find level text UI element in the scene if not already assigned.
    /// </summary>
    private void FindLevelTextIfNeeded()
    {
        if (levelText != null)
            return;

        Text[] allTexts = FindObjectsByType<Text>(FindObjectsSortMode.None);
        foreach (Text txt in allTexts)
        {
            if (txt.name.Contains("Level") || txt.name.Contains("level"))
            {
                levelText = txt;
                break;
            }
        }
    }

    /// <summary>
    /// Display level text for a few seconds.
    /// </summary>
    private void ShowLevelText()
    {
        if (levelText == null)
            return;

        levelText.text = $"LEVEL {CurrentLevel}";
        levelText.gameObject.SetActive(true);
        StartCoroutine(HideLevelTextAfterDelay());
    }

    /// <summary>
    /// Hide level text after a short delay.
    /// </summary>
    private IEnumerator HideLevelTextAfterDelay()
    {
        yield return new WaitForSeconds(levelTextDuration);

        // Re-find text in case scene changed
        if (levelText == null)
        {
            FindLevelTextIfNeeded();
        }

        if (levelText != null)
        {
            levelText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Load the appropriate scene for the current level.
    /// Can use either scene names or build indices.
    /// </summary>
    private void LoadSceneForCurrentLevel()
    {
        if (levelSceneNames != null && levelSceneNames.Length > 0)
        {
            // Use scene names if provided (loop through them if level exceeds array length)
            int nameIndex = (CurrentLevel - 1) % levelSceneNames.Length;
            SceneManager.LoadScene(levelSceneNames[nameIndex]);
        }
        else
        {
            // Use scene indices (assumes scenes are indexed sequentially)
            int sceneIndexToLoad = firstLevelSceneIndex + (CurrentLevel - 1);

            int sceneCount = SceneManager.sceneCountInBuildSettings;
            if (sceneCount <= 0)
            {
                Debug.LogWarning("No scenes in build settings. Cannot auto-load level scene.");
                return;
            }

            // If we exceed available scenes, loop back or use modulo
            if (sceneIndexToLoad >= sceneCount)
            {
                int range = Mathf.Max(1, sceneCount - firstLevelSceneIndex);
                sceneIndexToLoad = firstLevelSceneIndex + ((CurrentLevel - 1) % range);
            }

            SceneManager.LoadScene(sceneIndexToLoad);
        }
    }
}
