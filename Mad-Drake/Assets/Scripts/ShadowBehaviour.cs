using UnityEngine;
using UnityEngine.UI;

public class ShadowBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject toFollow;
    private Rigidbody2D rb2;
    private bool follow = false;
    [SerializeField]
    private float speed = 3.0f;
    [SerializeField]
    private float attackRange = 0.1f;
    private bool canAttack = false;
    private Animator anim;
    private Vector3 allignToCenter = new(0.0f, -0.5f);
    private readonly int layerMask = ~(1 << 2);

    [SerializeField]
    private int health = 5;
    [SerializeField]
    private Slider healthBar;
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private AudioSource shadowDamageAudio;

    private void Start()
    {
        rb2 = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        healthBar.maxValue = health;
    }

    private void Update()
    {
        if (toFollow.transform.position.x - transform.position.x < 0.0f)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                1);

            canvas.transform.localScale = new Vector3(
                Mathf.Abs(canvas.transform.localScale.x),
                canvas.transform.localScale.y,
                1);
        }
        else
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                1);

            canvas.transform.localScale = new Vector3(
                -Mathf.Abs(canvas.transform.localScale.x),
                canvas.transform.localScale.y,
                1);
        }

        if (canAttack)
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
        if(follow && toFollow != null)
        {
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

    public void SetFollowingObject(GameObject toFollow)
    {
        this.toFollow = toFollow;
    }
    public GameObject GetFollowingObject()
    {
        return toFollow;
    }
    public void SetFollowing(bool value)
    {
        follow = value;
    }
    public void TakeDamage(int value)
    {
        shadowDamageAudio.Play();
        health -= value;
        healthBar.value = health;
        if (health <= 0)
            gameObject.SetActive(false);
    }
}
