using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    private new Transform transform;
    private Rigidbody2D rb2;
    private TrailRenderer trailRenderer;

    private float xMovement = 0.0f, yMovement = 0.0f;
    public float speed = 6.0f;
    public float dashForce = 30.0f;
    [Range(0.01f, 0.99f)]
    public float changeDashForce = 0.9f;
    public float breakDash = 4.0f;
    private bool dashing = false;
    private bool startDash = false;

    private void Start()
    {
        transform = this.gameObject.transform;
        rb2 = this.gameObject.GetComponent<Rigidbody2D>();
        trailRenderer = this.gameObject.GetComponent<TrailRenderer>();
    }

    private void Update() 
    {
        xMovement = Input.GetAxis("Horizontal");
        yMovement = Input.GetAxis("Vertical");

        if (!dashing)
        {
            //moving
            transform.Translate(new Vector3(xMovement, yMovement, 0) * Time.deltaTime * speed);
            
            //dashing
            if(Input.GetKeyDown(KeyCode.Space))
            {
                startDash = true;
            }
        }
    }

    private void FixedUpdate()
    {
        //start dashing
        if(startDash && rb2 != null)
        {
            rb2.velocity = new Vector2(xMovement, yMovement).normalized * dashForce;
            trailRenderer.emitting = true;
            startDash = false;
            dashing = true;
        }

        //handle dash logic
        if(dashing && rb2.velocity.magnitude < breakDash)
        {
            dashing = false;
            rb2.velocity = Vector2.zero;
            trailRenderer.emitting = false;
        }
        else if(dashing)
        {
            rb2.velocity = rb2.velocity * changeDashForce;
        }
    }
}
