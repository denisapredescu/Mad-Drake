using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class PlayerGunController : MonoBehaviour
{
    public GunBar gunBar;
    private static TextMeshProUGUI gunGUI;
    private int maxBullets;
    private static int currentBullets;
    public GameObject gunCard;
    private static bool isChanged = false;

    private void Start()
    {
        maxBullets = GunFiringController.magazineSize;
        currentBullets = maxBullets;

        gunGUI = gunCard.GetComponent<TextMeshProUGUI>();
        gunGUI.text = $"{currentBullets}/{maxBullets}";

        gunBar.SetMaxBullets(maxBullets);
        gunBar.SetBullets(currentBullets);
    }

    public void Update()
    {
        if (isChanged)
        {
            gunBar.SetBullets(currentBullets);
            gunGUI.text = $"{currentBullets}/{maxBullets}";
            isChanged = false;
        }
    }

    public static void ChangeValueOfActiveMagazine(int currentValue)
    {
        currentBullets = currentValue;
        isChanged = true;
       
    }

}
