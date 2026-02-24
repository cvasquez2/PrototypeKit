using UnityEngine;
using System;

/// <summary>
/// Manages health system for game objects (player, enemies, etc.)
/// Handles taking damage, healing, and death events
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    // Events for other systems to subscribe to
    public event Action<int, int> OnHealthChanged; // currentHealth, maxHealth
    public event Action OnDeath;
    public event Action<int> OnDamageTaken; // damage amount

    // Public properties for accessing health state
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;

    /// <summary>
    /// Initialize health to max value on start
    /// </summary>
    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Apply damage to this object
    /// </summary>
    /// <param name="amount">Amount of damage to take</param>
    public void TakeDamage(int amount)
    {
        // Don't process damage if already dead
        if (currentHealth <= 0) return;

        // Reduce health and clamp to 0 minimum
        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnDamageTaken?.Invoke(amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Check for death
        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    /// <summary>
    /// Restore health by specified amount
    /// </summary>
    /// <param name="amount">Amount of health to restore</param>
    public void Heal(int amount)
    {
        // Can't heal if dead
        if (currentHealth <= 0) return;

        // Increase health and clamp to max
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Change the maximum health value
    /// </summary>
    /// <param name="newMaxHealth">New maximum health value</param>
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Fully restore health to maximum
    /// </summary>
    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
