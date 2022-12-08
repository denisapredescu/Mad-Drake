using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFiringController : MonoBehaviour
{
    [SerializeField]
    private GameObject bullet = null;
    [SerializeField]
    private GameObject tipOfGun = null;
    private Transform tipTransform = null;
    //a list of all particle effects that need to start when the gun fires
    private ParticleSystem[] tipParticleEffects = null;
    [SerializeField]
    private Transform parent = null;
    [SerializeField]
    private float firingDelay = 1.0f;
    private bool canFire = true;
    private Quaternion rotate180 = Quaternion.Euler(0, 0, 180);
    [SerializeField]
    private int magazineSize = 10;
    private int activeMagazine = 0;
    [SerializeField]
    private float reloadTime = 2.0f;
    //references to reuse the bullets
    private Queue<GameObject> inactiveBullets;
    private GameObject activeBullet = null;

    //the function passed to a bullet to add itself to a queue
    private void addBullet(GameObject gameObject)
    {
        this.inactiveBullets.Enqueue(gameObject);
    }

    private void Start()
    {
        inactiveBullets = new Queue<GameObject>();
        tipTransform = tipOfGun.transform;
        activeMagazine = magazineSize;

        //getting all the particle effects and making them 10 times faster
        tipParticleEffects = tipOfGun.GetComponentsInChildren<ParticleSystem>();
    }

    void Update()
    {
        //reload with delay
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
        }

        //fire rate
        if (canFire && activeMagazine > 0 && Input.GetMouseButton(0) && MenuController.GameRunning)
        {
            StartCoroutine(waitToFire());
            activeMagazine--;

            //muzzle flash for the gun
            foreach (var ParticleEffect in tipParticleEffects)
            {
                ParticleEffect.Play();
            }

            //if x-scale is -1 there is applied a 180 rotation to the bullet
            if (parent.localScale.x < 0.0f)
            {
                //checking if there is at least one inactive bullet or creating another
                if (inactiveBullets.Count > 0)
                {
                    activeBullet = inactiveBullets.Dequeue();
                    activeBullet.transform.position = tipTransform.position;
                    activeBullet.transform.rotation = tipTransform.rotation * rotate180;
                    activeBullet.SetActive(true);
                }
                else
                {
                    activeBullet = Instantiate(bullet, tipTransform.position, tipTransform.rotation * rotate180);
                    activeBullet.gameObject.GetComponent<BulletMovement>()?.setActionAddBullet(addBullet);
                }
            }
            else
            {
                if (inactiveBullets.Count > 0)
                {
                    activeBullet = inactiveBullets.Dequeue();
                    activeBullet.transform.position = tipTransform.position;
                    activeBullet.transform.rotation = tipTransform.rotation;
                    activeBullet.SetActive(true);
                }
                else
                {
                    activeBullet = Instantiate(bullet, tipTransform.position, tipTransform.rotation);
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

    private IEnumerator Reload()
    {
        yield return new WaitForSeconds(reloadTime);
        activeMagazine = magazineSize;
    }
}