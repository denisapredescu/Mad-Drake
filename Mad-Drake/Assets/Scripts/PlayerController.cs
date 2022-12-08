using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private new Transform transform;
    private Rigidbody2D rb2;
    private TrailRenderer trailRenderer;
    [SerializeField]
    private Animator animator;

    private float xMovement = 0.0f, yMovement = 0.0f;
    public float speed = 6.0f;
    public float dashForce = 30.0f;
    [Range(0.01f, 0.99f)]
    public float changeDashForce = 0.9f;
    public float breakDash = 4.0f;
    private bool dashing = false;
    private bool startDash = false;
    private bool run = false;

    private Vector3 mousePos;
    private new Camera camera;

    private const float cameraDistanceLimitLR = 9.2f;
    private const float cameraDistanceLimitUD = 5.2f;

    public RoomController roomControllerScript;


    private void Start()
    {
        transform = gameObject.transform;
        rb2 = gameObject.GetComponent<Rigidbody2D>();
        trailRenderer = gameObject.GetComponent<TrailRenderer>();
        camera = Camera.main;
    }

    private void Update()
    {
        mousePos = camera.ScreenToWorldPoint(Input.mousePosition);

        xMovement = Input.GetAxis("Horizontal");
        yMovement = Input.GetAxis("Vertical");

        if (!dashing)
        {
            //animate running when the player is not dashing
            if (new Vector2(xMovement, yMovement).magnitude > 0.1f)
                animator.SetBool("run", true);
            else
                animator.SetBool("run", false);

            //start moving
            run = true;

            //dashing
            if (Input.GetKeyDown(KeyCode.Space))
            {
                startDash = true;
            }
        }
        else
        {
            run = false;
            animator.SetBool("run", false);
        }

        RotatePlayer();
        MoveCamera();
    }

    private void FixedUpdate()
    {
        //moving
        if (run)
            rb2.MovePosition(transform.position + speed * Time.fixedDeltaTime * new Vector3(xMovement, yMovement, 0));

        //start dashing
        if (startDash && rb2 != null)
        {
            rb2.velocity = new Vector2(xMovement, yMovement).normalized * dashForce;
            trailRenderer.emitting = true;
            startDash = false;
            dashing = true;
        }

        //handle dash logic
        if (dashing && rb2.velocity.magnitude < breakDash)
        {
            dashing = false;
            rb2.velocity = Vector2.zero;
            trailRenderer.emitting = false;
        }
        else if (dashing)
        {
            rb2.velocity *= changeDashForce;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            collision.gameObject.SetActive(false);
            PlayerHealthController.AddGold();
        }
    }

    public void RotatePlayer()
    {
        if (MenuController.GameRunning)
        {
            //rotate player left and right
            if ((transform.position.x > mousePos.x && transform.localScale.x > 0.0f) ||
                (transform.position.x < mousePos.x && transform.localScale.x < 0.0f))
            {
                //this is added to smooth the transition
                if (Mathf.Abs(transform.position.x - mousePos.x) > 0.1f)
                {
                    transform.localScale = new Vector3(
                        -transform.localScale.x,
                        transform.localScale.y,
                        transform.localScale.z);
                }
            }
        }
    }

    private void MoveCamera()
    {
        // move to left
        if (camera.transform.position.x - transform.position.x > cameraDistanceLimitLR)
        {
            camera.transform.position = new Vector3(camera.transform.position.x - 18, camera.transform.position.y, camera.transform.position.z);
        }
        // move to right
        else if (camera.transform.position.x - transform.position.x < -cameraDistanceLimitLR)
        {
            camera.transform.position = new Vector3(camera.transform.position.x + 18, camera.transform.position.y, camera.transform.position.z);
        }
        // move up
        else if (camera.transform.position.y - transform.position.y < -cameraDistanceLimitUD)
        {
            camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y + 10, camera.transform.position.z);
        }
        //move down
        else if (camera.transform.position.y - transform.position.y > cameraDistanceLimitUD)
        {
            camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y - 10, camera.transform.position.z);
        }
        roomControllerScript.DoorsFollowCamera();
    }
}