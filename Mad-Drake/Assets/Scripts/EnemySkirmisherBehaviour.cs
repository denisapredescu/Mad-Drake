using MyUtility;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemySkirmisherBehaviour : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    private bool follow = false;
    [SerializeField]
    private float minimumDistanceToFire = 5.0f;
    [SerializeField]
    private float speed = 2.0f;
    [SerializeField]
    private float moveCooldown = 2.0f;
    private bool canRun = true;
    private bool canShoot = false;
    private Rigidbody2D rb2;
    private Animator anim;
    private bool run = false;

    [SerializeField]
    private int health = 5;
    [SerializeField]
    private Slider healthBar;
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private AudioSource enemyDamageAudio;

    private void Start()
    {
        rb2 = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        healthBar.maxValue = health;
        healthBar.value = health;
    }

    private void Update()
    {
        if(follow)
        {
            anim.SetBool("run", run);

            if (target.position.x - 0.1f >= transform.position.x)
            {
                transform.localScale = new Vector3(
                    -Mathf.Abs(transform.localScale.x), 
                    transform.localScale.y, 
                    1.0f);

                canvas.transform.localScale = new Vector3(
                    -Mathf.Abs(canvas.transform.localScale.x), 
                    canvas.transform.localScale.y, 
                    1.0f);
            }

            if (target.position.x + 0.1f <= transform.position.x)
            {
                transform.localScale = new Vector3(
                    Mathf.Abs(transform.localScale.x), 
                    transform.localScale.y, 
                    1.0f);

                canvas.transform.localScale = new Vector3(
                    Mathf.Abs(canvas.transform.localScale.x), 
                    canvas.transform.localScale.y, 
                    1.0f);
            }
        }
    }

    private void FixedUpdate()
    {
        if(follow && canRun)
        {
            if (FloatMath.Larger((target.position - transform.position).magnitude, minimumDistanceToFire))
            {
                canShoot = false;
                run = true;
                rb2.MovePosition(
                    transform.position +
                    speed * Time.fixedDeltaTime * (target.position - transform.position).normalized);
            } 
            else
            {
                rb2.velocity = Vector3.zero;
                canShoot = true;
                run = false;
                canRun = false;
                StartCoroutine(PrepareRunning());
            }
        }
        else
        {
            rb2.velocity = Vector3.zero;
            run = false;
        }
    }

    private IEnumerator PrepareRunning()
    {
        yield return new WaitForSeconds(moveCooldown);
        canRun = true;
    }
    public void SetFollowingObject(GameObject toFollow)
    {
        target = toFollow.transform;
    }
    public Transform Target()
    {
        return target;
    }
    public void SetFollowing(bool value)
    {
        follow = value;
    }
    public bool IsFollowing()
    {
        return follow;
    }
    public bool CanShoot()
    {
        return canShoot;
    }
    public void TakeDamage(int value)
    {
        enemyDamageAudio.Play();
        health -= value;
        healthBar.value = health;
        if (health <= 0)
            gameObject.SetActive(false);
    }
}
