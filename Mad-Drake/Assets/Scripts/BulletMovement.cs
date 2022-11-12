using System;
using System.Collections;
using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    private new Transform transform = null;
    [SerializeField]
    private float speed = 0.0f;
    [SerializeField]
    private float timeUntilDestruction = 5.0f;
    private Action<GameObject> addBullet = null;
    private bool startDeactivating = false;

    private void Start()
    {
        transform = GetComponent<Transform>();
        StartCoroutine(WaitToDeactivate());
    }

    private void OnEnable()
    {
        StartCoroutine(WaitToDeactivate());
    }

    private void Update()
    {
        if(startDeactivating)
        {
            Deactivate();
            return;
        }

        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if(Physics2D.Raycast(transform.position, transform.TransformDirection(Vector3.right), speed * Time.fixedDeltaTime - 0.2f))
        {
            startDeactivating = true;
        }
    }

    public void setActionAddBullet(Action<GameObject> action)
    {
        addBullet = action;
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
        startDeactivating = false;
        addBullet(gameObject);
    }

    private IEnumerator WaitToDeactivate()
    {
        yield return new WaitForSeconds(timeUntilDestruction);
        Deactivate();
    }
}
