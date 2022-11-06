using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerHealthController : MonoBehaviour
{
    public HealthBar healthBar;
    private bool playerIsDead;
    private static TextMeshProUGUI healthGUI;
    private static TextMeshProUGUI goldGUI;
    public int maxHealth = 5;
    public int currentHealth;
    public GameObject healthCard;
    public GameObject goldCard;
    
    private int goldScore;

    private void Start()
    {
        currentHealth = maxHealth;
           
        goldGUI = goldCard.GetComponent<TextMeshProUGUI>();
        goldGUI.text = "0 Gold";
        goldScore = 0;

        healthGUI = healthCard.GetComponent<TextMeshProUGUI>();
        healthGUI.text = $"{maxHealth}/{maxHealth}";

        playerIsDead = false;
        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(5);
    }

    public bool GetPlayerDeadStatus() 
    {
        return playerIsDead;
    }

    public void TakeDamage(int damage)
    {
        if (!playerIsDead)
        {
            currentHealth -= damage;
            healthBar.SetHealth(currentHealth);
            healthGUI.text = $"{currentHealth}/{maxHealth}";
            if (currentHealth <= 0)
            {
                playerIsDead = true;
            }
        }
    }

    private void AddGold()
    {
        goldScore++;
        goldGUI.text = $"{goldScore} Gold";
    }
}
