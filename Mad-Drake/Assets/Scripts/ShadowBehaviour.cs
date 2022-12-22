using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class ShadowBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject toFollow;
    private Rigidbody2D rb2;
    private bool follow = true;
    [SerializeField]
    private float speed = 3.0f;
    private Vector3 flipped = new(-1.0f, 1.0f, 1.0f);
    [SerializeField]
    private float attackRange = 0.1f;
    private bool canAttack = false;
    private Animator anim;
    private Vector3 allignToCenter = new(0.0f, -0.5f);
    private readonly int layerMask = ~(1 << 2);

    private void Start()
    {
        rb2 = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    public void SetFollowing(bool value)
    {
        follow = value;
    }

    private void Update()
    {
        if(canAttack)
        {
            if(toFollow.transform.position.y > transform.position.y)
            {
                anim.SetBool("follow_high", true);
            } 
            else
            {
                anim.SetBool("follow_high", false);
            }

            anim.SetBool("attack", true);
        }
        else
        {
            anim.SetBool("attack", false);
        }
    }

    private void FixedUpdate()
    {
        if(follow)
        {
            if (toFollow.transform.position.x - transform.position.x < 0.0f)
                transform.localScale = Vector3.one;
            else
                transform.localScale = flipped;

            RaycastHit2D hit = Physics2D.Raycast(
                transform.position + allignToCenter, 
                (toFollow.transform.position - transform.position - allignToCenter).normalized,  
                attackRange,
                layerMask);

            canAttack = (hit.collider != null);

            rb2.MovePosition(
                transform.position +
                speed * Time.fixedDeltaTime * (toFollow.transform.position - transform.position).normalized);
        }
    }
}
