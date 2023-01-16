using UnityEngine;

public class SwitchGuns : MonoBehaviour
{
    [SerializeField]
    private GameObject gun1;
    [SerializeField]
    private GameObject gun2;
    [SerializeField]
    private GameObject gun3;
    [SerializeField]
    private GameObject reloadAnim;
    private float activeGun = 1;

    private bool gun2Unlocked = false;
    private bool gun3Unlocked = false;

    public void UnlockGun2()
    {
        gun2Unlocked = true;
    }

    public void UnlockGun3()
    {
        gun3Unlocked = true;
    }

    private void ChangeGun(int value)
    {
        switch(value)
        {
            case 1: 
                gun2.SetActive(false);
                gun3.SetActive(false);
                gun1.SetActive(true);
                reloadAnim.SetActive(false);
                activeGun = 1;
                break;
            case 2:
                if (!gun2Unlocked)
                    break;
                gun1.SetActive(false);
                gun3.SetActive(false);
                gun2.SetActive(true);
                reloadAnim.SetActive(false);
                activeGun = 2;
                break;
            case 3:
                if (!gun3Unlocked)
                    break;
                gun1.SetActive(false);
                gun2.SetActive(false);
                gun3.SetActive(true);
                reloadAnim.SetActive(false);
                activeGun = 3;
                break;
            default: break;
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeGun(1);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeGun(2);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeGun(3);
        }
        else if(Input.GetKeyDown(KeyCode.Tab))
        {
            switch(activeGun)
            {
                case 1:
                    if (gun2Unlocked)
                        ChangeGun(2);
                    else if (gun3Unlocked)
                        ChangeGun(3);
                    else 
                        ChangeGun(1);
                    break;
                case 2:
                    if (gun3Unlocked)
                        ChangeGun(3);
                    else
                        ChangeGun(1);
                    break;
                case 3:
                    ChangeGun(1); 
                    break;
                default: break;
            }
        }
    }
}
