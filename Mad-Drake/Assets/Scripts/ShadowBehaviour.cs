using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject toFollow;
    private Rigidbody2D rb2;
    private bool follow = true;
    [SerializeField]
    private float speed = 1.0f;
    private Vector3 flipped = new(-1.0f, 1.0f, 1.0f);
    [SerializeField]
    private float maximumApproach = 1.0f;
    [SerializeField]
    private float attackRange = 0.1f;
    private Animator anim;

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
        if(follow && (toFollow.transform.position - transform.position).magnitude < maximumApproach + attackRange)
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

            if ((toFollow.transform.position - transform.position).magnitude > maximumApproach)
            {
                rb2.MovePosition(
                    transform.position +
                    speed * Time.deltaTime * (toFollow.transform.position - transform.position).normalized);
            }
        }
    }
}
