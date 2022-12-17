using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    public HealthBar healthBar;
    public GunBar gunBar;
    private bool playerIsDead;
    private static TextMeshProUGUI healthGUI;
    private static TextMeshProUGUI goldGUI;
    private static TextMeshProUGUI gunGUI;
    public int maxHealth = 5;
    public int currentHealth;
    private int maxBullets;
    private static int currentBullets;
    public GameObject healthCard;
    public GameObject goldCard;
    public GameObject gunCard;

    private static bool numberOfBulletsIsChanged = false;

    public static int goldScore = 0;

    private void Start()
    {
        maxHealth = 5;
        goldScore = 0;
        currentHealth = maxHealth;

        maxBullets = GunFiringController.magazineSize;
        currentBullets = maxBullets;

        healthGUI = healthCard.GetComponent<TextMeshProUGUI>();
        healthGUI.text = $"{maxHealth}/{maxHealth}";

        goldGUI = goldCard.GetComponent<TextMeshProUGUI>();
        goldGUI.text = $"{goldScore}";

        gunGUI = gunCard.GetComponent<TextMeshProUGUI>();
        gunGUI.text = $"{currentBullets}/{maxBullets}";

        playerIsDead = false;
        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(5);

        gunBar.SetMaxBullets(maxBullets);
        gunBar.SetBullets(currentBullets);
    }

    public void Update()
    {
        if (numberOfBulletsIsChanged)
        {
            gunBar.SetBullets(currentBullets);
            gunGUI.text = $"{currentBullets}/{maxBullets}";
            numberOfBulletsIsChanged = false;
        }
    }

    public static void ChangeValueOfActiveMagazine(int currentValue)
    {
        currentBullets = currentValue;
        numberOfBulletsIsChanged = true;
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
