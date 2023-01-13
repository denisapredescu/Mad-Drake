using System;
using System.Collections;
using System.Net;
using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    private new Transform transform = null;
    private SpriteRenderer spriteRenderer = null;
    private Rigidbody2D rb2 = null;
    [SerializeField]
    private float speed = 0.0f;
    [SerializeField]
    private int damage = 1;
    [SerializeField]
    private float timeUntilDestruction = 5.0f;
    //a function that adds this bullet refernce in the queue of a controller for later use
    private Action<GameObject> addBullet = null;
    private bool startDeactivating = false;
    [SerializeField]
    private ParticleSystem explosionEffect;

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
            rb2.MovePosition(transform.position + transform.TransformDirection(speed * Time.fixedDeltaTime * Vector3.right));
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (startDeactivating == true)
            return;

        if (!collision.gameObject.CompareTag("IgnoreBullet"))
        {
            startDeactivating = true;
            HitBreakableBoxIfHit(collision.gameObject);

            if (collision.gameObject.CompareTag("Shadow"))
                collision.gameObject.GetComponent<ShadowBehaviour>().TakeDamage(damage+GameObject.FindGameObjectWithTag("IgnoreBullet").GetComponent<PlayerController>().bonusDamage);
        }
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