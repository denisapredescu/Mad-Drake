using System;
using System.Collections;
using UnityEngine;

public class RocketMovement : MonoBehaviour
{
    private new Transform transform = null;
    private SpriteRenderer spriteRenderer = null;
    private Rigidbody2D rb2 = null;
    [SerializeField]
    private float speed = 1.0f;
    private int damage = 1;
    [SerializeField]
    private float timeUntilDestruction = 5.0f;
    //a function that adds this bullet refernce in the queue of a controller for later use
    private Action<GameObject> addBullet = null;
    private bool startDeactivating = false;
    [SerializeField]
    private ParticleSystem explosionEffect;
    private bool exploded = false;
    [SerializeField]
    private float explosionRange = 2.0f;

    private void Start()
    {
        transform = GetComponent<Transform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb2 = GetComponent<Rigidbody2D>();
        //auto disable bullet if it goes too far
        StartCoroutine(WaitToDeactivate());
    }

    private void OnEnable()
    {
        exploded = false;
        StartCoroutine(WaitToDeactivate());
    }

    private void Update()
    {
        //used when the object hits a collider
        if (startDeactivating && spriteRenderer.enabled == true)
            StartCoroutine(Deactivate());
    }

    private void HitBreakableBoxIfHit(GameObject objectHit)
    {
        if (objectHit.CompareTag("BreakableBox"))
            objectHit.GetComponent<BreakableBox>().HitBox();
    }

    private void FixedUpdate()
    {
        if (!startDeactivating)
        {
            rb2.MovePosition(transform.position + transform.TransformDirection(speed * Time.fixedDeltaTime * Vector3.right));
        }
        else if (!exploded)
        {
            exploded = true;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRange);
            foreach (Collider2D collider in colliders)
            {
                HitBreakableBoxIfHit(collider.gameObject);

                Vector3 forceDirection = collider.gameObject.transform.position - transform.position;
                float percentage = (explosionRange - forceDirection.magnitude) / explosionRange;

                if (collider.gameObject.CompareTag("Player"))
                {
                    collider.gameObject.GetComponent<HUDController>().TakeDamage((int)Mathf.Ceil(damage * percentage));
                }
                else if (collider.gameObject.CompareTag("Shadow"))
                {
                    collider.gameObject.GetComponent<ShadowBehaviour>().TakeDamage((int)Mathf.Ceil(damage * percentage));
                }
                else if (collider.gameObject.CompareTag("Shadow Guard"))
                {
                    collider.gameObject.GetComponent<EnemySkirmisherBehaviour>().TakeDamage((int)Mathf.Ceil(damage * percentage));
                }
            }
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (startDeactivating == true)
            return;

        if (!collision.gameObject.CompareTag("Player"))
            startDeactivating = true;
    }

    //passing the function from the controller back to the bullet to use it
    public void SetActionAddBullet(Action<GameObject> action)
    {
        addBullet = action;
    }

    public void SetDamage(uint value)
    {
        damage = (int)value;
    }

    private IEnumerator Deactivate()
    {
        explosionEffect.Play();
        spriteRenderer.enabled = false;
        //waits for the simulation to play
        yield return new WaitForSeconds(explosionEffect.main.duration / explosionEffect.main.simulationSpeed);
        gameObject.SetActive(false);
        spriteRenderer.enabled = true;
        startDeactivating = false;
        addBullet(gameObject);
    }

    private IEnumerator WaitToDeactivate()
    {
        yield return new WaitForSeconds(timeUntilDestruction);
        StartCoroutine(Deactivate());
    }
}
