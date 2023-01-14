using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGun : MonoBehaviour
{
    private Transform target;
    [SerializeField]
    private Transform parentTransform;
    [SerializeField]
    private Transform tipTransform;
    [SerializeField]
    private EnemySkirmisherBehaviour enemyBehaviour;

    private Vector2 difference;
    private float distTip;
    private float distTarget;

    [SerializeField]
    private GameObject bullet = null;
    private Quaternion rotate180 = Quaternion.Euler(0, 0, 180);
    private Queue<GameObject> inactiveBullets;
    private GameObject activeBullet = null;
    private bool gunCanFire = true;
    [SerializeField]
    private int bulletBurst = 2;
    [SerializeField]
    private float delayInBurst = 0.2f;
    [SerializeField]
    private float reloadTime = 2.0f;

    private void Start()
    {
        inactiveBullets = new Queue<GameObject>();
    }

    private void Update()
    {
        target = enemyBehaviour.Target();

        if(target != null && enemyBehaviour.IsFollowing())
        {
            CalculateDifferenceVector();
            RotateGun();

            if(enemyBehaviour.CanShoot() && gunCanFire)
            {
                gunCanFire = false;
                StartCoroutine(Fire());
            }
        }
    }

    private IEnumerator Fire()
    {
        for (int i = 0; i < bulletBurst; i++)
        {
            if (parentTransform.localScale.x < 0.0f)
            {
                //checking if there is at least one inactive bullet or creating another
                if (inactiveBullets.Count > 0)
                {
                    activeBullet = inactiveBullets.Dequeue();
                    activeBullet.transform.SetPositionAndRotation(tipTransform.position, tipTransform.rotation);
                    activeBullet.SetActive(true);
                }
                else
                {
                    activeBullet = Instantiate(bullet, tipTransform.position, tipTransform.rotation);
                    if (activeBullet.TryGetComponent<BulletMovement>(out var bulletMovement))
                    {
                        bulletMovement.SetActionAddBullet(AddBullet);
                    }
                }
            }
            else
            {
                if (inactiveBullets.Count > 0)
                {
                    activeBullet = inactiveBullets.Dequeue();
                    activeBullet.transform.SetPositionAndRotation(tipTransform.position, tipTransform.rotation * rotate180);
                    activeBullet.SetActive(true);
                }
                else
                {
                    activeBullet = Instantiate(bullet, tipTransform.position, tipTransform.rotation * rotate180);
                    if (activeBullet.TryGetComponent<BulletMovement>(out var bulletMovement))
                    {
                        bulletMovement.SetActionAddBullet(AddBullet);
                    }
                }
            }

            yield return new WaitForSeconds(delayInBurst);
        }

        yield return new WaitForSeconds(reloadTime);
        gunCanFire = true;
    }

    private void AddBullet(GameObject gameObject)
    {
        this.inactiveBullets.Enqueue(gameObject);
    }

    void CalculateDifferenceVector()
    {
        //using tipTransform instead of transform actually gives the right point

        distTip = (
            new Vector2(transform.position.x, transform.position.y) -
            new Vector2(tipTransform.position.x, tipTransform.position.y)).magnitude;
        distTarget = (
            new Vector2(transform.position.x, transform.position.y) -
            new Vector2(target.position.x, target.position.y)).magnitude;

        //check if the distance from center to tip is bigger than the distance from center to point
        //that would cause the gun to glitch out because the difference vector would be in a completely other direction

        if (distTip + 1.0f >= distTarget)
        {
            difference = target.position - transform.position;
        }
        else
        {
            difference = target.position - tipTransform.position;
        }

        //possible yet unlikely error that the mouse hits exactly the center
        if (difference.magnitude == 0.0f)
        {
            difference = Vector2.right;
        }
        else
        {
            difference.Normalize();
        }
    }

    void RotateGun()
    {
        float gunRotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        if (parentTransform.localScale.x <= 0.0f)
        {
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, gunRotationZ);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, gunRotationZ + 180.0f);
        }
    }
}
