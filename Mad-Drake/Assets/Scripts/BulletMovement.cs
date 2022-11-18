using System;
using System.Collections;
using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    private new Transform transform = null;
    private SpriteRenderer spriteRenderer = null;
    [SerializeField]
    private float speed = 0.0f;
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
        //auto disable bullet if it goes too far
        StartCoroutine(WaitToDeactivate());
    }

    private void OnEnable()
    {
        StartCoroutine(WaitToDeactivate());
    }

    private void Update()
    {
        //used when the raycast hits a collider
        if(startDeactivating)
        {
            if (spriteRenderer.enabled == true)
            {
                StartCoroutine(Deactivate());
            }
            return;
        }
        else
        {
            transform.Translate(Vector3.right * speed * Time.deltaTime);
        }

        //moving the bullet with translate for performance
    }

    private void FixedUpdate()
    {
        //check if the bullet would hit something, it is needed because using translate can lead to the bullet teleporting without touching the collider
        if(Physics2D.Raycast(transform.position, transform.TransformDirection(Vector3.right), speed * Time.fixedDeltaTime - 0.2f))
        {
            startDeactivating = true;
        }
    }

    //passing the function from the controller back to the bullet to use it
    public void setActionAddBullet(Action<GameObject> action)
    {
        addBullet = action;
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
        Deactivate();
    }
}
