using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector3 direction = player.position - transform.position;

        direction.Normalize();
        movement = direction;
        RotatePlayer();
    }
    private void FixedUpdate()
    {
        moveCharacter(movement);
    }
    void moveCharacter(Vector2 direction)
    {
        rb.MovePosition((Vector2)transform.position + (direction * moveSpeed * Time.deltaTime));
    }

    public void RotatePlayer()
    {
        //rotate player left and right
        if((transform.position.x > player.position.x && transform.localScale.x > 0.0f) || 
            (transform.position.x < player.position.x && transform.localScale.x < 0.0f))
        {
            //this is added to smooth the transition
            if(Mathf.Abs(transform.position.x - player.position.x) > 0.1f)
            {
                transform.localScale = new Vector3(
                    -transform.localScale.x,
                    transform.localScale.y,
                    transform.localScale.z);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("aicicicicici");

            movement = Vector2.zero;
            moveCharacter(movement);
        }
    }
}