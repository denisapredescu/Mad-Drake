using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFiringController : MonoBehaviour
{
    [SerializeField]
    private GameObject bullet = null;
    [SerializeField]
    private Transform tipOfGun = null;
    [SerializeField]
    private Transform parent = null;
    [SerializeField]
    private float firingDelay = 1.0f;
    private bool canFire = true;
    private Quaternion rotate180 = Quaternion.Euler(0, 0, 180);

    private Queue<GameObject> activeBullets;
    private GameObject activeBullet = null;

    private void addBullet(GameObject gameObject)
    {
        this.activeBullets.Enqueue(gameObject);
    }

    private void Start()
    {
        activeBullets = new Queue<GameObject>();
    }

    void Update()
    {
        if(canFire && Input.GetMouseButton(0))
        {
            StartCoroutine(waitToFire());
            if(parent.localScale.x < 0.0f)
            {
                if(activeBullets.Count > 0)
                {
                    activeBullet = activeBullets.Dequeue();
                    activeBullet.transform.position = tipOfGun.position;
                    activeBullet.transform.rotation = tipOfGun.rotation * rotate180;
                    activeBullet.SetActive(true);
                }
                else
                {
                    activeBullet = Instantiate(bullet, tipOfGun.position, tipOfGun.rotation * rotate180);
                    activeBullet.gameObject.GetComponent<BulletMovement>()?.setActionAddBullet(addBullet);
                }
            }
            else
            {
                if (activeBullets.Count > 0)
                {
                    activeBullet = activeBullets.Dequeue();
                    activeBullet.transform.position = tipOfGun.position;
                    activeBullet.transform.rotation = tipOfGun.rotation;
                    activeBullet.SetActive(true);
                }
                else
                {
                    activeBullet = Instantiate(bullet, tipOfGun.position, tipOfGun.rotation);
                    activeBullet.gameObject.GetComponent<BulletMovement>()?.setActionAddBullet(addBullet);
                }
            }
        }
    }

    private IEnumerator waitToFire()
    {
        canFire = false;
        yield return new WaitForSeconds(firingDelay);
        canFire = true;
    }
}
