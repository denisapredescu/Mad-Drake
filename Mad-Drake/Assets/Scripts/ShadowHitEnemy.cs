using System.Collections;
using UnityEngine;

public class ShadowHitEnemy : MonoBehaviour
{
    private GameObject toHit;
    private bool canHit = false;
    [SerializeField]
    private int damage = 1;
    [SerializeField]
    private float forceOfImpact = 10.0f;
    [SerializeField]
    private float timeForImpact = 1.0f;
    [SerializeField]
    private float delay = 1.0f;
    private bool waiting = false;

    private ShadowBehaviour shadowBehaviour;

    private void Start()
    {
        shadowBehaviour = GetComponentInParent<ShadowBehaviour>();
        toHit = shadowBehaviour.GetFollowingObject();
    }

    private void Update()
    {
        toHit = shadowBehaviour.GetFollowingObject();
    }

    private void FixedUpdate()
    {
        Debug.Log("ajunge aici");
        if (canHit && toHit != null)
        {
            Vector3 forces = toHit.transform.position - transform.position;
            toHit.GetComponent<PlayerController>().TakeDamage(damage, forceOfImpact * forces.normalized, timeForImpact);
            canHit = false;
            StartCoroutine(WaitToHitAgain());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!waiting && collision.gameObject == toHit)
        {
            canHit = true;
        }
    }

    private IEnumerator WaitToHitAgain()
    {
        waiting = true;
        yield return new WaitForSeconds(delay);
        waiting = false;
    }
}
