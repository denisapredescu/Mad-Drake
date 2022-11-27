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
    
    public static int goldScore = 0;

    private void Start()
    {
        maxHealth = 5;
        goldScore = 0;
        currentHealth = maxHealth;
           
        goldGUI = goldCard.GetComponent<TextMeshProUGUI>();
        goldGUI.text = $"{goldScore}";

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
        AddGold();
    }

    public static void AddGold()
    {
        goldScore++;
        goldGUI.text = $"{goldScore}";
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PickupIncreaseHealth"))
        {
            if(currentHealth < maxHealth)
            {
                currentHealth++;
                healthGUI.text = $"{currentHealth}/{maxHealth}";
                healthBar.SetHealth(currentHealth);
                collision.gameObject.SetActive(false);
            }
            
        }
        if (collision.CompareTag("PickupRestoreHealth"))
        {
            if (currentHealth < maxHealth)
            {
                currentHealth = maxHealth;
                healthGUI.text = $"{currentHealth}/{maxHealth}";
                healthBar.SetHealth(currentHealth);
                collision.gameObject.SetActive(false);
            }
        }
    }

}
