using System.Collections;
using System.IO;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    [SerializeField]
    private PlayerController playerController;
    [SerializeField]
    private HUDController hudController;
    [SerializeField]
    private SwitchGuns switchGuns;
    [SerializeField]
    private bool freshStart = false;
    [SerializeField]
    private Coins _coins = new();
    [SerializeField]
    private DamageTaken _damageTaken = new();
    [SerializeField]
    private WeaponUnlocked _weaponUnlocked = new();
    [SerializeField]
    private Upgrades _upgrades = new();

    private void Start()
    {
        if(!freshStart)
        {
            GetCoins();
            GetDamageTaken();
            GetWeaponUnlocked();
            GetUpgrades();
        }
        else
        {
            SaveCoins();
            SaveDamageTaken();
            SaveWeaponUnlocked();
            SaveUpgrades();
        }

        StartCoroutine(DelayDataModifiers());
    }

    private IEnumerator DelayDataModifiers()
    {
        yield return new WaitForEndOfFrame();
        hudController.maxHealth += _upgrades.health;
        HUDController.currentHealth = hudController.maxHealth;
        hudController.healthBar.SetMaxHealth(hudController.maxHealth);
        hudController.healthBar.SetHealth(HUDController.currentHealth);
        HUDController.healthGUI.text = $"{HUDController.currentHealth}/{hudController.maxHealth}";

        HUDController.goldScore = _coins.number;
        HUDController.goldGUI.text = $"{HUDController.goldScore}";

        playerController.speed += _upgrades.speed;
        playerController.bonusDamage += _upgrades.damage;
        HUDController.damageTaken = _damageTaken.damage;

        if (_weaponUnlocked.gun2)
            switchGuns.UnlockGun2();
        if (_weaponUnlocked.gun3)
            switchGuns.UnlockGun3();
    }

    public void AddCoins(int value)
    {
        _coins.number += value;
        SaveCoins();
    }
    public void AddDamageTaken(int value)
    {
        _damageTaken.damage += value;
        SaveDamageTaken();
    }
    public void AddWeapon(int weapon)
    {
        if (weapon == 2)
        {
            _weaponUnlocked.gun2 = true;
            SaveWeaponUnlocked();
        } 
        else if(weapon == 3)
        {
            _weaponUnlocked.gun3 = true;
            SaveWeaponUnlocked();
        }
    }
    public void AddHealth(int value)
    {
        _upgrades.health += value;
        SaveUpgrades();
    }
    public void AddSpeed(float speed)
    {
        _upgrades.speed += speed;
        SaveUpgrades();
    }
    public void AddDamage(int damage)
    {
        _upgrades.damage += damage;
        SaveUpgrades();
    }

    public void SaveCoins()
    {
        string coins = JsonUtility.ToJson(_coins);
        File.WriteAllText(Application.persistentDataPath + "/Coins.json", coins);
    }
    public void SaveDamageTaken()
    {
        string damage = JsonUtility.ToJson(_damageTaken);
        File.WriteAllText(Application.persistentDataPath + "/DamageTaken.json", damage);
    }
    public void SaveWeaponUnlocked()
    {
        string weapons = JsonUtility.ToJson(_weaponUnlocked);
        File.WriteAllText(Application.persistentDataPath + "/WeaponUnlocked.json", weapons);
    }
    public void SaveUpgrades()
    {
        string upgrades = JsonUtility.ToJson(_upgrades);
        File.WriteAllText(Application.persistentDataPath + "/Upgrades.json", upgrades);
    }

    public void GetCoins()
    {
        string coins = File.ReadAllText(Application.persistentDataPath + "/Coins.json");
        _coins = JsonUtility.FromJson<Coins>(coins);
    }
    public void GetDamageTaken()
    {
        string damage = File.ReadAllText(Application.persistentDataPath + "/DamageTaken.json");
        _damageTaken = JsonUtility.FromJson<DamageTaken>(damage);
    }
    public void GetWeaponUnlocked()
    {
        string weapons = File.ReadAllText(Application.persistentDataPath + "/WeaponUnlocked.json");
        _weaponUnlocked = JsonUtility.FromJson<WeaponUnlocked>(weapons);
    }
    public void GetUpgrades()
    {
        string upgrades = File.ReadAllText(Application.persistentDataPath + "/Upgrades.json");
        _upgrades = JsonUtility.FromJson<Upgrades>(upgrades);
    }
}

[System.Serializable]
public class Coins
{
    public int number = 0;
}

[System.Serializable]
public class DamageTaken
{
    public int damage = 0;
}

[System.Serializable]
public class WeaponUnlocked
{
    public bool gun2 = false;
    public bool gun3 = false;
}

[System.Serializable]
public class Upgrades
{
    public int health = 0;
    public int damage = 0;
    public float speed = 0;
}
