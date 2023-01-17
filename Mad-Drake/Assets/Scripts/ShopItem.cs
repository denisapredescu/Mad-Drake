using UnityEngine;

public class ShopItem : MonoBehaviour
{
    enum Upgrade { Speed, Damage, Health, Gun2, Gun3, None };
    [SerializeField]
    private Upgrade typeOfUpgrade = Upgrade.None;
    public int price;
    private GameObject player;
    private GameObject area;
    private GameObject text;
    // Start is called before the first frame update

    private SaveData saveData;

    void Start()
    {
        saveData = FindObjectOfType<SaveData>();
        player = GameObject.FindGameObjectWithTag("Player");
        area = transform.Find("AreaOfEffect").gameObject;
        text = transform.Find("Texts").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if(area.GetComponent<ShopItemArea>().isInArea) {
            text.SetActive(true);
            if(Input.GetKeyDown(KeyCode.E) && HUDController.goldScore >= price) {
                switch(typeOfUpgrade)
                {
                    case Upgrade.Speed:
                        UpdateSpeed();
                        TakePrice();
                        break;
                    case Upgrade.Damage:
                        UpdateDmg();
                        TakePrice();
                        break;
                    case Upgrade.Health:
                        UpdateHp();
                        TakePrice();
                        break;
                    case Upgrade.Gun2:
                        UnlockGun2();
                        gameObject.SetActive(false);
                        break;
                    case Upgrade.Gun3:
                        UnlockGun3();
                        gameObject.SetActive(false);
                        break;
                    default: break;
                }
            }
        }
        else {
            text.SetActive(false);
        }
    }

    void UpdateHp() {
        saveData.AddHealth(2);

        HUDController healthController = player.GetComponent<HUDController>();
        healthController.maxHealth += 2;
        HUDController.currentHealth = healthController.maxHealth;
        healthController.healthBar.SetMaxHealth(healthController.maxHealth);
        healthController.healthBar.SetHealth(HUDController.currentHealth);
        HUDController.healthGUI.text = $"{HUDController.currentHealth}/{healthController.maxHealth}";
    }

    void TakePrice() {
        saveData.AddCoins(-price);

        HUDController.goldScore -= price;
        HUDController.goldGUI.text = HUDController.goldGUI.text = $"{HUDController.goldScore}";
    }

    void UpdateSpeed() {
        saveData.AddSpeed(1.0f);

        player.GetComponent<PlayerController>().speed += 1.0f;
    }

    void UpdateDmg() {
        saveData.AddDamage(1);
        player.GetComponent<PlayerController>().bonusDamage += 1;
    }

    void UnlockGun2()
    {
        saveData.AddWeapon(2);
        player.GetComponent<SwitchGuns>().UnlockGun2();
    }

    void UnlockGun3()
    {
        saveData.AddWeapon(3);
        player.GetComponent<SwitchGuns>().UnlockGun3();
    }
}
