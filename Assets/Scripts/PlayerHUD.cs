using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages player HUD elements - health display, tutorial hints, and death message
/// Handles input for resetting game and dismissing tutorial hints
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;

    [Header("Tutorial Hints")]
    [SerializeField] private Text rotateHintText; // Disappears after Q/E pressed
    [SerializeField] private Text shootHintText;  // Disappears after Mouse0 pressed

    [Header("Death Message")]
    [SerializeField] private Text deathMessageText; // Shows when player dies, tells them to press R

    private bool rotateHintDismissed = false;
    private bool shootHintDismissed = false;

    /// <summary>
    /// Initialize HUD - find player health and setup UI elements
    /// </summary>
    private void Start()
    {
        // Find player health component if not assigned
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }
        }

        // Subscribe to health events and initialize UI
        if (playerHealth != null)
        {
            UpdateHealthUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            playerHealth.OnHealthChanged += OnHealthChanged;
            playerHealth.OnDeath += OnPlayerDeath;
        }

        // Initialize tutorial hints visibility
        if (rotateHintText != null)
        {
            rotateHintText.gameObject.SetActive(true);
        }
        if (shootHintText != null)
        {
            shootHintText.gameObject.SetActive(true);
        }
        if (deathMessageText != null)
        {
            deathMessageText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Handle input for reset and tutorial hint dismissal
    /// </summary>
    private void Update()
    {
        // Reset with R key
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }

        // Dismiss rotate hint after Q or E is pressed
        if (!rotateHintDismissed && (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E)))
        {
            DismissRotateHint();
        }

        // Dismiss shoot hint after Mouse0 is pressed
        if (!shootHintDismissed && Input.GetKeyDown(KeyCode.Mouse0))
        {
            DismissShootHint();
        }
    }

    /// <summary>
    /// Update health UI when health changes
    /// </summary>
    /// <param name="current">Current health value</param>
    /// <param name="max">Maximum health value</param>
    private void OnHealthChanged(int current, int max)
    {
        UpdateHealthUI(current, max);
    }

    /// <summary>
    /// Show death message when player dies
    /// </summary>
    private void OnPlayerDeath()
    {
        if (deathMessageText != null)
        {
            deathMessageText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Update health slider and text display
    /// </summary>
    /// <param name="current">Current health value</param>
    /// <param name="max">Maximum health value</param>
    private void UpdateHealthUI(int current, int max)
    {
        // Update health slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        // Update health text
        if (healthText != null)
        {
            healthText.text = $"{current}/{max}";
        }
    }

    /// <summary>
    /// Hide rotate hint after player uses Q/E
    /// </summary>
    private void DismissRotateHint()
    {
        rotateHintDismissed = true;
        if (rotateHintText != null)
        {
            rotateHintText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Hide shoot hint after player shoots
    /// </summary>
    private void DismissShootHint()
    {
        shootHintDismissed = true;
        if (shootHintText != null)
        {
            shootHintText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Reset game to level 1 using LevelManager
    /// </summary>
    private void ResetGame()
    {
        // Reset to level 1 using LevelManager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ResetToLevelOne();
        }
        else
        {
            // Fallback: reload current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    /// <summary>
    /// Clean up event subscriptions on destroy
    /// </summary>
    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= OnHealthChanged;
            playerHealth.OnDeath -= OnPlayerDeath;
        }
    }
}
