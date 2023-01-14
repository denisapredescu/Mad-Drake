using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballBehaviour : MonoBehaviour
{
    [SerializeField]
    private float speed;
    [SerializeField]
    private float forceOfImpact;
    private Vector3 targetPosition;
    GameObject fireball;
    GameObject explosion;
    private bool canMove;

    private void Start()
    {
        GameObject player = GameObject.Find("Player");
        Vector3 playerPosition = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z);

        float xStart = 37.18f;
        float yStart = 0;
        float xTarget = playerPosition.x;
        float yTarget = playerPosition.y;

        targetPosition = new Vector3(-10, yStart + ((yTarget-yStart)/(xTarget-xStart))*(-10-xStart), transform.position.z);

        fireball = transform.Find("FireBall").gameObject;
        explosion = transform.Find("Explosion").gameObject;
        explosion.GetComponent<Animator>().enabled = false;

        canMove = true;
    }

    private void Update()
    {
        if(canMove)
            transform.position = Vector2.MoveTowards(transform.position, targetPosition * 2, speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.name == "Player")
        {
            // 0.2 time 
            Vector3 forces = collision.gameObject.transform.position - transform.position;
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(1, forces.normalized * forceOfImpact, 0.2f);
            canMove = false;
            Destroy(fireball);
            explosion.GetComponent<Animator>().enabled = true;
            StartCoroutine(DestroyMe());
        }

        else if (collision.CompareTag("Wall") || collision.CompareTag("Door"))
        {
            canMove = false;
            Destroy(fireball);
            explosion.GetComponent<Animator>().enabled = true;
            StartCoroutine(DestroyMe());
        }
    }

    private IEnumerator DestroyMe()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
