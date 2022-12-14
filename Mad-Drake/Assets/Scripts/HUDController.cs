using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    public HealthBar healthBar;
    public GunBar gunBar;
    private static bool playerIsDead;
    private static TextMeshProUGUI healthGUI;
    private static TextMeshProUGUI goldGUI;
    private static TextMeshProUGUI gunGUI;
    private static TextMeshProUGUI bombGUI;
    public int maxHealth = 5;
    public int currentHealth;
    private int maxBullets;
    private static int currentBullets;
    public GameObject healthCard;
    public GameObject goldCard;
    public GameObject gunCard;
    public GameObject bombCard;
    [SerializeField]
    private GunFiringController playerFiringController;

    private static bool numberOfBulletsIsChanged = false;

    public static int goldScore = 0;
    public static int bombs = 3;

    private void Start()
    {
        maxHealth = 5;
        goldScore = 0;
        currentHealth = maxHealth;
        bombs = 3;


        maxBullets = playerFiringController.GetMagazineSize();
        currentBullets = maxBullets;

        healthGUI = healthCard.GetComponent<TextMeshProUGUI>();
        healthGUI.text = $"{maxHealth}/{maxHealth}";

        goldGUI = goldCard.GetComponent<TextMeshProUGUI>();
        goldGUI.text = $"{goldScore}";

        gunGUI = gunCard.GetComponent<TextMeshProUGUI>();
        gunGUI.text = $"{currentBullets}/{maxBullets}";

        bombGUI = bombCard.GetComponent<TextMeshProUGUI>();
        bombGUI.text = $"{bombs}";

        playerIsDead = false;
        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(5);

        gunBar.SetMaxBullets(maxBullets);
        gunBar.SetBullets(currentBullets);
    }

    public void Update()
    {
        if (currentHealth > 0)
        {
            playerIsDead = false;
        }

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

    public static bool GetPlayerDeadStatus() 
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
    public static void ReleaseBomb()
    {
        bombs--;
        bombGUI.text = $"{bombs}";
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
