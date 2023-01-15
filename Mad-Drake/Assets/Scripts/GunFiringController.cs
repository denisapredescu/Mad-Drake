using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFiringController : MonoBehaviour
{
    enum BulletType { Bullet, Rocket };
    [SerializeField]
    private GameObject bullet = null;
    [SerializeField]
    private BulletType bulletType = BulletType.Bullet;
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
    [SerializeField]
    public uint damage = 1;
    private int activeMagazine = 0;
    [SerializeField]
    private float reloadTime = 2.0f;
    [SerializeField]
    private GameObject reloadObjectAnimation = null;
    //used to not start more coroutines
    private bool activeReloading = false;
    //references to reuse the bullets
    private Queue<GameObject> inactiveBullets;
    private GameObject activeBullet = null;
    [SerializeField]
    private AudioSource reloadAudio;
    [SerializeField]
    private AudioSource gunshotAudio;

    public int GetMagazineSize()
    {
        return magazineSize;
    }

    //the function passed to a bullet to add itself to a queue
    private void AddBullet(GameObject gameObject)
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
        HUDController.ChangeValueOfMagazineSize(magazineSize);
        HUDController.ChangeValueOfActiveMagazine(magazineSize);
    }

    private void OnEnable()
    {
        HUDController.ChangeValueOfMagazineSize(magazineSize);
        HUDController.ChangeValueOfActiveMagazine(activeMagazine);
    }

    void Update()
    {
        HUDController.ChangeValueOfActiveMagazine(activeMagazine);
        //reload with delay
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!activeReloading)
            {
                activeReloading = true;
                activeMagazine = 0;
                StartCoroutine(Reload());
            }
        }

        //fire rate
        if (canFire && Input.GetMouseButton(0) && MenuController.GameRunning)
        {
            if (activeMagazine > 0)
            {
                StartCoroutine(WaitToFire());
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
                        activeBullet.transform.SetPositionAndRotation(tipTransform.position, tipTransform.rotation * rotate180);
                        activeBullet.SetActive(true);
                    }
                    else
                    {
                        activeBullet = Instantiate(bullet, tipTransform.position, tipTransform.rotation * rotate180);
                        if (bulletType == BulletType.Bullet)
                        {
                            activeBullet.GetComponent<BulletMovement>().SetActionAddBullet(AddBullet);
                        }
                        else
                        {
                            activeBullet.GetComponent<RocketMovement>().SetActionAddBullet(AddBullet);
                        }
                    }
                }
                else
                {
                    if (inactiveBullets.Count > 0)
                    {
                        activeBullet = inactiveBullets.Dequeue();
                        activeBullet.transform.SetPositionAndRotation(tipTransform.position, tipTransform.rotation);
                        if (bulletType == BulletType.Bullet)
                        {
                            activeBullet.GetComponent<BulletMovement>().SetDamage(damage);
                        }
                        else
                        {
                            activeBullet.GetComponent<RocketMovement>().SetDamage(damage);
                        }
                        activeBullet.SetActive(true);
                    }
                    else
                    {
                        activeBullet = Instantiate(bullet, tipTransform.position, tipTransform.rotation);
                        if (bulletType == BulletType.Bullet)
                        {
                            activeBullet.GetComponent<BulletMovement>().SetDamage(damage);
                            activeBullet.GetComponent<BulletMovement>().SetActionAddBullet(AddBullet);
                        }
                        else
                        {
                            activeBullet.GetComponent<RocketMovement>().SetDamage(damage);
                            activeBullet.GetComponent<RocketMovement>().SetActionAddBullet(AddBullet);
                        }
                    }
                }
            }
            else
            {
                if(!activeReloading)
                {
                    activeReloading = true;
                    StartCoroutine(Reload());
                }
            }
        }
    }

    private IEnumerator WaitToFire()
    {
        if(gunshotAudio != null)
            gunshotAudio.Play();
        canFire = false;
        yield return new WaitForSeconds(firingDelay);
        canFire = true;
    }

    private IEnumerator Reload()
    {
        if(reloadAudio != null)
            reloadAudio.Play();
        reloadObjectAnimation.SetActive(true);
        yield return new WaitForSeconds(reloadTime);
        activeReloading = false;
        activeMagazine = magazineSize;
        reloadObjectAnimation.SetActive(false);
    }
}