using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItem : MonoBehaviour
{
    public bool isHpUpgrade = false;
    public bool isDamageUpgrade = false;
    public bool isSpeedUpgrade = false;
    public int price;
    private GameObject player;
    private GameObject area;
    private HUDController coinController;
    private GameObject text;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("IgnoreBullet");
        area = transform.Find("AreaOfEffect").gameObject;
        text = transform.Find("Texts").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if(area.GetComponent<ShopItemArea>().isInArea) {
            text.SetActive(true);
            if(Input.GetKeyDown(KeyCode.E) && HUDController.goldScore >= price) {
                if(isHpUpgrade) {
                    UpdateHp();
                }
                if(isDamageUpgrade) {
                    UpdateDmg();
                }
                if(isSpeedUpgrade) {
                    UpdateSpeed();
                }
                TakePrice();
            }
        }
        else {
            text.SetActive(false);
        }
    }

    void UpdateHp() {
        HUDController healthController = player.GetComponent<HUDController>();
        healthController.maxHealth += 2;
        HUDController.currentHealth = healthController.maxHealth;
        healthController.healthBar.SetMaxHealth(healthController.maxHealth);
        healthController.healthBar.SetHealth(HUDController.currentHealth);
        HUDController.healthGUI.text = $"{HUDController.currentHealth}/{healthController.maxHealth}";
    }

    void TakePrice() {
        HUDController.goldScore -= price;
        HUDController.goldGUI.text = HUDController.goldGUI.text = $"{HUDController.goldScore}";
    }

    void UpdateSpeed() {
        player.GetComponent<PlayerController>().speed += 1.0f;
    }

    void UpdateDmg() {
        player.GetComponent<PlayerController>().bonusDamage += 1;
    }
}
